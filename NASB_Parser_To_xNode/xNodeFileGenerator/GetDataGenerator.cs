using System;
using System.Collections.Generic;
using System.Text;
using static NASB_Parser_To_xNode.xNodeTextGenerator;
using System.Linq;

namespace NASB_Parser_To_xNode
{
    public static class GetDataGenerator
    {
        private static bool isSAOrderedSensitive;
        public static void Generate(NASBParserFile nasbParserFile)
        {
            isSAOrderedSensitive = nasbParserFile.className.Equals("SAOrderedSensitive");
            bool classWithTID = false;

            if (nasbParserFile.parentClass != null && (Consts.classToTypeId.ContainsKey(nasbParserFile.className)))
            {
                classWithTID = true;
            }

            if (classWithTID && nasbParserFile.parentClass.Equals("ISerializable"))
            {
                AddToFileContents($"public {nasbParserFile.className} GetData()");
            }
            else
            {
                AddToFileContents($"public {(classWithTID ? "new " : "")}{nasbParserFile.className} GetData()");
            }

            OpenBlock();
            {
                var mainClassName = "objToReturn";
                AddToFileContents($"{nasbParserFile.className} {mainClassName} = new {nasbParserFile.className}();");

                if (classWithTID)
                {
                    AddToFileContents($"{mainClassName}.TID = TypeId.{Consts.classToTypeId[nasbParserFile.className]};");
                    AddToFileContents($"{mainClassName}.Version = Version;");
                }

                foreach (VariableObj variableObj in nasbParserFile.variables)
                {

                    var typeClassFileName = variableObj.variableType;
                    if (variableObj.variableType.Contains("."))
                    {
                        typeClassFileName = typeClassFileName.Replace(".", "_");
                    }
                    var nodeName = $"{typeClassFileName}_Node";

                    if (!variableObj.isOutput)
                    {
                        AddToFileContents($"{mainClassName}.{variableObj.name} = {variableObj.name};");
                        continue;
                    }

                    Dictionary<string, string> dict = null;
                    if (variableObj.variableType.Equals("StateAction")) dict = Consts.stateActionIds;
                    else if (variableObj.variableType.Equals("CheckThing")) dict = Consts.checkThingsIds;
                    else if (variableObj.variableType.Equals("Jump")) dict = Consts.jumpId;
                    else if (variableObj.variableType.Equals("FloatSource")) dict = Consts.floatSourceIds;
                    else if (variableObj.variableType.Equals("ObjectSource")) dict = Consts.objectSourceIds;

                    if (variableObj.isList)
                    {
                        if (isSAOrderedSensitive)
                        {
                            AddToFileContents($"foreach(NodePort port in DynamicOutputs)");
                            OpenBlock();
                                AddToFileContents("if (port.ConnectionCount <= 0) continue;");
                                AddToFileContents($"{typeClassFileName}Node {nodeName} = ({typeClassFileName}Node)port.Connection.node;");
                        } else
                        {
                            AddToFileContents($"foreach(NodePort port in GetPort(\"{variableObj.name}\").GetConnections())");
                            OpenBlock();
                                AddToFileContents($"{typeClassFileName}Node {nodeName} = ({typeClassFileName}Node)port.node;");
                        }
                        GenerateSwitchStatement(nodeName, variableObj, dict, mainClassName, typeClassFileName, true);
                        CloseBlock();
                    } else
                    {
                        AddToFileContents($"if (GetPort(\"{variableObj.name}\").ConnectionCount > 0)");
                        OpenBlock();
                        {
                            AddToFileContents($"{typeClassFileName}Node {nodeName} = ({typeClassFileName}Node)GetPort(\"{variableObj.name}\").GetConnection(0).node;");
                            GenerateSwitchStatement(nodeName, variableObj, dict, mainClassName, typeClassFileName, false);
                        }
                        CloseBlock();
                    }
                }
                AddToFileContents($"return {mainClassName};");
            }
            CloseBlock();
        }

        private static void GenerateSwitchStatement(string nodeName, VariableObj variableObj, Dictionary<string, string> dict, string mainClassName, string typeClassFileName, bool isList)
        {
            if (dict != null)
            {
                AddToFileContents($"switch ({nodeName}.TID)");
                OpenBlock();
                foreach (string key in dict.Keys)
                {
                    AddToFileContents($"case {variableObj.variableType}.TypeId.{key}:");
                    UpdateIndent(1);
                    {
                        if (isList)
                        {
                            if (isSAOrderedSensitive)
                            {
                                AddToFileContents($"{dict[key]}Node {key}_{nodeName} = ({dict[key]}Node)port.Connection.node;");
                            } else
                            {
                                AddToFileContents($"{dict[key]}Node {key}_{nodeName} = ({dict[key]}Node)port.node;");
                            }
                            AddToFileContents($"{mainClassName}.{variableObj.name}.Add({key}_{nodeName}.GetData());");
                        } else
                        {
                            AddToFileContents($"{dict[key]}Node {key}_{nodeName} = ({dict[key]}Node)GetPort(\"{variableObj.name}\").GetConnection(0).node;");
                            AddToFileContents($"{mainClassName}.{variableObj.name} = {key}_{nodeName}.GetData();");
                        }
                    }
                    UpdateIndent(-1);
                    AddToFileContents($"break;");
                }
                CloseBlock();
            }
            else
            {
                if (isList)
                {
                    AddToFileContents($"{mainClassName}.{variableObj.name}.Add({nodeName}.GetData());");
                }
                else
                {
                    AddToFileContents($"{mainClassName}.{variableObj.name} = {nodeName}.GetData();");
                }
            }
        }
    }
}
