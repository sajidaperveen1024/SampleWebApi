using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        public Task<IActionResult> GetPromotions()
        {
            return Task.FromResult<IActionResult>(Ok(new[] { "Promotion1", "Promotion2" }));
        }

    }
}
