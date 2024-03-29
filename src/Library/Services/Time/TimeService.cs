﻿using System;

namespace Library.Services.Time
{
    public class TimeService : ITimeService
    {
        public DateTime GetTime() => DateTime.UtcNow;

        public static string GetFormattedDate(DateTime date)
        {
            return string.Format(date.ToString("yyyy-M-dd_HH-mm-ss-FFF"));
        }

        public static long GetUnixTimestampInMilliseconds(DateTimeOffset utcNow)
        {
            return utcNow.ToUnixTimeMilliseconds();
        }
    }
}
