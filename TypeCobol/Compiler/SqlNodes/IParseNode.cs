using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes
{
    public interface IParseNode
    {
        /**
   * Perform semantic analysis of node and all of its children.
   * Throws exception if any semantic errors were found.
   */
        void analyze(Analyzer analyzer);

        /**
         * Returns the SQL string corresponding to this node.
         */
        string toSql();

    }
}
