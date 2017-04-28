using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class UnstringStatementEnd : CodeElementEnd
    {
        public UnstringStatementEnd() : base(CodeElementType.UnstringStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
