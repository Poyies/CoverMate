using Ganss.Xss;

namespace CoverMate.Helpers
{
    public static class HtmlSanitizerHelper
    {
        // Create an instance of the HtmlSanitizer.
        private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        /// <summary>
        /// Sanitizes the provided HTML or text input.
        /// </summary>
        /// <param name="input">The raw input string.</param>
        /// <returns>A sanitized version of the input string.</returns>
        public static string SanitizeHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // You can customize allowed tags or attributes if needed:
            // _sanitizer.AllowedTags.Add("iframe"); // Example

            return _sanitizer.Sanitize(input);
        }
    }
}
