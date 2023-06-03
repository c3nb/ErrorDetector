namespace Overlayer.Core.Utils
{
    public static class StringUtils
    {
        public static string GetBetween(this string str, string start, string end)
        {
            int sIdx = str.IndexOf(start);
            int eIdx = str.LastIndexOf(end);
            if (sIdx < 0 || eIdx < 0) return null;
            return str.Substring(sIdx + 1, eIdx - sIdx - 1);
        }
        public static string[] Split2(this string str, char separator)
        {
            int index = str.IndexOf(separator);
            if (index < 0) return new string[] { str };
            return new string[] { str.Substring(0, index), str.Substring(index + 1, str.Length - (index + 1)) };
        }
        public static string GetAfter(this string str, string after)
        {
            int index = str.IndexOf(after);
            if (index < 0) return null;
            return str.Substring(index + 1, str.Length - index - 1);
        }
        public static string GetBefore(this string str, string before)
        {
            int index = str.IndexOf(before);
            if (index < 0) return null;
            return str.Substring(0, index);
        }
    }
}