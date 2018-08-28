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
    }
}
