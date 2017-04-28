using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotOnExceptionCondition : CodeElement
    {
        public NotOnExceptionCondition() : base(CodeElementType.NotOnExceptionCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
