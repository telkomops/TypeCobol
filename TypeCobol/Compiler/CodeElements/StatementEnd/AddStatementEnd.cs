using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class AddStatementEnd : CodeElementEnd
    {
        public AddStatementEnd() : base(CodeElementType.AddStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

    }
}
