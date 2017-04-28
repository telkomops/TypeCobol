using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class DivideStatementEnd : CodeElementEnd
    {
        public DivideStatementEnd() : base(CodeElementType.DivideStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
