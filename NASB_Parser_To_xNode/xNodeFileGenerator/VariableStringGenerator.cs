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
            var accString = "public";
            string relativeNamespace = "";

            // Special case for TID
            if (variableObj.name.Equals("TID") && variableObj.variableType.Equals("TypeId"))
            {
                accString = "[HideInInspector] public";

                if (variableObj.variableType.Equals("TypeId"))
                    relativeNamespace = Utils.GetRelativeNamespace(nasbParserFile) + ".";
            }

            var startOfLine = $"{accString} {(variableObj.isStatic ? "static " : "")}{(variableObj.isReadonly ? "readonly " : "")}";

            // Handle Vector3 ambiguity
            if (variableObj.variableType.Equals("Vector3")) variableObj.variableType = "MovesetParser.Unity.Vector3";

            // Handle HBM ambiguity
            if (nasbParserFile.className.EndsWith("_HBM"))
            {
                if (variableObj.variableType.Equals("Manip"))
                {
                    var subStr = nasbParserFile.relativePath.Substring(nasbParserFile.relativePath.IndexOf("\\") + 1);
                    variableObj.variableType = subStr.Substring(0, subStr.LastIndexOf(".") + 1) + "Manip";
                }
            }

            // Handle List
            var fullType = GetFullType(variableObj);

            if (Consts.basicTypes.Contains(variableObj.variableType))
            {
                return ($"{startOfLine}{relativeNamespace}{fullType} {variableObj.name};");
            }

            // If the name matches enum only classes
            if (Consts.enumOnlyFiles.Contains(variableObj.variableType)
                // Special case
                || variableObj.variableType.Equals("SASetFloatTarget.SetFloat.ManipWay"))
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

            var variableClassName = variableObj.variableType;
            if (variableObj.variableType.IndexOf(".") > -1) variableClassName = variableObj.variableType.Substring(variableObj.variableType.LastIndexOf(".") + 1);
            if  (FindClassIncludingNested(variableClassName))
            {
                variableObj.isOutput = true;
                string outputAttributeText = "Output";
                
                if (variableObj.isList)
                    outputAttributeText += "(connectionType = ConnectionType.Multiple)";
                else
                    outputAttributeText += "(connectionType = ConnectionType.Override)";

                return ($"[{outputAttributeText}] public {relativeNamespace}{fullType} {variableObj.name};");
            }

            return ($"{startOfLine}{relativeNamespace}{fullType} {variableObj.name};");
        }

        public static string GetFullType(VariableObj variableObj)
        {
            return (variableObj.isList ? $"List<{variableObj.variableType}>" : variableObj.variableType);
        }

        public static bool FindClassIncludingNested(string className)
        {
            return LookForClassIncludingNestedRecursive(className, Program.nasbParserFiles);
        }

        private static bool LookForClassIncludingNestedRecursive(string className, List<NASBParserFile> parserFiles)
        {
            foreach(NASBParserFile file in parserFiles)
            {
                if (file.className.Equals(className)) return true;
                if (file.nestedClasses != null) {
                    if (LookForClassIncludingNestedRecursive(className, file.nestedClasses))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
