using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Symbols;

namespace TypeCobol.DocumentModel.Code.Scopes
{
    /// <summary>
    /// The Global Symbol Table is a special Namespace
    /// </summary>
    public class GlobalSymbolTable : NamespaceSymbol
    {
        /// <summary>
        /// Empty Constructor.
        /// </summary>
        public GlobalSymbolTable() : base("<<Global>>")
        {
            base.Kind = Kinds.Global;
        }
    }
}
