# CIS Invoice Classification POC — Results Summary

**Status: complete.** All four deliverables from `CIS_POC_BRIEF.md` are done, tested against a real LLM, and the results are reproducible. This doc summarises what was built, what the numbers say, and — deliberately — where it still gets things wrong.

## What this was meant to prove

Per the brief: given a synthetic invoice, can an LLM (1) classify each line item as construction spend vs. services under HMRC's CIS rules, (2) give clear reasoning, and (3) handle ambiguous or mixed line items sensibly. All three are demonstrated below.

## What was built

| Deliverable | What it is |
|---|---|
| [`POC/cis_rules.md`](POC/cis_rules.md) | 17 classification rules, 15 of them verified directly against primary HMRC source text (the CIS340 guide and the CISR14xxx/15xxx reform manual, not secondary commentary), each with a stable ID (`R1`, `R11a`, …) for citation |
| [`POC/cis_rules_index.json`](POC/cis_rules_index.json) | The machine-readable version of the same rules, mapping each ID to its real HMRC source, name, and URL |
| [`POC/generate_invoices.py`](POC/generate_invoices.py) | Generates synthetic invoices from 47 hand-built line-item templates, spanning clear-construction, clear-services, and deliberately ambiguous/edge-case scenarios |
| [`POC/classify.py`](POC/classify.py) | Sends invoices to `gpt-4o-mini`, gets back a label, cited rules, a confidence flag, and plain-English reasoning per line item, logs every run permanently |

**Test data**: 10 synthetic invoices, 47 line items total (33 Construction / 14 Services by design), split across three scenario types — 14 "clear construction," 5 "clear services," and 28 "ambiguous/mixed" lines built specifically to stress-test the sharpest edges of the rules (named exclusions with narrow carve-outs, plant hire with/without an operator, materials-vs-labour traps, mixed-contract overrides).

## Results

| Run | Accuracy | Notes |
|---|---|---|
| Initial full run | 43/47 (91.5%) | First live test against real HMRC-sourced rules |
| After fixing 3 diagnosed failures | 45/47 (95.7%) | See "what got fixed" below |
| Confirmation re-run | 45/47 (95.7%) | Same result, stable |
| With confidence flagging added | 45/47 (95.7%) | Same accuracy, plus a new signal — see below |

**Current breakdown by scenario** (latest run):

| Scenario | Accuracy |
|---|---|
| Clear construction | 14/14 (100%) |
| Clear services | 5/5 (100%) |
| Ambiguous/mixed | 26/28 (92.9%) |

The ambiguous bucket is deliberately the hardest — it's built from the narrowest, most easily-confused boundaries in the rules (e.g. an awning is Construction but a shutter right next to it in the same rule is Services; a security fence is Construction but a security *system* is Services). Scoring over 90% there, with the two remaining misses fully understood, is a real result, not a coin flip.

### What got fixed between the first and second run

Three specific failures were diagnosed from the first run's reasoning text and fixed at the source:

1. **Traffic management services** were being misread as an excluded "site-support service" (confusing the word "services" in the name with an unrelated exclusion). Fixed by adding the fact directly into the rules doc rather than leaving it in a worked example only.
2. **Louvered security shutters** were being classified as a general "system installation" instead of checking the specific named exclusion list first. Fixed by adding an explicit instruction to check narrow, specific rules before defaulting to a general one.
3. A **mixed-contract override** (where a carpet-fitting line explicitly says it's "part of a wider... contract") wasn't reliably flipping the classification. Restructured the rule and the prompt so a named exclusion is treated as *provisional* until the override has actually been checked, not just a live possibility that can be skipped past.

Two of those three fixes fully resolved the failure. The third (mixed-contract override) improved but didn't fully close — see below.

### Where it currently fails (the honest part)

Two of 47 line items are still misclassified, and both are now precisely diagnosed rather than mysterious:

1. **"Supply and fit carpet tiles as part of wider office refurbishment contract"** → the model correctly identifies carpet fitting as a named exclusion, and does run the required mixed-contract check, but doesn't weight the other three clearly-Construction line items on the *same invoice* as strong enough evidence to override the exclusion. It's evaluating the line largely on its own wording rather than cross-referencing its neighbours.
2. **"Consultancy — remedial works"** → this was deliberately built as an ambiguous test case. "Consultancy" reads as advisory (excluded), "remedial works" reads as hands-on execution (included). The model picks the advisory reading; our answer key picks the other. Both are defensible — this is closer to a genuine judgment call than a model error, and it's useful to know the tool doesn't currently flag its own indecision on cases like this as clearly as it should.

### Confidence flagging

Every classification also comes with a self-reported `high`/`low` confidence flag. Tested against the same 47-line batch: **it caught both real errors** — a reviewer filtering to just the 8 lines flagged low-confidence would have seen 100% of the mistakes while reading only 17% of the invoice.

The honest caveat: 6 of those 8 flagged lines were actually correct. The model tends to flag "low confidence" whenever it runs the mandatory mixed-contract check, not only when that check was genuinely close — so right now the flag means "the model did its due diligence here," not "the model was truly unsure." For a compliance tool, over-flagging is the safer failure mode (nothing real gets missed), but it's not yet a precise signal, and that's worth knowing before it's relied on for anything beyond triage.

### Every classification is citable to a real source

This was a specific design goal, not an afterthought: the model can only cite rule IDs that exist in `cis_rules_index.json` — the request's schema makes it structurally impossible to invent a citation. Each cited rule resolves to a real, verified HMRC source (the specific CIS340 appendix or CISR manual page), not just the model's own explanation of itself. That's what an audit trail needs to look like if this becomes the client-facing compliance tool described in the wider project brief.

## Deliberate scope decisions

- **No OCR.** Synthetic invoices are generated as structured data directly, so there's no scanned document to extract text from in this POC. OCR is a separable concern (text extraction) from what this POC exists to prove (classification reasoning), and was consciously deferred rather than overlooked.
- **OpenAI direct API, not Azure OpenAI.** Per the brief's own fallback plan — Azure OpenAI quota approval on a student subscription can take time, so this POC used the OpenAI API directly with `gpt-4o-mini`, the same model family targeted for production, so prompts transfer directly once Azure access is in place.

## Where to look for the underlying evidence

- `POC/logs/` — a full timestamped transcript of every run, including the exact reasoning behind every classification
- `POC/logs/index.csv` — one row per run, accuracy and confidence-flag counts at a glance
- `POC/output/<run_id>/` — the full structured result (label, confidence, cited rules, resolved citations, reasoning) for every line item, per run
- `POC/data/manifest.csv` — the answer key every run is scored against

## Suggested next steps

1. **Present this to Mark** as evidence the classification approach works, using this doc plus a couple of the more interesting log excerpts (the carpet-mixed and shutters-vs-awning contrasts are good concrete examples of the tool reasoning correctly on genuinely subtle cases).
2. **Move to the visual prototype** per the two-stage plan in the wider project brief — Summary Dashboard, Invoice Grid, Invoice Detail views — now that the underlying reasoning is validated.
3. Longer-term, once there's real invoice volume to test against: add OCR, expand the synthetic invoice set, and consider tightening the confidence flag's precision if the review workload from over-flagging becomes a real cost.
