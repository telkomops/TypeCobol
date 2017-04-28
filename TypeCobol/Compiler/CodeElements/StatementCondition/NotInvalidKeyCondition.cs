using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class NotInvalidKeyCondition : CodeElement
    {
        public NotInvalidKeyCondition() : base(CodeElementType.NotInvalidKeyCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
