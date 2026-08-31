"""
generate_invoices.py: synthetic subcontractor invoice generator for the CIS
classification POC (see ../CIS_POC_BRIEF.md).

Produces a mix of clear-construction, clear-services, and deliberately
ambiguous/mixed invoices, built by templating line-item descriptions drawn
from cis_rules.md's rule set (R1-R13) so classify.py has real edge cases to
be tested against, not just easy cases.

Each line item carries a "_ground_truth" block (label, rule_ids, note) that
is the answer key for scoring classify.py's output later. THE UNDERSCORE
PREFIX IS DELIBERATE: classify.py must strip/ignore any "_ground_truth" key
before building its prompt, it exists purely for evaluation, never as
context fed to the LLM. See manifest.csv for the same data flattened to one
row per line item.

Usage:
    python generate_invoices.py [--seed N] [--out-dir PATH]
"""

import argparse
import csv
import json
import random
from dataclasses import dataclass, field
from datetime import date, timedelta
from pathlib import Path
from typing import Optional

ANCHOR_DATE = date(2026, 8, 4)  # generated invoice dates fall in the ~120 days before this
DEFAULT_OUT_DIR = Path(__file__).parent / "data" / "invoices"
MANIFEST_PATH = Path(__file__).parent / "data" / "manifest.csv"


# One LineItemTemplate = one reusable "kind" of invoice line (e.g. "hire of
# tower crane, no operator"). generate_invoices.py picks a handful of these
# per invoice and calls .render() to turn each into an actual description +
# amount. The label/rule_ids/note fields are never shown to classify.py,
# they're the answer key used to check its output later.
@dataclass
class LineItemTemplate:
    id: str                # short unique key, also used as manifest.csv's template_id column
    description: str       # may contain {placeholder} slots filled in from `vars`
    amount_range: tuple    # (low, high) in GBP, a random amount is drawn from this range
    label: str  # "Construction" | "Services"
    rule_ids: list          # which rule(s) in cis_rules.md this line item is meant to test
    note: str                # human readable reason for the label, goes in the ground truth block
    vars: dict = field(default_factory=dict)  # placeholder name to list of possible fill in values
    gap: bool = False  # True if this case exposes a gap not yet covered by a dedicated rule

    def render(self, rng: random.Random):
        # Pick one random fill in value per placeholder, then substitute them
        # into the description string and roll a random amount in range.
        fill = {k: rng.choice(v) for k, v in self.vars.items()}
        description = self.description.format(**fill) if fill else self.description
        lo, hi = self.amount_range
        amount = round(rng.uniform(lo, hi), 2)
        return description, amount


