using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class StartStatementEnd : CodeElementEnd
    {
        public StartStatementEnd() : base(CodeElementType.StartStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
