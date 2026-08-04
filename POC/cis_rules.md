# CIS Classification Rules — Reference Doc

Living document. Source: HMRC CIS340 ("Construction Industry Scheme: a guide for
contractors and subcontractors") and the CIS Reform Manual (CISR14xxx series) on
gov.uk. Built for the POC classification prompt — each rule below should be
citable back to a real HMRC position, not an assumption. Iterate this doc every
time a test invoice gets misclassified: add the missing rule or sharpen an
ambiguous one rather than patching the prompt with a one-off exception.

Two output labels per line item: **Construction** (CIS-relevant) or
**Services** (non-CIS). Rules 12–13 cover the cases that don't cleanly fit
either label at face value (materials, mixed contracts) — resolve those down
to one of the two labels plus a note, don't invent a third label.

## Core test

**Rule 1 — Construction operations, broadly defined.** CIS applies to
construction, alteration, repair, extension, demolition, or dismantling of
buildings, structures, or works forming part of the land (roads, railways,
bridges, harbours, power lines, pipelines, reservoirs, sewers, industrial
plant). Site preparation, finishing, and integral/preparatory work (e.g.
scaffolding erected as part of a build) all count. This is the default
"Construction" bucket.

**Rule 2 — The scope test is broad and contract-level, not item-level.**
HMRC's own framing: "the expression 'relating to construction operations' is
quite broad... even if the construction element only forms part of the
overall contract." A contract to supply and install a fitted kitchen is a
construction contract because of the installation element, even though most
of its value is furniture. For this POC we still classify at line-item level
(per the brief), but when a line item is ambiguous, ask "does this sit inside
a contract whose overall purpose is a construction operation?" before
defaulting to Services.

## Included operations

**Rule 3 — Systems installation.** Installing heating, lighting,
air-conditioning, ventilation, power, drainage, sanitation, water supply, or
fire protection systems in a building/structure is Construction — whether new
build or retrofit into an existing building.

**Rule 4 — Site work and finishing.** Site preparation (laying foundations,
access works), demolition/dismantling, painting and decorating (internal or
external surfaces), and internal cleaning *carried out in the course of
construction/alteration/repair/restoration* are all Construction. Internal
cleaning of a building with no construction work happening is not (see Rule
9).

**Rule 5 — Supply-and-fix is Construction.** If the same party supplies
materials/components *and* installs them under one contract, the whole thing
is Construction — the installation obligation is what pulls it into scope,
even though pure manufacture/delivery (Rule 7) would not be. Fitted kitchens,
fitted windows, and similar supply-and-fit trades are the canonical case.

## Excluded operations

**Rule 6 — Professional/consultancy services are excluded, but only while
purely advisory.** Architects' and surveyors' professional work is excluded
"in all circumstances" when done within their normal discipline. Other
consultants (engineering, interior/exterior decoration, landscape design) are
excluded when they supply "technical advice, assessments, plans or designs"
and stand apart from execution. **The moment a professional moves into
project management, supervision, or hands-on remedial work, they cross into
Construction** — e.g. an architect acting as developer/project manager, or a
surveyor doing remedial work rather than just inspecting it. Classify on what
the line item actually describes them *doing*, not their job title.

**Rule 7 — Manufacture and off-site delivery are excluded; installation is
not.** "Manufacture of building or engineering components... materials,
plant or machinery, or delivery of these things to site" sits outside CIS —
this covers prefabricated beams, ready-mixed concrete, flooring materials,
glazing, doors, insulation, and components for heating/lighting/drainage/fire
systems, *as long as the supplier's obligation stops at delivery*. As soon as
installation is added (Rule 5), the whole line moves to Construction. Ready-mix
concrete delivered and poured via the driver's own chute stays outside CIS;
a third party pumping wet concrete around a site *is* Construction (it's a
site service, not a delivery).

**Rule 8 — Plant/equipment hire: operator presence is the switch.** Hiring
plant (scaffolding, cranes, cement mixers, excavators, compressors) **without**
an operator is Services/out-of-scope — it's a straight rental. Hiring the same
plant **with** an operator means the operator's labour is Construction, while
the bare hire cost is not. Scaffolding follows the same rule: supply-and-erect
is Construction; hire-only (no erection/dismantling labour) is Services.

**Rule 9 — Facilities/site-support services are excluded.** Running a
canteen or hostel, providing medical or safety services, providing security
*services* (e.g. guarding), and general site facilities management are
"clearly not construction work" regardless of being on a construction site.

**Rule 10 — Specific named exclusions.** HMRC excludes these even though
they involve installing something in a building: sign-writing and
erecting/repairing signboards or advertisements; installation of seating,
blinds, and shutters; installation of security/alarm systems, CCTV, and
public address systems; installation or replacement of computer/
telecommunications wiring through existing ducting; carpet fitting. Treat
these as Services even when they otherwise look like "installing a system in
a building" (Rule 3) — HMRC carves them out by name.

## Materials vs. labour

**Rule 11 — Materials cost is carved out of the deduction, not out of the
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
invoice (Rule 7) is the case that actually becomes Services/out-of-scope.

**Rule 11a — Treat "materials only, installation free" claims with
suspicion.** HMRC's own guidance flags this as a known dodge: a supply-and-fix
subcontractor may claim their charge relates "wholly" to materials and that
installation is free, to shrink the taxable labour portion — but commercially
they almost always incur a real cost for both. If a line item's wording
follows this pattern (all value attributed to goods, installation described
as free/included at no cost) and the amount looks high for materials alone,
flag it in the reasoning as likely mis-stated rather than accepting the
Services-leaning framing at face value; the underlying operation is still
Construction (Rule 5) and a labour estimate would be needed in practice.

## Mixed contracts / mixed invoices

**Rule 12 — An invoice can legitimately contain both labels.** Real
subcontractor invoices routinely mix a Construction line (fitting a system)
with a Services line (a genuinely separate consultancy fee, or hired-in plant
with no operator) or a materials line (Rule 11). Classify each line
independently; do not force one label onto the whole invoice. Only fall back
to Rule 2's contract-level framing when a single line item's own wording is
too thin to classify on its own (e.g. "sundries," "site costs") — in that
case, infer from the invoice's dominant purpose and say so explicitly in the
reasoning.

**Rule 13 — When wording is genuinely ambiguous, say what's missing rather
than guessing silently.** If a line item could go either way (e.g. "site
management," "consultancy — remedial works," "system maintenance"), classify
using the best available reading of Rules 6–10, but the reasoning must name
the fact that would resolve it (e.g. "classified as Construction on the
assumption this is hands-on remedial work, not inspection — would flip to
Services if this is a pure survey report").

## Worked examples

1. **"Install air conditioning system, Riverside Office, 2nd floor — £4,200"**
   → **Construction** (Rule 3: system installation in a building).

2. **"Architectural design and planning drawings for extension — £1,800"**
   → **Services** (Rule 6: pure advisory/design work, no execution role).

3. **"Structural survey following subsidence, remedial underpinning works —
   £6,500"** → **Construction** (Rule 6: surveyor has moved from inspection
   into remedial execution).

4. **"Supply of 40 fire doors, delivered to site — £3,100"** → **Services**
   (Rule 7: manufacture/delivery only, no installation obligation on this
   line).

5. **"Supply and fit 40 fire doors, including ironmongery — £5,900"** →
   **Construction** (Rule 5: supply-and-fix).

6. **"Hire of tower crane, 4 weeks, no operator — £2,400"** → **Services**
   (Rule 8: bare plant hire, no operator).

7. **"Tower crane hire with operator, 4 weeks — £6,800"** → **Construction**
   for the operator's labour portion (Rule 8); reasoning should note the bare
   hire cost within that figure isn't itself a CIS-taxable amount.

8. **"Installation of intruder alarm and CCTV, warehouse unit — £2,950"** →
   **Services** (Rule 10: named exclusion, even though it looks like a
   building system installation).

9. **"Site security guarding, 6 weeks — £4,000"** → **Services** (Rule 9:
   facilities/site-support, not construction work).

10. **"Materials: 12 tonnes ready-mix concrete, delivered — £1,650"** (on an
    invoice with no other lines from this supplier) → **Services**
    (Rule 7/11: standalone delivery, no laying/installation obligation).

11. **"Groundworks labour + materials (concrete foundations), fixed price —
    £14,200"** → **Construction** (Rule 1/11: whole line is one construction
    operation; note in reasoning that a materials sub-amount, if itemised,
    would be excluded from the deduction base but not from the classification).

12. **"Fitted kitchen — supply and install, including worktops and
    appliances — £11,000"** → **Construction** (Rule 2/5: installation
    element pulls the whole contract into scope even though most of the value
    is goods).

