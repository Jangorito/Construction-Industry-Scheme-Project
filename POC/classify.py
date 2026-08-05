"""
classify.py: sends synthetic invoice line items to an LLM for CIS
classification, following the rules in cis_rules.md / cis_rules_index.json.
See ../CIS_POC_BRIEF.md for the overall POC this belongs to.

What this script does, per invoice JSON in data/invoices/:
  1. Strips out the "_ground_truth" answer key fields (generate_invoices.py
     puts those there for scoring only, the model must never see them).
  2. Sends the remaining line items (description, amount) plus the full text
     of cis_rules.md to the model, and asks for a Construction/Services
     label, a confidence ("high"/"low"), cited rule id(s), and a short plain
     English reasoning per line. Confidence is the model's own self report,
     not a calibrated accuracy score, "high" doesn't guarantee correct. It's
     meant for triage: which lines to have a human sanity check first, not a
     number to certify anything on. Whether it's trustworthy enough for that
     even is itself worth validating before this goes anywhere near
     production, not assumed.
  3. The rule_ids field is constrained (via the request's JSON schema) to
     only the ids that actually exist in cis_rules_index.json, so the model
     cannot invent a citation that doesn't exist, this is the whole reason
     the rules doc has stable ids in the first place.
  4. Resolves each cited rule id against cis_rules_index.json to attach the
     real HMRC source (name, title, url) to the output, this is the
     citation feature: the classification result names the actual guidance
     it is based on, not just the model's own explanation.
  5. Compares the model's label against the ground truth (only after the
     model has answered) and writes both a per invoice JSON result and a
     flat results_summary.csv, plus a report of what got misclassified.
     These go under output/<run_id>/, its own folder per run, so an older
     run's results are never overwritten by a newer one.
  6. Everything that would normally just print to the console is also
     written, in full, to a dedicated log file for that run under logs/, so
     past runs stay reachable after the terminal window is gone. See
     setup_run_logger() below for how that is wired up, and logs/index.csv
     for a one row per run summary across every run you have ever done,
     including the output/<run_id>/ folder that run's results live in.

  The same run_id (a timestamp) names both output/<run_id>/ and
  logs/classify_<run_id>.log, so the two are always easy to match up: given
  either one, the other is just a matter of swapping "output" for "logs".

Requires the openai package (pip install openai) and the OPENAI_API_KEY
environment variable, unless run with --dry-run (which skips the API call
entirely and fills in a placeholder response, useful for checking the rest
of the pipeline without cost or a key).

Usage:
    python classify.py [--invoices-dir PATH] [--output-dir PATH]
                        [--model NAME] [--limit N] [--dry-run]
"""

import argparse
import csv
import json
import logging
import os
import sys
from datetime import datetime
from pathlib import Path

POC_DIR = Path(__file__).parent
RULES_MD_PATH = POC_DIR / "cis_rules.md"
RULES_INDEX_PATH = POC_DIR / "cis_rules_index.json"
DEFAULT_INVOICES_DIR = POC_DIR / "data" / "invoices"
DEFAULT_OUTPUT_DIR = POC_DIR / "output"
LOGS_DIR = POC_DIR / "logs"
LOG_INDEX_PATH = LOGS_DIR / "index.csv"
DEFAULT_MODEL = "gpt-4o-mini"


def setup_run_logger(dry_run: bool) -> tuple:
    # Every run gets its own log file, named by timestamp, grouped together
    # under logs/ so past runs are easy to find later just by sorting the
    # folder. One logger writes to both the console and that file at once,
    # so nothing needs to be printed twice or kept in sync by hand: call
    # logger.info(...) once, it shows up in both places automatically.
    run_id = datetime.now().strftime("%Y%m%d-%H%M%S") + ("-dryrun" if dry_run else "")
    LOGS_DIR.mkdir(parents=True, exist_ok=True)
    log_path = LOGS_DIR / f"classify_{run_id}.log"

    # A logger named after this specific run, rather than the shared root
    # logger, so running classify.py twice in the same process (as tests
    # might) never mixes two runs' handlers together.
    logger = logging.getLogger(f"classify.{run_id}")
    logger.setLevel(logging.INFO)
    logger.propagate = False

    formatter = logging.Formatter("%(asctime)s  %(message)s", datefmt="%H:%M:%S")

    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setFormatter(formatter)
    logger.addHandler(console_handler)

    file_handler = logging.FileHandler(log_path, encoding="utf-8")
    file_handler.setFormatter(formatter)
    logger.addHandler(file_handler)

    return logger, run_id, log_path


