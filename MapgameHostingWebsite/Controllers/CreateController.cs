using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MapgameHostingWebsite.Controllers
{
    public class CreateController : Controller
    {
        static private string databaseSecret = Environment.GetEnvironmentVariable("FIREBASE_DATABASE_SECRET");
        static private FirebaseClient firebaseDatabaseClient = new FirebaseClient(
            "https://mapgame-discord-bot.firebaseio.com/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(databaseSecret)
            });

        public IActionResult New(string type = null)
        {
            if (type != null)
            {
                type = type.ToLower();

                switch (type)
                {
                    case "discord":
                        return View("New/Discord");

                    case "new":
                        return View("New/Web");

                    default:
                        break;
                }
            }

            return View("New/Index");
        }

        public IActionResult DiscordServerSetup(string guildID, string userID, string checkKey, string errorMessage)
        {
            ViewData["guildID"] = guildID;
            ViewData["userID"] = userID;
            ViewData["checkKey"] = checkKey;

            try
            {
                string[] detailsFromDatabase = firebaseDatabaseClient.Child("discord-check-keys").Child(userID.ToString()).Child("create-guild").OnceSingleAsync<string>().Result.Split("|");

                if (detailsFromDatabase[0] == guildID.ToString() && detailsFromDatabase[1] == checkKey)
                {
                    WebRequest request = WebRequest.Create($"https://discord.com/api/guilds/{guildID}/channels");

                    request.Headers["Authorization"] = "Bot " + Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET");

                    WebResponse response = request.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    string responseText = reader.ReadToEnd();

                    JArray responseJArray = JArray.Parse(responseText);

                    List<string[]> channels = new List<string[]>();
                    foreach (var channel in responseJArray)
                    {
                        if (channel["type"].ToString() == "0")
                        {
                            channels.Add(new string[] { channel["name"].ToString(), channel["id"].ToString() });
                        }
                    }

                    ViewData["DiscordTextChannels"] = channels;

                    WebRequest request1 = WebRequest.Create($"https://discord.com/api/guilds/{guildID}/roles");

                    request1.Headers["Authorization"] = "Bot " + Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET");

                    WebResponse response1 = request1.GetResponse();

                    StreamReader reader1 = new StreamReader(response1.GetResponseStream());

                    string responseText1 = reader1.ReadToEnd();

                    JArray responseJArray1 = JArray.Parse(responseText1);

                    List<string[]> roles = new List<string[]>();
                    foreach (var role in responseJArray1)
                    {
                        roles.Add(new string[] { role["name"].ToString(), role["id"].ToString() });
                    }

                    ViewData["DiscordRoles"] = roles;

                    WebRequest request2 = WebRequest.Create($"https://discord.com/api/guilds/{guildID}/channels");

                    request2.Headers["Authorization"] = "Bot " + Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET");

                    WebResponse response2 = request2.GetResponse();

                    StreamReader reader2 = new StreamReader(response2.GetResponseStream());

                    string responseText2 = reader2.ReadToEnd();

                    JArray responseJArray2 = JArray.Parse(responseText2);

                    List<string[]> channels2 = new List<string[]>();
                    foreach (var channel in responseJArray)
                    {
                        if (channel["type"].ToString() == "4")
                        {
                            channels2.Add(new string[] { channel["name"].ToString(), channel["id"].ToString() });
                        }
                    }

                    ViewData["DiscordCategoryChannels"] = channels2;

                    if (errorMessage != null)
                    {
                        switch (errorMessage)
                        {
                            case "invalidwelcomechannel":
                                ViewData["ErrorMessage"] = "Error: Invalid channel to welcome new members in.";
                                break;

                            case "invalidwelcomerole":
                                ViewData["ErrorMessage"] = "Error: Invalid role to give to new members.";
                                break;

                            case "invalidapplicationchannel":
                                ViewData["ErrorMessage"] = "Error: Invalid channel to send nation applications to.";
                                break;

                            case "invalidmodrole":
                                ViewData["ErrorMessage"] = "Error: Invalid role required to accept/deny nation applications.";
                                break;

                            case "invalidnationchannelcategory":
                                ViewData["ErrorMessage"] = "Error: Invalid category to add nation channels to.";
                                break;

                            case "invalidnumberoftiles":
                                ViewData["ErrorMessage"] = "Error: Invalid number of tiles to grant each nation a day (number must be positive).";
                                break;

                            case "firebaseuploaderror":
                                ViewData["ErrorMessage"] = "Firebase Error: Unable to upload server setup to the database. Please try again.";
                                break;

                            default:
                                break;
                        }
                    }

                    return View();
                }
                else
                {
                    return View("Error/InvalidDiscordRedirect");
                }
            }
            catch
            {
                return View("Error/InvalidDiscordRedirect");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiscordServerPOST(string guildID, string userID, string checkKey)
        {
            IFormCollection formData = HttpContext.Request.Form;

            string[] detailsFromDatabase = firebaseDatabaseClient.Child("discord-check-keys").Child(userID.ToString()).Child("create-guild").OnceSingleAsync<string>().Result.Split("|");

            if (!(detailsFromDatabase[0] == guildID && detailsFromDatabase[1] == checkKey))
            {
                return View("Error/InvalidDiscordRedirect");
            }

            #region MainDiscordAPIRequests
            WebRequest request = WebRequest.Create($"https://discord.com/api/guilds/{guildID}/channels");

            request.Headers["Authorization"] = "Bot " + Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET");

            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());

            string responseText = reader.ReadToEnd();

            JArray responseJArray = JArray.Parse(responseText);

            List<string> validTextChannels = new List<string>();
            foreach (var channel in responseJArray)
            {
                if (channel["type"].ToString() == "0")
                {
                    validTextChannels.Add(channel["id"].ToString());
                }
            }

            WebRequest request1 = WebRequest.Create($"https://discord.com/api/guilds/{guildID}/roles");

            request1.Headers["Authorization"] = "Bot " + Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET");

            WebResponse response1 = request1.GetResponse();

            StreamReader reader1 = new StreamReader(response1.GetResponseStream());

            string responseText1 = reader1.ReadToEnd();

            JArray responseJArray1 = JArray.Parse(responseText1);

            List<string> validRoles = new List<string>();
            foreach (var role in responseJArray1)
            {
                validRoles.Add(role["id"].ToString());
            }

            WebRequest request2 = WebRequest.Create($"https://discord.com/api/guilds/{guildID}/channels");

            request2.Headers["Authorization"] = "Bot " + Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET");

            WebResponse response2 = request2.GetResponse();

            StreamReader reader2 = new StreamReader(response2.GetResponseStream());

            string responseText2 = reader2.ReadToEnd();

            JArray responseJArray2 = JArray.Parse(responseText2);

            List<string> validCategoryChannels = new List<string>();
            foreach (var channel in responseJArray)
            {
                if (channel["type"].ToString() == "4")
                {
                    validCategoryChannels.Add(channel["id"].ToString());
                }
            }
            #endregion

            #region FormSanitisation
            if (!validTextChannels.Contains(formData["welcome-channel-input"]))
            {
                return Redirect($"/Create/DiscordServerSetup?errorMessage=invalidwelcomechannel&guildID={guildID}&userID={userID}&checkKey={checkKey}");
            }
            if (!validRoles.Contains(formData["welcome-role-input"]))
            {
                return Redirect($"/Create/DiscordServerSetup?errorMessage=invalidwelcomerole&guildID={guildID}&userID={userID}&checkKey={checkKey}");
            }
            if (!validRoles.Contains(formData["mod-role-input"]))
            {
                return Redirect($"/Create/DiscordServerSetup?errorMessage=invalidmodrole&guildID={guildID}&userID={userID}&checkKey={checkKey}");
            }
            // templates are validated by the form itself
            if (!validCategoryChannels.Contains(formData["nation-channel-category-input"]))
            {
                return Redirect($"/Create/DiscordServerSetup?errorMessage=invalidnationchannelcategory&guildID={guildID}&userID={userID}&checkKey={checkKey}");
            }
            if (formData["custom-irl-select"] == "custom" && int.Parse(formData["number-of-tiles-input"].ToString()) < 0)
            {
                return Redirect($"/Create/DiscordServerSetup?errorMessage=invalidnumberoftiles&guildID={guildID}&userID={userID}&checkKey={checkKey}");
            }
            #endregion

            List<string> listOfFieldsForRegistration = new List<string>();
            for (int i = 0; i < formData.Count - 8; i++)
            {
                listOfFieldsForRegistration.Add(formData["nation-form-" + i.ToString() + "-input"]);
            }

            try
            {
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("autoRoleRoleID").PutAsync<string>(formData["welcome-role-input"]);
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("categoryToAddNationChannelsToID").PutAsync<string>(formData["nation-channel-category-input"]);
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("channelTemplate").PutAsync<string>(formData["channel-template-input"]);
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("customOrIrlNation").PutAsync<string>(formData["custom-irl-select"]);
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("listOfFieldsForRegistration").PutAsync(listOfFieldsForRegistration.ToArray());
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("nicknameTemplate").PutAsync<string>(formData["nickname-template-input"]);
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("numberOfTilesToClaimEachDay").PutAsync<string>(formData["number-of-tiles-input"]);
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("roleRequiredToProcessApplicationsID").PutAsync<string>(formData["mod-role-input"]);
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("setupComplete").PutAsync<string>("yes");
                await firebaseDatabaseClient.Child("discord-servers").Child(guildID).Child("config").Child("welcomeChannelID").PutAsync<string>(formData["welcome-channel-input"]);
            }
            catch
            {
                return Redirect($"/Create/DiscordServerSetup?errorMessage=firebaseuploaderror&guildID={guildID}&userID={userID}&checkKey={checkKey}");
            }

            await firebaseDatabaseClient.Child("discord-check-keys").Child(userID.ToString()).Child("create-guild").DeleteAsync();

            #region Create Firebase User for Mapgame
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(Environment.GetEnvironmentVariable("FIREBASE_JSON_CREDENTIAL_1") + Environment.GetEnvironmentVariable("FIREBASE_JSON_CREDENTIAL_2")),
            });

            string generatedPassword = "none";
            try
            {
                await FirebaseAuth.DefaultInstance.GetUserByEmailAsync($"{guildID}@example.com");
            }
            catch
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                int length = 10;
                generatedPassword = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
                UserRecordArgs newUserArgs = new UserRecordArgs()
                {
                    Email = $"{guildID}@example.com",
                    Password = generatedPassword,
                };
                UserRecord user = await FirebaseAuth.DefaultInstance.CreateUserAsync(newUserArgs);
                var newUserClaims = new Dictionary<string, object>()
                {
                    { "mapgameType", "discord" },
                };
                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.Uid, newUserClaims);
            }
            #endregion

            return Redirect($"/Create/MapgameCreated?type=discord&genPass={generatedPassword}&mapgameID={guildID}");
        }

        public IActionResult MapgameCreated(string type, string genPass, string mapgameID)
        {
            ViewData["MapgameType"] = type;
            ViewData["GeneratedPassword"] = genPass;
            ViewData["MapgameID"] = mapgameID;

            return View();
        }
    }
}
