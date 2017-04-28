using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class InvalidKeyCondition : CodeElement
    {
        public InvalidKeyCondition() : base(CodeElementType.InvalidKeyCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
