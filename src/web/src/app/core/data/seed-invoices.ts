import { Invoice, InvoiceLine } from '../models/invoice.model';

// A seed invoice has no overridden/overrideReason yet, InvoiceStore adds
// those (both false/empty) on init, mirroring the mockup's own state
// constructor which does the same thing to RAW_INVOICES.
export type SeedInvoiceLine = Omit<InvoiceLine, 'overridden' | 'overrideReason'>;
export type SeedInvoice = Omit<Invoice, 'lines'> & { lines: SeedInvoiceLine[] };

// Ported from the mockup's RAW_INVOICES (POC/Mock Up/CIS Invoice Compliance
// Tool/CIS Compliance Tool-standalone.html), which itself is real output
// from a validated POC run, not invented data.
export const SEED_INVOICES: SeedInvoice[] = [
  {
    id: 'inv-2026-0001', ref: 'INV-2026-0001', supplier: 'Ashcroft Building Services Ltd', date: '2026-07-30',
    lines: [
      { description: 'Erection of scaffolding for roof works, 3 weeks', amount: 1967.65, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'The line item describes the erection of scaffolding, which is integral to construction work. This falls under the scope of construction operations as per R1.' },
      { description: 'Preparation and laying of foundations, Plot 4', amount: 12612.57, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'The preparation and laying of foundations is a core construction activity, clearly defined under R1 as part of construction operations.' },
      { description: 'Install air conditioning system, 2nd floor office', amount: 5755.46, classification: 'construction', confidence: 'high', citation: 'R3 (CIS340)',
        reasoning: 'Installing an air conditioning system is classified as construction work since it involves the installation of a system within a building, as per R3.' },
      { description: 'Painting and decorating, internal walls and ceilings, office suite', amount: 4326.58, classification: 'construction', confidence: 'high', citation: 'R4 (CIS340)',
        reasoning: 'Painting and decorating internal walls and ceilings is considered construction work as it is part of the finishing operations during construction, according to R4.' },
    ],
  },
  {
    id: 'inv-2026-0002', ref: 'INV-2026-0002', supplier: 'BrightSpark M&E Contractors', date: '2026-05-22',
    lines: [
      { description: 'Install air conditioning system, warehouse unit 4', amount: 2977.12, classification: 'construction', confidence: 'high', citation: 'R3 (CIS340)',
        reasoning: 'This line describes the installation of an air conditioning system, which is classified as Construction under R3 because it involves installing a system in a building.' },
      { description: 'Erection of scaffolding for roof works, 4 weeks', amount: 3170.89, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'The erection of scaffolding is a construction operation as it is integral to the roof works, thus classified as Construction under R1.' },
      { description: 'Painting and decorating, internal walls and ceilings, office suite', amount: 2428.91, classification: 'construction', confidence: 'high', citation: 'R4 (CIS340)',
        reasoning: 'Painting and decorating internal walls and ceilings is considered site work and finishing, which falls under Construction as per R4.' },
      { description: 'Site clearance and earth-moving ahead of construction', amount: 7012.59, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'Site clearance and earth-moving are preparatory works that are part of construction operations, thus classified as Construction under R1.' },
      { description: 'Preparation and laying of foundations, Plot 4', amount: 12772.42, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'Preparation and laying of foundations is a core construction activity, classified as Construction under R1.' },
    ],
  },
  {
    id: 'inv-2026-0003', ref: 'INV-2026-0003', supplier: 'BrightSpark M&E Contractors', date: '2026-04-13',
    lines: [
      { description: 'Groundworks labour and materials, concrete foundations, fixed price', amount: 11879.38, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340), R11 (CISR15060)',
        reasoning: 'This line item describes groundworks labour and materials for concrete foundations, which falls under construction operations as defined in R1. The entire line is part of a construction contract.' },
      { description: 'Construction, repair and resurfacing of access road including white-lining', amount: 21722.45, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'The description includes construction, repair, and resurfacing of an access road, which is clearly a construction operation as per R1. This line is classified as Construction.' },
      { description: 'Provision of traffic management services during resurfacing works', amount: 1700.71, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'This line describes the provision of traffic management services during resurfacing works. According to R1, traffic management services supplied to a contractor carrying out construction operations are included, so this line is classified as Construction.' },
      { description: 'Supply and fit 30 fire doors, including ironmongery', amount: 4077.37, classification: 'construction', confidence: 'high', citation: 'R5 (CISR14220)',
        reasoning: 'The line item specifies supply and fit of fire doors, which is a supply-and-fix operation. This falls under R5, making it a construction operation. Therefore, it is classified as Construction.' },
      { description: 'Installation of drainage and sanitation pipework, new build', amount: 6723.36, classification: 'construction', confidence: 'high', citation: 'R3 (CIS340)',
        reasoning: 'This line describes the installation of drainage and sanitation pipework for a new build, which is a construction operation as per R3. Thus, it is classified as Construction.' },
    ],
  },
  {
    id: 'inv-2026-0004', ref: 'INV-2026-0004', supplier: 'Solace Security Systems Ltd', date: '2026-06-24',
    lines: [
      { description: 'Delivery of ready-mix concrete, 15 tonnes, no laying obligation', amount: 1545.68, classification: 'services', confidence: 'high', citation: 'R7 (CISR14220), R11 (CISR15060)',
        reasoning: 'This line item describes the delivery of ready-mix concrete without any laying obligation, which falls under the manufacture and delivery exclusion (R7). Since there is no installation obligation, it is classified as Services.' },
      { description: 'Sign-writing and installation of shop signboard', amount: 2254.72, classification: 'services', confidence: 'high', citation: 'R10 (CIS340)',
        reasoning: "The description includes both sign-writing and installation of a shop signboard. However, sign-writing is a named exclusion under R10, and since there is no indication in the line's wording that it is part of a larger construction contract, it is classified as Services." },
      { description: 'Installation of venetian blinds, office windows', amount: 668.76, classification: 'services', confidence: 'high', citation: 'R10 (CIS340)',
        reasoning: 'The installation of venetian blinds is a named exclusion under R10. The line does not indicate that it is part of a larger construction contract, so it is classified as Services.' },
    ],
  },
  {
    id: 'inv-2026-0005', ref: 'INV-2026-0005', supplier: 'Solace Security Systems Ltd', date: '2026-07-27',
    lines: [
      { description: 'Supply and fit carpet tiles, open-plan office', amount: 4665.77, classification: 'services', confidence: 'high', citation: 'R10 (CIS340)',
        reasoning: 'The line item describes the supply and fitting of carpet tiles, which falls under the named exclusion for carpet fitting (R10). I checked for a mixed-contract override but found no wording indicating this is part of a larger construction contract. Therefore, it is classified as Services.' },
      { description: 'Structural survey and written assessment report (no remedial works)', amount: 1283.81, classification: 'services', confidence: 'high', citation: 'R6 (CISR14270)',
        reasoning: 'This line item involves a structural survey and a written assessment report, which is purely advisory work without any execution or remedial action involved (R6). Therefore, it is classified as Services.' },
    ],
  },
  {
    id: 'inv-2026-0006', ref: 'INV-2026-0006', supplier: 'Thornfield Contractors Ltd', date: '2026-06-14',
    lines: [
      { description: 'Running of site canteen facility, 1 month', amount: 2878.05, classification: 'services', confidence: 'high', citation: 'R9 (CIS340)',
        reasoning: 'This line item describes the running of a site canteen facility, which is a facility/site-support service and therefore excluded under R9. There is no indication in the wording that this is part of a larger construction contract.' },
      { description: 'Hire of barge, contractor to provide own crew', amount: 4423.04, classification: 'services', confidence: 'high', citation: 'R8 (CIS340)',
        reasoning: 'The hire of a barge without a crew is classified as Services under R8, as it is bare plant hire without operator presence. There is no indication that this is part of a larger construction operation.' },
      { description: 'Supply and erect security fencing around site compound perimeter', amount: 5115.17, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'This line item involves the supply and erection of security fencing, which is considered Construction work as it is not classified as a security system under R10. The erection of fencing is a construction operation.' },
      { description: 'Supply and install retractable awning, shopfront', amount: 3138.56, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'The supply and installation of a retractable awning is classified as Construction under R1, as it involves installation work. There is no indication that this is part of a larger contract that would change its classification.' },
      { description: 'Demolition of existing outbuilding prior to new build', amount: 8183.91, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'Demolition of an existing outbuilding is classified as Construction under R1, as it is a construction operation. There are no indications that this is part of a mixed contract that would alter its classification.' },
      { description: 'Erection of scaffolding for roof works, 3 weeks', amount: 3495.35, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'Erection of scaffolding for roof works is classified as Construction under R1, as it is integral to the construction process. There is no indication that this is part of a larger contract that would change its classification.' },
    ],
  },
  {
    id: 'inv-2026-0007', ref: 'INV-2026-0007', supplier: 'Riverline Build & Fit-Out Ltd', date: '2026-07-01',
    lines: [
      { description: 'Erection of scaffolding for roof works, 2 weeks', amount: 2805.47, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340), R4 (CIS340)',
        reasoning: 'The erection of scaffolding is a construction operation directly related to the roof works, which falls under the scope of construction activities. Therefore, it is classified as Construction.' },
      { description: 'Supply of 30 fire doors, delivered to site (materials only)', amount: 3524.53, classification: 'services', confidence: 'high', citation: 'R7 (CISR14220), R11 (CISR15060)',
        reasoning: 'The supply of fire doors is classified as Services because it is a materials-only delivery with no installation obligation included in this line item, which aligns with the exclusion for manufacture and delivery without installation.' },
      { description: 'Supply of boiler and full heating system (installation included at no extra charge)', amount: 8911.67, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340), R5 (CISR14220), R11a (CISR15060)',
        reasoning: 'Although the installation is described as included at no extra charge, this line item is still classified as Construction because it involves the supply and installation of a heating system, which is a construction operation.' },
      { description: 'Installation of louvered security shutters, shopfront', amount: 3457.18, classification: 'services', confidence: 'high', citation: 'R10 (CIS340)',
        reasoning: 'The installation of louvered security shutters is classified as Services because it falls under the named exclusion for security systems, which are not considered construction work under the CIS rules.' },
      { description: 'Subcontractor accommodation costs, 6 weeks on-site', amount: 2895.94, classification: 'construction', confidence: 'high', citation: 'R11c (CISR15080)',
        reasoning: "Subcontractor accommodation costs are classified as Construction because they are part of the overall contract payment for the subcontractor's work on-site, and are fully subject to CIS deductions." },
    ],
  },
  {
    id: 'inv-2026-0008', ref: 'INV-2026-0008', supplier: 'Camden Bridge Construction Group Ltd', date: '2026-05-27',
    lines: [
      { description: 'Materials: 14 tonnes ready-mix concrete, delivered', amount: 1420.4, classification: 'services', confidence: 'high', citation: 'R7 (CISR14220), R11 (CISR15060)',
        reasoning: 'This line item describes the delivery of ready-mix concrete without any installation obligation, which falls under the manufacture and delivery exclusion (R7). Therefore, it is classified as Services.' },
      { description: 'Installation of CCTV for traffic monitoring, site access road', amount: 2773.8, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'The installation of CCTV for traffic monitoring is considered Construction as it supports a construction operation (R1). This is not a named exclusion since it is for traffic monitoring, not security purposes.' },
      { description: 'Supply and fit carpet tiles, open-plan office', amount: 3328.69, classification: 'services', confidence: 'high', citation: 'R10 (CIS340)',
        reasoning: 'Carpet fitting is a named exclusion under R10, which states that it is classified as Services. The line does not indicate any mixed contract signal that would override this exclusion.' },
      { description: 'Concrete pumping service, wet concrete distribution around site', amount: 2215.06, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'Concrete pumping service involves the distribution of wet concrete around the site, which is a construction operation (R1). This service is integral to the construction process, thus classified as Construction.' },
      { description: 'Supply and erect security fencing around site compound perimeter', amount: 5353.04, classification: 'construction', confidence: 'high', citation: 'R1 (CIS340)',
        reasoning: 'The supply and erection of security fencing is not classified as a security system under R10, but rather as ordinary construction work. Therefore, it is classified as Construction.' },
      { description: 'Supply of boiler and full heating system (installation included at no extra charge)', amount: 10613.68, classification: 'construction', confidence: 'low', citation: 'R5 (CISR14220), R11a (CISR15060)',
        reasoning: 'This line item describes a supply and installation of a heating system, which is a construction operation (R5). However, the framing suggests that installation is included at no extra charge, which is a pattern that raises suspicion (R11a). Despite this, the overall operation is still classified as Construction due to the installation obligation.' },
    ],
  },
];