def append_run_index(run_id: str, log_path: Path, run_output_dir: Path, args, stats: dict):
    # Adds one row to logs/index.csv summarising this run: enough to see at
    # a glance how a run's accuracy compared to earlier ones, and which log
    # file and output folder belong to it, without having to open every log
    # file just to find the one you want.
    LOG_INDEX_PATH.parent.mkdir(parents=True, exist_ok=True)
    is_new_file = not LOG_INDEX_PATH.exists()

    with LOG_INDEX_PATH.open("a", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        if is_new_file:
            writer.writerow([
                "run_id", "timestamp", "dry_run", "model", "invoices_dir",
                "num_invoices", "num_line_items", "correct", "accuracy_pct",
                "low_confidence_count", "log_path", "output_dir",
            ])
        pct = 100 * stats["correct"] / stats["total"] if stats["total"] else 0
        writer.writerow([
            run_id, datetime.now().isoformat(timespec="seconds"), args.dry_run, args.model,
            str(args.invoices_dir), stats["num_invoices"], stats["total"], stats["correct"],
            f"{pct:.1f}", len(stats["low_confidence"]), str(log_path), str(run_output_dir),
        ])


def load_rules():
    # Loads the two files the whole classification approach depends on: the
    # human readable rules doc (embedded directly into the prompt, this is
    # the "fold the rules doc into the prompt" step from the POC brief) and
    # its machine readable index (used to build the list of rule ids the
    # model is allowed to cite, and to resolve a cited id to its real HMRC
    # source afterward).
    rules_text = RULES_MD_PATH.read_text(encoding="utf-8")
    rules_index = json.loads(RULES_INDEX_PATH.read_text(encoding="utf-8"))
    rule_ids = [r["id"] for r in rules_index["rules"]]
    rules_by_id = {r["id"]: r for r in rules_index["rules"]}
    return rules_text, rule_ids, rules_by_id


SYSTEM_PROMPT_TEMPLATE = """You are classifying UK subcontractor invoice line items for the \
Construction Industry Scheme (CIS), following the rules doc below exactly.

Every line item must get exactly one label: "Construction" (CIS-relevant) or \
"Services" (non-CIS). Classify each line item independently, per rule R12, \
even when other lines on the same invoice have a different label. Where \
wording is genuinely ambiguous, apply R13: give your best answer but name \
in the reasoning what fact would change it, don't resolve it with false \
confidence.

Every line item also needs a confidence value: "high" or "low". Set "low" \
whenever any of these apply: you're relying on R13 because the wording is \
genuinely ambiguous, you had to make a judgment call about whether a \
mixed-contract override applies rather than finding a clear signal either \
way, or you considered two different rules that would give different \
answers and picked one over the other. Otherwise set "high". Don't default \
to "high" just because you were able to produce an answer, being able to \
answer and being sure are different things.

Follow this order, and don't skip a step: (1) Check whether this line \
matches one of R9's facilities/site-support exclusions or one of R10's \
five named exclusions (signage, seating/blinds/shutters, security systems, \
telecoms wiring, carpet fitting). A line item whose description contains a \
word like "services" is not automatically an R9 exclusion, check what the \
line actually describes doing, not just its label. (2) If it matches an \
R9/R10 exclusion, that match is provisional, not final. Before you write \
down Services, explicitly re-check for a mixed-contract override (R2/R12): \
does the line's own wording, or its place among this invoice's other \
lines, signal it's part of a bigger contract? If yes, the override wins \
and the label is Construction, regardless of the R9/R10 match. Only \
finalize Services once you've done this override check and found no such \
signal, and state in the reasoning that you checked for one either way. \
(3) Only fall back to a general inclusion rule like R1 or R3 once you've \
confirmed, via step (1), that this line isn't an R9/R10 exclusion in the \
first place.

For every line item, cite the rule id(s) (e.g. "R6", "R11a") that justify \
your answer. Only cite ids that appear in the rules doc below, never invent \
one. Keep each reasoning to 1-3 plain English sentences, suitable to show \
a client.

=== CIS CLASSIFICATION RULES ===
{rules_text}
=== END OF RULES ===
"""


def build_messages(invoice: dict, rules_text: str) -> list:
    # Builds the two chat messages sent to the model. The user message only
    # includes what a real invoice would actually show: line_id,
    # description, and amount. It deliberately omits every "_ground_truth"
    # key so the model is scored blind, exactly like it would be on a real
    # invoice with no known answer.
    system_prompt = SYSTEM_PROMPT_TEMPLATE.format(rules_text=rules_text)

    visible_invoice = {
        "invoice_id": invoice["invoice_id"],
        "supplier": invoice["supplier"],
        "customer": invoice["customer"],
        "date": invoice["date"],
        "line_items": [
            {
                "line_id": li["line_id"],
                "description": li["description"],
                "amount_gbp": li["amount_gbp"],
            }
            for li in invoice["line_items"]
        ],
    }

    user_prompt = (
        "Classify every line item on this invoice:\n\n"
        + json.dumps(visible_invoice, indent=2)
    )

    return [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": user_prompt},
    ]


def build_response_schema(rule_ids: list) -> dict:
    # The response is constrained so "label" can only be one of the two
    # allowed values, "confidence" can only be "high" or "low", and
    # "rule_ids" can only contain ids that exist in cis_rules_index.json.
    # This is what actually stops the model from citing a source that
    # doesn't exist: the enum makes an invalid id structurally impossible to
    # return, not just discouraged by wording. "confidence" is the model's
    # own self report, not a validated score, see the module docstring.
    return {
        "type": "json_schema",
        "json_schema": {
            "name": "cis_line_item_classification",
            "strict": True,
            "schema": {
                "type": "object",
                "properties": {
                    "line_items": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "line_id": {"type": "string"},
                                "label": {"type": "string", "enum": ["Construction", "Services"]},
                                "confidence": {"type": "string", "enum": ["high", "low"]},
                                "rule_ids": {
                                    "type": "array",
                                    "items": {"type": "string", "enum": rule_ids},
                                },
                                "reasoning": {"type": "string"},
                            },
                            "required": ["line_id", "label", "confidence", "rule_ids", "reasoning"],
                            "additionalProperties": False,
                        },
                    },
                },
                "required": ["line_items"],
                "additionalProperties": False,
            },
        },
    }


