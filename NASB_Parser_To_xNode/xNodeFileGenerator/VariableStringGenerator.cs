using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NASB_Parser_To_xNode
{
    public class VariableStringGenerator
    {
        public static string GetVariableString(VariableObj variableObj, NASBParserFile nasbParserFile, bool isNested)
        {
            //var accString = Utils.GetAccessabilityLevelString(variableObj.accessability);
            var accString = "public";
            string relativeNamespace = "";

            // Special case for TID and Version 
            if ((variableObj.name.Equals("TID") && variableObj.variableType.Equals("TypeId"))
                || (variableObj.name.Equals("Version") && variableObj.variableType.Equals("int")))
            {
                accString = "protected";

                if (variableObj.variableType.Equals("TypeId"))
                    relativeNamespace = Utils.GetRelativeNamespace(nasbParserFile) + ".";
            }

            var startOfLine = $"{accString} {(variableObj.isStatic ? "static " : "")}{(variableObj.isReadonly ? "readonly " : "")}";

            // Handle Vector3 ambiguity
            if (variableObj.variableType.Equals("Vector3")) variableObj.variableType = "NASB_Parser.Vector3";

            // Handle List
            var fullType = GetFullType(variableObj);

            if (Consts.basicTypes.Contains(variableObj.variableType))
            {
                return ($"{startOfLine}{relativeNamespace}{fullType} {variableObj.name};");
            }

            // If the name matches enum only classes
            if (Consts.enumOnlyFiles.Contains(variableObj.variableType))
            {
                return ($"{startOfLine}{relativeNamespace}{fullType} {variableObj.name};");
            }

            // If type is an enum contained within the class
            if (nasbParserFile.enums.Any(x => x.name.Equals(variableObj.variableType)))
            {
                if (!isNested)
                {
                    relativeNamespace = Utils.GetRelativeNamespace(nasbParserFile) + ".";
                }

                return ($"{startOfLine}{relativeNamespace}{fullType} {variableObj.name};");
            }

            return ($"[Output] public {relativeNamespace}{fullType} {variableObj.name};");
        }

        public static string GetFullType(VariableObj variableObj)
        {
            return (variableObj.isList ? $"List<{variableObj.variableType}>" : variableObj.variableType);
        }
    }
}
