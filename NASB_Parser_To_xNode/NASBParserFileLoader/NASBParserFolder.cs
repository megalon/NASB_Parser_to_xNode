using System;
using System.Collections.Generic;
using System.Text;

namespace NASB_Parser_To_xNode
{
    public class NASBParserFolder
    {
        public string folderName;
        public string sourceClass;

        public NASBParserFolder(string folderName, string sourceClass)
        {
            this.folderName = folderName;
            this.sourceClass = sourceClass;
        }
    }
}