def call_model(client, model: str, messages: list, schema: dict) -> dict:
    # The actual API call. Temperature 0 because this is a classification
    # task where we want the single most likely answer each time, not
    # creative variety.
    response = client.chat.completions.create(
        model=model,
        messages=messages,
        response_format=schema,
        temperature=0,
    )
    return json.loads(response.choices[0].message.content)


def dry_run_classify(invoice: dict) -> dict:
    # Stands in for call_model() when --dry-run is passed: no network call,
    # no API key needed. Deliberately does NOT copy the ground truth, it
    # alternates a placeholder answer per line so the rest of the pipeline
    # (validation, citation resolution, scoring, file output) can be
    # exercised end to end without pretending to test real classification
    # accuracy. The first line is given a bogus rule id that doesn't exist,
    # to prove resolve_citations() below actually catches an unresolvable
    # id rather than silently trusting it. Confidence alternates too, purely
    # so a --dry-run has at least one of each value to exercise the low
    # confidence reporting path below, not because it means anything here.
    line_items = []
    for i, li in enumerate(invoice["line_items"]):
        if i == 0:
            rule_ids, note = ["R99"], "dry run placeholder with a deliberately invalid rule id"
        else:
            rule_ids, note = ["R1"], "dry run placeholder, not a real classification"
        line_items.append({
            "line_id": li["line_id"],
            "label": "Construction" if i % 2 == 0 else "Services",
            "confidence": "high" if i % 3 else "low",
            "rule_ids": rule_ids,
            "reasoning": note,
        })
    return {"line_items": line_items}


