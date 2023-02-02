

using System.Globalization;

namespace Postgress
{
    using System.Net.Http.Headers;
    using System.Text;

    using Entities;

    using Newtonsoft.Json;

    using static Constants;
    using static Constants.Inventory;
    using static System.Net.Http.HttpMethod;

    public class PortalDeployResult
    {
        /// <summary>
        /// Portal ID
        /// </summary>
        public string PortalID { get; set; }

        public string PortalName { get; set; }

        public string PortalOwner { get; set; }

        public string Status { get; set; }

        public byte DeployedResonators { get; set; }

        /// <summary>
        /// Hacked Successfully?
        /// </summary>
        public bool Success { get; set; }
    }

    public class PortalHackResult
    {
        /// <summary>
        /// Portal ID
        /// </summary>
        public string PortalID { get; set; }

        /// <summary>
        /// To track a cooldown later "Cooldown is active. 90 seconds to go"
        /// </summary>
        public DateTime HackedAt { get; set; }

        /// <summary>
        /// Hacked Successfully?
        /// </summary>
        public bool Success { get; set; }
    }

    internal class Program
    {
        private const Team UserTeam = Team.Green;

        static HttpClient? httpClient;

        static async Task Main(string[] args)
        {
            Log("Hello stranger!");

            Log("Input initial geolocation.\r\nLatitude/longitude format: XX.XXXX\r\nZoom format: 0-4");

            Log("Latitude: ");
            var ltString = Console.ReadLine();
            if (!double.TryParse(ltString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var lt))
            {
                throw new Exception("Wrong latitude data!");
            }

            Log("Longitude: ");
            var lnString = Console.ReadLine();
            if (!double.TryParse(lnString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var ln))
            {
                throw new Exception("Wrong longitude data!");
            }

            Log("Zoom: ");
            var zoom = int.Parse(Console.ReadLine() ?? "0");

            while (true)
            {
                Log("\r\nType 'MS', 'MN', 'MW', 'ME' to move 1 tile south, north, west and east");
                Log("Type 'W' to watch, 'H' to hack, 'D' to deploy, 'R' to recharge. For example 'DHR' means deploy, hack and charge");
                Log("Type 'Z+' or 'Z-' to zoom in or out\r\n");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (Moves.Contains(input))
                {
                    Log($"\r\nLatitude: {lt}, Longitude: {ln}");
                    Move(input, ref lt, ref ln, zoom);
                    Log($"Latitude: {lt}, Longitude: {ln}\r\n");
                }
                else if (input is "Z+" or "Z-")
                {
                    Zoom(input, ref zoom);
                }
                else if (input.Contains((char)Commands.Watch) 
                         || input.Contains((char)Commands.Deploy) 
                         || input.Contains((char)Commands.Hack) 
                         || input.Contains((char)Commands.Recharge))
                {
                    var result = await ProcessCommand(input, lt, ln, zoom);
                    Log(result ? "[Success]\r\n" : "[Failure]\r\n", result ? ConsoleColor.Green : ConsoleColor.Red);
                }
                else if (string.Equals("exit", input, StringComparison.OrdinalIgnoreCase))
                {
                    Log("\r\nGoodbye!\r\n", ConsoleColor.Cyan);
                    break;
                }
                else
                {
                    Log("\r\nInvalid command, try again!\r\n", ConsoleColor.Red);
                    continue;
                }
            }
        }

        private static async Task<bool> ProcessCommand(string input, double lt, double ln, int zoom)
        {
            var shift = TileSize * Math.Pow(Base, zoom) / 20000; 

            var s = GetStringCoordinate(lt - shift);
            var n = GetStringCoordinate(lt + shift);
            var w = GetStringCoordinate(ln - shift);
            var e = GetStringCoordinate(ln + shift);
            
            var parameters = new Dictionary<string, string>
            {
                { "sw", $"{w},{s}" },
                { "ne", $"{e},{n}" },
                { "z", "15" }
            };

            var query = parameters.BuildQueryString();

            httpClient = InitializeHttpClient();
            var request = new HttpRequestMessage(Get, $"{Endpoints.InView}{query}");
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", Token);
            var result = await SendAsync<InViewResponse>(request);

            if (result is { Status: Messages.Success, Data: { } })
            {
                var portals = result.Data.ToList();
                if (!portals.Any())
                {
                    Log("[WARNING] No portals found on this tile", ConsoleColor.Yellow);
                    return true;
                }

                Log($"\r\nPortals found: {portals.Count}");
                Log($"Friendly: {portals.Count(x => x.Team == UserTeam)}", ConsoleColor.DarkGreen);
                Log($"Neutral: {portals.Count(x => x.Team == Team.None)}", ConsoleColor.DarkGray);
                Log($"Enemy: {portals.Count(x => x.Team != UserTeam && x.Team != Team.None)}\r\n", ConsoleColor.DarkRed);

                if (input == "W")
                {
                    return true;
                }

                Log("\r\nProcessing...\r\n");
                var processResult = await ProcessPortals(result.Data.ToList(), input);
                return true;
            }

            return false;
        }


