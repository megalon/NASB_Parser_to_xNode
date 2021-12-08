using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NASB_Parser_To_xNode.xNodeTextGenerator;

namespace NASB_Parser_To_xNode
{
    public static class SetDataGenerator
    {
        private static bool isSAOrderedSensitive;
        private static bool isFSFrame;
        public static void Generate(NASBParserFile nasbParserFile)
        {
            // SAOrderedSensitive is treated differently because we need dynamic ports to
            // show the number of connections
            isSAOrderedSensitive = nasbParserFile.className.Equals("SAOrderedSensitive");
            isFSFrame = nasbParserFile.className.Equals("FSFrame");

            AddToFileContents($"public int SetData({nasbParserFile.className} data, MovesetGraph graph, string assetPath, Vector2 nodeDepthXY)");
            OpenBlock();
            {
                AddToFileContents($"name = NodeEditorUtilities.NodeDefaultName(typeof({nasbParserFile.className}));");
                AddToFileContents("position.x = nodeDepthXY.x * Consts.NodeXOffset;");
                AddToFileContents("position.y = nodeDepthXY.y * Consts.NodeYOffset;");
                AddToFileContents("int variableCount = 0;");
                AddToFileContents($"");
                int numVariables = 0;

                foreach (VariableObj variableObj in nasbParserFile.variables)
                {
                    ++numVariables;
                    string nodeName = $"node_{variableObj.name}";

                    // Get the main type for this class if it is nested
                    var mainType = "";
                    if (variableObj.variableType.LastIndexOf(".") > 0 && !variableObj.variableType.Equals("NASB_Parser.Vector3"))
                    {
                        mainType = variableObj.variableType.Substring(variableObj.variableType.LastIndexOf(".") + 1);
                    }

                    // If this variable is from a nested class within one of the NASB_Parser files
                    if (nasbParserFile.nestedClasses.Any(x => x.className.Equals(mainType)))
                    {
                        NASBParserFile nestedClass = nasbParserFile.nestedClasses.Find(x => x.className.Equals(mainType));

                        var typeToCreate = variableObj.variableType;
                        if (variableObj.isList)
                        {
                            typeToCreate = "List<" + typeToCreate + ">";
                        }
                        AddToFileContents($"{variableObj.name} = data.{variableObj.name};");
                        AddToFileContents($"");

                        if (variableObj.isList)
                        {
                            // Create the list of nodes for this variable type and add them to the graph
                            AddToFileContents($"foreach ({variableObj.variableType} {variableObj.name}_item in {variableObj.name})");
                            OpenBlock();
                            {
                                AddNodeToGraph(nodeName, variableObj, null, null, "_item");
                                AddToFileContents("++variableCount;");
                            }
                            CloseBlock();
                        }
                        else
                        {
                            AddNodeToGraph(nodeName, variableObj, null, null, "");
                            AddToFileContents("++variableCount;");
                        }
                    }
                    else
                    {
                        if (isSAOrderedSensitive && variableObj.name.Equals("Actions"))
                        {
                            AddToFileContents("listSize = data.Actions.Count;");
                        } else
                        {
                            AddToFileContents($"{variableObj.name} = data.{variableObj.name};");
                        }

                        // If this variable is a class in the NASB_Parser, excluding enum only classes
                        if (Program.nasbParserFiles.Any(x => x.className.Equals(variableObj.variableType))
                            && !Consts.enumOnlyFiles.Contains(variableObj.variableType))
                        {
                            AddToFileContents($"");

                            Dictionary<string, string> dict = null;
                            if (variableObj.variableType.Equals("StateAction")) dict = Consts.stateActionIds;
                            else if (variableObj.variableType.Equals("CheckThing")) dict = Consts.checkThingsIds;
                            else if (variableObj.variableType.Equals("Jump")) dict = Consts.jumpId;
                            else if (variableObj.variableType.Equals("FloatSource")) dict = Consts.floatSourceIds;
                            else if (variableObj.variableType.Equals("ObjectSource")) dict = Consts.objectSourceIds;

                            if (variableObj.isList)
                            {
                                // Create the list of nodes for this variable type and add them to the graph
                                AddToFileContents($"foreach ({variableObj.variableType} {variableObj.name}_item in data.{variableObj.name})");
                                OpenBlock();
                                {
                                    if (isSAOrderedSensitive)
                                    {
                                        AddToFileContents("// Create dynamic ports based on number of actions");
                                        AddToFileContents("string portName = \"\" + (DynamicPorts.Count() + 1);");
                                        AddToFileContents("AddDynamicOutput(typeof(StateAction), ConnectionType.Override, TypeConstraint.None, portName);");
                                        AddToFileContents("");
                                    }

                                    GenerateSwitchStatement(nodeName, variableObj, dict, "_item");
                                    AddToFileContents("++variableCount;");
                                }
                                CloseBlock();
                            }
                            else
                            {
                                GenerateSwitchStatement(nodeName, variableObj, dict, "");
                                if (numVariables < nasbParserFile.variables.Count) AddToFileContents("++variableCount;");
                            }
                            AddToFileContents($"");
                        }
                    }
                }

                if (isFSFrame)
                {
                    AddToFileContents("Value = data.Value;");
                    AddToFileContents($"");
                }

                AddToFileContents("return variableCount;");
            }
            CloseBlock();
        }

