using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class InvalidKeyCondition : CodeElement
    {
        public InvalidKeyCondition() : base(CodeElementType.InvalidKeyCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
