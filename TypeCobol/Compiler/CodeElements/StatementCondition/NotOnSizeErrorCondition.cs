using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotOnSizeErrorCondition : CodeElement
    {
        public NotOnSizeErrorCondition() : base(CodeElementType.NotOnSizeErrorCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
