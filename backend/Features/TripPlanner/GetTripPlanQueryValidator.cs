using familly_trip_advisor.Features.TripPlanner.Models;
using FluentResults;
using System.Text.RegularExpressions;

namespace familly_trip_advisor.Features.TripPlanner
{
    public interface IGetTripPlanQueryValidator
    {
        public Result Validate(CreateTripPlanCommand query);
    }

    internal sealed class GetTripPlanQueryValidator : IGetTripPlanQueryValidator
    {
        private const int MinLength = 10;
        private const int MaxLength = 500;

        private static readonly Regex AllowedCharactersRegex =
            new(@"^[\p{L}\p{N}\s.,!?'\-():@#]+$", RegexOptions.Compiled);

        private static readonly string[] PromptInjectionPatterns =
        [
            "ignore previous instructions",
            "ignore above instructions",
            "disregard previous",
            "forget your instructions",
            "you are now",
            "act as",
            "pretend you are",
            "roleplay as",
            "system prompt",
            "jailbreak",
            "dan mode",
            "developer mode",
            "do anything now",
            "bypass",
            "override instructions",
        ];

        private static readonly string[] OffTopicPatterns =
        [
            "how to hack",
            "how to make a bomb",
            "how to kill",
            "suicide",
            "drugs",
            "illegal",
            "weapon",
            "pornograph",
            "explicit content",
            "nsfw",
        ];

        public Result Validate(CreateTripPlanCommand query)
        {
            if (string.IsNullOrWhiteSpace(query.Prompt))
            {
                return Result.Fail("Prompt must not be empty.");
            }

            var trimmed = query.Prompt.Trim();

            if (trimmed.Length < MinLength)
            {
                return Result.Fail($"Prompt must be at least {MinLength} characters long.");
            }

            if (trimmed.Length > MaxLength)
            {
                return Result.Fail($"Prompt must not exceed {MaxLength} characters.");
            }

            if (!AllowedCharactersRegex.IsMatch(trimmed))
            {
                return Result.Fail("Prompt contains invalid characters.");
            }

            foreach (var pattern in PromptInjectionPatterns)
            {
                if (trimmed.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Fail("Prompt contains content that cannot be processed.");
                }
            }

            foreach (var pattern in OffTopicPatterns)
            {
                if (trimmed.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Fail("Prompt contains content that is not related to trip planning.");
                }
            }

            return Result.Ok();
        }
    }
}
