using System;
using System.Globalization;

namespace HotelBilling.Shared.Helpers
{
    public static class DateHelper
    {
        private const string DefaultDateFormat = "dd-MM-yyyy";
        private const string DefaultDateTimeFormat = "dd-MM-yyyy HH:mm:ss";

        /// <summary>
        /// Format date to dd-MM-yyyy
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            return date.ToString(DefaultDateFormat);
        }

        /// <summary>
        /// Format date and time to dd-MM-yyyy HH:mm:ss
        /// </summary>
        public static string FormatDateTime(DateTime date)
        {
            return date.ToString(DefaultDateTimeFormat);
        }

        /// <summary>
        /// Convert string to DateTime safely
        /// </summary>
        public static DateTime? ParseDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Convert string to DateTime using specific format
        /// </summary>
        public static DateTime? ParseExactDate(string dateString, string format = DefaultDateFormat)
        {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Get current UTC time
        /// </summary>
        public static DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Get current local time
        /// </summary>
        public static DateTime GetNow()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Calculate difference in days between two dates
        /// </summary>
        public static int GetDaysDifference(DateTime startDate, DateTime endDate)
        {
            return (endDate - startDate).Days;
        }
    }
}