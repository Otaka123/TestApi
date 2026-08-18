namespace Application.Helpers
{
    public static class AppDubaiTime
    {
        private static readonly TimeZoneInfo DubaiZone =
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");

        public static DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DubaiZone);

        public static DateOnly Today =>
            DateOnly.FromDateTime(Now);

        public static DateTimeOffset NowOffset =>
            TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, DubaiZone);

        public static DateTime ConvertToDubaiDateTime(DateTime dateTimeUtc)
        {
            var utc = DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, DubaiZone);
        }

        public static DateOnly ConvertToDubaiDateOnly(DateOnly dateOnlyUtc)
        {
            var utc = DateTime.SpecifyKind(dateOnlyUtc.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var dubaiDateTime = TimeZoneInfo.ConvertTimeFromUtc(utc, DubaiZone);
            return DateOnly.FromDateTime(dubaiDateTime);
        }
    }
}
