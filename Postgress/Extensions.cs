using System.Web;

namespace Postgress
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        public static string BuildQueryString(
            this Dictionary<string, string> collection,
            string keySeparator = "&",
            string valueSeparator = "=")
        {
            var keyValues = collection
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value)).Select(
                    kvp => $"{kvp.Key}{valueSeparator}{HttpUtility.JavaScriptStringEncode(kvp.Value)}");

            var queryString = string.Join(keySeparator, keyValues);

            return string.IsNullOrWhiteSpace(queryString) ? string.Empty : $"?{queryString}";
        }
    }
}
