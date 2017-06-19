using System;

namespace TypeCobol.Compiler.CodeElements
{
    /// <summary>
    /// Sentence
    /// One or more statements terminated by a separator period.
    /// </summary>
    public class SentenceEnd : CodeElementEnd
    {
        public SentenceEnd() : base(CodeElementType.SentenceEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
