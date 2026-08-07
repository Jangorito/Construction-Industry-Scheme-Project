using System.Text.Json;
using CisCompliance.Core.Models;

namespace CisCompliance.Core.Classification;

// Ported verbatim from POC/classify.py's SYSTEM_PROMPT_TEMPLATE. This exact
// wording (the 3 step exclusion / override / inclusion procedure, the
// reasoning-before-label instruction, the confidence rules) was iteratively
// tuned against real failures to reach 95.7% validated accuracy. Treat this
// as a port, not a rewrite: do not "clean up" the wording without re-running
// the POC's validated test set, a change here can silently change behaviour.
public static class ClassificationPrompt
{
    public const string SystemPromptTemplate = """
        You are classifying UK subcontractor invoice line items for the Construction Industry Scheme (CIS), following the rules doc below exactly.

        Every line item must get exactly one label: "Construction" (CIS-relevant) or "Services" (non-CIS). Classify each line item independently, per rule R12, even when other lines on the same invoice have a different label. Where wording is genuinely ambiguous, apply R13: give your best answer but name in the reasoning what fact would change it, don't resolve it with false confidence.

        Every line item also needs a confidence value: "high" or "low". Set "low" whenever any of these apply: you're relying on R13 because the wording is genuinely ambiguous, you had to make a judgment call about whether a mixed-contract override applies rather than finding a clear signal either way, or you considered two different rules that would give different answers and picked one over the other. Otherwise set "high". Don't default to "high" just because you were able to produce an answer, being able to answer and being sure are different things.

        Follow this order, and don't skip a step: (1) Check whether this line matches one of R9's facilities/site-support exclusions or one of R10's five named exclusions (signage, seating/blinds/shutters, security systems, telecoms wiring, carpet fitting). A line item whose description contains a word like "services" is not automatically an R9 exclusion, check what the line actually describes doing, not just its label. (2) If it matches an R9/R10 exclusion, that match is provisional, not final. Before you write down Services, check for a mixed-contract override (R2/R12), but the bar is this: the flagged line's own wording must itself signal it's part of a bigger contract (phrases like "as part of," "part of the wider... contract," or similar). A line with no such wording of its own does not get the override just because other Construction lines happen to sit on the same invoice, one invoice can bundle several unrelated jobs from the same supplier, sharing an invoice is not the same as sharing a contract. When the line's own wording does carry that signal, then treat other clearly-Construction lines on the invoice as corroborating evidence that reinforces the override, not as a substitute for it. If the line's own wording carries the signal, the override wins and the label is Construction, regardless of the R9/R10 match. Only finalize Services once you've checked for that self-referential signal in the line's own wording and found none, and state in the reasoning what you checked. (3) Only fall back to a general inclusion rule like R1 or R3 once you've confirmed, via step (1), that this line isn't an R9/R10 exclusion in the first place.

        For every line item, write your reasoning first, before deciding the label. Work through steps (1) to (3) above in the reasoning itself, then let the label be whatever that reasoning actually concluded, don't decide the label first and write reasoning to match it afterward. Cite the rule id(s) (e.g. "R6", "R11a") that justify your answer. Only cite ids that appear in the rules doc below, never invent one. Keep each reasoning to 1-3 plain English sentences, suitable to show a client.

        === CIS CLASSIFICATION RULES ===
        {rules_text}
        === END OF RULES ===

        """;

    public static string BuildSystemPrompt(string rulesText) =>
        SystemPromptTemplate.Replace("{rules_text}", rulesText);

    // Mirrors build_messages()'s "visible_invoice" reconstruction: only the
    // fields a real invoice would show (invoice_id, supplier, customer,
    // date, and per line line_id/description/amount_gbp). There is no
    // ground truth to strip here, this DTO never had one to begin with.
    public static string BuildUserMessage(InvoiceClassificationRequest request)
    {
        var visibleInvoice = new
        {
            invoice_id = request.InvoiceId,
            supplier = request.Supplier,
            customer = request.Customer,
            date = request.Date.ToString("yyyy-MM-dd"),
            line_items = request.LineItems.Select(li => new
            {
                line_id = li.LineId,
                description = li.Description,
                amount_gbp = li.AmountGbp,
            }),
        };

        var json = JsonSerializer.Serialize(visibleInvoice, new JsonSerializerOptions { WriteIndented = true });
        return "Classify every line item on this invoice:\n\n" + json;
    }
}
