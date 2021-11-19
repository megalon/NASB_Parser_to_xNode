using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace NASB_Parser_To_xNode
{
    public static class xNodeTextGenerator
    {
        private static int indentCount;
        private static string indent;
        private static string fileContents;

        private static string[] otherImports = { "UnityEngine", "UnityEditor", "XNode", "XNodeEditor", "NASB_Parser" };
        private static List<string> basicTypes = new List<string> { "bool", "int", "string", "float", "double" };
        private static Dictionary<string, string> specialCaseImports = new Dictionary<string, string>{
            { "SASetFloatTarget", "static NASB_Parser.StateActions.SASetFloatTarget" },
            {"SAManipHurtbox", "static NASB_Parser.StateActions.SAManipHurtbox" },
            {"SAManipHitbox", "static NASB_Parser.StateActions.SAManipHitbox" },
            {"SAGUAMessageObject", "static NASB_Parser.StateActions.SAGUAMessageObject" },
            {"SAMapAnimation", "static NASB_Parser.StateActions.SAMapAnimation" },
            {"SAStandardInput", "static NASB_Parser.StateActions.SAStandardInput" },
        };

        public static string GenerateXNodeFileText(NASBParserFile nasbParserFile)
        {
            fileContents = "";
            indentCount = 0;
            indent = "";

            // Imports
            foreach (string importString in nasbParserFile.imports)
            {
                AddToFileContents("using " + importString + ";");
            }

            foreach (string importString in otherImports)
            {
                AddToFileContents("using " + importString + ";");
            }

            foreach (NASBParserFolder folder in Consts.folders)
            {
                var extraParserImport = "NASB_Parser." + folder.folderName;
                if (!nasbParserFile.imports.Contains(extraParserImport))
                    AddToFileContents("using " + extraParserImport + ";");
            }

            if (specialCaseImports.ContainsKey(nasbParserFile.className))
            {
                AddToFileContents("using " + specialCaseImports[nasbParserFile.className] + ";");
            }

            AddToFileContents("");
            AddToFileContents("namespace NASB_Moveset_Editor");
            OpenBlock();
            {
                HandleClass(nasbParserFile);
            }
            CloseBlock();

            return fileContents;
        }

        private static void HandleClass(NASBParserFile nasbParserFile)
        {
            // Convert ISerializable to Node
            if (nasbParserFile.parentClass != null && nasbParserFile.parentClass.Equals("ISerializable")) nasbParserFile.parentClass = "";
            AddToFileContents("[Serializable]");
            AddToFileContents($"public {(nasbParserFile.isAbstract ? "abstract " : "")}class {nasbParserFile.className}Node : {nasbParserFile.parentClass}Node");
            OpenBlock();
            {
                // Variables
                foreach (VariableObj variableObj in nasbParserFile.variables)
                {
                    //var accString = Utils.GetAccessabilityLevelString(variableObj.accessability);
                    var accString = "public";
                    var startOfLine = $"{accString} {(variableObj.isStatic ? "static " : "")}{(variableObj.isReadonly ? "readonly " : "")}";
                    if (basicTypes.Contains(variableObj.variableType))
                    {
                        AddToFileContents($"{startOfLine}{variableObj.variableType} {variableObj.name};");
                    }
                    else if (nasbParserFile.enums.Any(x => x.name.Equals(variableObj.variableType)))
                    {
                        // Type is an enum contained within the class
                        AddToFileContents($"{startOfLine}{variableObj.variableType} {variableObj.name};");
                    }
                    else if (variableObj.variableType.Equals("Vector3"))
                    {
                        // Special case for Vector3 collision with NASB_Parser
                        AddToFileContents($"{startOfLine}UnityEngine.Vector3 {variableObj.name};");
                    }
                    else
                    {
                        AddToFileContents($"[Output] public {variableObj.variableType} {variableObj.name};");
                    }
                }

                // Enums
                foreach (EnumObj enumObj in nasbParserFile.enums)
                {
                    AddToFileContents("");
                    var accString = Utils.GetAccessabilityLevelString(enumObj.accessability);
                    AddToFileContents($"{accString} enum {enumObj.name}");
                    OpenBlock();
                    {
                        foreach (string enumName in enumObj.enumNames)
                        {
                            AddToFileContents($"{enumName},");
                        }
                    }
                    CloseBlock();
                }

                // Init function
                AddToFileContents("");
                AddToFileContents("protected override void Init()");
                OpenBlock();
                {
                    AddToFileContents("base.Init();");
                    if (Consts.classToTypeId.ContainsKey(nasbParserFile.className))
                        AddToFileContents($"TID = TypeId.{Consts.classToTypeId[nasbParserFile.className]};");
                }
                CloseBlock();

                // Handle nested classes
                foreach (NASBParserFile nestedClass in nasbParserFile.nestedClasses)
                {
                    Console.WriteLine($"Writing nested class {nestedClass.relativePath} for {nasbParserFile.relativePath}");
                    AddToFileContents("");
                    HandleClass(nestedClass);
                }
            }
            CloseBlock();
        }

        private static void AddToFileContents(string line)
        {
            fileContents += indent + line + "\n";
        }

        private static void OpenBlock()
        {
            AddToFileContents("{");
            UpdateIndent(1);
        }

        private static void CloseBlock()
        {
            UpdateIndent(-1);
            AddToFileContents("}");
        }

        private static void UpdateIndent(int indentDelta)
        {
            indentCount += indentDelta;
            if (indentCount < 0) indentCount = 0;
            
            indent = "";
            for (int i = 0; i < indentCount; ++i)
            {
                indent += "\t";
            }
        }
    }
}
