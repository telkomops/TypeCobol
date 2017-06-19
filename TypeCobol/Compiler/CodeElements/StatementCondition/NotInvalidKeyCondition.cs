using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotInvalidKeyCondition : CodeElement
    {
        public NotInvalidKeyCondition() : base(CodeElementType.NotInvalidKeyCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
