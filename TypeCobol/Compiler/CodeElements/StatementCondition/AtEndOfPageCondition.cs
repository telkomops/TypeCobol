using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class AtEndOfPageCondition : CodeElement
    {
        public AtEndOfPageCondition() : base(CodeElementType.AtEndOfPageCondition)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

    }
}
