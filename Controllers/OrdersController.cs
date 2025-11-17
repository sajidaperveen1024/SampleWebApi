using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        public OrdersController() { }

        public int OrdersCount { get; set; }    


    }
}
