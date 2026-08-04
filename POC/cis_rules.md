# CIS Classification Rules — Reference Doc

Living document. Source: HMRC CIS340 ("Construction Industry Scheme: a guide for
contractors and subcontractors") and the CIS Reform Manual (CISR14xxx/CISR15xxx
series) on gov.uk. Built for the POC classification prompt — each rule below
should be citable back to a real HMRC position, not an assumption. Iterate this
doc every time a test invoice gets misclassified: add the missing rule or
sharpen an ambiguous one rather than patching the prompt with a one-off
exception.

Two output labels per line item: **Construction** (CIS-relevant) or
**Services** (non-CIS). Rules R12–R13 cover the cases that don't cleanly fit
either label at face value (materials, mixed contracts) — resolve those down
to one of the two labels plus a note, don't invent a third label.

**Rule IDs and citations.** Every rule has a stable ID (`R1`, `R6`, `R11a`, …)
and a `Source:` line naming what backs it — either a real HMRC reference
(CIS340, a CISR page) or `POC-METHOD` for a rule that's our own line-item
classification protocol rather than a restatement of HMRC guidance. Each
HMRC-sourced rule is also marked `primary` (we read the actual gov.uk
page/PDF ourselves) or `secondary` (sourced via search-result summaries of
commentary sites, not yet checked against the primary text — see Open Items).

**Why IDs matter beyond this doc:** the classification prompt should have the
LLM cite rule IDs it applied (e.g. `["R6", "R11a"]`), not free-text citations
or URLs — an LLM asked to recall "CISR14270" from memory can get the number
wrong or misquote it. [`cis_rules_index.json`](cis_rules_index.json) is the
machine-readable version of the table below (id → title, source, url,
verification status); `classify.py` and any later DB/UI layer should resolve
rule IDs against that file rather than trusting LLM-generated citation text.
Keep both files in sync when a rule changes.

## Core test

**R1 — Construction operations, broadly defined.** CIS applies to
construction, alteration, repair, extension, demolition, or dismantling of
buildings, structures, or works forming part of the land (roads, railways,
bridges, harbours, power lines, pipelines, reservoirs, sewers, industrial
plant). Site preparation, finishing, and integral/preparatory work (e.g.
scaffolding erected as part of a build) all count. This is the default
"Construction" bucket.
*Source: CIS340 (primary).*

**R2 — The scope test is broad and contract-level, not item-level.** HMRC's
own framing: "the expression 'relating to construction operations' is quite
broad... even if the construction element only forms part of the overall
contract." A contract to supply and install a fitted kitchen is a
construction contract because of the installation element, even though most
of its value is furniture. For this POC we still classify at line-item level
(per the brief), but when a line item is ambiguous, ask "does this sit inside
a contract whose overall purpose is a construction operation?" before
defaulting to Services.
*Source: CISR14020 (primary).*

## Included operations

**R3 — Systems installation.** Installing heating, lighting,
air-conditioning, ventilation, power, drainage, sanitation, water supply, or
fire protection systems in a building/structure is Construction — whether new
build or retrofit into an existing building.
*Source: CIS340 (primary).*

**R4 — Site work and finishing.** Site preparation (laying foundations,
access works), demolition/dismantling, painting and decorating (internal or
external surfaces), and internal cleaning *carried out in the course of
construction/alteration/repair/restoration* are all Construction. Internal
cleaning of a building with no construction work happening is not (see R9).
*Source: CIS340 (primary).*

**R5 — Supply-and-fix is Construction.** If the same party supplies
materials/components *and* installs them under one contract, the whole thing
is Construction — the installation obligation is what pulls it into scope,
even though pure manufacture/delivery (R7) would not be. Fitted kitchens,
fitted windows, and similar supply-and-fit trades are the canonical case.
*Source: CISR14220 (primary).*

## Excluded operations

**R6 — Professional/consultancy services are excluded, but only while purely
advisory.** Architects' and surveyors' professional work is excluded "in all
circumstances" when done within their normal discipline. Other consultants
(engineering, interior/exterior decoration, landscape design) are excluded
when they supply "technical advice, assessments, plans or designs" and stand
apart from execution. **The moment a professional moves into project
management, supervision, or hands-on remedial work, they cross into
Construction** — e.g. an architect acting as developer/project manager, or a
surveyor doing remedial work rather than just inspecting it. Classify on what
the line item actually describes them *doing*, not their job title.
*Source: CISR14270 (primary).*

