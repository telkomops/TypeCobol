using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class CallStatementEnd : CodeElementEnd
    {
        public CallStatementEnd() : base(CodeElementType.CallStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
