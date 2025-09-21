using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleWebApi.Db;
using SampleWebApi.Model;

namespace SampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] ProductQueryParameters queryParams)
        {
            var productsQuery = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(queryParams.Name))
                productsQuery = productsQuery.Where(p => p.Name.Contains(queryParams.Name));

            if (queryParams.MinPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= queryParams.MinPrice.Value);

            if (queryParams.MaxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= queryParams.MaxPrice.Value);

            productsQuery = queryParams.SortBy?.ToLower() switch
            {
                "price" => queryParams.Desc
                    ? productsQuery.OrderByDescending(p => p.Price)
                    : productsQuery.OrderBy(p => p.Price),
                _ => queryParams.Desc
                    ? productsQuery.OrderByDescending(p => p.Name)
                    : productsQuery.OrderBy(p => p.Name)
            };


            var items = await productsQuery
        .Skip((queryParams.Page - 1) * queryParams.PageSize)
        .Take(queryParams.PageSize)
        .ToListAsync();

            return Ok(items);

            //return await _context.Products.ToListAsync();
        }
        [HttpGet("{id}")]

        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }






    }
}
