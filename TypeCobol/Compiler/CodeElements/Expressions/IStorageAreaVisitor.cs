using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.CodeElements.Expressions
{
    /// <summary>
    /// Interface for a Visitor on a Storage Area only
    /// </summary>
    /// <typeparam name="R">The return type</typeparam>
    /// <typeparam name="D">The data argument type</typeparam>
    public interface IStorageAreaVisitor<R, D>
    {
        R Visit(TypeCobol.Compiler.CodeElements.DataOrConditionStorageArea that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.IntrinsicStorageArea that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.IndexStorageArea that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.StorageAreaPropertySpecialRegister that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.FilePropertySpecialRegister that, D data);
        R Visit(TypeCobol.Compiler.CodeElements.FunctionCallResult that, D data);                                
    }
}
