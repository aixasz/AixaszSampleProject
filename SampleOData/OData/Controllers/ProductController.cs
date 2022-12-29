using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using WebApi;

namespace WebApi.Controllers;

public class ProductController : ODataController
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    [EnableQuery(PageSize = 10)]
    public ActionResult<IQueryable<Product>> Get()
        => Ok(_context.Products);

    [HttpGet]
    [EnableQuery]
    public async Task<ActionResult<Product?>> Get([FromODataUri] Guid key)
        => Ok(await _context.Products.FindAsync(key));

    [HttpPost]
    [EnableQuery]
    public async Task<ActionResult<Product>> Post([FromBody] Product entity)
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    [HttpPatch]
    [EnableQuery]
    public async Task<ActionResult<Product>> Patch([FromODataUri] Guid key, Delta<Product> updatedEntity)
    {
        var product = await _context.Products.FindAsync(key);
        if (product is null) return NotFound();

        updatedEntity.Patch(product);
        await _context.SaveChangesAsync();
        return product;
    }

    [HttpDelete]
    [EnableQuery]
    public async Task<ActionResult> Delete([FromODataUri] Guid key)
    {
        await _context.Products.Where(x => x.Id == key).ExecuteDeleteAsync();
        return NoContent();
    }
}