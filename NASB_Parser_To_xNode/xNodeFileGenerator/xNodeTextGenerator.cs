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
                    AddToFileContents($"using static {Utils.GetRelativeNamespace(nasbParserFile)};");
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
                HandleClass(nasbParserFile, false);
            }
            CloseBlock();

            return fileContents;
        }

        private static void HandleClass(NASBParserFile nasbParserFile, bool isNested)
        {
            // AddToFileContents("[Serializable]");

            string classDeclaration = $"public {(nasbParserFile.isAbstract ? "abstract " : "")}class {nasbParserFile.className}";
            if (isNested)
            {
                if (nasbParserFile.parentClass != null)
                {
                    if (nasbParserFile.parentClass.Equals("ISerializable")) nasbParserFile.parentClass = "BaseMovesetNode";
                    classDeclaration += $" : {nasbParserFile.parentClass}";
                }
            }
            else
            {
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
            }

            AddToFileContents(classDeclaration);

            OpenBlock();
            {
                // Variables
                foreach (VariableObj variableObj in nasbParserFile.variables)
                {
                    AddToFileContents(VariableStringGenerator.GetVariableString(variableObj, nasbParserFile, isNested));
                }

                // Enums, if this is a nested class
                if (isNested)
                {
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

                    // Set all variables
                    AddToFileContents("");
                    AddToFileContents($"public void SetData({nasbParserFile.className} data, MovesetGraph graph, string assetPath, Vector2 xyPos)");
                    OpenBlock();
                    {
                        AddToFileContents($"name = NodeEditorUtilities.NodeDefaultName(typeof({nasbParserFile.className}));");
                        AddToFileContents("position.x = xyPos.x;");
                        AddToFileContents("position.y = xyPos.y;");
                        AddToFileContents("int variableCount = 0;");
                        int numVariables = 0;
                        foreach (VariableObj variableObj in nasbParserFile.variables)
                        {
                            ++numVariables;
                            // Check if this is a nested class
                            if (nasbParserFile.nestedClasses.Any(x => x.className.Equals(variableObj.variableType)))
                            {
                                NASBParserFile nestedClass = nasbParserFile.nestedClasses.Find(x => x.className.Equals(variableObj.variableType));
                                // We have to build a new object for the nested type

                                var typeToCreate = variableObj.variableType;
                                if (variableObj.isList)
                                {
                                    typeToCreate = "List<" + variableObj.variableType + ">";
                                }

                                AddToFileContents($"{variableObj.name} = new {typeToCreate}();");

                                if (variableObj.isList)
                                {
                                    AddToFileContents($"foreach (var {variableObj.name}_item in data.{variableObj.name})");
                                    OpenBlock();
                                    {
                                        AddToFileContents($"{variableObj.variableType} temp = new {variableObj.variableType}();");
                                        foreach (VariableObj nestedVariable in nestedClass.variables)
                                        {
                                            // Dumb special case cast
                                            if (nasbParserFile.className.Equals("SASetFloatTarget") 
                                                && variableObj.name.Equals("Sets")
                                                && nestedVariable.name.Equals("Way"))
                                            {
                                                AddToFileContents($"temp.{nestedVariable.name} = (SetFloat.ManipWay){variableObj.name}_item.{nestedVariable.name};");
                                            }
                                            else
                                            {
                                                AddToFileContents($"temp.{nestedVariable.name} = {variableObj.name}_item.{nestedVariable.name};");
                                            }
                                        }
                                        AddToFileContents($"{variableObj.name}.Add(temp);");
                                    }
                                    CloseBlock();
                                } else
                                {
                                    foreach (VariableObj nestedVariable in nestedClass.variables)
                                    {
                                        AddToFileContents($"{variableObj.name}.{nestedVariable.name} = data.{variableObj.name}.{nestedVariable.name};");
                                    }
                                }
                            } else
                            {
                                AddToFileContents($"{variableObj.name} = data.{variableObj.name};");

                                // If this variable is a class in the NASB_Parser, excluding enum only classes
                                if (Program.nasbParserFiles.Any(x => x.className.Equals(variableObj.variableType))
                                    && !Consts.enumOnlyFiles.Contains(variableObj.variableType))
                                {
                                    if (!variableObj.isList)
                                    {
                                        // Create the node for this variable type and add it to the graph
                                        AddToFileContents($"");
                                        string nodeName = $"node_{variableObj.name}";
                                        AddToFileContents($"{variableObj.variableType}Node {nodeName} = graph.AddNode<{variableObj.variableType}Node>();");
                                        AddToFileContents($"GetPort(\"{variableObj.name}\").Connect({nodeName}.GetPort(\"NodeInput\"));");
                                        AddToFileContents($"AssetDatabase.AddObjectToAsset({nodeName}, assetPath);");
                                        AddToFileContents($"{nodeName}.SetData({variableObj.name}, graph, assetPath, xyPos + new Vector2(Consts.NodeXOffset, variableCount * Consts.NodeYOffset));");
                                        if (numVariables < nasbParserFile.variables.Count) AddToFileContents("++variableCount;");
                                        AddToFileContents($"");
                                    }
                                }
                            }
                        }
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

        public static void AddToFileContents(string line)
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