**R7 — Manufacture and off-site delivery are excluded; installation is not.**
"Manufacture of building or engineering components... materials, plant or
machinery, or delivery of these things to site" sits outside CIS — this
covers prefabricated beams, ready-mixed concrete, flooring materials,
glazing, doors, insulation, and components for heating/lighting/drainage/fire
systems, *as long as the supplier's obligation stops at delivery*. As soon as
installation is added (R5), the whole line moves to Construction. Ready-mix
concrete delivered and poured via the driver's own chute stays outside CIS; a
third party pumping wet concrete around a site *is* Construction (it's a site
service, not a delivery).
*Source: CISR14220 (primary).*

**R8 — Plant/equipment hire: operator presence is the switch.** Hiring plant
(scaffolding, cranes, cement mixers, excavators, compressors) **without** an
operator is Services/out-of-scope — it's a straight rental. Hiring the same
plant **with** an operator means the operator's labour is Construction, while
the bare hire cost is not. Scaffolding follows the same rule: supply-and-erect
is Construction; hire-only (no erection/dismantling labour) is Services.
*Source: CIS340 (secondary — sourced via commentary summaries, not yet
checked against primary CIS340 text; see Open Items).*

**R9 — Facilities/site-support services are excluded.** Running a canteen or
hostel, providing medical or safety services, providing security *services*
(e.g. guarding), and general site facilities management are "clearly not
construction work" regardless of being on a construction site.
*Source: CIS340 (primary).*

**R10 — Specific named exclusions.** HMRC excludes these even though they
involve installing something in a building: sign-writing and
erecting/repairing signboards or advertisements; installation of seating,
blinds, and shutters; installation of security/alarm systems, CCTV, and
public address systems; installation or replacement of computer/
telecommunications wiring through existing ducting; carpet fitting. Treat
these as Services even when they otherwise look like "installing a system in
a building" (R3) — HMRC carves them out by name.
*Source: CIS340 (secondary — sourced via commentary summaries, not yet
checked against primary CIS340 text; see Open Items).*

## Materials vs. labour

**R11 — Materials cost is carved out of the deduction, not out of the
contract.** Under FA04/S61(1), a CIS deduction applies only to the part of a
payment that is *not* the "direct cost of materials to the subcontractor" —
what the subcontractor can actually demonstrate they paid, for materials
specifically relating to *this* contract (not materials bought for other
contracts, and not materials the main contractor supplied to them for free or
fully reimbursed — in either of those cases the subcontractor has no direct
cost of their own to deduct). Within a genuine Construction line item (e.g.
supply-and-fix), this still means the line item itself is part of a
Construction contract. Don't reclassify a materials sub-line as "Services"
just because it isn't taxed; instead classify it Construction and flag it as
"materials — not subject to deduction" in the reasoning. A pure materials
*supply-only* invoice from a merchant with no installation obligation on that
invoice (R7) is the case that actually becomes Services/out-of-scope.
*Source: CISR15060 (primary — verified against source PDF).*

**R11a — Treat "materials only, installation free" claims with suspicion.**
HMRC's own guidance flags this as a known dodge: a supply-and-fix
subcontractor may claim their charge relates "wholly" to materials and that
installation is free, to shrink the taxable labour portion — but commercially
they almost always incur a real cost for both. If a line item's wording
follows this pattern (all value attributed to goods, installation described
as free/included at no cost) and the amount looks high for materials alone,
flag it in the reasoning as likely mis-stated rather than accepting the
Services-leaning framing at face value; the underlying operation is still
Construction (R5) and a labour estimate would be needed in practice.
*Source: CISR15060 (primary — verified against source PDF).*

## Mixed contracts / mixed invoices

**R12 — An invoice can legitimately contain both labels.** Real
subcontractor invoices routinely mix a Construction line (fitting a system)
with a Services line (a genuinely separate consultancy fee, or hired-in plant
with no operator) or a materials line (R11). Classify each line
independently; do not force one label onto the whole invoice. Only fall back
to R2's contract-level framing when a single line item's own wording is too
thin to classify on its own (e.g. "sundries," "site costs") — in that case,
infer from the invoice's dominant purpose and say so explicitly in the
reasoning.
*Source: POC-METHOD — our own line-item classification protocol, not a
restatement of an HMRC rule (though it leans on the framing in R2).*

