using Ardalis.GuardClauses;
using familly_trip_advisor.Features.TripPlanner.Models;
using familly_trip_advisor.Shared;
using Microsoft.Extensions.Options;

namespace familly_trip_advisor.Features.TripPlanner.Planning.Prompts
{
    public interface IIntentionPromptBuilder
    {
        string BuildIntentionPrompt(string userPrompt);
    }

    internal sealed class IntentionPromptBuilder : IIntentionPromptBuilder
    {
        private readonly IOptions<TripPlannerOptions> _options;
        private readonly IDateTimeProvider _dateTimeProvider;

        public IntentionPromptBuilder(IOptions<TripPlannerOptions> options, IDateTimeProvider dateTimeProvider)
        {
            _options = Guard.Against.Null(options);
            _dateTimeProvider = Guard.Against.Null(dateTimeProvider);
        }

        public string BuildIntentionPrompt(string userPrompt)
        {
            var home = _options.Value;
            var today = _dateTimeProvider.GetDateOnly();

            var systemPrompt = "You are a trip intent extractor. Analyze the user message and return ONLY a valid JSON object — no markdown, no explanation, no code fences.\n\n" +
                $"Today's date is {today:yyyy-MM-dd} ({today:dddd}).\n" +
                $"Home location: \"{home.HomeName ?? "Home"}\" at latitude {home.HomeLatitude}, longitude {home.HomeLongitude}.\n\n" +
                "Rules:\n" +
                "- \"date\": resolve relative expressions like \"Saturday\", \"Friday\", \"next weekend\" to the nearest upcoming date in yyyy-MM-dd format.\n" +
                "- \"destination\": the place name the user mentioned, or null if none.\n" +
                "- \"latitude\" and \"longitude\": GPS coordinates of the destination. If no destination is mentioned, use the home coordinates.\n" +
                "- \"isHomeLocation\": true if home coordinates are used, false if a destination was detected.\n" +
                "- \"preferredActivity\": if the user expresses a preference for indoor or outdoor activities (e.g. \"prefer indoor\", \"something outside\", \"stay inside\"), set this to \"Indoor\" or \"Outdoor\". Otherwise set it to null.\n\n" +
                "Return exactly this JSON structure:\n" +
                "{\n" +
                "  \"date\": \"yyyy-MM-dd\",\n" +
                "  \"destination\": \"City name or null\",\n" +
                "  \"latitude\": 0.0,\n" +
                "  \"longitude\": 0.0,\n" +
                "  \"isHomeLocation\": true,\n" +
                "  \"preferredActivity\": \"Indoor or Outdoor or null\"\n" +
                "}";

            var fullPrompt = $"{systemPrompt}\n\nUser message: {userPrompt}";

            return fullPrompt;
        }
    }
}
