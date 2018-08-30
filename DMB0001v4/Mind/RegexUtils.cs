using System.Text.RegularExpressions;

namespace DMB0001v4.Mind
{
    /// <summary>
    /// Keeps all methods of Regex Patterns matching.
    /// </summary>
    public static class RegexUtils
    {
        /// <summary>
        /// Converts given wildcard to regular .* and .
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns>regular wildcard pattern</returns>
        public static string RegularPattern(string phrase) 
            => string.Format("^{0}$", Regex.Escape(phrase).Replace("\\*", ".*").Replace("\\?", "."));
    }
}