using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class ReturnStatementEnd : CodeElementEnd
    {
        public ReturnStatementEnd() : base(CodeElementType.ReturnStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
