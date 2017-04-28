using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class ReadStatementEnd : CodeElementEnd
    {
        public ReadStatementEnd() : base(CodeElementType.ReadStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
