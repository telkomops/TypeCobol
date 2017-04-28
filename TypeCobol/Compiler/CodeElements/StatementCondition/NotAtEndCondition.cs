using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotAtEndCondition : CodeElement
    {
        public NotAtEndCondition() : base(CodeElementType.NotAtEndCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
