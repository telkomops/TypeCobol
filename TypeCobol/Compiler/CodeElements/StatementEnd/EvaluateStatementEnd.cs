using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class EvaluateStatementEnd : CodeElementEnd
    {
        public EvaluateStatementEnd() : base(CodeElementType.EvaluateStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
