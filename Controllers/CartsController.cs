using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        public IActionResult Index()
        {
            return Ok("CartsController is working!");
        }

        public Task<IActionResult> GetCartItems(int cartId)
        {
            // Logic to retrieve cart items by cartId
            return Task.FromResult<IActionResult>(Ok(new { CartId = cartId, Items = new string[] { "Item1", "Item2" } }));
        }   

    }
}
