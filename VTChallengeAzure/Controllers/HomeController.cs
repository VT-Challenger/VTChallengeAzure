using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VTChallengeAzure.Filters;
using VTChallengeAzure.Models;

namespace VTChallengeAzure.Controllers {
    public class HomeController : Controller {

        [AuthorizeUsers]
        public IActionResult Index() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}