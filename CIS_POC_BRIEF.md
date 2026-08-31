# CIS Invoice Classification, Proof of Concept Brief

## Project context

This is a proof-of-concept for a larger internal tool: a **CIS (Construction Industry Scheme) invoice compliance tool** that uses AI to classify invoice line items as either:
- **Construction spend** (CIS-relevant per HMRC rules), or
- **Services** (non-CIS)

The end goal is an Angular + C# production app with Azure Blob Storage and Azure OpenAI, deployed internally, giving a stakeholder ("Mark") a summary dashboard, an invoice grid, and a line-item drill-down view with AI reasoning and manual override.

**This POC is a scoped-down first step** to validate that the classification *reasoning* works before any of that infrastructure is built. Nothing here should depend on Azure, C#, Angular, or a database, those come later, after Mark signs off on a separate visual prototype.

## What this POC needs to prove

Given a synthetic invoice with line items, can an LLM:
1. Classify each line item as construction spend vs. services, per HMRC CIS rules
2. Give clear reasoning for the classification
3. Handle ambiguous/mixed line items sensibly

## Sequencing (do these in order)

1. **Build a CIS rules reference doc first.** Pull real HMRC CIS guidance (materials vs. labour, construction operations vs. professional/consultancy services, mixed invoices, edge cases). Turn it into a concise rules doc, 10–15 concrete rules plus a few worked examples. This is the foundation the classification prompt is built on.
2. **Fold the rules doc into the classification prompt** and test it against synthetic invoices immediately, don't treat rules-writing and POC-running as separate phases.
3. **Generate synthetic test invoices**: a mix of clear-construction, clear-services, and deliberately ambiguous/mixed line items to stress-test edge cases.
4. **Run classification, inspect failures, iterate the rules doc.** Most of the real value is here, each misclassification points at a missing or ambiguous rule.

## Tech stack for this POC phase only

Keep this deliberately minimal and disposable, the only goal is validating the classification logic, not building infrastructure.

- **Language:** Python
- **OCR:** `pytesseract` or `PyMuPDF` for local/zero-dependency OCR (Azure Document Intelligence later if staying in the Azure ecosystem)
- **LLM classification:** OpenAI direct API, `gpt-4o-mini` (fallback while Azure OpenAI quota is pending, same model family as the eventual production target, so prompts transfer directly)
- **Synthetic invoice generation:** a Python script templating realistic line items against the CIS rules doc
- **Storage:** local JSON/CSV only, no database yet
- **Output/review:** a simple script or notebook printing invoice → classification → reasoning, no UI needed yet

### Explicitly out of scope for this POC
- No C# API layer
- No Angular front end
- No Azure Blob Storage
- No database
- No Azure OpenAI dependency (use OpenAI direct API as fallback; Azure quota approval can take up to 10 business days on a student subscription)

## Deliverables from this POC

1. `cis_rules.md`, the HMRC CIS classification rules reference doc (living document, iterated as failures surface)
2. `generate_invoices.py`, synthetic invoice generator
3. `classify.py`, script that sends invoice line items + rules doc to the LLM and returns classification + reasoning
4. A handful of test runs showing invoice → classification → reasoning, including at least a few ambiguous cases, to demonstrate the approach works and where it currently fails

## Context for later (not part of this POC, but good to know)

- **Production stack:** Angular front end (Mark's preference), C# API layer, Azure Blob Storage for invoice ingestion, Azure OpenAI (targeting GPT-4o-mini) for classification
- **Azure setup:** Student Azure subscription is hitting quota approval friction; recommended path once unblocked is a classic Azure OpenAI resource (not AI Foundry) in East US region, better quota approval track record
- **Two-stage plan:** (1) visual sign-off on a clickable HTML/design-tool prototype covering Summary Dashboard, Invoice Grid, and Invoice Detail views, then (2) Angular + C# production build
- **Stakeholder milestone:** presentation to Mark planned for week commencing the 17th or 24th; ask for an hour, bundle clarifying questions rather than sending them one at a time
