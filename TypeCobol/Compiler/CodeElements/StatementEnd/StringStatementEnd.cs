using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class StringStatementEnd : CodeElementEnd
    {
        public StringStatementEnd() : base(CodeElementType.StringStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
