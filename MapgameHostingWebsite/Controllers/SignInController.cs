using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MapgameHostingWebsite.Controllers
{
    public class SignInController : Controller
    {
        public IActionResult Index(bool signOut)
        {
            if (signOut)
            {
                ViewData["SignOut"] = "true";
            }
            else
            {
                ViewData["SignOut"] = "false";
            }

            return View();
        }

        public IActionResult Hosting()
        {
            return View();
        }
    }
}
