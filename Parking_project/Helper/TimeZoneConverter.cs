namespace Parking_project.Helper
{
    public class TimeZoneConverter
    {
        public static DateTime KosovoTimeToUtc(DateTime kosovoDateTime)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var unspecified = DateTime.SpecifyKind(kosovoDateTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
        }

        public static DateTime UtcToKosovoTime(DateTime utcDateTime)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
        }
    }
}
