using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotOnOverflowCondition : CodeElement
    {
        public NotOnOverflowCondition() : base(CodeElementType.NotOnOverflowCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
