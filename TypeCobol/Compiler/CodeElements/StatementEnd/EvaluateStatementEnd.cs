using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class EvaluateStatementEnd : CodeElementEnd
    {
        public EvaluateStatementEnd() : base(CodeElementType.EvaluateStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
