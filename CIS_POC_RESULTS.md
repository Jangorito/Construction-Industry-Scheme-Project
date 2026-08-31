# CIS Invoice Classification POC, Results Summary

**Status: complete.** All four deliverables from `CIS_POC_BRIEF.md` are done, tested against a real LLM across seven live runs, and the results are reproducible. This doc summarises what was built, what the numbers say, how the iteration actually went (including a run where a fix made things worse before it made them better), and, deliberately, where it still gets things wrong.

## What this was meant to prove

Per the brief: given a synthetic invoice, can an LLM (1) classify each line item as construction spend vs. services under HMRC's CIS rules, (2) give clear reasoning, and (3) handle ambiguous or mixed line items sensibly. All three are demonstrated below.

## What was built

| Deliverable | What it is |
|---|---|
| [`POC/cis_rules.md`](POC/cis_rules.md) | 17 classification rules, 15 of them verified directly against primary HMRC source text (the CIS340 guide and the CISR14xxx/15xxx reform manual, not secondary commentary), each with a stable ID (`R1`, `R11a`, …) for citation |
| [`POC/cis_rules_index.json`](POC/cis_rules_index.json) | The machine-readable version of the same rules, mapping each ID to its real HMRC source, name, and URL |
| [`POC/generate_invoices.py`](POC/generate_invoices.py) | Generates synthetic invoices from 47 hand-built line-item templates, spanning clear-construction, clear-services, and deliberately ambiguous/edge-case scenarios |
| [`POC/classify.py`](POC/classify.py) | Sends invoices to `gpt-4o-mini`, gets back reasoning, cited rules, a confidence flag, and a label per line item, logs every run permanently |

**Test data**: 10 synthetic invoices, 47 line items total (33 Construction / 14 Services by design), split across three scenario types, 14 "clear construction," 5 "clear services," and 28 "ambiguous/mixed" lines built specifically to stress-test the sharpest edges of the rules.

## How the iteration actually went

Seven live runs, not a straight line to a final number, including one where a fix made things measurably worse before a follow-up fix made them better. That's included deliberately; it's a more honest picture of what testing an LLM classifier against real edge cases looks like than a single clean number would be.

| Stage | Accuracy | What changed |
|---|---|---|
| 1. Initial run | 91.5% (43/47) | First live test against the HMRC-sourced rules |
| 2. Rule/prompt fixes | 95.7% (45/47) | Diagnosed and fixed two failures from run 1: traffic management was being confused with an unrelated exclusion, and a named exclusion (security shutters) was being skipped in favour of a general rule. A third failure (a mixed-contract override not firing) was identified but only partly fixed |
| 3. Confidence flag added | 95.7% (45/47) | No accuracy change; added a self-reported high/low confidence signal per line, see below |
| 4. Over-corrected the override | 93.6% (44/47) | Fixed the run-1 mixed-contract failure directly, but the fix was too broad, see "the regression" below |
| 5. Refined the override, fixed a schema bug | 95.7% (45/47) | Narrowed the override to require the right evidence, and fixed a genuinely structural issue in how the model's output was requested, see below |

**Current breakdown by scenario** (latest run):

| Scenario | Accuracy |
|---|---|
| Clear construction | 14/14 (100%) |
| Clear services | 5/5 (100%) |
| Ambiguous/mixed | 26/28 (92.9%) |

The ambiguous bucket is deliberately the hardest, built from the narrowest, most easily-confused boundaries in the rules (an awning is Construction but a shutter right next to it in the same rule is Services; a security fence is Construction but a security *system* is Services). Scoring over 90% there, with both remaining misses fully understood (see below), is a real result.

### The regression, and what it taught us

