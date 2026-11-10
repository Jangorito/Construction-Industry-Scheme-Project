# CIS Classification Rules - Reference Doc

Living document. Source: HMRC CIS340 ("Construction Industry Scheme: a guide for
contractors and subcontractors") and the CIS Reform Manual (CISR14xxx/CISR15xxx
series) on gov.uk. Built for the POC classification prompt - each rule below
should be citable back to a real HMRC position, not an assumption. Iterate this
doc every time a test invoice gets misclassified: add the missing rule or
sharpen an ambiguous one rather than patching the prompt with a one-off
exception.

Two output labels per line item: **Construction** (CIS-relevant) or
**Services** (non-CIS). Rules R11b–R13 cover the cases that don't cleanly fit
either label at face value (materials-adjacent costs, mixed contracts) -
resolve those down to one of the two labels plus a note, don't invent a
third label.

**Rule IDs and citations.** Every rule has a stable ID (`R1`, `R6`, `R11a`, …)
and a `Source:` line naming what backs it - either a real HMRC reference
(CIS340, a CISR page) or `POC-METHOD` for a rule that's our own line-item
classification protocol rather than a restatement of HMRC guidance. Each
HMRC-sourced rule is also marked `primary` (we read the actual gov.uk
page/PDF ourselves) or `secondary` (sourced via search-result summaries of
commentary sites, not yet checked against the primary text - see Open Items).

**Why IDs matter beyond this doc:** the classification prompt should have the
LLM cite rule IDs it applied (e.g. `["R6", "R11a"]`), not free-text citations
or URLs - an LLM asked to recall "CISR14270" from memory can get the number
wrong or misquote it. [`cis_rules_index.json`](cis_rules_index.json) is the
machine-readable version of the table below (id → title, source, url,
verification status); `classify.py` and any later DB/UI layer should resolve
rule IDs against that file rather than trusting LLM-generated citation text.
Keep both files in sync when a rule changes.

## Core test

**R1 - Construction operations, broadly defined.** CIS applies to
construction, alteration, repair, extension, demolition, or dismantling of
buildings, structures, or works forming part of the land (roads, railways,
bridges, harbours, power lines, pipelines, reservoirs, sewers, industrial
plant). Site preparation, finishing, and integral/preparatory work (e.g.
scaffolding erected as part of a build) all count. This is the default
"Construction" bucket.

