using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
