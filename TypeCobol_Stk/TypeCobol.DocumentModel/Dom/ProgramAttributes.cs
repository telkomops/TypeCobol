using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.DocumentModel.Dom
{
    public class ProgramAttributes : CodeElementGroup
    {
        /// <summary>
        /// Program identification Code Element
        /// </summary>
        public TypeCobol.Compiler.CodeElements.ProgramIdentification ProgramIdentification
        {
            get
            {
                return (TypeCobol.Compiler.CodeElements.ProgramIdentification)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        /// <summary>
        /// Optional Library Copy
        /// </summary>
        public TypeCobol.Compiler.CodeElements.LibraryCopyCodeElement LibraryCopyOpt
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ProgramAttributes()
            : base(CodeDomType.ProgramAttributes)
        {
        }

        public ProgramAttributes(TypeCobol.Compiler.CodeElements.ProgramIdentification prdId, TypeCobol.Compiler.CodeElements.LibraryCopyCodeElement libCopy)
            : base(CodeDomType.ProgramAttributes)
        {
            ProgramIdentification = prdId;
            LibraryCopyOpt = libCopy;
        }

        public override void Accept<R, D>(TypeCobol.DocumentModel.Dom.Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<Compiler.CodeElements.CodeElement> GetEnumerator()
        {
            if (this.ProgramIdentification != null)
                yield return this.ProgramIdentification;
            if (this.LibraryCopyOpt != null)
                yield return this.LibraryCopyOpt;
        }
    }
}