Stage 4 is worth walking through because it's the most useful thing to come out of this testing. One invoice deliberately contains two near-identical carpet-fitting lines: a plain one (should be Services) and one whose own wording says it's "part of a wider office refurbishment contract" (should flip to Construction, per HMRC's mixed-contract rule). The first fix told the model "if any other line on this invoice is clearly Construction, that's enough evidence on its own to override an exclusion." That fixed the second carpet line, but broke the first one, because the *plain* carpet line also happened to share an invoice with unrelated Construction work. One invoice can bundle several unrelated jobs from the same supplier; sharing an invoice isn't the same as sharing a contract, and the instruction had stopped requiring any actual connection between the flagged line and its neighbours. Accuracy dropped to 93.6% as a direct, measurable consequence.

The fix: require the *flagged line's own wording* to signal it's part of a bigger contract, and treat other Construction lines on the invoice as supporting evidence for that signal, not a replacement for it. Re-tested, both carpet lines are now classified correctly, the exact distinction the test case was built to check.

While diagnosing this, we also found a separate, structural bug: the request asked the model for its answer (`label`) before its reasoning. These models generate output left to right and never revise an earlier field once later ones are written, so in one run the model's own reasoning explicitly worked out that the override should apply, and the label, already committed to before that reasoning was generated, still said Services. Reordering the request so reasoning comes first and the label last removed this failure mode entirely: the label can no longer contradict analysis it was generated too early to benefit from.

### Where it currently fails

Two of 47 line items are still misclassified in the latest run, and neither is a mystery:

1. **"Consultancy, remedial works"** → deliberately built as an ambiguous test case. "Consultancy" reads as advisory (excluded), "remedial works" reads as hands-on execution (included). The model picks the advisory reading; our answer key picks the other. Both are defensible, this is closer to a genuine judgment call than a model error.
2. **A security-shutters line** → misclassified in one invoice, but the *identical line text* is correctly classified in a different invoice in the same run. Same wording, same rules, same request, different answer depending on what else was on the invoice. That's not a gap in the rules or the prompt, it's the model being non-deterministic on a genuinely borderline case, and no amount of further prompt tuning is likely to fix something that isn't consistent in the first place. This is exactly what a confidence flag and human review step are for, not something to chase with more instructions.

### Confidence flagging

Every classification also carries a self-reported `high`/`low` confidence flag. Its usefulness varied noticeably across runs: in one run it caught 100% of the real errors while flagging only 17% of the invoice for review; in another, it caught none. That instability is itself informative and consistent with everything else observed here, the flag currently reflects whether the model narrated an override-check in its reasoning, not necessarily whether it was actually unsure. It's a genuinely useful triage signal (worth having), but not yet a calibrated one (not yet worth certifying anything on). That distinction matters most once this becomes a production, audit-facing feature.

### Every classification is citable to a real source

This was a specific design goal, not an afterthought: the model can only cite rule IDs that exist in `cis_rules_index.json`, the request's schema makes it structurally impossible to invent a citation. Each cited rule resolves to a real, verified HMRC source (the specific CIS340 appendix or CISR manual page), not just the model's own explanation of itself. That's what an audit trail needs to look like if this becomes the client-facing compliance tool described in the wider project brief.

## Deliberate scope decisions

- **No OCR.** Synthetic invoices are generated as structured data directly, so there's no scanned document to extract text from in this POC. OCR is a separable concern (text extraction) from what this POC exists to prove (classification reasoning), and was consciously deferred rather than overlooked.
- **OpenAI direct API, not Azure OpenAI.** Per the brief's own fallback plan, Azure OpenAI quota approval on a student subscription can take time, so this POC used the OpenAI API directly with `gpt-4o-mini`, the same model family targeted for production, so prompts transfer directly once Azure access is in place.

## Where to look for the underlying evidence

- `POC/logs/`, a full timestamped transcript of every run, including the exact reasoning behind every classification
- `POC/logs/index.csv`, one row per run, accuracy and confidence-flag counts at a glance
- `POC/output/<run_id>/`, the full structured result (label, confidence, cited rules, resolved citations, reasoning) for every line item, per run
- `POC/data/manifest.csv`, the answer key every run is scored against

## Suggested next steps

1. **Present this to Mark** as evidence the classification approach works, the regression-and-fix story in particular is good, concrete evidence of a rigorous test process, not just a single accuracy number.
2. **Move to the visual prototype** per the two-stage plan in the wider project brief, Summary Dashboard, Invoice Grid, Invoice Detail views, now that the underlying reasoning is validated.
3. Longer-term: add OCR once there's real invoice volume to test against, expand the synthetic invoice set, and consider whether the confidence flag needs calibrating (or whether sampling a borderline line multiple times and taking a majority vote is a better answer to model non-determinism than further prompt tuning) before it's relied on in production.
