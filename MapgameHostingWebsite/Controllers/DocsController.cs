using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MapgameHostingWebsite.Controllers
{
    public class DocsController : Controller
    {
        public IActionResult Discord()
        {
            return View();
        }
    }
}
