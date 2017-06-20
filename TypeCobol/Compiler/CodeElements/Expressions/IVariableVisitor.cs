using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.CodeElements.Expressions
{
    /// <summary>
    /// Interface for a visitor on variables only
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="D"></typeparam>
    public interface IVariableVisitor<R, D>
    {
        R Visit(TypeCobol.Compiler.CodeElements.IntegerVariable that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.NumericVariable that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.CharacterVariable that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.AlphanumericVariable that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.SymbolReferenceVariable that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.Variable that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.ReceivingStorageArea that, D data);        
    }
}
