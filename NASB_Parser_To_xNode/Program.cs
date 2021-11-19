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
            Consts.GenerateClassToTypeIdDict();

            string mainPath = "D:/megalon-github/NASB/NASB_Parser/NASB_Parser/";
            string outputPath = "D:/UnityProjects/NASB-Character-NodeEditor/Assets/NASB Moveset Editor/Editor Scripts/Nodes";

            LoadAllNASBClasses(mainPath);
            WriteAllXNodeFiles(outputPath);
        }

        private static void WriteAllXNodeFiles(string outputPath)
        {
            foreach (NASBParserFile file in nasbParserFiles)
            {
                string fileText = xNodeTextGenerator.GenerateXNodeFileText(file);
                Console.WriteLine(fileText);
            }
        }

        private static void LoadAllNASBClasses(string mainPath)
        {
            NASBParserFolder[] folders = {
                new NASBParserFolder("FloatSources", "FloatSource"),
                new NASBParserFolder("Jumps", "Jump"),
                new NASBParserFolder("CheckThings", "CheckThing"),
                new NASBParserFolder("StateActions", "StateAction"),
                new NASBParserFolder("ObjectSources", "ObjectSource")
            };

            nasbParserFiles = new List<NASBParserFile>();
            foreach (NASBParserFolder folder in folders)
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
