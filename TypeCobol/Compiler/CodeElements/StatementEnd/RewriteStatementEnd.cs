using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class RewriteStatementEnd : CodeElementEnd
    {
        public RewriteStatementEnd() : base(CodeElementType.RewriteStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
