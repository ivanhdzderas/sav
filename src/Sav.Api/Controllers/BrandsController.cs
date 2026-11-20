using Microsoft.AspNetCore.Mvc;
using Sav.Application.Brands;

namespace Sav.Api.Controllers;

[ApiController]
[Route("api/brands")]
public sealed class BrandsController(IBrandService brands) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<BrandDto>> List([FromQuery] string? search, CancellationToken ct) =>
        brands.ListAsync(search, ct);

    [HttpGet("{id:int}")]
    public Task<BrandDto> Get(int id, CancellationToken ct) => brands.GetAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<BrandDto>> Create(SaveBrandRequest request, CancellationToken ct)
    {
        var created = await brands.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public Task<BrandDto> Update(int id, SaveBrandRequest request, CancellationToken ct) =>
        brands.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await brands.DeactivateAsync(id, ct);
        return NoContent();
    }
}
