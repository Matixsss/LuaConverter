using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LuaConverter.Model
{
    static class LineParser
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
            string[] unrefined = text.Substring(newStartIndex, endIndex - newStartIndex).Split("\r\n").Select(s => s.Trim()).ToArray();
            for (int i = 0; i< unrefined.Length;i++)
            {
                if(!Regex.Match(unrefined[i], "^--.*").Success)
                {
                    unrefined[i] = Regex.Replace(unrefined[i], "^\"", "");
                    unrefined[i] = Regex.Replace(unrefined[i], "\",*;*$", "");
                }
                else
                {
                    unrefined[i] = Regex.Replace(unrefined[i], "\",*;*", "");
                    Trace.WriteLine("Naprawa komentarza "+unrefined[i]);
                }
            }
            string refined = string.Join("\r\n", unrefined).TrimEnd();
            return refined;
        }
    }
}
