using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Symbols;

namespace TypeCobol.DocumentModel.Code.Types
{
    /// <summary>
    /// Class that represents a record type
    /// </summary>
    public class RecordType : TypeCobolType
    {
        public RecordType() : base (Tags.Record)
        {

        }
        /// <summary>
        /// Fields in this Records.
        /// </summary>
        public List<VariableSymbol> Fields
        {
            get;
            set;
        }
    }    
}