        private static void GenerateSwitchStatement(string nodeName, VariableObj variableObj, Dictionary<string, string> dict, string itemText)
        {
            // Create the node for this variable type and add it to the graph
            if (dict != null)
            {
                AddToFileContents($"switch ({variableObj.name}{itemText}.TID)");
                OpenBlock();
                foreach (string key in dict.Keys)
                {
                    // Ignore BaseIdentifier, because we don't want to make BaseIdentifier nodes
                    if (key.Equals("BaseIdentifier")) continue;

                    AddToFileContents($"case {variableObj.variableType}.TypeId.{key}:");
                    UpdateIndent(1);
                    {
                        AddNodeToGraph(nodeName, variableObj, key, dict[key], itemText);
                    }
                    UpdateIndent(-1);
                    AddToFileContents($"break;");

                }
                CloseBlock();
            }
            else
            {
                AddNodeToGraph(nodeName, variableObj, null, null, itemText);
            }
        }

        private static void AddNodeToGraph(string nodeName, VariableObj variableObj, string key, string value, string itemText)
        {
            if (key != null && !key.Equals(string.Empty))
            {
                nodeName = $"{key}_{nodeName}";
                AddToFileContents($"{value}Node {nodeName} = graph.AddNode<{value}Node>();");

                if (isSAOrderedSensitive)
                {
                    // Connect dynamic port to new node
                    AddToFileContents($"GetPort(portName).Connect({nodeName}.GetPort(\"NodeInput\"));");
                } else
                {
                    AddToFileContents($"GetPort(\"{variableObj.name}\").Connect({nodeName}.GetPort(\"NodeInput\"));");
                }

                AddToFileContents($"AssetDatabase.AddObjectToAsset({nodeName}, assetPath);");
                AddToFileContents($"variableCount += {nodeName}.SetData(({value}){variableObj.name}{itemText}, graph, assetPath, nodeDepthXY + new Vector2(1, variableCount));");
            }
            else
            {
                var typeName = variableObj.variableType;
                if (variableObj.variableType.Contains("."))
                {
                    typeName = typeName.Replace(".", "_");
                }
                AddToFileContents($"{typeName}Node {nodeName} = graph.AddNode<{typeName}Node>();");
                AddToFileContents($"GetPort(\"{variableObj.name}\").Connect({nodeName}.GetPort(\"NodeInput\"));");
                AddToFileContents($"AssetDatabase.AddObjectToAsset({nodeName}, assetPath);");
                AddToFileContents($"variableCount += {nodeName}.SetData({variableObj.name}{itemText}, graph, assetPath, nodeDepthXY + new Vector2(1, variableCount));");
            }
        }
    }
}
