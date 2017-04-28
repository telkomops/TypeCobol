using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class SubtractStatementEnd : CodeElementEnd
    {
        public SubtractStatementEnd() : base(CodeElementType.SubtractStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
