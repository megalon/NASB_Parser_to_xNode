using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NASB_Parser_To_xNode
{
    public static class xNodeTextGenerator
    {
        private static int indentCount;
        private static string indent;
        private static string fileContents;

        private static string[] otherImports = { "UnityEngine", "UnityEditor", "XNode", "XNodeEditor" };
        private static List<string> basicTypes = new List<string> { "bool", "int", "string", "float", "double" };
        public static string GenerateXNodeFileText(NASBParserFile nasbParserFile)
        {
            fileContents = "";
            indentCount = 0;
            indent = "";

            foreach (string importString in nasbParserFile.imports)
            {
                AddToFileContents("using " + importString + ";");
            }

            foreach (string importString in otherImports)
            {
                AddToFileContents("using " + importString + ";");
            }

            AddToFileContents("");
            AddToFileContents("namespace NASB_Moveset_Editor");
            OpenBlock();
            {
                AddToFileContents("[Serializable]");
                string className = Path.GetFileNameWithoutExtension(nasbParserFile.relativePath);

                // Convert ISerializable to Node
                if (nasbParserFile.parentClass != null && nasbParserFile.parentClass.Equals("ISerializable")) nasbParserFile.parentClass = "";

                AddToFileContents($"public class {className}Node : {nasbParserFile.parentClass}Node");
                OpenBlock();
                {
                    foreach (VariableObj variableObj in nasbParserFile.variables)
                    {
                        var accString = Utils.GetAccessabilityLevelString(variableObj.accessability);
                        if (basicTypes.Contains(variableObj.variableType))
                        {
                            AddToFileContents($"public {variableObj.variableType} {variableObj.name};");
                        } else
                        {
                            AddToFileContents($"[Output] public {variableObj.variableType} {variableObj.name};");
                        }
                    }

                    foreach (EnumObj enumObj in nasbParserFile.enums)
                    {
                        AddToFileContents("");
                        var accString = Utils.GetAccessabilityLevelString(enumObj.accessability);
                        AddToFileContents($"{accString} enum {enumObj.name}");
                        OpenBlock();
                        {
                            foreach (string enumNames in enumObj.enumNames)
                            {
                                AddToFileContents($"{enumNames},");
                            }
                        }
                        CloseBlock();
                    }
                }
                CloseBlock();
            }
            CloseBlock();

            return fileContents;
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