        private static string GetStringCoordinate(double input) =>
            Math.Round(input, 4).ToString(CultureInfo.InvariantCulture).Replace(',', '.');

        private static async Task<IEnumerable<PortalHackResult>> ProcessPortals(List<Portal> portals, string command)
        {
            var portalHackResult = new List<PortalHackResult>(portals.Count);


            foreach (var portal in portals)
            {
                // Deploy
                if (command.Contains((char)Commands.Deploy))
                {
                    if (portal.Team == UserTeam || portal.Team == Team.None)
                    {
                        var inventory = await GetInventory();
                        Thread.Sleep(250);
                        Log($"Inventory items: {inventory.Count}");
                        await DeployPortal(portal, inventory);
                        Thread.Sleep(250);
                    }
                    else
                    {
                        Log($"{portal.ID} Enemy portal. Can't deploy!", ConsoleColor.DarkRed);
                    }
                }

                // Hack
                if (command.Contains((char)Commands.Hack))
                {
                    await HackPortal(portal, portalHackResult);
                    Thread.Sleep(250);
                }

                // Recharge
                if (command.Contains((char)Commands.Recharge))
                {
                    if (portal.Team == UserTeam)
                    {
                        await RechargePortal(portal);
                    }
                    else
                    {
                        Log($"[{portal.ID}] Enemy or neutral portal. Can't recharge!", ConsoleColor.DarkRed);
                    }
                }
            }

            return portalHackResult;
        }

