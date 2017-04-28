using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class OnExceptionCondition : CodeElement
    {
        public OnExceptionCondition() : base(CodeElementType.OnExceptionCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
