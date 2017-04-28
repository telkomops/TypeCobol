using System;

namespace TypeCobol.Compiler.CodeElements
{
    /// <summary>
    /// Environment division
    /// </summary>
    public class EnvironmentDivisionHeader : CodeElement
    {
        public EnvironmentDivisionHeader() : base(CodeElementType.EnvironmentDivisionHeader)
        { }
        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
