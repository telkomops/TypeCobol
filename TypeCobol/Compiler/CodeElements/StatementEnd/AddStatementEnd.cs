using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class AddStatementEnd : CodeElementEnd
    {
        public AddStatementEnd() : base(CodeElementType.AddStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

    }
}
