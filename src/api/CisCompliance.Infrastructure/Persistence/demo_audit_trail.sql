-- Demo query for Mark: shows the audit-trail guarantee this whole persistence
-- layer exists for. Every overridden line still shows what the AI originally
-- said (PredictedLabel) alongside what a human corrected it to
-- (EffectiveLabel), with the reason and timestamp for the correction.
-- Run against CisComplianceDb (localhost\SQLEXPRESS).

SELECT
    i.InvoiceId,
    i.Supplier,
    l.LineId,
    l.Description,
    l.AmountGbp,
    l.PredictedLabel,      -- what the AI originally said, never overwritten
    l.PredictedConfidence,
    l.EffectiveLabel,      -- what's actually used, may differ from predicted
    l.IsOverridden,
    l.OverrideReason,
    l.OverriddenAtUtc
FROM Invoices i
JOIN InvoiceLineEntity l ON l.InvoiceId = i.InvoiceId
WHERE l.IsOverridden = 1
ORDER BY l.OverriddenAtUtc DESC;

-- Full picture for one invoice, including citations:
-- SELECT i.InvoiceId, i.Supplier, i.Customer, i.Date, i.CreatedAtUtc,
--        l.LineId, l.Description, l.AmountGbp, l.PredictedLabel, l.EffectiveLabel,
--        c.RuleId, c.SourceName, c.SourceTitle
-- FROM Invoices i
-- JOIN InvoiceLineEntity l ON l.InvoiceId = i.InvoiceId
-- JOIN RuleCitationEntity c ON c.InvoiceLineEntityId = l.Id
-- WHERE i.InvoiceId = 'INV-2026-0001'
-- ORDER BY l.Id;
