using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Exceptions;
using CisCompliance.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CisCompliance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassifyController : ControllerBase
{
    private readonly IClassificationService _classificationService;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<ClassifyController> _logger;

    public ClassifyController(
        IClassificationService classificationService, IInvoiceRepository invoiceRepository, ILogger<ClassifyController> logger)
    {
        _classificationService = classificationService;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceClassificationResult>> Classify(
        [FromBody] InvoiceClassificationRequest request, CancellationToken cancellationToken)
    {
        if (request.LineItems.Count == 0)
        {
            return BadRequest("The invoice must contain at least one line item.");
        }

        try
        {
            var result = await _classificationService.ClassifyAsync(request, cancellationToken);

            // No try/catch around this: a save failure surfaces as an
            // unhandled 500 rather than a silently-lost record. The real
            // OpenAI call above already happened (and cost real tokens) by
            // this point, swallowing a persistence failure here would mean
            // the caller has no way to know the result was never recorded.
            await _invoiceRepository.SaveAsync(request, result, cancellationToken);

            return Ok(result);
        }
        catch (ClassificationModelException ex)
        {
            _logger.LogError(ex, "Classification failed for invoice {InvoiceId}", request.InvoiceId);
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway, title: "Classification model call failed");
        }
    }
}
