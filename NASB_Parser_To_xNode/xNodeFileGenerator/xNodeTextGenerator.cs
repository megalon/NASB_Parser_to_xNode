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
        private static Dictionary<string, string> specialImports = new Dictionary<string, string> {
            { "SAManipHitbox", "static NASB_Parser.StateActions.SAManipHitbox" },
            { "SAManipHurtbox", "static NASB_Parser.StateActions.SAManipHurtbox" }
        };
        private static Dictionary<string, string> specialTypes = new Dictionary<string, string>
        {
            { "ManipWay", "SASetFloatTarget.SetFloat.ManipWay" },
        };

        public static string GenerateXNodeFileText(NASBParserFile nasbParserFile, bool isNested)
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

            if (specialImports.ContainsKey(nasbParserFile.className))
            {
                AddToFileContents($"using {specialImports[nasbParserFile.className]};");
            }

            foreach (NASBParserFolder folder in Consts.folders)
            {
                var extraParserImport = "NASB_Parser." + folder.folderName;
                if (!nasbParserFile.imports.Contains(extraParserImport))
                    AddToFileContents("using " + extraParserImport + ";");
            }

            foreach (NASBParserFolder folder in Consts.folders)
            {
                AddToFileContents($"using NASB_Moveset_Editor.{folder.folderName};");
            }

            if (nasbParserFile.parentClass != null
                && !nasbParserFile.parentClass.Equals(string.Empty)
                && !Path.GetDirectoryName(nasbParserFile.relativePath).Equals(string.Empty))
            {
                if (nasbParserFile.parentClass.Equals("ISerializable"))
                {
                    if (!isNested) AddToFileContents($"using static {Utils.GetRelativeNamespace(nasbParserFile)};");
                } else 
                {
                    Utils.RecurseThroughParentNamespaces(nasbParserFile);
                }
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
                HandleClass(nasbParserFile, isNested);
            }
            CloseBlock();

            return fileContents;
        }

        private static void HandleClass(NASBParserFile nasbParserFile, bool isNested)
        {
            // AddToFileContents("[Serializable]");

            // Fix variables with type namespace of nested class
            foreach (VariableObj variable in nasbParserFile.variables)
            {
                if (specialTypes.ContainsKey(variable.variableType))
                {
                    variable.variableType = specialTypes[variable.variableType];
                } else if (nasbParserFile.nestedClasses.Any(x => x.className.Equals(variable.variableType)))
                {
                    variable.variableType = nasbParserFile.className + "." + variable.variableType;
                }
            }

            if (isNested)
            {
                nasbParserFile.className = nasbParserFile.relativePath.Replace(".", "_");
                nasbParserFile.className = nasbParserFile.className.Substring(nasbParserFile.className.LastIndexOf("\\") + 1);
            }

            string classDeclaration = $"public {(nasbParserFile.isAbstract ? "abstract " : "")}class {nasbParserFile.className}";
            if (nasbParserFile.className.Equals("IdState"))
            {
                // IdState doesn't need the "[Input]" that BaseMovesetNode has
                classDeclaration += $"Node : Node";
            } else if (nasbParserFile.parentClass == null || nasbParserFile.parentClass.Equals("ISerializable"))
            {
                classDeclaration += $"Node : BaseMovesetNode";
            } else
            {
                classDeclaration += $"Node : {nasbParserFile.parentClass}Node";
            }

            AddToFileContents(classDeclaration);

            OpenBlock();
            {
                // Variables
                foreach (VariableObj variableObj in nasbParserFile.variables)
                {
                    AddToFileContents(VariableStringGenerator.GetVariableString(variableObj, nasbParserFile, isNested));
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

                    // Override function to fix warning in Unity log
                    AddToFileContents("");
                    AddToFileContents("public override object GetValue(NodePort port)");
                    OpenBlock();
                    {
                        AddToFileContents("return null;");
                    }
                    CloseBlock();

                    AddToFileContents("");

                    if (isNested)
                    {
                        nasbParserFile.className = nasbParserFile.relativePath.Substring(nasbParserFile.relativePath.LastIndexOf("\\") + 1);
                    }

                    // SetData function
                    SetDataGenerator.Generate(nasbParserFile);

                    GetDataGenerator.Generate(nasbParserFile);
                }

                // Handle nested classes
                foreach (NASBParserFile nestedClass in nasbParserFile.nestedClasses)
                {
                    Console.WriteLine($"Found nested class {nestedClass.relativePath} for {nasbParserFile.relativePath}");
                    //AddToFileContents("");
                    //HandleClass(nestedClass, true);
                }
            }
            CloseBlock();
        }

        public static void AddToFileContents(string line)
        {
            fileContents += indent + line + "\n";
        }

        public static void OpenBlock()
        {
            AddToFileContents("{");
            UpdateIndent(1);
        }

        public static void CloseBlock()
        {
            UpdateIndent(-1);
            AddToFileContents("}");
        }

        public static void UpdateIndent(int indentDelta)
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
