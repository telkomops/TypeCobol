using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class WriteStatementEnd : CodeElementEnd
    {
        public WriteStatementEnd() : base(CodeElementType.WriteStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
