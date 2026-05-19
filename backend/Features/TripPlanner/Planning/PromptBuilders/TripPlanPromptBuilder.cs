using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Infrastructure.WeatherbitClient;
using System.Text;

namespace familly_trip_advisor.Features.TripPlanner.Planning.Prompts
{
    public interface ITripPlanPromptBuilder
    {
        string BuildTripPlanPrompt(
            TripIntentionDto intention,
            ForecastWeatherDto weather,
            Activity activityType,
            TripPlacesDto places);
    }

    internal sealed class TripPlanPromptBuilder : ITripPlanPromptBuilder
    {
        public string BuildTripPlanPrompt(
            TripIntentionDto intention,
            ForecastWeatherDto weather,
            Activity activityType,
            TripPlacesDto places)
        {
            var sb = new StringBuilder();

            // ── System role ──────────────────────────────────────────────────────────
            sb.AppendLine("You are a family trip planner. Your job is to select the best places from the provided lists and write a short plan summary.");
            sb.AppendLine();

            // ── Trip context ─────────────────────────────────────────────────────────
            sb.AppendLine("## Trip Details");
            sb.AppendLine($"- Destination: {intention.Destination ?? "Home area"}");
            sb.AppendLine($"- Date: {intention.Date:dddd, MMMM d yyyy}");
            sb.AppendLine($"- Activity preference: {activityType}");
            sb.AppendLine($"- Weather: avg {weather.Temp:F1}°C, min {weather.MinTemp:F1}°C, max {weather.MaxTemp:F1}°C, clouds {weather.CloudsPercentage:F0}%, wind {weather.WindSpeed:F1} m/s");
            sb.AppendLine();

            // ── Candidate places ─────────────────────────────────────────────────────
            AppendPlaceList(sb, "Activities", places.Activities
                .Select((p, i) => $"[A{i + 1}] {p.Name} | {p.Category} | {p.ActivityType} | {p.DistanceMeters:F0} m | {p.Address}"));

            AppendPlaceList(sb, "Restaurants", places.Restaurants
                .Select((p, i) => $"[R{i + 1}] {p.Name} | {string.Join(", ", p.Categories.Take(2))} | {p.DistanceMeters:F0} m | {p.Address}"));

            AppendPlaceList(sb, "Parking", places.Parking
                .Select((p, i) => $"[P{i + 1}] {p.Name ?? "Unnamed parking"} | {p.ParkingType} | {p.DistanceMeters:F0} m | {p.Address}"));

            // ── Output rules ─────────────────────────────────────────────────────────
            sb.AppendLine("## Rules");
            sb.AppendLine("- Pick exactly 2 or 3 activities, 2 or 3 restaurants, and 2 or 3 parking spots.");
            sb.AppendLine("- Prefer places CLOSER to the destination (smaller distance is better).");
            sb.AppendLine($"- For activities, prefer {activityType} options that match the weather.");
            sb.AppendLine("- For restaurants, prefer variety in cuisine when possible.");
            sb.AppendLine("- For parking, prefer covered or multi-storey on cloudy/rainy days.");
            sb.AppendLine("- Write a 2-3 sentence plain-English summary of the day plan.");
            sb.AppendLine();

            // ── Required output format ────────────────────────────────────────────────
            sb.AppendLine("## Output format");
            sb.AppendLine("Return ONLY a JSON object. No explanation before or after it.");
            sb.AppendLine("Use the exact IDs from the lists above (e.g. A1, R2, P1).");
            sb.AppendLine();
            sb.AppendLine("```json");
            sb.AppendLine("{");
            sb.AppendLine("  \"activities\": [\"A1\", \"A3\"],");
            sb.AppendLine("  \"restaurants\": [\"R2\", \"R4\"],");
            sb.AppendLine("  \"parking\": [\"P1\", \"P2\"],");
            sb.AppendLine("  \"summary\": \"A short description of the trip plan.\"");
            sb.AppendLine("}");
            sb.AppendLine("```");

            return sb.ToString();
        }

        private static void AppendPlaceList(StringBuilder sb, string title, IEnumerable<string> lines)
        {
            var items = lines.ToList();
            if (items.Count == 0)
            {
                return;
            }

            sb.AppendLine($"## Available {title}");
            foreach (var line in items)
            {
                sb.AppendLine(line);
            }
            sb.AppendLine();
        }
    }
}
