using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class PerformStatementEnd : CodeElementEnd
    {
        public PerformStatementEnd() : base(CodeElementType.PerformStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