**Traffic management is Construction, not an R9 exclusion.** CIS340
Appendix C names "the provision of traffic management services supplied to
a contractor carrying out construction operations" as included. Don't let
the word "services" in the name pull this toward R9's facilities/site-support
exclusions (canteens, guarding, medical) - what matters is what the line
item actually describes doing, and traffic management in support of a
construction contract is itself part of that contract. (Traffic management
for a *non*-construction operation, or bare equipment supplied but not
placed into position, is the case that's genuinely excluded.)
*Source: CIS340 **(primary)**.*

**R2 - The scope test is broad and contract-level, not item-level.** HMRC's
own framing: "the expression 'relating to construction operations' is quite
broad... even if the construction element only forms part of the overall
contract." A contract to supply and install a fitted kitchen is a
construction contract because of the installation element, even though most
of its value is furniture. For this POC we still classify at line-item level
(per the brief), but when a line item is ambiguous, ask "does this sit inside
a contract whose overall purpose is a construction operation?" before
defaulting to Services.
*Source: CISR14020 **(primary)**; the same principle is stated directly in CIS340
Appendix A.3–A.4 ("Mixed contracts": "If a contract includes some work that
is within the scheme and some that is nothing to do with construction at all
then all payments made under that contract will come within the scheme -
even if shown on separate invoices") **(primary)**.*

## Included operations

**R3 - Systems installation.** Installing heating, lighting,
air-conditioning, ventilation, power, drainage, sanitation, water supply, or
fire protection systems in a building/structure is Construction - whether new
build or retrofit into an existing building.
*Source: CIS340 **(primary)**.*

**R4 - Site work and finishing.** Site preparation (laying foundations,
access works), demolition/dismantling, painting and decorating (internal or
external surfaces), and internal cleaning *carried out in the course of
construction/alteration/repair/restoration* are all Construction. Internal
cleaning of a building with no construction work happening is not (see R9).
*Source: CIS340 **(primary)**.*

**R5 - Supply-and-fix is Construction.** If the same party supplies
materials/components *and* installs them under one contract, the whole thing
is Construction - the installation obligation is what pulls it into scope,
even though pure manufacture/delivery (R7) would not be. Fitted kitchens,
fitted windows, and similar supply-and-fit trades are the canonical case.
*Source: CISR14220 **(primary)**.*

## Excluded operations

**R6 - Professional/consultancy services are excluded, but only while purely
advisory.** Architects' and surveyors' professional work is excluded "in all
circumstances" when done within their normal discipline. Other consultants
(engineering, interior/exterior decoration, landscape design) are excluded
when they supply "technical advice, assessments, plans or designs" and stand
apart from execution. **The moment a professional moves into project
management, supervision, or hands-on remedial work, they cross into
Construction** - e.g. an architect acting as developer/project manager, or a
surveyor doing remedial work rather than just inspecting it. Classify on what
the line item actually describes them *doing*, not their job title.
*Source: CISR14270 **(primary)**.*

**R7 - Manufacture and off-site delivery are excluded; installation is not.**
"Manufacture of building or engineering components... materials, plant or
machinery, or delivery of these things to site" sits outside CIS - this
covers prefabricated beams, ready-mixed concrete, flooring materials,
glazing, doors, insulation, and components for heating/lighting/drainage/fire
systems, *as long as the supplier's obligation stops at delivery*. As soon as
installation is added (R5), the whole line moves to Construction. Ready-mix
concrete delivered and poured via the driver's own chute stays outside CIS; a
third party pumping wet concrete around a site *is* Construction (it's a site
service, not a delivery).
*Source: CISR14220 **(primary)**.*

**R8 - Plant/equipment hire: operator presence is the switch.** CIS340's
Appendix C table lists "plant hire with operator for use on site" as a whole
included construction operation - classify the full line as Construction, not
just an operator-labour portion within it. Without an operator, plant hire is
named as excluded: "hire of scaffolding equipment (without labour)" and
"delivery, repair or maintenance of construction plant or hire equipment
without an operator - for example, concrete mixers, pumps and skips" are both
Services. Scaffolding follows the same split: "erection or dismantling of
scaffolding falsework and formwork" (labour) is Construction; bare hire with
no erection/dismantling is Services.
*Source: CIS340, Appendix A.19 (integral works - scaffolding) and Appendix C
"Construction" included/excluded tables **(primary)**.*

**R9 - Facilities/site-support services are excluded.** Running a canteen or
hostel, providing medical or safety services, providing security *services*
(e.g. guarding), and general site facilities management are "clearly not
construction work" regardless of being on a construction site.
*Source: CIS340 **(primary)**.*

**R10 - Specific named exclusions.** Five categories are excluded by name,
each narrower than the plain description suggests:
- **Signage** (A.29): sign-writing and erecting/installing/repairing
  signboards and advertisements - "very specific," does **not** extend to
  traffic signs or street furniture, which stay Construction.
- **Seating, blinds, shutters** (A.30): fixed/theatre/stadium seating,
  louvered shutters, venetian and other internal blinds, curtain fittings.
  Awnings are explicitly **not** excluded - Construction.
- **Security systems** (A.31): burglar alarms, CCTV, and public address
  systems - but only when installed as a dedicated security *system*. A door
  fitted with a lock, or fencing, isn't a "system" even though both deter
  intruders, so fitting either stays Construction. CCTV installed for a
  non-security purpose (e.g. traffic management) is Construction. Preparatory
  groundwork for a security system (digging, erecting the posts CCTV will sit
  on) is Construction even though installing the system itself is excluded.
- **Telecoms/computer wiring through pre-existing ducting**: narrow - only
  threading wiring through ducting that's already there. Installing
  telecoms/electronic-communications infrastructure itself (cable TV,
  telecoms distribution systems) is Construction (R1).
- **Carpet fitting** (A.21, Statement of Practice 12 (1981)): the sole
  exception among "finishing operations" - every other floor covering
  (vinyl, linoleum, laminate wood) is Construction; carpet alone is Services.

Treat all five as Services even when they otherwise look like "installing a
system in a building" (R3) - but check the narrower boundary in each case
before applying the exclusion.

**A match against any of the five exclusions above is provisional, not
final, until you've checked for a mixed-contract override.** CIS340 A.4
states this for carpet fitting by name ("if carpet fitting is part of a
mixed contract, then all the contract comes within the scheme"), but the
underlying principle is R2/R12's general mixed-contract rule, and it
applies to all five R10 exclusions, not just carpet. Finding a named
exclusion is not the end of the analysis, it's the point where you stop and
re-check - but the bar for the override is the *flagged line's own
wording* showing it's part of a bigger contract ("as part of," "part of
the wider... contract," or similar), not merely sharing an invoice with
unrelated Construction lines. One invoice can bundle several unrelated
jobs from the same supplier - sharing an invoice is not the same as
sharing a contract. When the line's own wording does carry that signal,
other clearly-Construction lines on the same invoice count as
*corroborating* evidence that reinforces the override, not as a
substitute for it on their own. Only let a named exclusion stand as your
final answer once you've checked the line's own wording for that
self-referential signal and found none - don't let a correct-sounding
exclusion feel "done" before the override check has had a real chance to
overturn it, and don't let an unrelated Construction line elsewhere on the
invoice flip an exclusion that has no such signal of its own.
*Source: CIS340, Appendix A.4, A.28–A.31 and Appendix C "Construction"/
"Finishing operations" tables **(primary)**.*

## Materials vs. labour

**R11 - Materials cost is carved out of the deduction, not out of the
contract.** Under FA04/S61(1), a CIS deduction applies only to the part of a
payment that is *not* the "direct cost of materials to the subcontractor" -
what the subcontractor can actually demonstrate they paid, for materials
specifically relating to *this* contract (not materials bought for other
contracts, and not materials the main contractor supplied to them for free or
fully reimbursed - in either of those cases the subcontractor has no direct
cost of their own to deduct). Within a genuine Construction line item (e.g.
supply-and-fix), this still means the line item itself is part of a
Construction contract. Don't reclassify a materials sub-line as "Services"
just because it isn't taxed; instead classify it Construction and flag it as
"materials - not subject to deduction" in the reasoning. A pure materials
*supply-only* invoice from a merchant with no installation obligation on that
invoice (R7) is the case that actually becomes Services/out-of-scope.
*Source: CISR15060 (primary - verified against source PDF).*

**R11a - Treat "materials only, installation free" claims with suspicion.**
HMRC's own guidance flags this as a known dodge: a supply-and-fix
subcontractor may claim their charge relates "wholly" to materials and that
installation is free, to shrink the taxable labour portion - but commercially
they almost always incur a real cost for both. If a line item's wording
follows this pattern (all value attributed to goods, installation described
as free/included at no cost) and the amount looks high for materials alone,
flag it in the reasoning as likely mis-stated rather than accepting the
Services-leaning framing at face value; the underlying operation is still
Construction (R5) and a labour estimate would be needed in practice.
*Source: CISR15060 (primary - verified against source PDF).*

**R11b - Hired plant cost can be treated as "materials" in the deduction
calculation.** Where a subcontractor hires plant (scaffolding, cranes,
cement mixers, etc.) from a third party to carry out their own construction
work, the cost of that hire - plus consumables like fuel needed to run it -
may be treated as "materials" for calculating the CIS deduction, even though
it isn't materials in the ordinary sense. This only matters for
subcontractors **without gross payment status** - a gross-status
subcontractor is paid with no deduction at all, so there's nothing for this
treatment to apply to. This is a deduction-calculation rule only: it doesn't
touch R8's scope question (is the plant-hire *line item itself* Construction
or Services). It only applies to plant actually hired from a third party -
if the subcontractor owns the plant, including plant acquired via
hire-purchase, there's no "notional" materials deduction for it, though
fuel/consumables can still count. (Tugs, barges, and vessels used in
constructing docks/marinas/offshore installations follow the same
crewed/bare-hire split as R8: crewed = plant hire with operator = subject to
CIS; bare-hired with the contractor's own crew = not subject to CIS.)
*Source: CISR14260 and CISR15090 (both primary) - resolves the open item
CISR15060 pointed to without detail.*

**R11c - Travel, subsistence, and accommodation are not materials.**
Payments a contractor makes for a subcontractor's travel, subsistence, or
accommodation - paid directly, reimbursed, or paid to a nominated third
party (e.g. a hotel) - are part of the overall contract payment under
FA04/S60 and are **fully subject to the CIS deduction**, unlike direct
materials costs (R11). Don't treat these as a materials-style carve-out in
the reasoning, and don't classify them as Services either - they're
Construction, taxed in full alongside the labour value, regardless of
whether the obligation to pay them is written, verbal, express, or implied.
The one exception: if a contractor lets a subcontractor use the contractor's
*own* facilities (e.g. a vehicle, or accommodation the contractor already
holds) entirely free, with no cost ever passed on, no deduction arises on
that value - but HMRC itself flags this pattern as unusual and a possible
sign of disguised employment (PAYE) rather than genuine subcontracting, so
treat it as something to flag in reasoning, not a default assumption.
*Source: CISR15080 **(primary)**.*

**R11d - Land purchase bundled into a construction contract (niche).** If a
single contract requires a subcontractor (e.g. a developer) to acquire land
as well as carry out construction, the whole payment is technically subject
to deduction under S60(1)/S61(1) since materials are the only carve-out -
but the portion reimbursing the subcontractor for the land purchase itself
can be treated as "materials" and excluded from the deduction, on the same
"direct cost" logic as R11. Unlikely to appear as an invoice line item in
this POC (land purchase reads as a property transaction, not a line item on
a subcontractor's invoice) - recorded here for completeness.
*Source: CISR15070 **(primary)**.*

## Mixed contracts / mixed invoices

**R12 - An invoice can legitimately contain both labels.** Real
subcontractor invoices routinely mix a Construction line (fitting a system)
with a Services line (a genuinely separate consultancy fee, or hired-in plant
with no operator) or a materials line (R11). Classify each line
independently; do not force one label onto the whole invoice. Only fall back
to R2's contract-level framing when a single line item's own wording is too
thin to classify on its own (e.g. "sundries," "site costs") - in that case,
infer from the invoice's dominant purpose and say so explicitly in the
reasoning.
*Source: POC-METHOD - our own line-item classification protocol, not a
restatement of an HMRC rule (though it leans on the framing in R2).*

**R13 - When wording is genuinely ambiguous, say what's missing rather than
guessing silently.** If a line item could go either way (e.g. "site
management," "consultancy - remedial works," "system maintenance"), classify
using the best available reading of R6–R10, but the reasoning must name the
fact that would resolve it (e.g. "classified as Construction on the
assumption this is hands-on remedial work, not inspection - would flip to
Services if this is a pure survey report").
*Source: POC-METHOD - our own line-item classification protocol.*

## Worked examples

1. **"Install air conditioning system, Riverside Office, 2nd floor - £4,200"**
   → **Construction** (R3: system installation in a building).

2. **"Architectural design and planning drawings for extension - £1,800"**
   → **Services** (R6: pure advisory/design work, no execution role).

3. **"Structural survey following subsidence, remedial underpinning works -
   £6,500"** → **Construction** (R6: surveyor has moved from inspection into
   remedial execution).

4. **"Supply of 40 fire doors, delivered to site - £3,100"** → **Services**
   (R7: manufacture/delivery only, no installation obligation on this line).

5. **"Supply and fit 40 fire doors, including ironmongery - £5,900"** →
   **Construction** (R5: supply-and-fix).

6. **"Hire of tower crane, 4 weeks, no operator - £2,400"** → **Services**
   (R8: bare plant hire, no operator).

7. **"Tower crane hire with operator, 4 weeks - £6,800"** → **Construction**
   (R8: CIS340 lists "plant hire with operator for use on site" as a whole
   included operation - classify the full line, not just a labour portion
   within it).

8. **"Installation of intruder alarm and CCTV, warehouse unit - £2,950"** →
   **Services** (R10: named security-system exclusion, even though it looks
   like a building system installation).

8a. **"Installation of CCTV for traffic monitoring, site access road -
   £1,800"** → **Construction** (R10: same equipment as example 8, but CCTV
   installed for a non-security purpose - traffic management - isn't the
   named exclusion; it's Construction).

8b. **"Supply and erect security fencing around compound perimeter -
   £3,600"** → **Construction** (R10: fencing deters intruders but isn't a
   security *system* under A.31 - it's ordinary construction work, not the
   named security-systems exclusion).

9. **"Site security guarding, 6 weeks - £4,000"** → **Services** (R9:
   facilities/site-support, not construction work).

10. **"Materials: 12 tonnes ready-mix concrete, delivered - £1,650"** (on an
    invoice with no other lines from this supplier) → **Services**
    (R7/R11: standalone delivery, no laying/installation obligation).

11. **"Groundworks labour + materials (concrete foundations), fixed price -
    £14,200"** → **Construction** (R1/R11: whole line is one construction
    operation; note in reasoning that a materials sub-amount, if itemised,
    would be excluded from the deduction base but not from the classification).

12. **"Fitted kitchen - supply and install, including worktops and
    appliances - £11,000"** → **Construction** (R2/R5: installation element
    pulls the whole contract into scope even though most of the value is
    goods).

13. **"Supply of boiler and full heating system - £9,800 (installation
    included at no extra charge)"** → **Construction** (R5), but flag in
    reasoning per R11a: attributing the entire value to "materials" with
    installation described as free is the exact pattern HMRC calls out as
    likely understating the labour portion - don't take the framing at face
    value.

14. **"Traffic management services for site access road during resurfacing
    works - £2,200"** → **Construction** (R1: CIS340 Appendix C lists "the
    provision of traffic management services supplied to a contractor
    carrying out construction operations" as included; contrast with traffic
    management for *non*-construction operations, or bare traffic-management
    equipment supplied but not placed into position, both of which are
    Services).

