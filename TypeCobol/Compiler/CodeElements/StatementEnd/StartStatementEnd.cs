using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class StartStatementEnd : CodeElementEnd
    {
        public StartStatementEnd() : base(CodeElementType.StartStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
