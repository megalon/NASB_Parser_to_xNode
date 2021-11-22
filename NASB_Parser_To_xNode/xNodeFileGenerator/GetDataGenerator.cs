using System;
using System.Collections.Generic;
using System.Text;
using static NASB_Parser_To_xNode.xNodeTextGenerator;
using System.Linq;

namespace NASB_Parser_To_xNode
{
    public static class GetDataGenerator
    {
        public static void Generate(NASBParserFile nasbParserFile)
        {
            AddToFileContents($"public {nasbParserFile.className} GetData()");
            OpenBlock();
            {
                var mainClassName = "objToReturn";
                AddToFileContents($"{nasbParserFile.className} {mainClassName} = new {nasbParserFile.className}();");
                foreach (VariableObj variableObj in nasbParserFile.variables)
                {

                    if (variableObj.isOutput)
                    {
                        // ((AgentStateNode)GetPort("State").node).GetData();
                        // AddToFileContents($"{mainClassName}.{variableObj.name} = (({variableObj.variableType}Node)GetPort(\"{variableObj.name}\").node).GetData();");
                        AddToFileContents($"// [Output]");
                        AddToFileContents($"{mainClassName}.{variableObj.name} = {variableObj.name};");
                    } else
                    {
                        AddToFileContents($"{mainClassName}.{variableObj.name} = {variableObj.name};");
                    }
                }
                AddToFileContents($"return {mainClassName};");
            }
            CloseBlock();
        }
    }
}
