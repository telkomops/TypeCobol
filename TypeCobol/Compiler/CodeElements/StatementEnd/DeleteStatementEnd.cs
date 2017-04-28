using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class DeleteStatementEnd : CodeElementEnd
    {
        public DeleteStatementEnd() : base(CodeElementType.DeleteStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