        private static async Task HackPortal(Portal portal, List<PortalHackResult> portalHackResult)
        {
            var json = JsonConvert.SerializeObject(new { guid = portal.ID }, Formatting.None);

            var request = new HttpRequestMessage(Post, $"{Endpoints.Discover}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var result = await SendAsync<HackResponse>(request);

            if (result is { Status: Messages.Success, Loot: { }, Error: null })
            {
                Log($"Portal [{portal.ID}]: Hacked! Looted {result.Loot.Count()} items!");
                portalHackResult.Add(new PortalHackResult { HackedAt = DateTime.Now, PortalID = portal.ID, Success = true });
            }
            else
            {
                Log($"Portal [{portal.ID}]: {result.Error}");
                portalHackResult.Add(new PortalHackResult { PortalID = portal.ID, Success = false });
            }
        }

        private static async Task DeployPortal(Portal portal, List<Entities.Inventory> inventory)
        {
            var parameters = new Dictionary<string, string>
            {
                { "guid", portal.ID }
            };
            var query = parameters.BuildQueryString();
            var request = new HttpRequestMessage(Get, $"{Endpoints.Point}{query}");
            var result = await SendAsync<PortalResponse>(request);

            if (result?.Data == null || result.Status != Messages.Success || !string.IsNullOrWhiteSpace(result.Error))
            {
                Log($"Can't get Portal data. Portal ID: [{portal.ID}]. {result?.Error}", ConsoleColor.Red);
                return;
            }

            var freeSlots = MaxResonators - result.Data.Slots.Count();
            if (freeSlots > 0)
            {
                Log($"Deploying '{result.Data.Title}' [{portal.ID}]", ConsoleColor.Gray);

                var deployResult = await DeployPortal(result.Data, inventory, freeSlots);

                Log(deployResult);
            }
        }

        private static async Task<List<Entities.Inventory>> GetInventory()
        {
            var request = new HttpRequestMessage(Get, $"{Endpoints.Inventory}");
            var result = await SendAsync<InventoryResponse>(request);

            if (result is { Status: Messages.Success, Inventory: { }, Error: null })
            {
                return result.Inventory;
            }

            Log("Can't get inventory!", ConsoleColor.Red);
            return null;
        }

        private static async Task<string> DeployPortal(PortalData portal, List<Entities.Inventory> inventory, int availableSlots)
        {
            var freeSlots = availableSlots;

            while (freeSlots > 0)
            {
                var core = GetHighestLevelResonator(inventory, out var levelResonators);

                var json = JsonConvert.SerializeObject(new { guid = portal.ID, core = core }, Formatting.None);
                var request = new HttpRequestMessage(Post, $"{Endpoints.Deploy}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await SendAsync<PortalResponse>(request);
                var slot = 1 + availableSlots - freeSlots;

                if (result is { Status: Messages.Success, Data: { }, Error: null })
                {
                    Log($"Portal [{portal.ID}] slot ({slot}) deployed", ConsoleColor.DarkGreen);
                    
                    levelResonators.Amount--;
                    freeSlots--;
                }
                else
                {
                    Log($"Portal [{portal.ID}] slot ({slot}) deployment failed! {result?.Error}", ConsoleColor.Red);
                    throw new Exception(result?.Error);
                }
            }

            return $"Portal `{portal.Title}` [{portal.ID}] deployed with {availableSlots} resonators";
        }

        private static string GetHighestLevelResonator(List<Entities.Inventory> inventory, out Entities.Inventory levelResonators)
        {


            foreach (var level in Levels)
            {
                levelResonators = inventory.FirstOrDefault(x => x.Type == 1 && x.LevelOrLink == level);

                if (levelResonators != null && levelResonators.Amount > 0)
                {
                    return levelResonators.ID;
                }

            }

            const string noResonatorsLeft = "[ERROR] No resonators left!";
            Log(noResonatorsLeft);
            throw new Exception(noResonatorsLeft);
        }

        private static async Task RechargePortal(Portal portal)
        {
            if (portal.Energy == 1d)
            {
                Log($"[{portal.ID}]: {Messages.FullyCharged}", ConsoleColor.DarkGreen);
                return;
            }


            while (true)
            {
                var json = JsonConvert.SerializeObject(new { guid = portal.ID }, Formatting.None);

                var request = new HttpRequestMessage(Post, $"{Endpoints.Repair}")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var result = await SendAsync<RepairResponse>(request);

                Thread.Sleep(250);

                if (result is { Status: Messages.Success, Error: Messages.FullyCharged })
                {
                    Log($"[{portal.ID}]: {Messages.FullyCharged}", ConsoleColor.DarkGreen);
                    break;
                }
            }
            
        }

        private static async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            httpClient ??= InitializeHttpClient();

            using var response = await httpClient.SendAsync(request);
            var data = response.Content.ReadAsStringAsync().Result;

            var result = JsonConvert.DeserializeObject<T>(data);
            return result;
        }

        private static HttpClient InitializeHttpClient()
        {
            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            };

            var newHttpClient = new HttpClient(socketsHandler);
            newHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Token);
            newHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            return newHttpClient;
        }

        private static void Move(string command, ref double latitude, ref double longitude, int zoom)
        {
            var move = Math.Round(Math.Pow(2, zoom) * TileSize / 10000, 4);
            switch (command)
            {
                case MoveSouth:
                {
                    Log("Moving south to", ConsoleColor.DarkCyan);
                    latitude = Math.Round(latitude - move, 4);
                    break;
                }
                case MoveNorth:
                {
                    Log("Moving north to", ConsoleColor.DarkCyan);
                    latitude = Math.Round(latitude + move, 4);
                    break;
                }
                case MoveEast:
                {
                    Log("Moving east to ", ConsoleColor.DarkCyan);
                    longitude = Math.Round(longitude + move, 4);
                    break;
                }
                case MoveWest:
                {
                    Log("Moving west to", ConsoleColor.DarkCyan);
                    longitude = Math.Round(longitude - move, 4);
                    break;
                }
                // TODO: borders for 90/180 or - ?
            }
        }

        private static void Zoom(string command, ref int zoom)
        {
            switch (command)
            {
                case "Z+":
                {
                    if (zoom < 4)
                    {
                        zoom++;
                    }

                    break;
                }
                case "Z-":
                {
                    if (zoom > 0)
                    {
                        zoom--;
                    }

                    break;
                }
            }

            Log($"Zoom level: {zoom}", ConsoleColor.Cyan);
        }

        private static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            if (color != ConsoleColor.White)
            {
                Console.ForegroundColor = color;
            }
            
            Console.WriteLine(message);

            if (color != ConsoleColor.White)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}

