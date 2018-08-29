using System;

namespace DMB0001v4.Mind
{
    public static class TimeUtils
    {
        /// <summary>
        /// Returns current timestamp.
        /// </summary>
        /// <returns>current timestamp</returns>
        public static string Now() => DateTime.Now.ToString("yyyyMMddHHmmssffff");

        /// <summary>
        /// Returns Date of Birth of Bot.
        /// </summary>
        /// <returns>moment of DOB</returns>
        public static DateTime DOB() => new DateTime(2018, 08, 28, 09, 24, 00);

        /// <summary>
        /// Counts difference in days between givne dates.
        /// </summary>
        /// <param name="earlier">earlier date</param>
        /// <param name="later">later date</param>
        /// <returns>number of days between</returns>
        public static int DaysBetween(DateTime earlier, DateTime later) => (int)(Math.Ceiling(earlier.Subtract(later).TotalDays));

        /// <summary>
        /// Returns current age of bot in days.
        /// </summary>
        /// <returns></returns>
        public static int HowOldInDays() => DaysBetween(DOB(), DateTime.Now);
    }
}
