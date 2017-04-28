using System;

namespace TypeCobol.Compiler.CodeElements
{
    /// <summary>
    /// The FILE-CONTROL paragraph associates each file in the COBOL program with
    /// an external data set, and specifies file organization, access mode, and other
    /// information.
    /// </summary>
    public class FileControlParagraphHeader : CodeElement
    {
        public FileControlParagraphHeader() : base(CodeElementType.FileControlParagraphHeader)
        { }
        public override void Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }
    }
}
