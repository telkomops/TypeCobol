using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.DocumentModel.Dom
{
    /// <summary>
    /// A Cobol Program.
    /// </summary>
    public class CobolProgram : CodeElementGroup
    {
        /// <summary>
        /// Program Attributes
        /// </summary>
        public ProgramAttributes ProgramAttributes
        {
            get;
            set;
        }

        public EnvironmentDivision EnvironmentDivision
        {
            get;
            set;
        }

        public TypeCobol.Compiler.CodeElements.ProgramEnd ProgramEnd
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CobolProgram()
            : base(CodeDomType.CobolProgram)
        {
        }

        public override void Accept<R,D>(TypeCobol.DocumentModel.Dom.Visitor.CodeDomVisitor<R,D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<Compiler.CodeElements.CodeElement> GetEnumerator()
        {
            if (ProgramAttributes != null)
                yield return ProgramAttributes;
            if (EnvironmentDivision != null)
                yield return EnvironmentDivision;
            if (ProgramEnd != null)
                yield return ProgramEnd;
        }
    }
}
