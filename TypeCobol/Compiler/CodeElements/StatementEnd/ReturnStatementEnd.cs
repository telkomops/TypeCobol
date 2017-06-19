using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class ReturnStatementEnd : CodeElementEnd
    {
        public ReturnStatementEnd() : base(CodeElementType.ReturnStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
