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
        private static bool isFSFrame;
        public static void Generate(NASBParserFile nasbParserFile)
        {
            isSAOrderedSensitive = nasbParserFile.className.Equals("SAOrderedSensitive");
            isFSFrame = nasbParserFile.className.Equals("FSFrame");

            // TODO: FIX THIS now that we don't use the classToTypeId 


            //bool classWithTID = false;

            //if (nasbParserFile.parentClass != null && (Consts.classToTypeId.ContainsKey(nasbParserFile.className)))
            //{
            //    classWithTID = true;
            //}

            //if (classWithTID && nasbParserFile.parentClass.Equals("IBulkSerializer"))
            //{
            //    AddToFileContents($"public {nasbParserFile.className} GetData()");
            //}
            //else
            //{
            //    AddToFileContents($"public {(classWithTID ? "new " : "")}{nasbParserFile.className} GetData()");
            //}

            bool classWithTID = true;

            AddToFileContents($"public {nasbParserFile.className} GetData()");
            //


            OpenBlock();
            {
                var mainClassName = "objToReturn";
                AddToFileContents($"{nasbParserFile.className} {mainClassName} = new {nasbParserFile.className}();");

                if (classWithTID)
                {
                    AddToFileContents($"{mainClassName}.TID = TypeId.{nasbParserFile.className};");
                    AddToFileContents($"{mainClassName}.Version = Version;");
                }

                if (isFSFrame)
                {
                    AddToFileContents($"{mainClassName}.Value = Value;");
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

                    string[] idsArray = null;
                    if (variableObj.variableType.Equals("StateAction")) idsArray = Consts.stateActionTypeIds;
                    else if (variableObj.variableType.Equals("CheckThing")) idsArray = Consts.checkThingTypeIds;
                    else if (variableObj.variableType.Equals("Jump")) idsArray = Consts.jumpTypeIds;
                    else if (variableObj.variableType.Equals("FloatSource")) idsArray = Consts.floatSourceTypeIds;
                    else if (variableObj.variableType.Equals("ObjectSource")) idsArray = Consts.objectSourceTypeIds;

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
                        GenerateSwitchStatement(nodeName, variableObj, idsArray, mainClassName, typeClassFileName, true);
                        CloseBlock();
                    } else
                    {
                        AddToFileContents($"if (GetPort(\"{variableObj.name}\").ConnectionCount > 0)");
                        OpenBlock();
                        {
                            AddToFileContents($"{typeClassFileName}Node {nodeName} = ({typeClassFileName}Node)GetPort(\"{variableObj.name}\").GetConnection(0).node;");
                            GenerateSwitchStatement(nodeName, variableObj, idsArray, mainClassName, typeClassFileName, false);
                        }
                        CloseBlock();
                    }
                }
                AddToFileContents($"return {mainClassName};");
            }
            CloseBlock();
        }

        private static void GenerateSwitchStatement(string nodeName, VariableObj variableObj, string[] idsArray, string mainClassName, string typeClassFileName, bool isList)
        {
            if (idsArray != null)
            {
                AddToFileContents($"switch ({nodeName}.TID)");
                OpenBlock();
                foreach (string id in idsArray)
                {
                    AddToFileContents($"case {variableObj.variableType}.TypeId.{id}:");
                    UpdateIndent(1);
                    {
                        if (isList)
                        {
                            if (isSAOrderedSensitive)
                            {
                                AddToFileContents($"{id}Node {id}_{nodeName} = ({id}Node)port.Connection.node;");
                            } else
                            {
                                AddToFileContents($"{id}Node {id}_{nodeName} = ({id}Node)port.node;");
                            }
                            AddToFileContents($"{mainClassName}.{variableObj.name}.Add({id}_{nodeName}.GetData());");
                        } else
                        {
                            AddToFileContents($"{id}Node {id}_{nodeName} = ({id}Node)GetPort(\"{variableObj.name}\").GetConnection(0).node;");
                            AddToFileContents($"{mainClassName}.{variableObj.name} = {id}_{nodeName}.GetData();");
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
