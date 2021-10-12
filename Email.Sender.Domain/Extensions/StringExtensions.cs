using System.Linq;

namespace Email.Sender.Domain.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveWhiteSpaces(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return new string(value.ToCharArray()
                                   .Where(c => !char.IsWhiteSpace(c))
                                   .ToArray());
        }
    }
}