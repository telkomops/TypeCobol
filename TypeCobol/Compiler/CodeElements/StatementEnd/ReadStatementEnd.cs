using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class ReadStatementEnd : CodeElementEnd
    {
        public ReadStatementEnd() : base(CodeElementType.ReadStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
