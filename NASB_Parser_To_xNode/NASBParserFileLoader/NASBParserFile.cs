using System;
using System.Collections.Generic;
using System.Text;

namespace NASB_Parser_To_xNode
{
    public class NASBParserFile
    {
        public string relativePath;
        public List<string> imports;
        public string @namespace;
        public AccessabilityLevel classType;
        public bool isAbstract;
        public bool isStatic;
        public string className;
        public string parentClass;
        public List<VariableObj> variables;
        public List<EnumObj> enums;
        public List<NASBParserFile> nestedClasses;

        public NASBParserFile()
        {
            imports = new List<string>();
            variables = new List<VariableObj>();
            enums = new List<EnumObj>();
            nestedClasses = new List<NASBParserFile>();
        }
    }

    public class VariableObj : BaseObj
    {
        public string variableType;
        public bool isList;
        public bool isStatic;
        public bool isReadonly;
    }

    public class EnumObj : BaseObj
    {
        public List<string> enumNames;

        public EnumObj()
        {
            enumNames = new List<string>();
        }
    }

    public abstract class BaseObj
    {
        public string name;
        public AccessabilityLevel accessability;
    }

    public enum AccessabilityLevel
    {
        Public,
        Private,
        Protected
    }
}
