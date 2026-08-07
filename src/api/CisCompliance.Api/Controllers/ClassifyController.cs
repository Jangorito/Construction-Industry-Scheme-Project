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
    private readonly ILogger<ClassifyController> _logger;

    public ClassifyController(IClassificationService classificationService, ILogger<ClassifyController> logger)
    {
        _classificationService = classificationService;
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
            return Ok(await _classificationService.ClassifyAsync(request, cancellationToken));
        }
        catch (ClassificationModelException ex)
        {
            _logger.LogError(ex, "Classification failed for invoice {InvoiceId}", request.InvoiceId);
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway, title: "Classification model call failed");
        }
    }
}
