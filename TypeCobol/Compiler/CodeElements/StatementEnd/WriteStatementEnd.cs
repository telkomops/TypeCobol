using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class WriteStatementEnd : CodeElementEnd
    {
        public WriteStatementEnd() : base(CodeElementType.WriteStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
