using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotAtEndOfPageCondition : CodeElement
    {
        public NotAtEndOfPageCondition() : base(CodeElementType.NotAtEndOfPageCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
