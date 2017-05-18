using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom
{
    /// <summary>
    /// Base class for all Statements, basically a Statement is considered as a group of
    /// CodeElement.
    /// </summary>
    public abstract class Statement : CodeElementGroup
    {
        public Statement(CodeDomType type)
            : base(type)
        {
        }
    }
}
