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

        private static string[] otherImports = { "UnityEngine", "UnityEditor", "XNode", "XNodeEditor", "MovesetParser" };
        private static Dictionary<string, string> specialImports = new Dictionary<string, string> {
            { "SAManipHitbox", "static MovesetParser.StateActions.SAManipHitbox" },
            { "SAManipHurtbox", "static MovesetParser.StateActions.SAManipHurtbox" },
            { "SAOrderedSensitive", "System.Linq" }
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

            foreach(string commentLine in Consts.commentHeaderText)
            {
                AddToFileContents("// * " + commentLine);
            }

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
                var extraParserImport = "MovesetParser." + folder.folderName;
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

            if (nasbParserFile.@namespace != null && nasbParserFile.@namespace.Contains("MovesetParser."))
            {
                namespaceSubfolder = nasbParserFile.@namespace.Substring("MovesetParser.".Length);
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
            bool isSAOrderedSensitive = nasbParserFile.className.Equals("SAOrderedSensitive");

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
                // Replace . with _ for nested class names
                var parentFolder = nasbParserFile.relativePath.Substring(0, nasbParserFile.relativePath.LastIndexOf("\\"));
                nasbParserFile.parentClass = Consts.classesToNamespaces.First(x => x.Value.Equals(parentFolder)).Key;
                nasbParserFile.className = nasbParserFile.relativePath.Replace(".", "_");
                nasbParserFile.className = nasbParserFile.className.Substring(nasbParserFile.className.LastIndexOf("\\") + 1);
            } else if (
                nasbParserFile.relativePath.Contains("\\") 
                && nasbParserFile.parentClass != null
                && nasbParserFile.parentClass.Equals("IBulkSerializer")
                && !Consts.classesToNamespaces.ContainsKey(nasbParserFile.className))
            {
                // Set parent class for loose files in folders
                var parentFolder = nasbParserFile.relativePath.Substring(0, nasbParserFile.relativePath.LastIndexOf("\\"));
                nasbParserFile.parentClass = Consts.classesToNamespaces.First(x => x.Value.Equals(parentFolder)).Key;
            }

            string classDeclaration = $"public {(nasbParserFile.isAbstract ? "abstract " : "")}class {nasbParserFile.className}";
            if (nasbParserFile.className.Equals("IdState"))
            {
                // IdState doesn't need the "[Input]" that BaseMovesetNode has, so just inherit from default Node
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
                // IdState has no input, so skip it
                if (!nasbParserFile.className.Equals("IdState"))
                {
                    string inputStartText = "[Input(connectionType = ConnectionType.Override)] public ";
                    if (nasbParserFile.parentClass == null || nasbParserFile.parentClass.Equals("ISerializable"))
                    {
                        if (Consts.looseFiles.Contains(nasbParserFile.className))
                        {
                            AddToFileContents($"{inputStartText}{nasbParserFile.className} NodeInput;");
                        }
                    } else if (Consts.classesToNamespaces.ContainsKey(nasbParserFile.parentClass))
                    {
                        if (nasbParserFile.className.Contains("_"))
                        {
                            AddToFileContents($"{inputStartText}{nasbParserFile.className.Replace("_", ".")} NodeInput;");
                        } else if(Consts.specialInputTypes.Contains(nasbParserFile.className)) {
                            AddToFileContents($"{inputStartText}{Consts.specialInputTypes[Consts.specialInputTypes.IndexOf(nasbParserFile.className)]} NodeInput;");
                        }
                        else
                        {
                            AddToFileContents($"{inputStartText}{nasbParserFile.parentClass} NodeInput;");
                        }
                    } else
                    {
                        // Nodes that derive from other classes than the "base" classes
                    }
                }

                // Variables
                foreach (VariableObj variableObj in nasbParserFile.variables)
                {
                    // Skip the "Actions" variable for SAOrderedSensitive
                    if (isSAOrderedSensitive && variableObj.name.Equals("Actions"))
                    {
                        // Add the listSize variable, and set it to an output
                        variableObj.isOutput = true;
                        AddToFileContents("[Range(2, 50)] public int listSize = 0;");
                        continue;
                    }

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
                        if (Consts.specialClassVersions.ContainsKey(nasbParserFile.className))
                            AddToFileContents($"Version = {Consts.specialClassVersions[nasbParserFile.className]};");

                        if (nasbParserFile.className.Equals("InputValidator"))
                        {
                            AddToFileContents("// InputValidator should default to Inside, since zero is unused");
                            AddToFileContents($"SegCompare = CtrlSegCompare.Inside;");
                        }
                    }
                    CloseBlock();

                    // Override GetValue function to fix warning in Unity log
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

                    // SetData and GetData functions
                    SetDataGenerator.Generate(nasbParserFile);
                    AddToFileContents("");
                    GetDataGenerator.Generate(nasbParserFile);
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
