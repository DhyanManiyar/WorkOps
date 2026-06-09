using System;
using System.Globalization;

namespace WorkOps.Helpers
{
    public static class DateTimeHelper
    {
        // Convert to Indian Standard Time (IST)
        public static DateTime ToIST(this DateTime dateTime)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), istZone);
        }

        // Get current IST time
        public static DateTime NowIST()
        {
            return DateTime.UtcNow.ToIST();
        }

        // Format date as dd/MM/yyyy
        public static string ToDateString(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy");
        }

        // Format date as dd/MM/yyyy hh:mm tt
        public static string ToDateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy hh:mm tt");
        }

        // Format time as hh:mm tt
        public static string ToTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("hh:mm tt");
        }

        // Get start of day
        public static DateTime StartOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        // Get end of day
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
        }

        // Calculate age from date of birth
        public static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        // Get month name
        public static string GetMonthName(int month)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        // Get day name
        public static string GetDayName(DateTime date)
        {
            return date.ToString("dddd");
        }

        // Check if date is weekend
        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        // Calculate working days between two dates (excluding weekends)
        public static int GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            int workingDays = 0;
            DateTime currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (!IsWeekend(currentDate))
                {
                    workingDays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return workingDays;
        }

        // Get first day of current month
        public static DateTime FirstDayOfMonth()
        {
            var today = DateTime.Today;
            return new DateTime(today.Year, today.Month, 1);
        }

        // Get last day of current month
        public static DateTime LastDayOfMonth()
        {
            var today = DateTime.Today;
            return new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        }

        // Parse date string (dd/MM/yyyy)
        public static DateTime? ParseDate(string dateString)
        {
            if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return null;
        }
    }
}