15. **"Supply and fit carpet tiles, 3rd floor office - £4,100"** →
    **Services** (R10: carpet fitting is the sole named exception among
    finishing operations - note in reasoning that vinyl, linoleum, or
    laminate flooring on the same job would be Construction).

16. **"Subcontractor accommodation costs, 3 months on-site - £3,600"** →
    **Construction** (R11c: travel/subsistence/accommodation the contractor
    pays on a subcontractor's behalf are part of the taxable contract
    payment - don't mistake this for a materials-style exclusion, and don't
    classify it as Services just because it isn't labour or goods).

## Sources

- [What is the Construction Industry Scheme?](https://www.gov.uk/what-is-the-construction-industry-scheme) - gov.uk overview, included/excluded work summary
- [CIS340: guide for contractors and subcontractors](https://www.gov.uk/government/publications/construction-industry-scheme-cis-340/construction-industry-scheme-a-guide-for-contractors-and-subcontractors-cis-340) - the primary source guide, including Appendix A (scope of construction operations, A.1–A.31), Appendix B (Finance Act 2004 S74 text), and Appendix C (the full included/excluded operations tables, C.1–C.3). Verified directly against the source PDF (78 pages) on 2026-08-04 - this is what backs R1, R3–R5, R7–R10, and the mixed-contracts principle in R2.
- [CISR14020 - scope of CIS](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14020) - mixed-contract / broad-scope principle (R2)
- [CISR14220 - manufacture and delivery](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14220) - manufacture/delivery vs. supply-and-fix (R5, R7)
- [CISR14270 - professional work](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14270) - advisory vs. execution/supervision test (R6)
- [CISR14330 - index of construction operations](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14330) - navigation index into the CISR14xxx series
- [CISR15060 - payments: materials](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr15060) - FA04/S61(1) direct-cost-of-materials deduction, chain/reimbursement restriction, and the "materials only, free installation" scrutiny point (R11, R11a); verified directly against the source PDF, not secondary commentary
- [CISR14260 - construction operations: plant hire](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14260) - hired plant/fuel treated as "materials" in the deduction calculation, owned/hire-purchase plant excluded from that treatment, crewed vs. bare-hired vessels (R11b)
- [CISR15090 - payments: plant hire as 'materials'](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr15090) - same plant-hire-as-materials rule, adds that it only matters for subcontractors without gross payment status (R11b)
- [CISR15070 - payments: land purchases](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr15070) - land purchase reimbursement treated as "materials" within a bundled contract (R11d)
- [CISR15080 - contract payments: travel, subsistence and accommodation](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr15080) - these expenses are part of the taxable contract payment, not a materials-style exclusion (R11c)

## Open items to verify before production use

- Multi-trade invoices where the same supplier does both supply-only and
  supply-and-fix lines on one document haven't been stress-tested yet.
- CISR14305 (traffic management, cited in the CISR manual index) hasn't been
  read directly - R1/example 14's traffic-management treatment comes from
  CIS340 Appendix C instead, which is sufficient for the POC but worth
  cross-checking if traffic management line items turn out to be common.
