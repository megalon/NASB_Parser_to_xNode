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
        private static List<string> basicTypes = new List<string> { "bool", "int", "string", "float", "double", "UnityEngine.Vector3" };

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

            AddToFileContents("");

            string namespaceSubfolder = "";

            if (nasbParserFile.@namespace != null && nasbParserFile.@namespace.Contains("NASB_Parser."))
            {
                namespaceSubfolder = nasbParserFile.@namespace.Substring("NASB_Parser.".Length);
            }

            AddToFileContents("namespace NASB_Moveset_Editor" + (namespaceSubfolder.Equals(string.Empty) ? "" : "." + namespaceSubfolder));

            OpenBlock();
            {
                HandleClass(nasbParserFile, false);
            }
            CloseBlock();

            return fileContents;
        }

        private static void HandleClass(NASBParserFile nasbParserFile, bool isNested)
        {
            AddToFileContents("[Serializable]");

            string classDeclaration = $"public {(nasbParserFile.isAbstract ? "abstract " : "")}class {nasbParserFile.className}";
            if (isNested)
            {
                if (nasbParserFile.parentClass != null)
                {
                    if (nasbParserFile.parentClass.Equals("ISerializable")) nasbParserFile.parentClass = "Node";
                    classDeclaration += $" : {nasbParserFile.parentClass}";
                }
            }
            else
            {
                if (nasbParserFile.parentClass != null && nasbParserFile.parentClass.Equals("ISerializable")) nasbParserFile.parentClass = "";
                classDeclaration += $"Node : {nasbParserFile.parentClass}Node";
            }

            AddToFileContents(classDeclaration);

            OpenBlock();
            {
                // Variables
                foreach (VariableObj variableObj in nasbParserFile.variables)
                {
                    //var accString = Utils.GetAccessabilityLevelString(variableObj.accessability);
                    var accString = "public";
                    var startOfLine = $"{accString} {(variableObj.isStatic ? "static " : "")}{(variableObj.isReadonly ? "readonly " : "")}";

                    // Handle Vector3 ambiguity
                    if (variableObj.variableType.Equals("Vector3")) variableObj.variableType = "UnityEngine.Vector3";

                    // Handle List
                    var fullType = variableObj.isList ? $"List<{variableObj.variableType}>" : variableObj.variableType;

                    if (basicTypes.Contains(variableObj.variableType))
                    {
                        AddToFileContents($"{startOfLine}{fullType} {variableObj.name};");
                    }
                    else if (nasbParserFile.enums.Any(x => x.name.Equals(variableObj.variableType)))
                    {
                        // Type is an enum contained within the class
                        AddToFileContents($"{startOfLine}{fullType} {variableObj.name};");
                    }
                    else
                    {
                        // If the name matches a nested class, we don't want to give it the [Output] attribute
                        if (nasbParserFile.nestedClasses.Any(x => x.className.Equals(variableObj.variableType)) || isNested)
                        {
                            AddToFileContents($"{startOfLine}{fullType} {variableObj.name};");
                        } else
                        {
                            // All other types are set to [Output]
                            AddToFileContents($"[Output] public {fullType} {variableObj.name};");
                        }
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


                // Node specific functions
                if (nasbParserFile.parentClass != null)
                {
                    AddToFileContents("");
                    AddToFileContents("protected override void Init()");
                    OpenBlock();
                    {
                        AddToFileContents("base.Init();");
                        if (Consts.classToTypeId.ContainsKey(nasbParserFile.className))
                            AddToFileContents($"TID = TypeId.{Consts.classToTypeId[nasbParserFile.className]};");
                    }
                    CloseBlock();
                }

                // Handle nested classes
                foreach (NASBParserFile nestedClass in nasbParserFile.nestedClasses)
                {
                    Console.WriteLine($"Writing nested class {nestedClass.relativePath} for {nasbParserFile.relativePath}");
                    AddToFileContents("");
                    HandleClass(nestedClass, true);
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
