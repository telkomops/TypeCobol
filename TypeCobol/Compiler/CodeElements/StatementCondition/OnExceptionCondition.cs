using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class OnExceptionCondition : CodeElement
    {
        public OnExceptionCondition() : base(CodeElementType.OnExceptionCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
