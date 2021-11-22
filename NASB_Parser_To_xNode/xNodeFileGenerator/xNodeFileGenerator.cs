using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NASB_Parser_To_xNode
{
    public static class xNodeFileGenerator
    {
        public static void GenerateXNodeFile(string fileText, string outputDir, string relativePath)
        {
            if (relativePath.EndsWith(".cs"))
            {
                relativePath = relativePath.Substring(0, relativePath.IndexOf(".cs"));
            }

            var path = Path.Combine(outputDir, $"{relativePath}Node.cs");
            var dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if(File.Exists(path)) {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (string line in fileText.Split('\n'))
                {
                    sw.WriteLine(line);
                }
            }
        }
    }
}
