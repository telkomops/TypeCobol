using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Symbols;
using TypeCobol.DocumentModel.Dom.Visitor;

namespace TypeCobol.DocumentModel.Phase
{
    /// <summary>
    /// The class implemenst the second phase of the enter processing, it enter in their
    /// corresponding scope all DataDivision definitions.
    /// </summary>
    public class StorageEnter : CodeDomVisitor<Context<TypeCobolSymbol>, Context<TypeCobolSymbol>>
    {
    }
}
