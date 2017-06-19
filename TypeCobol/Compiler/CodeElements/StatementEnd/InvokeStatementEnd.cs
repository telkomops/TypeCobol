using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class InvokeStatementEnd : CodeElementEnd
    {
        public InvokeStatementEnd() : base(CodeElementType.InvokeStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
