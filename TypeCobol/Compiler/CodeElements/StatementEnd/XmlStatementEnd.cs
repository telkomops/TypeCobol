using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class XmlStatementEnd : CodeElementEnd
    {
        public XmlStatementEnd() : base(CodeElementType.XmlStatementEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