def resolve_citations(rule_ids: list, rules_by_id: dict) -> list:
    # Turns a list of rule ids the model cited into the real HMRC source
    # info behind each one, by looking it up in cis_rules_index.json rather
    # than trusting anything the model said about where the rule came from.
    # An id that doesn't resolve (shouldn't happen given the schema enum,
    # but checked anyway in case a non-strict fallback path is ever used) is
    # kept in the output with resolved=False so it stays visible, not dropped.
    citations = []
    for rid in rule_ids:
        rule = rules_by_id.get(rid)
        if rule is None:
            citations.append({"rule_id": rid, "resolved": False})
            continue
        citations.append({
            "rule_id": rid,
            "resolved": True,
            "source_name": rule["source_name"],
            "source_title": rule["source_title"],
            "source_url": rule["source_url"],
            "verified": rule["verified"],
        })
    return citations


def score_invoice(invoice: dict, model_output: dict, rules_by_id: dict) -> dict:
    # Combines the model's answer for each line with that line's
    # "_ground_truth" block (read only now, after the model has already
    # answered blind) to produce one result record per line: what the model
    # said, what the real answer is, whether it matched, and the resolved
    # citations for whatever rule ids the model cited.
    ground_truth_by_line = {
        li["line_id"]: li["_ground_truth"] for li in invoice["line_items"]
    }
    predicted_by_line = {
        li["line_id"]: li for li in model_output["line_items"]
    }

    results = []
    for li in invoice["line_items"]:
        line_id = li["line_id"]
        gt = ground_truth_by_line[line_id]
        predicted = predicted_by_line.get(line_id)

        if predicted is None:
            # The model dropped a line item entirely, treat as a miss
            # rather than crashing. This is exactly the kind of failure the
            # brief wants surfaced, not hidden.
            results.append({
                "line_id": line_id,
                "description": li["description"],
                "amount_gbp": li["amount_gbp"],
                "predicted_label": None,
                "predicted_confidence": None,
                "predicted_rule_ids": [],
                "reasoning": None,
                "citations": [],
                "expected_label": gt["label"],
                "expected_rule_ids": gt["rule_ids"],
                "expected_note": gt["note"],
                "gap_flag": gt["gap"],
                "correct": False,
                "error": "model did not return this line_id",
            })
            continue

        citations = resolve_citations(predicted["rule_ids"], rules_by_id)
        results.append({
            "line_id": line_id,
            "description": li["description"],
            "amount_gbp": li["amount_gbp"],
            "predicted_label": predicted["label"],
            "predicted_confidence": predicted["confidence"],
            "predicted_rule_ids": predicted["rule_ids"],
            "reasoning": predicted["reasoning"],
            "citations": citations,
            "expected_label": gt["label"],
            "expected_rule_ids": gt["rule_ids"],
            "expected_note": gt["note"],
            "gap_flag": gt["gap"],
            "correct": predicted["label"] == gt["label"],
            "error": None,
        })

    return {
        "invoice_id": invoice["invoice_id"],
        "scenario": invoice["scenario"],
        "supplier": invoice["supplier"],
        "line_results": results,
    }


