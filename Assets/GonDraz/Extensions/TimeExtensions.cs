using System;
using System.Collections.Generic;
using System.Linq;

namespace GonDraz.Extensions
{
    public static class TimeExtensions
    {
        public static string FormatTimeMmssColon(this float time, int maxElements = 2)
        {
            return ConvertToFormattedTime(time, maxElements);
        }

        public static string FormatTimeMmssColon(this long time, int maxElements = 2)
        {
            return ConvertToFormattedTime(time, maxElements);
        }

        public static string FormatTimeMmssColon(this int time, int maxElements = 2)
        {
            return ConvertToFormattedTime(time, maxElements);
        }

        private static string ConvertToFormattedTime(double time, int maxElements)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            return timeSpan.ConvertTimeToString(maxElements);
        }


        public static string ConvertTimeToString(this TimeSpan time, int maxElements = 2)
        {
            time = TimeSpan.FromTicks(Math.Abs(time.Ticks));
            var parts = new List<string>();

            if (time.Days > 0) parts.Add($"{time.Days}d");
            if (time.Hours > 0) parts.Add($"{(parts.Count > 0 ? time.Hours.ToString("00") : time.Hours)}h");
            if (time.Minutes > 0) parts.Add($"{(parts.Count > 0 ? time.Minutes.ToString("00") : time.Minutes)}m");
            if (time.Seconds > 0) parts.Add($"{(parts.Count > 0 ? time.Seconds.ToString("00") : time.Seconds)}s");

            return string.Join(" ", parts.Take(maxElements));
        }
    }
}