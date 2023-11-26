using Microsoft.AspNetCore.Mvc;

namespace KonyveloIroda.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int? id)
        {
            return View("News" + id);
        }
    }
}
