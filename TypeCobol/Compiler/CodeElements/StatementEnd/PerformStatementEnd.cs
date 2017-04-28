using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class PerformStatementEnd : CodeElementEnd
    {
        public PerformStatementEnd() : base(CodeElementType.PerformStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
