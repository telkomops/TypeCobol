using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class SearchStatementEnd : CodeElementEnd
    {
        public SearchStatementEnd() : base(CodeElementType.SearchStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
