using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.DocumentModel.Code.Symbols;

namespace TypeCobol.DocumentModel.Code.Types
{
    /// <summary>
    /// A Cobol Type
    /// </summary>
    public class TypeCobolType : ISemanticData
    {
        /// <summary>
        /// Type tags
        /// </summary>
        public enum Tags
        {
            Picture,
            Array,
            Pointer,
            Record,
            Program,
            Function,
            Typedef
        }

        /// <summary>
        /// Getter on type tag.
        /// </summary>
        public Tags Tag
        {
            get;
            internal set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tag">TypeCobol type</param>
        protected TypeCobolType(Tags tag)
        {
            this.Tag = tag;
        }

        /// <summary>
        /// The Symbol associated to this type if any: This for a Program or a Function or a TYPEDEF
        /// </summary>
        public TypeCobolSymbol Symbol
        {
            get;
            set;
        }

        public SemanticKinds SemanticKind
        {
            get { return SemanticKinds.Type; }
        }
    }
}
