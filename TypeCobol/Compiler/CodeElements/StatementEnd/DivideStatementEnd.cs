using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class DivideStatementEnd : CodeElementEnd
    {
        public DivideStatementEnd() : base(CodeElementType.DivideStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
