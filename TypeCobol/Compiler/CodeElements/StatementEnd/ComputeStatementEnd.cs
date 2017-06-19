using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class ComputeStatementEnd : CodeElementEnd
    {
        public ComputeStatementEnd() : base(CodeElementType.ComputeStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
