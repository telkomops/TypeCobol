using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotAtEndCondition : CodeElement
    {
        public NotAtEndCondition() : base(CodeElementType.NotAtEndCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
