using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NASB_Parser_To_xNode
{
    public static class NASBParserFileLoader
    {
        public enum ReadingState
        {
            Start,
            FileOpened,
            ReadingImportsAndNamespace,
            ReadingThings,
            SearchingForCloseBracket,
            Finished
        }

        public enum ReadingThing
        {
            Nothing,
            Class,
            Variable,
            Enum,
            Unwanted
        }

        public static NASBParserFile LoadNASBParserFile(string filePath, string mainPath)
        {
            NASBParserFile nasbParserFile = new NASBParserFile();

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    ReadingState mainFileReadingState = ReadingState.FileOpened;

                    string line;
                    string className = Path.GetFileNameWithoutExtension(filePath);
                    nasbParserFile.relativePath = filePath.Substring(mainPath.Length);
                    nasbParserFile.className = Path.GetFileNameWithoutExtension(nasbParserFile.relativePath);

                    mainFileReadingState = ReadingState.ReadingImportsAndNamespace;
                    ReadingThing topLevelThing = ReadingThing.Nothing;

                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        switch (mainFileReadingState)
                        {
                            case ReadingState.ReadingImportsAndNamespace:
                                string importString;
                                if (TryGetImportString(line, out importString))
                                {
                                    nasbParserFile.imports.Add(importString);
                                }
                                if (line.Contains("namespace "))
                                {
                                    nasbParserFile.@namespace = line.Trim().Substring(line.IndexOf("namespace ") + "namespace ".Length);
                                    mainFileReadingState = ReadingState.ReadingThings;
                                }
                                break;
                            case ReadingState.ReadingThings:
                                // Look for something to read before trying to read it
                                if (topLevelThing == ReadingThing.Nothing)
                                {
                                    topLevelThing = LookForThing(line);
                                }

                                switch (topLevelThing)
                                {
                                    case ReadingThing.Class:
                                        // Read top level classes
                                        ReadClass(sr, line, nasbParserFile);
                                        topLevelThing = ReadingThing.Nothing;
                                        break;
                                    case ReadingThing.Enum:
                                        // Read top level enums
                                        ReadEnum(sr, line, nasbParserFile);
                                        topLevelThing = ReadingThing.Nothing;
                                        break;
                                    case ReadingThing.Unwanted:
                                        // Skip unwanted thing
                                        mainFileReadingState = ReadingState.SearchingForCloseBracket;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case ReadingState.SearchingForCloseBracket:
                                if (CheckForClosingBracket(line)) mainFileReadingState = ReadingState.ReadingThings;
                                break;
                            case ReadingState.Finished:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return nasbParserFile;
        }

        private static void ReadClass(StreamReader sr, string line, NASBParserFile nasbParserFile)
        {
            if (!line.Contains(" class ")) return;

            if (line.Contains(":"))
            {
                nasbParserFile.parentClass = line.Substring(line.IndexOf(" : ") + " : ".Length);
            }

            nasbParserFile.classType = Utils.GetAccessabilityLevel(line);

            int nameIndex = 2;
            if (line.Contains(" abstract "))
            {
                nasbParserFile.isAbstract = true;
                nameIndex++;
            }

            if (line.Contains(" static "))
            {
                nasbParserFile.isStatic = true;
                nameIndex++;
            }

            var split = line.Trim().Split(" ");
            nasbParserFile.className = split[nameIndex];

            ReadingThing thingInClass;
            ReadingState classReadingState = ReadingState.ReadingThings;

            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                thingInClass = LookForThing(line);

                switch (classReadingState)
                {
                    case ReadingState.ReadingThings:
                        switch (thingInClass)
                        {
                            case ReadingThing.Variable:
                                ReadVariable(line, nasbParserFile);
                                break;
                            case ReadingThing.Class:
                                NASBParserFile nestedClass = new NASBParserFile();
                                ReadClass(sr, line, nestedClass);
                                nasbParserFile.nestedClasses.Add(nestedClass);
                                break;
                            case ReadingThing.Enum:
                                ReadEnum(sr, line, nasbParserFile);
                                break;
                            case ReadingThing.Unwanted:
                                classReadingState = ReadingState.SearchingForCloseBracket;
                                break;
                            default:
                                break;
                        }
                        break;
                    case ReadingState.SearchingForCloseBracket:
                        if (CheckForClosingBracket(line)) classReadingState = ReadingState.ReadingThings;
                        break;
                }
            }
        }

        private static void ReadEnum(StreamReader sr, string line, NASBParserFile nasbParserFile)
        {
            EnumObj enumObj = new EnumObj();
            enumObj.accessability = Utils.GetAccessabilityLevel(line);

            line = line.Trim();

            var split = line.Split(" ");
            enumObj.name = split[2];

            while (!sr.EndOfStream)
            {
                line = sr.ReadLine().Trim();

                if (line.Equals("{")) continue;
                if (line.Equals("}")) break;

                if (line.Contains(","))
                {
                    line = line.Substring(0, line.IndexOf(","));
                }

                enumObj.enumNames.Add(line);
            }

            nasbParserFile.enums.Add(enumObj);
        }

        private static void ReadVariable(string line, NASBParserFile nasbParserFile)
        {
            VariableObj variableObj = new VariableObj();
            variableObj.accessability = Utils.GetAccessabilityLevel(line);
            if (line.IndexOf("List<") > -1 && line.IndexOf(">") > -1) variableObj.isList = true;


            line = line.Trim();
            // Remove the trailing ; if it exists
            if (line.IndexOf(";") > -1)
            {
                line = line.Substring(0, line.IndexOf(";"));
            }

            var split = line.Split(" ");

            if (variableObj.isList)
                variableObj.variableType = Utils.GetStringBetweenStrings(split[1], "List<", ">");
            else
                variableObj.variableType = split[1];

            variableObj.name = split[2];

            nasbParserFile.variables.Add(variableObj);
        }

        public static bool CheckForClosingBracket(string line)
        {
            return line.Contains("}");
        }

        public static ReadingThing LookForThing(string line)
        {
            int openParenIndex = line.IndexOf("(");
            int closeParenIndex = line.IndexOf(")");

            if (line.Contains("class "))
            {
                return ReadingThing.Class;
            }
            else if (line.Contains("public ") || line.Contains("protected ") || line.Contains("private ") || line.Contains("internal "))
            {
                // If this is a constructor or some other function with parenthesis
                // Ignore special case where variables are initialized on the same line they are defined
                if ((openParenIndex > -1 || closeParenIndex > -1) && !line.Contains("= new "))
                {
                    return ReadingThing.Unwanted;
                }

                if (line.Contains(" enum "))
                {
                    return ReadingThing.Enum;
                }

                // We must be reading a variable otherwise
                return ReadingThing.Variable;
            }

            return ReadingThing.Nothing;
        }

        public static bool TryGetImportString(string line, out string importString)
        {
            try
            {
                importString = Utils.GetStringBetweenStrings(line, "using ", ";");
            }
            catch
            {
                importString = null;
            }
            return null != importString;
        }
    }
}
