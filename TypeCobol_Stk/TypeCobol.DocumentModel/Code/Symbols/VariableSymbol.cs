using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.DocumentModel.Code.Symbols
{
    /// <summary>
    /// The Symbol of a Variable declaration
    /// </summary>
    public class VariableSymbol : TypeCobolSymbol
    {
        /// <summary>
        /// Named constructor
        /// </summary>
        /// <param name="name"></param>
        public VariableSymbol(String name)
            : base(name, Kinds.Variable)
        {
        }

        /// <summary>
        /// Level of this variable.
        /// </summary>
        public String Level
        {
            get;
            set;
        }
    }
}
