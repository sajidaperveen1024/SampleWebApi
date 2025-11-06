using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers
{
    public class SaleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
