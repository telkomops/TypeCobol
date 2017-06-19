using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class SubtractStatementEnd : CodeElementEnd
    {
        public SubtractStatementEnd() : base(CodeElementType.SubtractStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
