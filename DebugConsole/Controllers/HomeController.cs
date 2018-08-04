using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UChainDB.Example.Chain.DebugConsole.Models;

namespace UChainDB.Example.Chain.DebugConsole.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
