using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class SearchStatementEnd : CodeElementEnd
    {
        public SearchStatementEnd() : base(CodeElementType.SearchStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
