using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotOnExceptionCondition : CodeElement
    {
        public NotOnExceptionCondition() : base(CodeElementType.NotOnExceptionCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
