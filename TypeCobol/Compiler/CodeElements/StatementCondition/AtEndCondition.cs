using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class AtEndCondition : CodeElement
    {
        public AtEndCondition() : base(CodeElementType.AtEndCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
