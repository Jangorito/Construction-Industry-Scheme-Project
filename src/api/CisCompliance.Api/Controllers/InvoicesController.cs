using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace CisCompliance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _repository;

    public InvoicesController(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StoredInvoice>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _repository.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<ActionResult<StoredInvoice>> GetById(string id, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPatch("{invoiceId}/lines/{lineId}/override")]
    public async Task<ActionResult<StoredInvoice>> OverrideLine(
        string invoiceId, string lineId, [FromBody] OverrideLineRequest body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Reason))
        {
            return BadRequest("A reason is required to override a line's classification.");
        }

        var updated = await _repository.OverrideLineAsync(invoiceId, lineId, body.NewLabel, body.Reason.Trim(), cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
