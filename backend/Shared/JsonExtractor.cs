using System.Text.RegularExpressions;

namespace familly_trip_advisor.Shared
{
    public static partial class JsonExtractor
    {
        public static string ExtractJson(string raw)
        {
            var match = JsonBlockRegex().Match(raw);
            return match.Success ? match.Groups[1].Value.Trim() : raw.Trim();
        }

        [GeneratedRegex(@"```(?:json)?\s*([\s\S]*?)```", RegexOptions.IgnoreCase)]
        private static partial Regex JsonBlockRegex();
    }
}
