﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NASB_Parser_To_xNode
{
    class Utils
    {
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
            else if (line.StartsWith("protected"))
            {
                return AccessabilityLevel.Protected;
            }
            else
            {
                throw new Exception("Uknown class type!");
            }
        }
    }
}