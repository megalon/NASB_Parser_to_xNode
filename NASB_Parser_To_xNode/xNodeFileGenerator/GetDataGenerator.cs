﻿using System;
using System.Collections.Generic;
using System.Text;
using static NASB_Parser_To_xNode.xNodeTextGenerator;
using System.Linq;

namespace NASB_Parser_To_xNode
{
    public static class GetDataGenerator
    {
        private static bool isSAOrderSensitive;
        private static bool isFSFrame;
        public static void Generate(NASBParserFile nasbParserFile)
        {
            isSAOrderSensitive = nasbParserFile.className.Equals("SAOrderSensitive");
            isFSFrame = nasbParserFile.className.Equals("FSFrame");

            bool classWithTID = Utils.HasTypeID(nasbParserFile.className);

            if (classWithTID
                && nasbParserFile.parentClass != null
                && nasbParserFile.parentClass.Equals("IBulkSerializer")
            )
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
                    AddToFileContents($"{mainClassName}.TID = TypeId.{nasbParserFile.className};");
                }

                if (isFSFrame)
                {
                    AddToFileContents($"{mainClassName}.Value = Value;");
                }

                bool firstArrayVariable = true;

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

                    if (variableObj.isList || variableObj.isArray)
                    {
                        if (variableObj.isArray)
                        {
                            AddToFileContents($"{mainClassName}.{variableObj.name} = new {variableObj.variableType}[GetPort(\"{variableObj.name}\").ConnectionCount];");
                            AddToFileContents($"{(firstArrayVariable ? "int " : "")}i = 0;");
                            firstArrayVariable = false;
                        }

                        if (isSAOrderSensitive)
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
                        GenerateSwitchStatement(nodeName, variableObj, idsArray, mainClassName, typeClassFileName);
                        
                        if (variableObj.isArray)
                        {
                            AddToFileContents("++i;");
                        }

                        CloseBlock();
                    }
                    else
                    {
                        AddToFileContents($"if (GetPort(\"{variableObj.name}\").ConnectionCount > 0)");
                        OpenBlock();
                        {
                            AddToFileContents($"{typeClassFileName}Node {nodeName} = ({typeClassFileName}Node)GetPort(\"{variableObj.name}\").GetConnection(0).node;");
                            GenerateSwitchStatement(nodeName, variableObj, idsArray, mainClassName, typeClassFileName);
                        }
                        CloseBlock();
                    }
                }
                AddToFileContents($"return {mainClassName};");
            }
            CloseBlock();
        }

        private static void GenerateSwitchStatement(string nodeName, VariableObj variableObj, string[] idsArray, string mainClassName, string typeClassFileName)
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
                        if (variableObj.isList || variableObj.isArray)
                        {
                            if (isSAOrderSensitive)
                            {
                                AddToFileContents($"{id}Node {id}_{nodeName} = ({id}Node)port.Connection.node;");
                            }
                            else
                            {
                                AddToFileContents($"{id}Node {id}_{nodeName} = ({id}Node)port.node;");
                            }

                            if (variableObj.isList)
                            {
                                AddToFileContents($"{mainClassName}.{variableObj.name}.Add({id}_{nodeName}.GetData());");
                            }
                            else
                            {
                                AddToFileContents($"{mainClassName}.{variableObj.name}[i] = {id}_{nodeName}.GetData();");
                            }
                        }
                        else
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
                if (variableObj.isList)
                {
                    AddToFileContents($"{mainClassName}.{variableObj.name}.Add({nodeName}.GetData());");
                }
                else if (variableObj.isArray)
                {
                    AddToFileContents($"{mainClassName}.{variableObj.name}[i] = {nodeName}.GetData();");
                }
                else
                {
                    AddToFileContents($"{mainClassName}.{variableObj.name} = {nodeName}.GetData();");
                }
            }
        }
    }
}
