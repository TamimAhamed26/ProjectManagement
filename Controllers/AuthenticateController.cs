using Microsoft.AspNetCore.Mvc;

namespace ProjectManagement.Controllers
{
    public class AuthenticateController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
