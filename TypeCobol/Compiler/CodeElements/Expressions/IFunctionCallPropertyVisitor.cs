using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.CodeElements.Expressions
{
    /// <summary>
    /// Interface for a Visitor on Function call properties.
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="D"></typeparam>
    public interface IFunctionCallPropertyVisitor<R, D>
    {
        R Visit(TypeCobol.Compiler.CodeElements.IntrinsicFunctionCall that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.UserDefinedFunctionCall that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.ProcedureCall that, D data);
    }
}
