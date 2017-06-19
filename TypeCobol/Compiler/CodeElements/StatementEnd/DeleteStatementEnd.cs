using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class DeleteStatementEnd : CodeElementEnd
    {
        public DeleteStatementEnd() : base(CodeElementType.DeleteStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
