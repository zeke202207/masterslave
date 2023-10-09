namespace NetX.Common;

public static class ExtentionHelper
{
    public static long DateTimeToUnixTimestamp(this DateTime dateTime)
    {
        var unixTime = dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        return (long)unixTime;
    }

    public static DateTime UnixTimestampToDateTime(this long unixTimestamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimestamp);
        return dateTime.ToLocalTime();
    }

    public static string AddRandomPort(this string url)
    {
        Random random = new Random();
        int port = random.Next(1024, 65535);
        return url + ":" + port;
    }
}