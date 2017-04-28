using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class MultiplyStatementEnd : CodeElementEnd
    {
        public MultiplyStatementEnd() : base(CodeElementType.MultiplyStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
