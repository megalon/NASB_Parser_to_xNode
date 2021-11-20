using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NASB_Parser_To_xNode
{
    class Utils
    {
        public static void RecurseThroughParentNamespaces(NASBParserFile nasbParserFile)
        {
            if (nasbParserFile.parentClass == null || nasbParserFile.parentClass.Equals(string.Empty)) return;
            if (nasbParserFile.parentClass.Equals("ISerializable")) return;
            if (Path.GetDirectoryName(nasbParserFile.relativePath).Equals(string.Empty)) return;

            xNodeTextGenerator.AddToFileContents($"using static NASB_Parser.{Path.GetDirectoryName(nasbParserFile.relativePath)}.{nasbParserFile.parentClass};");
            if (Program.nasbParserFiles.Any(x => x.className.Equals(nasbParserFile.parentClass)))
            {
                RecurseThroughParentNamespaces(Program.nasbParserFiles.Find(x => x.className.Equals(nasbParserFile.parentClass)));
            }
        }

        public static string GetRelativeNamespace(NASBParserFile nasbParserFile)
        {
            return $"NASB_Parser.{Path.GetDirectoryName(nasbParserFile.relativePath)}.{nasbParserFile.className}";
        }

        public static string GetStringBetweenStrings(string line, string start, string end)
        {
            string segment = null;

            if (!line.Contains(start))
            {
                throw new Exception($"Start string \"{start}\" not found in \"{line}\"!");
            }

            segment = line.Substring(line.IndexOf(start) + start.Length);

            if (!segment.Contains(end))
            {
                throw new Exception($"End string \"{end}\" not found after \"{start}\" in \"{segment}\"!");
            }

            return segment.Substring(0, segment.IndexOf(end));
        }

        public static AccessabilityLevel GetAccessabilityLevel(string line)
        {
            line = line.Trim();
            if (line.StartsWith("public "))
            {
                return AccessabilityLevel.Public;
            }
            else if (line.StartsWith("private "))
            {
                return AccessabilityLevel.Private;
            }
            else if (line.StartsWith("protected "))
            {
                return AccessabilityLevel.Protected;
            }
            else
            {
                throw new Exception("Uknown class type!");
            }
        }

        public static string GetAccessabilityLevelString(AccessabilityLevel accessabilityLevel)
        {
            switch (accessabilityLevel)
            {
                case AccessabilityLevel.Public:
                    return "public";
                case AccessabilityLevel.Protected:
                    return "protected";
                case AccessabilityLevel.Private:
                    return "private";
            }
            throw new Exception("Invalid AccessabilityLevel!");
        }
    }
}
