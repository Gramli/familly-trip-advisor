namespace familly_trip_advisor.Shared
{
    public interface IDateTimeProvider
    {
        DateTime GetDateTime();
        DateOnly GetDateOnly();
    }
    internal sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetDateTime() => DateTime.Now;
        public DateOnly GetDateOnly() => DateOnly.FromDateTime(DateTime.Today);
    }
}
