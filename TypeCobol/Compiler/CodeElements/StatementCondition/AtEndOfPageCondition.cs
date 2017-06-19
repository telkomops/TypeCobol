using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class AtEndOfPageCondition : CodeElement
    {
        public AtEndOfPageCondition() : base(CodeElementType.AtEndOfPageCondition)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

    }
}