def write_outputs(all_results: list, output_dir: Path) -> Path:
    # One JSON file per invoice (full detail, including citations and
    # reasoning) plus a single flat results_summary.csv (one row per line
    # item) so the whole run can be skimmed or opened in a spreadsheet
    # without digging through individual JSON files. `output_dir` is always
    # this specific run's own folder (output/<run_id>/, built in main()), so
    # nothing written here ever overwrites an earlier run's results.
    output_dir.mkdir(parents=True, exist_ok=True)

    for result in all_results:
        path = output_dir / f"{result['invoice_id']}.result.json"
        path.write_text(json.dumps(result, indent=2), encoding="utf-8")

    summary_path = output_dir / "results_summary.csv"
    with summary_path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow([
            "invoice_id", "scenario", "line_id", "description", "amount_gbp",
            "predicted_label", "predicted_confidence", "expected_label", "correct",
            "predicted_rule_ids", "expected_rule_ids", "gap_flag", "reasoning", "error",
        ])
        for result in all_results:
            for line in result["line_results"]:
                writer.writerow([
                    result["invoice_id"], result["scenario"], line["line_id"],
                    line["description"], line["amount_gbp"], line["predicted_label"],
                    line["predicted_confidence"], line["expected_label"], line["correct"],
                    "|".join(line["predicted_rule_ids"]), "|".join(line["expected_rule_ids"]),
                    line["gap_flag"], line["reasoning"], line["error"],
                ])

    return summary_path


def compute_stats(all_results: list) -> dict:
    # Walks every line result once and builds the numbers both the report
    # and the run index need: overall total/correct, the same broken down
    # per scenario, the full list of misclassified lines, and separately
    # the full list of lines the model itself flagged low confidence on.
    # Those two lists overlap but aren't the same thing: a low confidence
    # line can still have landed on the correct label, and that's exactly
    # the case this is for, a human reviewing "what should I double check"
    # wants both the ones that were wrong and the ones the model itself
    # wasn't sure about, not just the ones scoring happens to catch. Kept
    # separate from log_report() below so main() can also pull "total" and
    # "correct" straight out of this for the logs/index.csv row, instead of
    # counting everything a second time.
    total = 0
    correct = 0
    by_scenario = {}
    misses = []
    low_confidence = []

    for result in all_results:
        scenario = result["scenario"]
        by_scenario.setdefault(scenario, {"total": 0, "correct": 0})
        for line in result["line_results"]:
            total += 1
            by_scenario[scenario]["total"] += 1
            if line["correct"]:
                correct += 1
                by_scenario[scenario]["correct"] += 1
            else:
                misses.append((result["invoice_id"], line))
            if line["predicted_confidence"] == "low":
                low_confidence.append((result["invoice_id"], line))

    return {
        "num_invoices": len(all_results),
        "total": total,
        "correct": correct,
        "by_scenario": by_scenario,
        "misses": misses,
        "low_confidence": low_confidence,
    }


def log_report(logger, stats: dict):
    # Writes the same summary either to the console alone, or to the
    # console and the run's log file at once, depending on how `logger` was
    # set up: overall accuracy, accuracy broken down by scenario (clear
    # cases should score near perfect, ambiguous_mixed is where the real
    # signal lives), and the full list of misclassified lines with what was
    # predicted vs expected. This is the "inspect failures" step from the
    # brief, now kept permanently instead of scrolling off the terminal.
    total = stats["total"]
    correct = stats["correct"]

    if total:
        logger.info(f"Overall accuracy: {correct}/{total} ({100 * correct / total:.1f}%)")
    else:
        logger.info("No results.")

    for scenario, counts in stats["by_scenario"].items():
        pct = 100 * counts["correct"] / counts["total"] if counts["total"] else 0
        logger.info(f"  {scenario}: {counts['correct']}/{counts['total']} ({pct:.1f}%)")

    misses = stats["misses"]
    if misses:
        logger.info(f"{len(misses)} misclassified line(s):")
        for invoice_id, line in misses:
            gap_note = " [known rules-doc gap]" if line["gap_flag"] else ""
            logger.info(f"  {invoice_id} / {line['line_id']}{gap_note}")
            logger.info(f"    \"{line['description']}\"")
            logger.info(f"    predicted: {line['predicted_label']} (rules: {line['predicted_rule_ids']})")
            logger.info(f"    expected:  {line['expected_label']} (rules: {line['expected_rule_ids']}) "
                        f"- {line['expected_note']}")
            if line["reasoning"]:
                logger.info(f"    model reasoning: {line['reasoning']}")
    else:
        logger.info("No misclassifications.")

    low_confidence = stats["low_confidence"]
    if low_confidence:
        logger.info(f"{len(low_confidence)} line(s) flagged low confidence by the model "
                     "(review candidates regardless of whether they scored correct):")
        for invoice_id, line in low_confidence:
            correctness = "correct" if line["correct"] else "INCORRECT"
            logger.info(f"  {invoice_id} / {line['line_id']} ({correctness})")
            logger.info(f"    \"{line['description']}\"")
            logger.info(f"    predicted: {line['predicted_label']} (rules: {line['predicted_rule_ids']})")
            if line["reasoning"]:
                logger.info(f"    model reasoning: {line['reasoning']}")
    else:
        logger.info("No lines flagged low confidence.")


