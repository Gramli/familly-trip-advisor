using FluentResults;

namespace familly_trip_advisor.Shared
{
    public static class ResultExtensions
    {
        public static string ToErrorString(this ResultBase result) =>
            string.Join(", ", result.Errors);
    }
}