13. **"Supply of boiler and full heating system — £9,800 (installation
    included at no extra charge)"** → **Construction** (Rule 5), but flag in
    reasoning per Rule 11a: attributing the entire value to "materials" with
    installation described as free is the exact pattern HMRC calls out as
    likely understating the labour portion — don't take the framing at face
    value.

## Sources

- [What is the Construction Industry Scheme?](https://www.gov.uk/what-is-the-construction-industry-scheme) — gov.uk overview, included/excluded work summary
- [CIS340: guide for contractors and subcontractors](https://www.gov.uk/government/publications/construction-industry-scheme-cis-340/construction-industry-scheme-a-guide-for-contractors-and-subcontractors-cis-340) — the primary source guide; "operations within/not within the scheme" lists
- [CISR14020 — scope of CIS](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14020) — mixed-contract / broad-scope principle (Rule 2)
- [CISR14220 — manufacture and delivery](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14220) — manufacture/delivery vs. supply-and-fix (Rules 5, 7)
- [CISR14270 — professional work](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14270) — advisory vs. execution/supervision test (Rule 6)
- [CISR14330 — index of construction operations](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr14330) — navigation index into the CISR14xxx series
- [CISR15060 — payments: materials](https://www.gov.uk/hmrc-internal-manuals/construction-industry-scheme-reform/cisr15060) — FA04/S61(1) direct-cost-of-materials deduction, chain/reimbursement restriction, and the "materials only, free installation" scrutiny point (Rules 11, 11a); verified directly against the source PDF, not secondary commentary

## Open items to verify before production use

- No worked example yet for traffic management (CISR14305) — add once a
  synthetic invoice surfaces it.
- Multi-trade invoices where the same supplier does both supply-only and
  supply-and-fix lines on one document haven't been stress-tested yet.
