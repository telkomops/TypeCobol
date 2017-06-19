using System;

namespace TypeCobol.Compiler.CodeElements
{
    /// <summary>
    /// Factory IDENTIFICATION DIVISION
    /// A factory IDENTIFICATION DIVISION contains only a factory paragraph
    /// header.
    /// </summary>
    public class FactoryIdentification : CodeElement
    {
        public FactoryIdentification() : base(CodeElementType.FactoryIdentification)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
