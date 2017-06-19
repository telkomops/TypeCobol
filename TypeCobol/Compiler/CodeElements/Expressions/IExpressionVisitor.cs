using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.CodeElements.Expressions
{
    /// <summary>
    /// Interface for a visitor on expression only
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="D"></typeparam>
    public interface IExpressionVisitor<R,D>
    {
        R Visit(TypeCobol.Compiler.CodeElements.LogicalOperation that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.ClassCondition that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.ConditionNameConditionOrSwitchStatusCondition that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.RelationCondition that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.SignCondition that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.ConditionOperand that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.ArithmeticOperation that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.NumericVariableOperand that, D data);
    }
}
