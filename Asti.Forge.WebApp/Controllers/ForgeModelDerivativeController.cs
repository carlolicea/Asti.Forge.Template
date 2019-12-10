using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Asti.Forge.WebApp.Controllers
{
    public class ForgeModelDerivativeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}