def main():
    # CLI entry point: loads the rules, loops over every invoice JSON,
    # classifies it (live or --dry-run), scores it against ground truth,
    # writes the results, and logs the report to both the console and this
    # run's permanent log file.
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--invoices-dir", type=Path, default=DEFAULT_INVOICES_DIR,
                         help="Directory of invoice JSON files to classify (default: data/invoices)")
    parser.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT_DIR,
                         help="Root directory for results, a subfolder named after this run's id is "
                              "created inside it (default: output)")
    parser.add_argument("--model", default=DEFAULT_MODEL,
                         help=f"OpenAI model to use (default: {DEFAULT_MODEL})")
    parser.add_argument("--limit", type=int, default=None,
                         help="Only classify the first N invoices (useful for a quick, cheap smoke test)")
    parser.add_argument("--dry-run", action="store_true",
                         help="Skip the API call, use a placeholder response instead, no key or cost needed")
    args = parser.parse_args()

    logger, run_id, log_path = setup_run_logger(args.dry_run)
    logger.info(f"Starting classify.py run {run_id} (model={args.model}, dry_run={args.dry_run})")

    # Same run_id as the log file, so output/<run_id>/ and this run's log
    # always name-match, and a new run can never clobber an older one's
    # results.
    run_output_dir = args.output_dir / run_id

    rules_text, rule_ids, rules_by_id = load_rules()

    invoice_paths = sorted(args.invoices_dir.glob("*.json"))
    if args.limit:
        invoice_paths = invoice_paths[:args.limit]
    if not invoice_paths:
        logger.info(f"No invoice JSON files found in {args.invoices_dir}, run generate_invoices.py first.")
        sys.exit(1)

    client = None
    schema = None
    if not args.dry_run:
        # Imported here rather than at module load, so --dry-run works even
        # if the openai package or an API key isn't set up yet.
        from openai import OpenAI
        api_key = os.environ.get("OPENAI_API_KEY")
        if not api_key:
            logger.info("OPENAI_API_KEY is not set. Either set it, or pass --dry-run to test the "
                        "pipeline without calling the API.")
            sys.exit(1)
        client = OpenAI(api_key=api_key)
        schema = build_response_schema(rule_ids)

    all_results = []
    for path in invoice_paths:
        invoice = json.loads(path.read_text(encoding="utf-8"))
        logger.info(f"Classifying {invoice['invoice_id']} ({invoice['scenario']}, "
                    f"{len(invoice['line_items'])} line items)...")

        if args.dry_run:
            model_output = dry_run_classify(invoice)
        else:
            messages = build_messages(invoice, rules_text)
            model_output = call_model(client, args.model, messages, schema)

        all_results.append(score_invoice(invoice, model_output, rules_by_id))

    summary_path = write_outputs(all_results, run_output_dir)
    stats = compute_stats(all_results)
    log_report(logger, stats)
    append_run_index(run_id, log_path, run_output_dir, args, stats)

    logger.info(f"Per-invoice results written to: {run_output_dir}")
    logger.info(f"Flat summary written to: {summary_path}")
    logger.info(f"This run's log: {log_path}")
    logger.info(f"All runs' index: {LOG_INDEX_PATH}")
    if args.dry_run:
        logger.info("(This was a --dry-run: no API call was made, the accuracy numbers above are "
                    "meaningless, they only prove the pipeline runs end to end.)")


if __name__ == "__main__":
    main()
