namespace XFramework.Core.Abstractions.Utility
{
    public static class StringFormatter
    {
        /// <summary>
        /// Retrieves a substring, or add padding behind.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string SubStringOrPadding(string text, int length, char padding = ' ')
        {
            // If the length is 0, return an empty string
            if (length == 0) return string.Empty;

            if (text == null) text = string.Empty;

            text = text.Trim();

            // If the length are equal, than return it directly
            if (text.Length == length) return text;

            // If the source is too long, return a substring
            if (text.Length > length) return text.Substring(0, length);

            // Return the source plus paddings
            return string.Concat(text, new string(padding, length - text.Length));
        }
    }
}
