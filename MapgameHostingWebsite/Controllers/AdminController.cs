using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MapgameHostingWebsite.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Discord(string mapgameID)
        {
            WebRequest request = WebRequest.Create($"https://discord.com/api/guilds/{mapgameID}");

            Console.WriteLine(Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET"));
            request.Headers["Authorization"] = "Bot " + Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET");

            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());

            string responseText = reader.ReadToEnd();

            JObject responseJObject = JObject.Parse(responseText);

            ViewData["DiscordServerName"] = responseJObject["name"];

            return View();
        }

        public IActionResult Error(string errorMessage)
        {
            ViewData["ErrorMessage"] = errorMessage;

            return View();
        }
    }
}
