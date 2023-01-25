

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

        private const string south = "30.3182";
        private const string north = "30.3369";
        private const string west = "59.7959";
        private const string east = "59.8094";

        static HttpClient? httpClient;

        static async Task Main(string[] args)
        {
            Log("Hello World!");

            httpClient = InitializeHttpClient();

            var parameters = new Dictionary<string, string>
            {
                { "sw", $"{south},{west}" },
                { "ne", $"{north},{east}" },
                { "z", "15" }
            };
            var query = parameters.BuildQueryString();

            var request = new HttpRequestMessage(Get, $"{Endpoints.InView}{query}");
            var result = await SendAsync<InViewResponse>(request);

            if (result is { Status: Messages.Success, Data: { } } && result.Data.Any())
            {
                var portals = result.Data.ToList();

                Log($"Portals found: {portals.Count}. Press 'Enter' to hack/deploy, 'H' to hack only, other keys to exit");
                Log($"Friendly: {portals.Count(x => x.Team == UserTeam)}");
                Log($"Neutral: {portals.Count(x => x.Team == Team.None)}");
                Log($"Enemy: {portals.Count(x => x.Team != UserTeam && x.Team != Team.None)}");

                var key = Console.ReadKey();
                if (key.Key is ConsoleKey.Enter or ConsoleKey.H)
                {
                    Log("\r\nProcessing...\r\n");
                    await ProcessPortals(result.Data.ToList(), key.Key == ConsoleKey.H);
                }
                else
                {
                    Log("\r\nFinished!");
                    return;
                }

            }
        }

        private static async Task<IEnumerable<PortalHackResult>> ProcessPortals(List<Portal> portals, bool hackOnly)
        {
            var portalHackResult = new List<PortalHackResult>(portals.Count);


            foreach (var portal in portals)
            {
                if (!hackOnly)
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
                        Log($"Enemy portal. Can't deploy!");
                    }
                }

                await HackPortal(portal, portalHackResult);
                
                // sleep 1s to reduce system load
                Thread.Sleep(500);
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
                Log($"Can't get Portal data. Portal ID: [{portal.ID}]. {result?.Error}");
                return;
            }

            var freeSlots = MaxResonators - result.Data.Slots.Count();
            if (freeSlots > 0)
            {
                Log($"Deploying '{result.Data.Title}' [{portal.ID}]");

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

            Log("Can't get inventory!");
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
                    Log($"Portal [{portal.ID}] slot ({slot}) deployed");
                    
                    levelResonators.Amount--;
                    freeSlots--;
                }
                else
                {
                    Log($"Portal [{portal.ID}] slot ({slot}) deployment failed! {result?.Error}");
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

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}

