using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotOnSizeErrorCondition : CodeElement
    {
        public NotOnSizeErrorCondition() : base(CodeElementType.NotOnSizeErrorCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
