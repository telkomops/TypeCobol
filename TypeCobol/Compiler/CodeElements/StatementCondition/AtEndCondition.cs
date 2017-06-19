using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class AtEndCondition : CodeElement
    {
        public AtEndCondition() : base(CodeElementType.AtEndCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
