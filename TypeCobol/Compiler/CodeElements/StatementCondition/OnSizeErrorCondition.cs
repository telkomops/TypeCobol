using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class OnSizeErrorCondition : CodeElement
    {
        public OnSizeErrorCondition() : base(CodeElementType.OnSizeErrorCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
