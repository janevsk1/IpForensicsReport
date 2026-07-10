using Microsoft.AspNetCore.Mvc;

namespace IpForensicsReport.Api.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
