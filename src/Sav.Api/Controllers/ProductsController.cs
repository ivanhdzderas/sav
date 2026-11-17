using Microsoft.AspNetCore.Mvc;
using Sav.Application.Common;
using Sav.Application.Products;

namespace Sav.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<ProductDto>> Search(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        products.SearchAsync(search, page, pageSize, ct);

    [HttpGet("{id:int}")]
    public Task<ProductDto> Get(int id, CancellationToken ct) => products.GetAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(SaveProductRequest request, CancellationToken ct)
    {
        var created = await products.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public Task<ProductDto> Update(int id, SaveProductRequest request, CancellationToken ct) =>
        products.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await products.DeactivateAsync(id, ct);
        return NoContent();
    }
}
