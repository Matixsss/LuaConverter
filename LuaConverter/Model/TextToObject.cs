using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LuaConverter.Model
{
    static class TextToObject
    {
        public static string OneLineString(ref string text, int indexAtEqual)
        {
            var startIndex = indexAtEqual+1;
            var endIndex = text.IndexOf("\r\n", startIndex);
            string unrefined = text.Substring(startIndex, endIndex - startIndex).Trim();
            string refined = Regex.Replace(unrefined, "\",*", "");
            return refined;
        }

        public static string MultiLineString(ref string text, int indexAtEqual)
        {
            var startIndex = text.IndexOf('{',indexAtEqual + 1)+1;
            var endIndex = text.IndexOf("},", startIndex);
            Regex regex = new Regex("[^\r\n]");
            var newStartIndex = regex.Match(text, startIndex).Index;
            string unrefined = string.Join("\r\n", text.Substring(newStartIndex, endIndex - newStartIndex).Split("\r\n").Select(s=> s.Trim()));
            string refined = Regex.Replace(unrefined, "\",*", "").TrimEnd();
            return refined;
        }
    }
}
