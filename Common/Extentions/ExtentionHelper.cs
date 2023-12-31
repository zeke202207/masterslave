﻿namespace NetX.Common;

public static class ExtentionHelper
{
    public static long DateTimeToUnixTimestamp(this DateTime dateTime)
    {
        var unixTime = dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        return (long)unixTime;
    }

    public static DateTime UnixTimestampToDateTime(this long unixTimestamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTimestamp);
        return dateTime.ToLocalTime();
    }

    public static string AddRandomPort(this string url)
    {
        return url + ":" + Random.Shared.Next(1024, 65535);
    }

    public static int AddRandomPort(this int port)
    {
        return Random.Shared.Next(1024, 65535);
    }
}