# ---------------------------------------------------------------------------
# Clear construction: unambiguous Construction-labelled line items
# ---------------------------------------------------------------------------
CLEAR_CONSTRUCTION = [
    LineItemTemplate("cc_ac_install", "Install air conditioning system, {location}",
                      (2200, 6500), "Construction", ["R3"],
                      "Complete system installation in a building.",
                      vars={"location": ["2nd floor office", "ground floor retail unit", "warehouse unit 4"]}),
    LineItemTemplate("cc_foundations", "Preparation and laying of foundations, {plot}",
                      (6000, 18000), "Construction", ["R1", "R4"],
                      "Site preparation/construction, core included operation.",
                      vars={"plot": ["Plot 4", "Plot 9", "site block B"]}),
    LineItemTemplate("cc_demolition", "Demolition of existing outbuilding prior to new build",
                      (3000, 9000), "Construction", ["R1"],
                      "Demolition is a named included operation."),
    LineItemTemplate("cc_paint", "Painting and decorating, internal walls and ceilings, {location}",
                      (1200, 4500), "Construction", ["R4"],
                      "Painting/decorating explicitly included.",
                      vars={"location": ["office suite", "communal stairwell", "3rd floor"]}),
    LineItemTemplate("cc_kitchen", "Supply and install fitted kitchen units, worktops and appliances",
                      (6000, 14000), "Construction", ["R2", "R5"],
                      "Supply-and-fix pulls the whole line into scope."),
    LineItemTemplate("cc_firedoors_fit", "Supply and fit {n} fire doors, including ironmongery",
                      (3500, 7000), "Construction", ["R5"], "Supply-and-fix.",
                      vars={"n": ["20", "30", "40"]}),
    LineItemTemplate("cc_scaffold_erect", "Erection of scaffolding for roof works, {duration}",
                      (1800, 4200), "Construction", ["R8"],
                      "Erection/dismantling labour is integral works, included.",
                      vars={"duration": ["2 weeks", "3 weeks", "4 weeks"]}),
    LineItemTemplate("cc_drainage", "Installation of drainage and sanitation pipework, new build",
                      (4500, 11000), "Construction", ["R3"], "System installation."),
    LineItemTemplate("cc_lifts", "Installation of passenger lift per building specification",
                      (15000, 32000), "Construction", ["R1", "R3"],
                      "Lifts/plant needed by the building spec, included."),
    LineItemTemplate("cc_traffic_mgmt", "Provision of traffic management services during resurfacing works",
                      (1500, 3200), "Construction", ["R1"],
                      "Traffic management supplied to a contractor carrying out construction ops is included."),
    LineItemTemplate("cc_groundworks_combined", "Groundworks labour and materials, concrete foundations, fixed price",
                      (9000, 19000), "Construction", ["R1", "R11"],
                      "Whole line is one construction operation; materials sub-amount is excluded from the "
                      "deduction, not from the classification."),
    LineItemTemplate("cc_glazing_fit", "Installation and repair of glazing, {location}",
                      (2500, 6000), "Construction", ["R1"],
                      "Installation of glazing is a listed included operation.",
                      vars={"location": ["shopfront", "office block reception", "warehouse windows"]}),
    LineItemTemplate("cc_roads", "Construction, repair and resurfacing of access road including white-lining",
                      (8000, 22000), "Construction", ["R1"], "Named included operation."),
    LineItemTemplate("cc_siteclearance", "Site clearance and earth-moving ahead of construction",
                      (4000, 9500), "Construction", ["R4"], "Preparatory works, included."),
    LineItemTemplate("cc_asbestos", "Removal of asbestos as part of building alteration works",
                      (5000, 12000), "Construction", ["R1"],
                      "Alteration example named directly in CIS340."),
]

