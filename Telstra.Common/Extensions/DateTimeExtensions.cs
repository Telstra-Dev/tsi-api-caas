using System;

namespace Telstra.Common
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset ToDateTimeNoOffset(this DateTime initialDateTime)
        {
            var newDateTimeOffset = new DateTimeOffset(initialDateTime, TimeSpan.Zero);
            return newDateTimeOffset;
        }

        public static DateTimeOffset ToDateTimeNoOffset(this DateTimeOffset initialDateTime)
        {
            var newDateTimeOffset = new DateTimeOffset(initialDateTime.DateTime, TimeSpan.Zero);
            return newDateTimeOffset;
        }

        public static DateTime ToDateTime(this string dt, string format = null)
        {
            if (dt.IsNotNull())
            {
                if (format == null)
                {
                    if (DateTime.TryParse(dt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var _dt)) return _dt;
                    else if (DateTime.TryParseExact(dt, "MM/dd/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var __dt)) return __dt;
                    else
                        throw new NotSupportedException("Invalid Date Time format - " + dt);
                }


                if (DateTime.TryParseExact(dt, format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var output))
                    return output;
            }

            return DateTime.MinValue;
        }

        public static long ToEpoch(this string dt, string format)
        {
            return (long)ToDateTime(dt, format).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long ToEpoch(this DateTime dt)
        {
            return (long)dt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
