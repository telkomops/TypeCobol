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

        public DataDivision DataDivision
        {
            get;
            set;
        }

        public ProcedureDivision ProcedureDivision
        {
            get;
            set;
        }

        public NestedPrograms NestedPrograms
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
            : this(CodeDomType.CobolProgram)
        {
        }

        /// <summary>
        /// Specialization constructor.
        /// </summary>
        /// <param name="type"></param>
        protected CobolProgram(CodeDomType type)
            : base(type)
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
            if (this.DataDivision != null)
                yield return this.DataDivision;
            if (this.ProcedureDivision != null)
                yield return ProcedureDivision;
            if (ProgramEnd != null)
                yield return ProgramEnd;
        }
    }

    /// <summary>
    /// Nested Programs List.
    /// </summary>
    public class NestedPrograms : List<CobolProgram>
    {
        public NestedPrograms()
        {
        }   
    }

    /// <summary>
    /// A Function declaration must be seen has a special case of a CobolProgram
    /// </summary>
    public class FunctionDeclaration : CobolProgram
    {
        /// <summary>
        /// The Function declaration Header
        /// </summary>
        public TypeCobol.Compiler.CodeElements.FunctionDeclarationHeader FunctionDeclarationHeader
        {
            get
            {
                return (TypeCobol.Compiler.CodeElements.FunctionDeclarationHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }
        public TypeCobol.Compiler.CodeElements.FunctionDeclarationEnd FunctionDeclarationEnd
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public FunctionDeclaration() : base(CodeDomType.FunctionDeclaration)
        {
        }

        /// <summary>
        /// Header constructor
        /// </summary>
        public FunctionDeclaration(TypeCobol.Compiler.CodeElements.FunctionDeclarationHeader header)
            : base(CodeDomType.FunctionDeclaration)
        {
            this.FunctionDeclarationHeader = header;
        }

        public override void Accept<R, D>(TypeCobol.DocumentModel.Dom.Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<Compiler.CodeElements.CodeElement> GetEnumerator()
        {
            if (this.FunctionDeclarationHeader != null)
                yield return this.FunctionDeclarationHeader;
            if (this.DataDivision != null)
                yield return this.DataDivision;
            if (this.ProcedureDivision != null)
                yield return ProcedureDivision;
            if (this.FunctionDeclarationEnd != null)
                yield return this.FunctionDeclarationEnd;
        }
    }
}
