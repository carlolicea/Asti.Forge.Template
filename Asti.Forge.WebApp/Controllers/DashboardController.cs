using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asti.Forge.WebApp.Controllers
{
    public class DashboardController : Controller
    {
        [Route("Dashboard")]
        [Route("Dashboard/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}