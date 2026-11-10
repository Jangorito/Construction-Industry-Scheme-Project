using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Exceptions;
using CisCompliance.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CisCompliance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtractController : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "application/pdf", "image/png", "image/jpeg" };

    // Comfortably under Kestrel's unconfigured ~28.6MB default (no infra
    // config change needed) and generous for a realistic scanned invoice.
    private const long MaxFileSizeBytes = 15 * 1024 * 1024;

    private readonly IInvoiceExtractionService _extractionService;
    private readonly ILogger<ExtractController> _logger;

    public ExtractController(IInvoiceExtractionService extractionService, ILogger<ExtractController> logger)
    {
        _extractionService = extractionService;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<InvoiceClassificationRequest>> Extract(
        [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A file is required.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest($"Unsupported content type '{file.ContentType}'. Only PDF, PNG or JPEG files are accepted.");
        }

        await using var stream = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);

        try
        {
            var result = await _extractionService.ExtractAsync(buffer.ToArray(), file.ContentType, file.FileName, cancellationToken);
            return Ok(result);
        }
        catch (InvoiceExtractionException ex)
        {
            _logger.LogError(ex, "Extraction failed for uploaded file {FileName}", file.FileName);
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway, title: "Extraction model call failed");
        }
    }
}
