using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;

namespace MapgameHostingWebsite.Controllers
{
    public class PlayerActionsController : Controller
    {
        static private string databaseSecret = Environment.GetEnvironmentVariable("FIREBASE_DATABASE_SECRET");
        static private FirebaseClient firebaseDatabaseClient = new FirebaseClient(
            "https://mapgame-discord-bot.firebaseio.com/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(databaseSecret)
            });

        public IActionResult MakeMapClaim(string mapgameID, string nationID, string checkKey)
        {
            ViewData["mapgameID"] = mapgameID;
            ViewData["nationID"] = nationID;
            ViewData["checkKey"] = checkKey;

            try
            {
                string[] detailsFromDatabase = firebaseDatabaseClient.Child("discord-check-keys").Child(nationID.ToString()).Child("map-claim").OnceSingleAsync<string>().Result.Split("|");

                if (detailsFromDatabase[0] == mapgameID.ToString() && detailsFromDatabase[1] == checkKey)
                {
                    return View();
                }
                else
                {
                    return View("Error");
                }
            }
            catch
            {
                return View("Error");
            }
        }

        /*[HttpPost]
        public async Task<IActionResult> MakeMapClaimPOST(string mapgameID, string nationID, string checkKey)
        {

        }*/
    }
}
