using CisCompliance.Core.Models;

namespace CisCompliance.Infrastructure.Persistence;

internal sealed record SeedLine(
    string Description, decimal AmountGbp, ClassificationLabel Label, ConfidenceLevel Confidence,
    string Reasoning, string[] RuleIds);

internal sealed record SeedInvoice(string InvoiceId, string Supplier, string Customer, DateOnly Date, SeedLine[] Lines);

// The same 8 real, validated POC invoices previously hardcoded in the
// Angular app's seed-invoices.ts (now deleted, the database is the source
// of truth). Citations here are just rule ids, CisComplianceDbSeeder
// resolves them against the real cis_rules_index.json via CitationResolver,
// the same code path a live classification uses, rather than hand-writing
// resolved source details that could drift out of sync with the rules doc.
// Customer is invented, seed-invoices.ts never had one (nothing in the UI
// renders it), this is cosmetic only.
internal static class InvoiceSeedData
{
    public static readonly SeedInvoice[] Invoices =
    [
        new("INV-2026-0001", "Ashcroft Building Services Ltd", "Oakfield Property Group", new DateOnly(2026, 7, 30),
        [
            new("Erection of scaffolding for roof works, 3 weeks", 1967.65m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The line item describes the erection of scaffolding, which is integral to construction work. This falls under the scope of construction operations as per R1.", ["R1"]),
            new("Preparation and laying of foundations, Plot 4", 12612.57m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The preparation and laying of foundations is a core construction activity, clearly defined under R1 as part of construction operations.", ["R1"]),
            new("Install air conditioning system, 2nd floor office", 5755.46m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Installing an air conditioning system is classified as construction work since it involves the installation of a system within a building, as per R3.", ["R3"]),
            new("Painting and decorating, internal walls and ceilings, office suite", 4326.58m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Painting and decorating internal walls and ceilings is considered construction work as it is part of the finishing operations during construction, according to R4.", ["R4"]),
        ]),
        new("INV-2026-0002", "BrightSpark M&E Contractors", "Oakfield Property Group", new DateOnly(2026, 5, 22),
        [
            new("Install air conditioning system, warehouse unit 4", 2977.12m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "This line describes the installation of an air conditioning system, which is classified as Construction under R3 because it involves installing a system in a building.", ["R3"]),
            new("Erection of scaffolding for roof works, 4 weeks", 3170.89m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The erection of scaffolding is a construction operation as it is integral to the roof works, thus classified as Construction under R1.", ["R1"]),
            new("Painting and decorating, internal walls and ceilings, office suite", 2428.91m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Painting and decorating internal walls and ceilings is considered site work and finishing, which falls under Construction as per R4.", ["R4"]),
            new("Site clearance and earth-moving ahead of construction", 7012.59m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Site clearance and earth-moving are preparatory works that are part of construction operations, thus classified as Construction under R1.", ["R1"]),
            new("Preparation and laying of foundations, Plot 4", 12772.42m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Preparation and laying of foundations is a core construction activity, classified as Construction under R1.", ["R1"]),
        ]),
        new("INV-2026-0003", "BrightSpark M&E Contractors", "Camden Bridge Estates", new DateOnly(2026, 4, 13),
        [
            new("Groundworks labour and materials, concrete foundations, fixed price", 11879.38m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "This line item describes groundworks labour and materials for concrete foundations, which falls under construction operations as defined in R1. The entire line is part of a construction contract.", ["R1", "R11"]),
            new("Construction, repair and resurfacing of access road including white-lining", 21722.45m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The description includes construction, repair, and resurfacing of an access road, which is clearly a construction operation as per R1. This line is classified as Construction.", ["R1"]),
            new("Provision of traffic management services during resurfacing works", 1700.71m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "This line describes the provision of traffic management services during resurfacing works. According to R1, traffic management services supplied to a contractor carrying out construction operations are included, so this line is classified as Construction.", ["R1"]),
            new("Supply and fit 30 fire doors, including ironmongery", 4077.37m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The line item specifies supply and fit of fire doors, which is a supply-and-fix operation. This falls under R5, making it a construction operation. Therefore, it is classified as Construction.", ["R5"]),
            new("Installation of drainage and sanitation pipework, new build", 6723.36m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "This line describes the installation of drainage and sanitation pipework for a new build, which is a construction operation as per R3. Thus, it is classified as Construction.", ["R3"]),
        ]),
        new("INV-2026-0004", "Solace Security Systems Ltd", "Riverline Estates Ltd", new DateOnly(2026, 6, 24),
        [
            new("Delivery of ready-mix concrete, 15 tonnes, no laying obligation", 1545.68m, ClassificationLabel.Services, ConfidenceLevel.High,
                "This line item describes the delivery of ready-mix concrete without any laying obligation, which falls under the manufacture and delivery exclusion (R7). Since there is no installation obligation, it is classified as Services.", ["R7", "R11"]),
            new("Sign-writing and installation of shop signboard", 2254.72m, ClassificationLabel.Services, ConfidenceLevel.High,
                "The description includes both sign-writing and installation of a shop signboard. However, sign-writing is a named exclusion under R10, and since there is no indication in the line's wording that it is part of a larger construction contract, it is classified as Services.", ["R10"]),
            new("Installation of venetian blinds, office windows", 668.76m, ClassificationLabel.Services, ConfidenceLevel.High,
                "The installation of venetian blinds is a named exclusion under R10. The line does not indicate that it is part of a larger construction contract, so it is classified as Services.", ["R10"]),
        ]),
        new("INV-2026-0005", "Solace Security Systems Ltd", "Riverline Estates Ltd", new DateOnly(2026, 7, 27),
        [
            new("Supply and fit carpet tiles, open-plan office", 4665.77m, ClassificationLabel.Services, ConfidenceLevel.High,
                "The line item describes the supply and fitting of carpet tiles, which falls under the named exclusion for carpet fitting (R10). I checked for a mixed-contract override but found no wording indicating this is part of a larger construction contract. Therefore, it is classified as Services.", ["R10"]),
            new("Structural survey and written assessment report (no remedial works)", 1283.81m, ClassificationLabel.Services, ConfidenceLevel.High,
                "This line item involves a structural survey and a written assessment report, which is purely advisory work without any execution or remedial action involved (R6). Therefore, it is classified as Services.", ["R6"]),
        ]),
        new("INV-2026-0006", "Thornfield Contractors Ltd", "Camden Bridge Estates", new DateOnly(2026, 6, 14),
        [
            new("Running of site canteen facility, 1 month", 2878.05m, ClassificationLabel.Services, ConfidenceLevel.High,
                "This line item describes the running of a site canteen facility, which is a facility/site-support service and therefore excluded under R9. There is no indication in the wording that this is part of a larger construction contract.", ["R9"]),
            new("Hire of barge, contractor to provide own crew", 4423.04m, ClassificationLabel.Services, ConfidenceLevel.High,
                "The hire of a barge without a crew is classified as Services under R8, as it is bare plant hire without operator presence. There is no indication that this is part of a larger construction operation.", ["R8"]),
            new("Supply and erect security fencing around site compound perimeter", 5115.17m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "This line item involves the supply and erection of security fencing, which is considered Construction work as it is not classified as a security system under R10. The erection of fencing is a construction operation.", ["R1"]),
            new("Supply and install retractable awning, shopfront", 3138.56m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The supply and installation of a retractable awning is classified as Construction under R1, as it involves installation work. There is no indication that this is part of a larger contract that would change its classification.", ["R1"]),
            new("Demolition of existing outbuilding prior to new build", 8183.91m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Demolition of an existing outbuilding is classified as Construction under R1, as it is a construction operation. There are no indications that this is part of a mixed contract that would alter its classification.", ["R1"]),
            new("Erection of scaffolding for roof works, 3 weeks", 3495.35m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Erection of scaffolding for roof works is classified as Construction under R1, as it is integral to the construction process. There is no indication that this is part of a larger contract that would change its classification.", ["R1"]),
        ]),
        new("INV-2026-0007", "Riverline Build & Fit-Out Ltd", "Riverline Estates Ltd", new DateOnly(2026, 7, 1),
        [
            new("Erection of scaffolding for roof works, 2 weeks", 2805.47m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The erection of scaffolding is a construction operation directly related to the roof works, which falls under the scope of construction activities. Therefore, it is classified as Construction.", ["R1", "R4"]),
            new("Supply of 30 fire doors, delivered to site (materials only)", 3524.53m, ClassificationLabel.Services, ConfidenceLevel.High,
                "The supply of fire doors is classified as Services because it is a materials-only delivery with no installation obligation included in this line item, which aligns with the exclusion for manufacture and delivery without installation.", ["R7", "R11"]),
            new("Supply of boiler and full heating system (installation included at no extra charge)", 8911.67m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Although the installation is described as included at no extra charge, this line item is still classified as Construction because it involves the supply and installation of a heating system, which is a construction operation.", ["R1", "R5", "R11a"]),
            new("Installation of louvered security shutters, shopfront", 3457.18m, ClassificationLabel.Services, ConfidenceLevel.High,
                "The installation of louvered security shutters is classified as Services because it falls under the named exclusion for security systems, which are not considered construction work under the CIS rules.", ["R10"]),
            new("Subcontractor accommodation costs, 6 weeks on-site", 2895.94m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Subcontractor accommodation costs are classified as Construction because they are part of the overall contract payment for the subcontractor's work on-site, and are fully subject to CIS deductions.", ["R11c"]),
        ]),
        new("INV-2026-0008", "Camden Bridge Construction Group Ltd", "Camden Bridge Estates", new DateOnly(2026, 5, 27),
        [
            new("Materials: 14 tonnes ready-mix concrete, delivered", 1420.40m, ClassificationLabel.Services, ConfidenceLevel.High,
                "This line item describes the delivery of ready-mix concrete without any installation obligation, which falls under the manufacture and delivery exclusion (R7). Therefore, it is classified as Services.", ["R7", "R11"]),
            new("Installation of CCTV for traffic monitoring, site access road", 2773.80m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The installation of CCTV for traffic monitoring is considered Construction as it supports a construction operation (R1). This is not a named exclusion since it is for traffic monitoring, not security purposes.", ["R1"]),
            new("Supply and fit carpet tiles, open-plan office", 3328.69m, ClassificationLabel.Services, ConfidenceLevel.High,
                "Carpet fitting is a named exclusion under R10, which states that it is classified as Services. The line does not indicate any mixed contract signal that would override this exclusion.", ["R10"]),
            new("Concrete pumping service, wet concrete distribution around site", 2215.06m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "Concrete pumping service involves the distribution of wet concrete around the site, which is a construction operation (R1). This service is integral to the construction process, thus classified as Construction.", ["R1"]),
            new("Supply and erect security fencing around site compound perimeter", 5353.04m, ClassificationLabel.Construction, ConfidenceLevel.High,
                "The supply and erection of security fencing is not classified as a security system under R10, but rather as ordinary construction work. Therefore, it is classified as Construction.", ["R1"]),
            new("Supply of boiler and full heating system (installation included at no extra charge)", 10613.68m, ClassificationLabel.Construction, ConfidenceLevel.Low,
                "This line item describes a supply and installation of a heating system, which is a construction operation (R5). However, the framing suggests that installation is included at no extra charge, which is a pattern that raises suspicion (R11a). Despite this, the overall operation is still classified as Construction due to the installation obligation.", ["R5", "R11a"]),
        ]),
    ];
}
