using Newtonsoft.Json;

namespace Postgress
{
    using System.Net.Http.Headers;
    using System.Text;

    using Entities;

    using static Constants;

    public class PortalDeployResult
    {
        /// <summary>
        /// Portal ID
        /// </summary>
        public string PortalID { get; set; }


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
        static HttpClient? httpClient;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            httpClient = InitializeHttpClient();

            var parameters = new Dictionary<string, string>
            {
                { "sw", "30.30817483185179,59.83046254218334" },
                { "ne", "30.340156000339253,59.853583185887544" },
                { "z", "15" }
            };
            var query = parameters.BuildQueryString();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{Endpoints.InView}{query}");

            using var response = await httpClient.SendAsync(request);
            var data = response.Content.ReadAsStringAsync().Result;

            var result = JsonConvert.DeserializeObject<InViewResponse>(data);

            if (result is { Status: Messages.Success, Data: { } } && result.Data.Any())
            {
                Console.WriteLine($"Portals found: {result.Data.Count()}. Press 'Enter' to hack, other keys to exit");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("\r\nHacking...\r\n");
                    await ProcessPortals(result.Data);
                }
                else
                {
                    Console.WriteLine("\r\nFinished!");
                    return;
                }

            }
        }

        private static async Task ProcessPortals(IEnumerable<Portal> data)
        {
            var portals = data.ToList();
            
            var portalHackResult = await HackPortals(portals);

            await DeployPortals(portals);
        }

        private static async Task DeployPortals(List<Portal> portals)
        {
            
        }

        private static async Task<IEnumerable<PortalHackResult>> HackPortals(List<Portal> portals)
        {
            var portalHackResult = new List<PortalHackResult>(portals.Capacity);

            foreach (var portal in portals)
            {
                var json = JsonConvert.SerializeObject(new { guid = portal.ID }, Formatting.None);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{Endpoints.Discover}")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                httpClient ??= InitializeHttpClient();
                var response = await httpClient.SendAsync(request);
                var data = response.Content.ReadAsStringAsync().Result;

                var result = JsonConvert.DeserializeObject<HackResponse>(data);

                if (result is { Status: Messages.Success, Loot: { }, Error: null })
                {
                    Console.WriteLine($"Portal [{portal.ID}]: Hacked! Looted {result.Loot.Count()} items!");
                    portalHackResult.Add(new PortalHackResult { HackedAt = DateTime.Now, PortalID = portal.ID, Success = true });
                }
                else
                {
                    Console.WriteLine($"Portal [{portal.ID}]: {result.Error}");
                    portalHackResult.Add(new PortalHackResult { PortalID = portal.ID, Success = false });
                }

            }

            return portalHackResult;
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
    }
}

