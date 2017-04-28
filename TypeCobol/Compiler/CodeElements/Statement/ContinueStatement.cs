using System;

namespace TypeCobol.Compiler.CodeElements
{
    /// <summary>
    /// p319: CONTINUE statement
    /// The CONTINUE statement is a no operation statement. CONTINUE indicates that no executable instruction is present.
    /// </summary>
    public class ContinueStatement : StatementElement
    {
        public ContinueStatement() : base(CodeElementType.ContinueStatement, StatementType.ContinueStatement)
        { }

        public override bool VisitCodeElement(IASTVisitor astVisitor) {
            return base.VisitCodeElement(astVisitor) && astVisitor.Visit(this);
        }

        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
