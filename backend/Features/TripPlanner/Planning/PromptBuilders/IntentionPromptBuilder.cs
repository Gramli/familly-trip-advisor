using Ardalis.GuardClauses;
using familly_trip_advisor.Features.TripPlanner.Places;
using familly_trip_advisor.Shared;

namespace familly_trip_advisor.Features.TripPlanner.Planning.Prompts
{
    public interface IIntentionPromptBuilder
    {
        string BuildIntentionPrompt(string userPrompt);
    }

    internal sealed class IntentionPromptBuilder : IIntentionPromptBuilder
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private static readonly string AvailableCategories =
            string.Join(", ", PlaceCategoryActivityMap.TopLevelCategories);

        public IntentionPromptBuilder(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = Guard.Against.Null(dateTimeProvider);
        }

        public string BuildIntentionPrompt(string userPrompt)
        {
            var today = _dateTimeProvider.GetDateOnly();

            return $$"""
                You are a trip intent extractor. Analyze the user message and return ONLY a valid JSON object — no markdown, no explanation, no code fences.

                Today's date is {{today:yyyy-MM-dd}} ({{today:dddd}}).

                Rules:
                - "date": resolve relative expressions like "Saturday", "Friday", "next weekend" to the nearest upcoming date in yyyy-MM-dd format.
                - "destination": the place name the user mentioned, or null if none.
                - "preferredActivity": if the user expresses a preference for indoor or outdoor activities (e.g. "prefer indoor", "something outside", "stay inside"), set this to "Indoor" or "Outdoor". Otherwise set it to null.
                - "categories": pick zero or more values from this exact list that match what the user wants to do: {{AvailableCategories}}. Return as a JSON array of strings. If the user does not mention any specific activity or interest, return null.
                - "preferences": short phrase (≤12 words): who is going and their key interests. Null if none. Example: "Family with kids, science museums, casual dining."

                Return exactly this JSON structure:
                {
                  "date": "yyyy-MM-dd",
                  "destination": "City name or null",
                  "preferredActivity": "Indoor or Outdoor or null",
                  "categories": ["category1", "category2"] or null,
                  "preferences": "Family with kids, science museums, casual dining." or null
                }

                User message: {{userPrompt}}
                """;
        }
    }
}
