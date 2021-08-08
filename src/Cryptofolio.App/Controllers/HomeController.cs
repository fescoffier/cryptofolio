using Cryptofolio.App.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;

namespace Cryptofolio.App.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/me")]
        public object Me([FromServices] IOptionsSnapshot<ApiOptions> optionsSnapshot, [FromServices] IAntiforgery antiforgery)
        {
            var antiforgeryTokenSet = antiforgery.GetAndStoreTokens(HttpContext);
            return new
            {
                user = new
                {
                    id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    emai = User.FindFirstValue(ClaimTypes.Email)
                },
                api = optionsSnapshot.Value,
                antiforgery = new
                {
                    formFieldName = antiforgeryTokenSet.FormFieldName,
                    token = antiforgeryTokenSet.RequestToken
                }
            };
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
