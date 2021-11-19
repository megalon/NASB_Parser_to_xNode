using System;
using System.Collections.Generic;
using System.Text;

namespace NASB_Parser_To_xNode
{
    class NASBParserFile
    {
        public string relativePath;
        public List<string> imports;
        public string @namespace;
        public AccessabilityLevel classType;
        public bool isAbstract;
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

    class VariableObj : BaseObj
    {
        public string variableType;
        public bool isList;
    }

    class EnumObj : BaseObj
    {
        public List<string> enumNames;

        public EnumObj()
        {
            enumNames = new List<string>();
        }
    }

    class BaseObj
    {
        public string name;
        public AccessabilityLevel accessability;
    }

    enum AccessabilityLevel
    {
        Public,
        Private,
        Protected
    }
}
