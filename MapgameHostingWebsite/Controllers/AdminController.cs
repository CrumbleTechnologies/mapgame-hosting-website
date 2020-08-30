using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Firebase.Database;
using Firebase.Database.Query;
using MapgameHostingWebsite.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MapgameHostingWebsite.Controllers
{
    public class AdminController : Controller
    {
        static private string databaseSecret = Environment.GetEnvironmentVariable("FIREBASE_DATABASE_SECRET");
        static private FirebaseClient firebaseDatabaseClient = new FirebaseClient(
            "https://mapgame-discord-bot.firebaseio.com/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(databaseSecret)
            });

        public readonly IWebHostEnvironment env;

        private const string s3BucketName = "mapgame-hosting-website-bucket";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest2;
        private static IAmazonS3 s3Client;

        public AdminController(IWebHostEnvironment env)
        {
            this.env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Discord(string mapgameID, string page)
        {
            ViewData["MapgameID"] = mapgameID;

            if (page != null)
            {
                switch (page.ToLower())
                {
                    case "nationapplications":
                        IReadOnlyCollection<FirebaseObject<NationApplication>> firebaseNationApplications = await firebaseDatabaseClient.Child("discord-servers").Child(mapgameID).Child("nationApplications").OnceAsync<NationApplication>();

                        Dictionary<string, NationApplication> nationApplicationsDictionary = new Dictionary<string, NationApplication> { };
                        Dictionary<string, string> nationApplicationsMembersDictionary = new Dictionary<string, string> { };
                        Dictionary<string, string> nationApplicationsMapClaimsDictionary = new Dictionary<string, string> { };
                        s3Client = new AmazonS3Client(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), Environment.GetEnvironmentVariable("AWS_SECRET_KEY"), bucketRegion);
                        TransferUtility fileTransferUtility = new TransferUtility(s3Client);
                        for (int i = 0; i < firebaseNationApplications.Count; i++)
                        {
                            nationApplicationsDictionary.Add(firebaseNationApplications.ElementAt(i).Key, firebaseNationApplications.ElementAt(i).Object);

                            WebRequest request2 = WebRequest.Create($"https://discord.com/api/users/{firebaseNationApplications.ElementAt(i).Key}");

                            request2.Headers["Authorization"] = "Bot " + Environment.GetEnvironmentVariable("DISCORD_BOT_SECRET");

                            WebResponse response2 = request2.GetResponse();

                            StreamReader reader2 = new StreamReader(response2.GetResponseStream());

                            string responseText2 = reader2.ReadToEnd();

                            JObject responseJObject2 = JObject.Parse(responseText2);

                            nationApplicationsMembersDictionary.Add(responseJObject2["id"].ToString(), responseJObject2["username"].ToString());

                            Bitmap baseMap = new Bitmap(env.WebRootFileProvider.GetFileInfo("res/images/epic-map.png").CreateReadStream());

                            MapClaim mapClaim = ParseMapCode(firebaseNationApplications.ElementAt(i).Object.MapClaimCode);

                            for (int j = 0; j < mapClaim.Length; j++)
                            {
                                FloodFill(baseMap, new Point(mapClaim.Locations[j].X, mapClaim.Locations[j].Y), Color.White, mapClaim.Colours[j]);
                            }

                            Bitmap changedMap = new Bitmap(baseMap);

                            string generatedFilename = GenerateImageFileName(env);

                            MemoryStream ms = new MemoryStream();
                            changedMap.Save(ms, ImageFormat.Png);

                            fileTransferUtility.Upload(ms, s3BucketName, generatedFilename + ".png");

                            GetPreSignedUrlRequest urlRequest = new GetPreSignedUrlRequest
                            {
                                BucketName = s3BucketName,
                                Key = generatedFilename + ".png",
                                Expires = DateTime.Now.AddMinutes(5),
                            };
                            string urlString = s3Client.GetPreSignedURL(urlRequest);

                            nationApplicationsMapClaimsDictionary.Add(firebaseNationApplications.ElementAt(i).Key, urlString);
                        }
                        ViewData["NationApplications"] = nationApplicationsDictionary;
                        ViewData["NationApplicationsMembers"] = nationApplicationsMembersDictionary;
                        ViewData["NationApplicationsMapClaims"] = nationApplicationsMapClaimsDictionary;
                        ViewData["NationApplicationFields"] = await firebaseDatabaseClient.Child("discord-servers").Child(mapgameID).Child("config").Child("listOfFieldsForRegistration").OnceSingleAsync<string[]>();

                        return View("Discord/NationApplications");

                    default:
                        break;
                }
            }

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

        #region ImageProcessingMethods
        private static bool ColorMatch(Color a, Color b)
        {
            return (a.ToArgb() & 0xffffff) == (b.ToArgb() & 0xffffff);
        }

        private static void FloodFill(Bitmap bmp, Point pt, Color targetColor, Color replacementColor)
        {
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(pt);
            while (q.Count > 0)
            {
                Point n = q.Dequeue();
                if (!ColorMatch(bmp.GetPixel(n.X, n.Y), targetColor))
                    continue;
                Point w = n, e = new Point(n.X + 1, n.Y);
                while ((w.X >= 0) && ColorMatch(bmp.GetPixel(w.X, w.Y), targetColor))
                {
                    bmp.SetPixel(w.X, w.Y, replacementColor);
                    if ((w.Y > 0) && ColorMatch(bmp.GetPixel(w.X, w.Y - 1), targetColor))
                        q.Enqueue(new Point(w.X, w.Y - 1));
                    if ((w.Y < bmp.Height - 1) && ColorMatch(bmp.GetPixel(w.X, w.Y + 1), targetColor))
                        q.Enqueue(new Point(w.X, w.Y + 1));
                    w.X--;
                }
                while ((e.X <= bmp.Width - 1) && ColorMatch(bmp.GetPixel(e.X, e.Y), targetColor))
                {
                    bmp.SetPixel(e.X, e.Y, replacementColor);
                    if ((e.Y > 0) && ColorMatch(bmp.GetPixel(e.X, e.Y - 1), targetColor))
                        q.Enqueue(new Point(e.X, e.Y - 1));
                    if ((e.Y < bmp.Height - 1) && ColorMatch(bmp.GetPixel(e.X, e.Y + 1), targetColor))
                        q.Enqueue(new Point(e.X, e.Y + 1));
                    e.X++;
                }
            }
        }

        private static MapClaim ParseMapCode(string code)
        {
            string[] codeList1 = code.Split(",");
            List<string[]> codeList2 = new List<string[]>();
            foreach (var code1 in codeList1)
            {
                codeList2.Add(code1.Split("="));
            }
            List<List<int[]>> codeList3 = new List<List<int[]>>();
            List<Color> colours = new List<Color>();
            List<Location> locations = new List<Location>();
            foreach (var code1 in codeList2)
            {
                Color colour = ColorTranslator.FromHtml(code1[1]);
                int[] colourIntArray = new int[] { colour.R, colour.G, colour.B };
                codeList3.Add(new List<int[]> { new int[] { int.Parse(code1[0].Split(".")[0]), int.Parse(code1[0].Split(".")[1]) }, colourIntArray });

                colours.Add(Color.FromArgb(colourIntArray[0], colourIntArray[1], colourIntArray[2]));
                locations.Add(new Location(int.Parse(code1[0].Split(".")[0]), int.Parse(code1[0].Split(".")[1])));
            }

            MapClaim mapClaim = new MapClaim(colours.ToArray(), locations.ToArray());

            return mapClaim;
        }
        #endregion

        private static string GenerateImageFileName(IWebHostEnvironment env)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 20;
            string filename = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            if (!System.IO.File.Exists(env.WebRootPath + "/res/images/" + filename + ".png")) {
                return filename;
            } else
            {
                return GenerateImageFileName(env);
            }
        }
    }
}
