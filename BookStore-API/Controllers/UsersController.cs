using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    public class UsersController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}