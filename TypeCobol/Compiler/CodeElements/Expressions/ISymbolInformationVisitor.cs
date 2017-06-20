using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.CodeElements.Expressions
{
    /// <summary>
    /// Visitor on Symbol Information only
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="D"></typeparam>
    public interface ISymbolInformationVisitor<R, D>
    {
        R Visit(TypeCobol.Compiler.CodeElements.SymbolDefinition that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.SymbolReference that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.AmbiguousSymbolReference that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.QualifiedSymbolReference that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.TypeCobolQualifiedSymbolReference that, D data);         
        R Visit(TypeCobol.Compiler.CodeElements.SymbolDefinitionOrReference that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.ExternalName that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.QualifiedTextName that, D data);     
        R Visit(TypeCobol.Compiler.CodeElements.ExternalNameOrSymbolReference that, D data);     
    }
}
