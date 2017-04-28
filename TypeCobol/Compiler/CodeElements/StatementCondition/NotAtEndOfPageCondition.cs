using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotAtEndOfPageCondition : CodeElement
    {
        public NotAtEndOfPageCondition() : base(CodeElementType.NotAtEndOfPageCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
