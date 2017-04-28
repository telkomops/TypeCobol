using System;

namespace TypeCobol.Compiler.CodeElements
{
    public class XmlStatementEnd : CodeElementEnd
    {
        public XmlStatementEnd() : base(CodeElementType.XmlStatementEnd)
        { }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