# ---------------------------------------------------------------------------
# Clear services: unambiguous Services-labelled line items
# ---------------------------------------------------------------------------
CLEAR_SERVICES = [
    LineItemTemplate("cs_arch_design", "Architectural design and planning drawings for extension",
                      (1200, 4000), "Services", ["R6"], "Pure advisory/design work."),
    LineItemTemplate("cs_survey_report", "Structural survey and written assessment report (no remedial works)",
                      (800, 2500), "Services", ["R6"], "Consultative/advisory only."),
    LineItemTemplate("cs_doors_delivery", "Supply of {n} fire doors, delivered to site (materials only)",
                      (2000, 4500), "Services", ["R7"],
                      "Manufacture/delivery only, no installation obligation on this line.",
                      vars={"n": ["20", "30", "40"]}),
    LineItemTemplate("cs_crane_nooperator", "Hire of tower crane, {duration}, no operator",
                      (1200, 3000), "Services", ["R8"], "Bare plant hire, no operator.",
                      vars={"duration": ["2 weeks", "4 weeks", "6 weeks"]}),
    LineItemTemplate("cs_security_guard", "Site security guarding, {duration}",
                      (2500, 5500), "Services", ["R9"], "Facilities/site-support, named exclusion.",
                      vars={"duration": ["4 weeks", "6 weeks", "8 weeks"]}),
    LineItemTemplate("cs_canteen", "Running of site canteen facility, {duration}",
                      (1800, 4200), "Services", ["R9"], "Site facilities running, excluded.",
                      vars={"duration": ["1 month", "3 months"]}),
    LineItemTemplate("cs_alarm_cctv", "Installation of intruder alarm and CCTV, {location}",
                      (1800, 4200), "Services", ["R10"], "Named security-system exclusion.",
                      vars={"location": ["warehouse unit", "office reception", "retail unit"]}),
    LineItemTemplate("cs_carpet", "Supply and fit carpet tiles, {location}",
                      (2200, 5200), "Services", ["R10"],
                      "Carpet fitting is the sole excluded finishing operation.",
                      vars={"location": ["3rd floor office", "reception area", "open-plan office"]}),
    LineItemTemplate("cs_signage", "Sign-writing and installation of shop signboard",
                      (900, 2600), "Services", ["R10"], "Named exclusion, signage."),
    LineItemTemplate("cs_seating", "Installation of fixed seating, staff break room",
                      (1500, 3800), "Services", ["R10"], "Named exclusion, seating."),
    LineItemTemplate("cs_concrete_delivery", "Delivery of ready-mix concrete, {qty}, no laying obligation",
                      (900, 2200), "Services", ["R7", "R11"], "Delivery only.",
                      vars={"qty": ["8 tonnes", "12 tonnes", "15 tonnes"]}),
    LineItemTemplate("cs_telecoms_wiring", "Replacement of telecommunications wiring through existing ducting",
                      (700, 1800), "Services", ["R10"],
                      "Narrow named exclusion, wiring through pre-existing ducting only."),
    LineItemTemplate("cs_scaffold_hire_only", "Hire of scaffolding equipment, {duration}, no erection or dismantling",
                      (800, 2000), "Services", ["R8"], "Supply-only, no labour.",
                      vars={"duration": ["2 weeks", "4 weeks"]}),
    LineItemTemplate("cs_blinds", "Installation of venetian blinds, office windows",
                      (600, 1600), "Services", ["R10"], "Named exclusion, blinds."),
]

