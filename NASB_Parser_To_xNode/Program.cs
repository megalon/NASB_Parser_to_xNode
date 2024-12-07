using System;
using System.Collections.Generic;
using System.IO;

namespace NASB_Parser_To_xNode
{
    class Program
    {
        public static List<NASBParserFile> nasbParserFiles;

        static void Main(string[] args)
        {
            string mainPath = "C:/Users/megalon/main/megalon-github/NASB-Moveset-Parser/Parser";
            string outputPath = "C:/Users/megalon/main/UnityProjects/NASB Moveset Editor/Assets/Editor/NASB Moveset Editor/Scripts/Nodes";

            LoadAllNASBClasses(mainPath);
            WriteAllXNodeFiles(outputPath);
        }

        private static void WriteAllXNodeFiles(string outputPath)
        {
            foreach (NASBParserFile nasbParserFile in nasbParserFiles)
            {
                if (Consts.classesToIgnore.Contains(nasbParserFile.className)) continue;
                if (Consts.enumOnlyFiles.Contains(nasbParserFile.className)) continue;

                string fileText = xNodeTextGenerator.GenerateXNodeFileText(nasbParserFile, false);
                xNodeFileGenerator.GenerateXNodeFile(fileText, outputPath, nasbParserFile.relativePath);

                foreach (NASBParserFile nestedFile in nasbParserFile.nestedClasses)
                {
                    if (Consts.classesToIgnore.Contains(nestedFile.className)) continue;
                    if (Consts.enumOnlyFiles.Contains(nasbParserFile.className)) continue;

                    fileText = xNodeTextGenerator.GenerateXNodeFileText(nestedFile, true);
                    xNodeFileGenerator.GenerateXNodeFile(fileText, outputPath, nestedFile.relativePath);
                }
            }
        }

        private static void LoadAllNASBClasses(string mainPath)
        {
            nasbParserFiles = new List<NASBParserFile>();
            foreach (NASBParserFolder folder in Consts.folders)
            {
                string inputDir = Path.Combine(mainPath, folder.folderName);
                var filePaths = Directory.GetFiles(inputDir);
                foreach (string filePath in filePaths)
                {
                    nasbParserFiles.Add(NASBParserFileLoader.LoadNASBParserFile(filePath, mainPath));
                }
            }

            foreach (string filePath in Directory.GetFiles(mainPath))
            {
                if (Consts.looseFiles.Contains(Path.GetFileNameWithoutExtension(filePath)))
                {
                    nasbParserFiles.Add(NASBParserFileLoader.LoadNASBParserFile(filePath, mainPath));
                }
            }
        }
    }
}
