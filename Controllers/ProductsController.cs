using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleWebApi.Db;
using SampleWebApi.Model;

namespace SampleWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private static readonly HashSet<string> AllowedSort =
        new(StringComparer.OrdinalIgnoreCase) { "name", "price" };

    private readonly AppDbContext _context;
    public ProductsController(AppDbContext context) => _context = context;

    private static string ToEtag(byte[] rowVersion) => $"W/\"{Convert.ToBase64String(rowVersion)}\"";

    private static IQueryable<Product> ApplySort(IQueryable<Product> q, string? sortBy, bool desc)
    {
        var key = AllowedSort.Contains(sortBy ?? "") ? sortBy!.ToLower() : "name";
        return (key, desc) switch
        {
            ("price", true) => q.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            ("price", false) => q.OrderBy(p => p.Price).ThenBy(p => p.Id),
            ("name", true) => q.OrderByDescending(p => p.Name).ThenBy(p => p.Id),
            _ => q.OrderBy(p => p.Name).ThenBy(p => p.Id)
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] ProductQueryParameters query,
        CancellationToken ct)
    {
        var products = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
            products = products.Where(p => p.Name.Contains(query.Search));

        if (query.MinPrice.HasValue)
            products = products.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            products = products.Where(p => p.Price <= query.MaxPrice.Value);

        if (query.InStock.HasValue)
            products = products.Where(p => p.InStock == query.InStock.Value);

        products = ApplySort(products, query.SortBy, query.Desc);

        // Paging (with total count header)
        var total = await products.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var size = Math.Clamp(query.PageSize, 1, 200);

        var items = await products
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.InStock, ToEtag(p.RowVersion)))
            .ToListAsync(ct);

        Response.Headers["X-Total-Count"] = total.ToString();
        Response.Headers["X-Page"] = page.ToString();
        Response.Headers["X-Page-Size"] = size.ToString();

        return Ok(items);
    }

    public async Task<ActionResult<ProductDto>> Head(int id, CancellationToken ct)
        => await GetProduct(id, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id, CancellationToken ct)
    {
        var p = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        var etag = ToEtag(p.RowVersion);

        // 304 support (If-None-Match)
        if (Request.Headers.TryGetValue("If-None-Match", out var inm) && inm.ToString() == etag)
            return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers.ETag = etag;
        return Ok(new ProductDto(p.Id, p.Name, p.Price, p.InStock, etag));
    }



    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto dto,
        CancellationToken ct)
    {
        var entity = new Product { Name = dto.Name, Price = dto.Price, InStock = dto.InStock };
        _context.Products.Add(entity);
        await _context.SaveChangesAsync(ct);

        var etag = ToEtag(entity.RowVersion);
        Response.Headers.ETag = etag;

        var result = new ProductDto(entity.Id, entity.Name, entity.Price, entity.InStock, etag);
        return CreatedAtAction(nameof(GetProduct), new { id = entity.Id }, result);
    }

    // PUT with optimistic concurrency via If-Match ETag header
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var entity = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return NotFound();

        if (!Request.Headers.TryGetValue("If-Match", out var ifMatch))
            return Problem(statusCode: StatusCodes.Status428PreconditionRequired,
                           title: "Missing If-Match header",
                           detail: "Send the ETag from a previous GET in the If-Match header.");

        // Validate ETag
        var currentEtag = ToEtag(entity.RowVersion);
        if (ifMatch.ToString() != currentEtag)
            return StatusCode(StatusCodes.Status412PreconditionFailed,
                              new ProblemDetails { Title = "Precondition Failed", Detail = "ETag does not match current resource state." });

        entity.Name = dto.Name;
        entity.Price = dto.Price;
        entity.InStock = dto.InStock;

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return StatusCode(StatusCodes.Status409Conflict,
                              new ProblemDetails { Title = "Concurrency conflict", Detail = "The resource was modified by another request." });
        }

        Response.Headers.ETag = ToEtag(entity.RowVersion);
        return NoContent();
    }

    // Soft-delete by default; hard-delete with ?hard=true
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id, [FromQuery] bool hard = false, CancellationToken ct = default)
    {
        var entity = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return NotFound();

        if (hard)
        {
            _context.Products.Remove(entity);
        }
        else
        {
            entity.IsDeleted = true;
        }

        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    // Optional: restore soft-deleted
    [HttpPost("{id:int}:restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
    {
        // Disable global filter to find deleted row
        var entity = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return NotFound();
        if (!entity.IsDeleted) return NoContent();

        entity.IsDeleted = false;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
