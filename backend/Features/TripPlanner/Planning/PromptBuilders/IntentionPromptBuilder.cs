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

            return $$"""
                You are a trip intent extractor. Analyze the user message and return ONLY a valid JSON object — no markdown, no explanation, no code fences.

                Today's date is {{today:yyyy-MM-dd}} ({{today:dddd}}).
                Home location: "{{home.HomeName ?? "Home"}}" at latitude {{home.HomeLatitude}}, longitude {{home.HomeLongitude}}.

                Rules:
                - "date": resolve relative expressions like "Saturday", "Friday", "next weekend" to the nearest upcoming date in yyyy-MM-dd format.
                - "destination": the place name the user mentioned, or null if none.
                - "latitude" and "longitude": GPS coordinates of the destination. If no destination is mentioned, use the home coordinates.
                - "isHomeLocation": true if home coordinates are used, false if a destination was detected.
                - "preferredActivity": if the user expresses a preference for indoor or outdoor activities (e.g. "prefer indoor", "something outside", "stay inside"), set this to "Indoor" or "Outdoor". Otherwise set it to null.

                Return exactly this JSON structure:
                {
                  "date": "yyyy-MM-dd",
                  "destination": "City name or null",
                  "latitude": 0.0,
                  "longitude": 0.0,
                  "isHomeLocation": true,
                  "preferredActivity": "Indoor or Outdoor or null"
                }

                User message: {{userPrompt}}
                """;
        }
    }
}
