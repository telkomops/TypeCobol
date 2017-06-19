using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class CallStatementEnd : CodeElementEnd
    {
        public CallStatementEnd() : base(CodeElementType.CallStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
