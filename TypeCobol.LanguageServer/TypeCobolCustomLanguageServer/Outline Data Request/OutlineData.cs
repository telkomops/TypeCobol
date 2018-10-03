using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

namespace TypeCobol.LanguageServer.TypeCobolCustomLanguageServerProtocol
{
    public class OutlineData
    {
        public class Node
        {
            public String id;
            public string name;
            public string value;
            public string parent;
        }

        public Node[] data;

        public OutlineData(Node[] data)
        {
            this.data = data;
        }
    }
}