# ---------------------------------------------------------------------------
# Ambiguous / edge cases: the actual stress test. Deliberately built around
# the narrower boundaries verified against the primary CIS340/CISR text
# (see cis_rules.md R6, R8, R10, R11-R11d, R13).
# ---------------------------------------------------------------------------
AMBIGUOUS = [
    LineItemTemplate("am_survey_remedial", "Structural survey following subsidence, remedial underpinning works",
                      (4500, 9000), "Construction", ["R6"],
                      "Surveyor has moved from inspection into hands-on remedial execution."),
    LineItemTemplate("am_crane_operator", "Hire of tower crane, {duration}, with operator",
                      (4500, 9000), "Construction", ["R8"],
                      "Plant hire with operator is a whole included operation, not a labour-portion split.",
                      vars={"duration": ["2 weeks", "4 weeks"]}),
    LineItemTemplate("am_cctv_traffic", "Installation of CCTV for traffic monitoring, site access road",
                      (1200, 3000), "Construction", ["R10"],
                      "CCTV for a non-security purpose isn't the named security-systems exclusion."),
    LineItemTemplate("am_fencing", "Supply and erect security fencing around site compound perimeter",
                      (2200, 5500), "Construction", ["R10"],
                      "Fencing deters intruders but isn't a security 'system', ordinary construction."),
    LineItemTemplate("am_boiler_free_install", "Supply of boiler and full heating system "
                      "(installation included at no extra charge)",
                      (7000, 13000), "Construction", ["R5", "R11a"],
                      "'Free installation' framing is a known dodge, flag as likely understating the labour value."),
    LineItemTemplate("am_accommodation", "Subcontractor accommodation costs, {duration} on-site",
                      (1800, 4200), "Construction", ["R11c"],
                      "Travel/accommodation costs are part of the taxable payment, not a materials-style exclusion.",
                      vars={"duration": ["6 weeks", "3 months"]}),
    LineItemTemplate("am_awning", "Supply and install retractable awning, shopfront",
                      (1500, 3500), "Construction", ["R10"],
                      "Awnings are explicitly NOT excluded, unlike blinds/shutters next to them in the rule."),
    LineItemTemplate("am_shutters", "Installation of louvered security shutters, {location}",
                      (1800, 4200), "Services", ["R10"],
                      "Shutters ARE the named exclusion, unlike the awning case above.",
                      vars={"location": ["shopfront", "warehouse loading bay"]}),
    LineItemTemplate("am_site_mgmt_fee", "Site management fee",
                      (2000, 6000), "Construction", ["R2", "R13"],
                      "Thin wording, classified on inferred dominant purpose per R13; would flip with more context."),
    LineItemTemplate("am_consultancy_remedial", "Consultancy, remedial works",
                      (1500, 4500), "Construction", ["R6", "R13"],
                      "'Consultancy' suggests advisory (R6→Services) but 'remedial works' suggests hands-on "
                      "execution, resolved toward Construction per R13; flag the ambiguity in reasoning."),
    LineItemTemplate("am_concrete_pump_op", "Concrete pumping service, wet concrete distribution around site",
                      (1200, 2800), "Construction", ["R7"],
                      "Third-party pumping is a site service, not a delivery, contrasts with plain chute delivery."),
    LineItemTemplate("am_kitchen_supply_fix", "Fitted kitchen, supply and install, including worktops "
                      "and appliances",
                      (9000, 15000), "Construction", ["R2", "R5"],
                      "Installation element pulls the whole contract into scope even though most value is goods."),
    LineItemTemplate("am_ready_mix_standalone", "Materials: {qty} ready-mix concrete, delivered",
                      (1400, 2200), "Services", ["R7", "R11"],
                      "Standalone delivery on this invoice, no laying/installation obligation.",
                      vars={"qty": ["10 tonnes", "12 tonnes", "14 tonnes"]}),
    LineItemTemplate("am_vessel_crewed", "Hire of barge with crew for dock construction works",
                      (6000, 14000), "Construction", ["R8", "R11b"],
                      "Crewed vessel hire = plant hire with operator."),
    LineItemTemplate("am_vessel_bare", "Hire of barge, contractor to provide own crew",
                      (2500, 6000), "Services", ["R8"],
                      "Bare vessel hire with the contractor's own crew = plant hire without operator."),
    LineItemTemplate("am_carpet_mixed", "Supply and fit carpet tiles as part of wider office refurbishment contract",
                      (2000, 4500), "Construction", ["R10", "R2"],
                      "Carpet fitting alone is excluded, but as part of a mixed contract the whole contract, "
                      "and this line, comes within the scheme."),
    LineItemTemplate("am_forestry", "Tree felling and landscaping in the ordinary course of estate management "
                      "(no development project)",
                      (1200, 3000), "Services", ["R4"],
                      "Not yet a dedicated rule, CIS340 A.21 distinguishes forestry/estate management "
                      "(excluded) from development-linked tree felling (included). Promote to a rule if this "
                      "trips up classify.py.", gap=True),
    LineItemTemplate("am_forestry_dev", "Tree felling and site clearance as part of new housing development, "
                      "Plot {n}",
                      (2500, 6000), "Construction", ["R4"],
                      "Same activity as the estate-management case above, but tied to a development project, "
                      "included. Promote to a rule if this trips up classify.py.", gap=True,
                      vars={"n": ["7", "12", "B3"]}),
]

# Lookup by template id, mainly handy if you want to inspect or reuse a
# specific template from a Python shell rather than reading the lists above.
ALL_TEMPLATES = {t.id: t for t in CLEAR_CONSTRUCTION + CLEAR_SERVICES + AMBIGUOUS}

