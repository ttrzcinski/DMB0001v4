namespace DMB0001v4.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checkes, if string has some content (not null and not only white spaces).
        /// </summary>
        /// <param name="input">string to check</param>
        /// <returns>true means has content, false otherwise</returns>
        public static bool HasContent(this string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }
    }
}
