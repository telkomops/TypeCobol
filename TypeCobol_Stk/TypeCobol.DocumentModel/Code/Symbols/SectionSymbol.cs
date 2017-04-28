using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Scopes;

namespace TypeCobol.DocumentModel.Code.Symbols
{
    /// <summary>
    /// A Symbol that represents a Cobol Section
    /// </summary>
    public class SectionSymbol : TypeCobolSymbol
    {
        public SectionSymbol(String name)
            : base(name, Kinds.Section)
        {
            Paragraphs = new TypeCobolScope<ParagraphSymbol>();
        }

        /// <summary>
        /// Paragraph scope of the Section.
        /// </summary>
        public TypeCobolScope<ParagraphSymbol> Paragraphs
        {
            get;
            protected set;
        }

    }
}