# Fake supplier and customer names, grouped so an invoice's supplier matches
# the flavour of work on it.
CONSTRUCTION_SUPPLIERS = [
    "Ashcroft Building Services Ltd", "Northbridge Groundworks Ltd",
    "BrightSpark M&E Contractors", "Hollowell Flooring & Interiors Ltd",
]
SERVICES_SUPPLIERS = [
    "Meridian Architects LLP", "Fenwick Plant Hire Ltd",
    "Solace Security Systems Ltd", "Vantage Facilities Services Ltd",
]
GENERAL_SUPPLIERS = [
    "Camden Bridge Construction Group Ltd", "Oakfield General Contracting Ltd",
    "Riverline Build & Fit-Out Ltd", "Thornfield Contractors Ltd",
]
CUSTOMERS = [
    "Riverside Developments Ltd", "Oakfield Property Group",
    "Camden Retail Partners", "Meadowgate Asset Management Ltd",
    "Harborne Estates Ltd",
]

# Defines the batch of invoices generate_invoices() will produce. Each tuple
# is one scenario type: how many invoices, how many lines per invoice, which
# template pool(s) to draw lines from (with weights, so "ambiguous_mixed"
# invoices are mostly tricky lines but still have some easy ones mixed in,
# like a real invoice would), and which supplier names fit that scenario.
SCENARIOS = [
    # (name, invoice_count, line_count_range, template_pools_with_weights, supplier_pool)
    ("clear_construction", 3, (3, 5), [(CLEAR_CONSTRUCTION, 1.0)], CONSTRUCTION_SUPPLIERS),
    ("clear_services", 2, (2, 4), [(CLEAR_SERVICES, 1.0)], SERVICES_SUPPLIERS),
    ("ambiguous_mixed", 5, (3, 6),
     [(AMBIGUOUS, 0.65), (CLEAR_CONSTRUCTION, 0.2), (CLEAR_SERVICES, 0.15)], GENERAL_SUPPLIERS),
]


def pick_templates(rng: random.Random, pools_with_weights, n: int) -> list:
    # Picks n templates for one invoice. First rolls which pool to draw from
    # (respecting the scenario's weights), then picks a random template from
    # that pool. Tries to avoid repeating the same template twice on one
    # invoice, but gives up on that constraint (accepts a repeat) once we've
    # already used every template available across all the pools involved,
    # so it can't loop forever asking for more lines than exist.
    chosen = []
    used_ids = set()
    attempts = 0
    while len(chosen) < n and attempts < n * 20:
        attempts += 1
        pool, _ = rng.choices(pools_with_weights, weights=[w for _, w in pools_with_weights])[0]
        candidate = rng.choice(pool)
        if candidate.id in used_ids and len(used_ids) < sum(len(p) for p, _ in pools_with_weights):
            continue
        used_ids.add(candidate.id)
        chosen.append(candidate)
    return chosen


def build_invoice(rng: random.Random, invoice_num: int, scenario: str, line_range: tuple,
                   pools_with_weights, suppliers: list) -> dict:
    # Assembles one full invoice dict: picks how many lines it has, renders
    # each chosen template into a real description and amount, and wraps it
    # all up with an invoice id, date, supplier, and customer.
    invoice_id = f"INV-2026-{invoice_num:04d}"
    n_lines = rng.randint(*line_range)
    templates = pick_templates(rng, pools_with_weights, n_lines)

    invoice_date = ANCHOR_DATE - timedelta(days=rng.randint(1, 120))
    supplier = rng.choice(suppliers)
    customer = rng.choice(CUSTOMERS)

    line_items = []
    for i, tmpl in enumerate(templates, start=1):
        description, amount = tmpl.render(rng)
        line_items.append({
            "line_id": f"{invoice_id}-L{i}",
            "description": description,
            "amount_gbp": amount,
            # classify.py must never read this block when building its
            # prompt, it is the answer key only, used for scoring afterward.
            "_ground_truth": {
                "label": tmpl.label,
                "rule_ids": tmpl.rule_ids,
                "note": tmpl.note,
                "gap": tmpl.gap,
                "template_id": tmpl.id,
            },
        })

    return {
        "invoice_id": invoice_id,
        "scenario": scenario,
        "supplier": supplier,
        "customer": customer,
        "date": invoice_date.isoformat(),
        "line_items": line_items,
        "total_gbp": round(sum(li["amount_gbp"] for li in line_items), 2),
    }


