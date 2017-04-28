using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class InvokeStatementEnd : CodeElementEnd
    {
        public InvokeStatementEnd() : base(CodeElementType.InvokeStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
