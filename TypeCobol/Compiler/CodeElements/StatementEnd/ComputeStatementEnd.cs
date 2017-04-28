using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class ComputeStatementEnd : CodeElementEnd
    {
        public ComputeStatementEnd() : base(CodeElementType.ComputeStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
