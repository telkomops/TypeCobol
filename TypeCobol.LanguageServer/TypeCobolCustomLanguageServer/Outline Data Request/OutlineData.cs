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
            public string id;
            public string name;
            public string parent;
            public string line;
            public Node[] childNodes;
        }

        public Node[] data;

        public OutlineData(Node[] data)
        {
            this.data = data;
        }
    }
}