**R13 — When wording is genuinely ambiguous, say what's missing rather than
guessing silently.** If a line item could go either way (e.g. "site
management," "consultancy — remedial works," "system maintenance"), classify
using the best available reading of R6–R10, but the reasoning must name the
fact that would resolve it (e.g. "classified as Construction on the
assumption this is hands-on remedial work, not inspection — would flip to
Services if this is a pure survey report").
*Source: POC-METHOD — our own line-item classification protocol.*

## Worked examples

1. **"Install air conditioning system, Riverside Office, 2nd floor — £4,200"**
   → **Construction** (R3: system installation in a building).

2. **"Architectural design and planning drawings for extension — £1,800"**
   → **Services** (R6: pure advisory/design work, no execution role).

3. **"Structural survey following subsidence, remedial underpinning works —
   £6,500"** → **Construction** (R6: surveyor has moved from inspection into
   remedial execution).

4. **"Supply of 40 fire doors, delivered to site — £3,100"** → **Services**
   (R7: manufacture/delivery only, no installation obligation on this line).

5. **"Supply and fit 40 fire doors, including ironmongery — £5,900"** →
   **Construction** (R5: supply-and-fix).

6. **"Hire of tower crane, 4 weeks, no operator — £2,400"** → **Services**
   (R8: bare plant hire, no operator).

7. **"Tower crane hire with operator, 4 weeks — £6,800"** → **Construction**
   for the operator's labour portion (R8); reasoning should note the bare
   hire cost within that figure isn't itself a CIS-taxable amount.

8. **"Installation of intruder alarm and CCTV, warehouse unit — £2,950"** →
   **Services** (R10: named exclusion, even though it looks like a building
   system installation).

9. **"Site security guarding, 6 weeks — £4,000"** → **Services** (R9:
   facilities/site-support, not construction work).

10. **"Materials: 12 tonnes ready-mix concrete, delivered — £1,650"** (on an
    invoice with no other lines from this supplier) → **Services**
    (R7/R11: standalone delivery, no laying/installation obligation).

11. **"Groundworks labour + materials (concrete foundations), fixed price —
    £14,200"** → **Construction** (R1/R11: whole line is one construction
    operation; note in reasoning that a materials sub-amount, if itemised,
    would be excluded from the deduction base but not from the classification).

12. **"Fitted kitchen — supply and install, including worktops and
    appliances — £11,000"** → **Construction** (R2/R5: installation element
    pulls the whole contract into scope even though most of the value is
    goods).

13. **"Supply of boiler and full heating system — £9,800 (installation
    included at no extra charge)"** → **Construction** (R5), but flag in
    reasoning per R11a: attributing the entire value to "materials" with
    installation described as free is the exact pattern HMRC calls out as
    likely understating the labour portion — don't take the framing at face
    value.

## Sources

- [What is the Construction Industry Scheme?](https://www.gov.uk/what-is-the-construction-industry-scheme) — gov.uk overview, included/excluded work summary
- [CIS340: guide for contractors and subcontractors](https://www.gov.uk/government/publications/construction-industry-scheme-cis-340/construction-industry-scheme-a-guide-for-contractors-and-subcontractors-cis-340) — the primary source guide; "operations within/not within the scheme" lists
- [CISR14020 — scope of CIS](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14020) — mixed-contract / broad-scope principle (R2)
- [CISR14220 — manufacture and delivery](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14220) — manufacture/delivery vs. supply-and-fix (R5, R7)
- [CISR14270 — professional work](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14270) — advisory vs. execution/supervision test (R6)
- [CISR14330 — index of construction operations](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14330) — navigation index into the CISR14xxx series
- [CISR15060 — payments: materials](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr15060) — FA04/S61(1) direct-cost-of-materials deduction, chain/reimbursement restriction, and the "materials only, free installation" scrutiny point (R11, R11a); verified directly against the source PDF, not secondary commentary

## Open items to verify before production use

- **R8 (plant hire) and R10 (named exclusions)** are marked `secondary` —
  sourced via AI-summarized search results of commentary sites, not a direct
  read of the primary CIS340 text (the way R11/R11a were verified against the
  CISR15060 PDF). Re-verify both against the actual CIS340 guide before this
  doc is used beyond the POC, and flip their `verified` status to `primary`
  in both this file and `cis_rules_index.json` once done.
- No worked example yet for traffic management (CISR14305) — add once a
  synthetic invoice surfaces it.
- Multi-trade invoices where the same supplier does both supply-only and
  supply-and-fix lines on one document haven't been stress-tested yet.
