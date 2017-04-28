using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class OnOverflowCondition : CodeElement
    {
        public OnOverflowCondition() : base(CodeElementType.OnOverflowCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
