using Cryptofolio.App.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Cryptofolio.App.Controllers
{
    public class HomeController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
