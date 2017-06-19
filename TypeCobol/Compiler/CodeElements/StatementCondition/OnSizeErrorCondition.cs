using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class OnSizeErrorCondition : CodeElement
    {
        public OnSizeErrorCondition() : base(CodeElementType.OnSizeErrorCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