def generate_invoices(seed: int) -> list:
    # Top level driver: walks through every scenario in SCENARIOS and builds
    # the requested number of invoices for each, numbering them 0001, 0002,
    # ... in order across the whole batch. One shared rng, seeded once, so
    # the entire run is reproducible from a single --seed value.
    rng = random.Random(seed)
    invoices = []
    invoice_num = 1
    for scenario, count, line_range, pools_with_weights, suppliers in SCENARIOS:
        for _ in range(count):
            invoices.append(build_invoice(rng, invoice_num, scenario, line_range, pools_with_weights, suppliers))
            invoice_num += 1
    return invoices


def write_outputs(invoices: list, out_dir: Path, manifest_path: Path):
    # Writes two things: one JSON file per invoice (what classify.py will
    # actually read, mimicking what a real invoice document would look like
    # once its data is extracted), and a single flat manifest.csv with one
    # row per line item, which is the full ground truth answer key used to
    # score classify.py's results afterward.
    out_dir.mkdir(parents=True, exist_ok=True)
    manifest_path.parent.mkdir(parents=True, exist_ok=True)

    for inv in invoices:
        path = out_dir / f"{inv['invoice_id']}.json"
        path.write_text(json.dumps(inv, indent=2), encoding="utf-8")

    with manifest_path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow([
            "invoice_id", "scenario", "supplier", "customer", "date", "line_id",
            "description", "amount_gbp", "ground_truth_label", "ground_truth_rule_ids",
            "ground_truth_note", "gap_flag",
        ])
        for inv in invoices:
            for li in inv["line_items"]:
                gt = li["_ground_truth"]
                writer.writerow([
                    inv["invoice_id"], inv["scenario"], inv["supplier"], inv["customer"], inv["date"],
                    li["line_id"], li["description"], li["amount_gbp"], gt["label"],
                    "|".join(gt["rule_ids"]), gt["note"], gt["gap"],
                ])


def print_summary(invoices: list):
    # Just a console report so you can see at a glance what got generated
    # without having to open the JSON or CSV files, counts of invoices per
    # scenario, the Construction vs Services split, and how many lines are
    # flagged as testing a known gap in the current rules doc.
    total_lines = sum(len(inv["line_items"]) for inv in invoices)
    label_counts = {"Construction": 0, "Services": 0}
    gap_count = 0
    for inv in invoices:
        for li in inv["line_items"]:
            label_counts[li["_ground_truth"]["label"]] += 1
            if li["_ground_truth"]["gap"]:
                gap_count += 1

    print(f"Generated {len(invoices)} invoices, {total_lines} line items.")
    for scenario, count, *_ in SCENARIOS:
        print(f"  {scenario}: {count} invoices")
    print(f"Ground truth split: {label_counts['Construction']} Construction, "
          f"{label_counts['Services']} Services")
    if gap_count:
        print(f"{gap_count} line item(s) flagged as testing a known rules-doc gap "
              f"(gap_flag=True in manifest.csv), see cis_rules.md Open Items.")


def main():
    # CLI entry point: parses arguments, generates the invoices, writes them
    # to disk, and prints the summary. Run this file directly to use it,
    # e.g. `python generate_invoices.py` or `python generate_invoices.py
    # --seed 42` for a different (still reproducible) batch.
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--seed", type=int, default=7, help="RNG seed for reproducible generation (default: 7)")
    parser.add_argument("--out-dir", type=Path, default=DEFAULT_OUT_DIR,
                         help="Directory to write per-invoice JSON files (default: data/invoices)")
    parser.add_argument("--manifest", type=Path, default=MANIFEST_PATH,
                         help="Path for the ground-truth manifest CSV (default: data/manifest.csv)")
    args = parser.parse_args()

    invoices = generate_invoices(args.seed)
    write_outputs(invoices, args.out_dir, args.manifest)
    print_summary(invoices)
    print(f"\nInvoices written to: {args.out_dir}")
    print(f"Ground-truth manifest written to: {args.manifest}")


if __name__ == "__main__":
    main()
