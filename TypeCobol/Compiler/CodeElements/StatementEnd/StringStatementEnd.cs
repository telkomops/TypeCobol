using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class StringStatementEnd : CodeElementEnd
    {
        public StringStatementEnd() : base(CodeElementType.StringStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
