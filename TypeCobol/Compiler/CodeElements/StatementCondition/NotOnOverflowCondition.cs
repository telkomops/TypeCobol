using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotOnOverflowCondition : CodeElement
    {
        public NotOnOverflowCondition() : base(CodeElementType.NotOnOverflowCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
