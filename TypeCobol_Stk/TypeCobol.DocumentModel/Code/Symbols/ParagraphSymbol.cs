using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.DocumentModel.Code.Symbols
{
    /// <summary>
    /// A Symbol that represents a Cobol Paragraph
    /// </summary>
    public class ParagraphSymbol : TypeCobolSymbol
    {
        /// <summary>
        /// Named Constructor
        /// </summary>
        /// <param name="name"></param>
        public ParagraphSymbol(String name)
            : base(name, Kinds.Paragraph)
        {
        }
    }
}
