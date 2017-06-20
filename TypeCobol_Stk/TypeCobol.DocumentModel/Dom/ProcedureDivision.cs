using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom
{
    /// <summary>
    /// The COBOL Procedure Division
    /// </summary>
    public class ProcedureDivision : CodeElementGroup
    {
        /// <summary>
        /// The Procedure Division Header Code Element
        /// </summary>
        public ProcedureDivisionHeader ProcedureDivisionHeader
        {
            get
            {
                return (ProcedureDivisionHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        /// <summary>
        /// The Declaratives
        /// </summary>
        public Declaratives Declaratives
        {
            get;
            set;
        }

        /// <summary>
        /// The elments inside this Procedure Division : Functions or Sections...
        /// </summary>
        public ProcedureDivisionElements Elements
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ProcedureDivision()
            : base(CodeDomType.ProcedureDivision)
        {
        }

        /// <summary>
        /// Header Constructor
        /// </summary>
        /// <param name="header">The Procedure Division Header Element</param>
        public ProcedureDivision(ProcedureDivisionHeader header)
            : base((CodeElementType)CodeDomType.ProcedureDivision, header)
        {
        }

        /// <summary>
        /// Header, Sections Constructor
        /// </summary>
        /// <param name="header"></param>
        /// <param name="sections"></param>
        public ProcedureDivision(ProcedureDivisionHeader header, Sections sections)
            : base((CodeElementType)CodeDomType.ProcedureDivision, header)
        {
            if (sections != null)
            {
                this.Elements = new ProcedureDivisionElements();
                foreach (Section s in sections)
                {
                    this.Elements.Add(new ProcedureDivisionElement.Section(s));
                }
            }
        }

        public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.ProcedureDivisionHeader != null)
                yield return this.ProcedureDivisionHeader;
            if (this.Declaratives != null)
                yield return this.Declaratives;
            if (this.Elements != null)
            {
                foreach (ProcedureDivisionElement e in this.Elements)
                {
                    yield return e.Target;
                }
            }
        }

    }

    /// <summary>
    /// An Element inside the Procedure division
    /// </summary>
    public class ProcedureDivisionElement : CodeElementProxy<CodeElement>
    {
        public ProcedureDivisionElement(CodeElement element)
            : base(element)
        {
        }

        /// <summary>
        /// A Function Declaration
        /// </summary>
        public class Function : ProcedureDivisionElement
        {
            FunctionDeclaration FunctionDeclaration
            {
                get
                {
                    return (FunctionDeclaration)base.Target;
                }
                set
                {
                    base.Target = value;
                }
            }
            /// <summary>
            /// Cosntrcutor
            /// </summary>
            /// <param name="funDecl"></param>
            public Function(FunctionDeclaration funDecl)
                : base(funDecl)
            {
            }
        }

        /// <summary>
        /// A Section Declaration
        /// </summary>
        public class Section : ProcedureDivisionElement
        {
            public TypeCobol.DocumentModel.Dom.Section SectionDeclaration
            {
                get
                {
                    return (TypeCobol.DocumentModel.Dom.Section)base.Target;
                }
                set
                {
                    base.Target = value;
                }
            }
            /// <summary>
            /// Cosntructor.
            /// </summary>
            /// <param name="section"></param>
            public Section(TypeCobol.DocumentModel.Dom.Section section)
                : base(section)
            {
            }
        }
    }

    /// <summary>
    /// The List of Elements inside the Procedure Division.
    /// </summary>
    public class ProcedureDivisionElements : List<ProcedureDivisionElement>
    {
        public ProcedureDivisionElements()
        {
        }
    }

    /// <summary>
    /// A Sentence
    /// </summary>
    public class Sentence : CodeElementGroup
    {
        public Sentence()
            : base(CodeDomType.Sentence)
        {
        }

        /// <summary>
        /// Sentence End constructor.
        /// </summary>
        /// <param name="SentenceEnd"></param>
        public Sentence(TypeCobol.Compiler.CodeElements.SentenceEnd SentenceEnd)
            : this(null, SentenceEnd)
        {
            this.SentenceEnd = SentenceEnd;
        }

        /// <summary>
        /// Exec statement constructor.
        /// </summary>
        /// <param name="execStmt"></param>
        public Sentence(SingleStatement.ExecStatement execStmt)
            : this(new Statements() { execStmt }, null)
        {
            this.SentenceEnd = SentenceEnd;
        }

        /// <summary>
        /// Statements, Sentence End constructor.
        /// </summary>
        /// <param name="SentenceEnd"></param>
        public Sentence(Statements Statements, TypeCobol.Compiler.CodeElements.SentenceEnd SentenceEnd)
            : base(CodeDomType.Sentence)
        {
            this.Statements = Statements;
            this.SentenceEnd = SentenceEnd;
        }

        /// <summary>
        /// Optional All statements
        /// </summary>
        public Statements Statements
        {
            get;
            set;
        }
        /// <summary>
        /// Optional End Sentence Code element
        /// </summary>
        public TypeCobol.Compiler.CodeElements.SentenceEnd SentenceEnd
        {
            get;
            set;
        }

        public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.Statements != null)
            {
                foreach (Statement stmt in this.Statements)
                {
                    yield return stmt;
                }
            }
            if (this.SentenceEnd != null)
                yield return this.SentenceEnd;
        }
    }

    /// <summary>
    /// The List of Sentences
    /// </summary>
    public class Sentences : List<Sentence>
    {
        public Sentences()
        {
        }
    }

    /// <summary>
    /// A Cobol Paragraph
    /// </summary>
    public class Paragraph : CodeElementGroup
    {
        /// <summary>
        /// The ParagraphHeader CodeElement if any, can be null;
        /// </summary>
        public ParagraphHeader ParagraphHeader
        {
            get
            {
                return (ParagraphHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        /// <summary>
        /// All Sentences
        /// </summary>
        public Sentences Sentences
        {
            get;
            set;
        }

        /// <summary>
        /// Paragrah Header constructor
        /// </summary>
        /// <param name="header">ParagraphHeader</param>
        /// <param name="sentences">Sentences</param>
        public Paragraph(ParagraphHeader header, Sentences sentences)
            : base((CodeElementType)CodeDomType.Paragraph, header)
        {
            this.Sentences = sentences;
        }

        /// <summary>
        /// Sentences constructor
        /// </summary>
        /// <param name="sentences"></param>
        public Paragraph(Sentences sentences)
            : base(CodeDomType.Paragraph)
        {
            this.Sentences = sentences;
        }

        public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.ParagraphHeader != null)
                yield return this.ParagraphHeader;
            if (this.Sentences != null)
            {
                foreach (Sentence s in this.Sentences)
                    yield return s;
            }
        }
    }

    /// <summary>
    /// A List of Paragraph
    /// </summary>
    public class Paragraphs : List<Paragraph>
    {
        public Paragraphs()
        {
        }
    }

    /// <summary>
    /// A Cobol Section
    /// </summary>
    public abstract class Section : CodeElementGroup
    {
        /// <summary>
        /// Type Target Proxy CodeElement constructor
        /// </summary>
        /// <param name="type">Code Element Type</param>
        /// <param name="target">Proxy Element</param>
        public Section(CodeElementType type, CodeElement target)
            : base(type, target)
        {
        }

        public Section(CodeDomType type)
            : base(type)
        {
        }

        /// <summary>
        /// The Standard section
        /// </summary>
        public class Standard : TypeCobol.DocumentModel.Dom.Section
        {
            /// <summary>
            /// The SectionHeader
            /// </summary>
            public SectionHeader SectionHeader
            {
                get
                {
                    return (SectionHeader)base.Target;
                }
                set
                {
                    base.Target = value;
                }
            }

            /// <summary>
            /// This is used by a Declaration Section
            /// </summary>
            protected UseStatement UseStatement
            {
                get;
                set;
            }

            /// <summary>
            /// List of Paragraphs.
            /// </summary>
            public Paragraphs Paragraphs
            {
                get;
                set;
            }

            /// <summary>
            /// Header, Paragraphs constructor.
            /// </summary>
            /// <param name="header">The Section Header</param>
            /// <param name="paragraphs">List of Pargraphs</param>
            public Standard(SectionHeader header, Paragraphs paragraphs)
                : this(CodeDomType.SectionStandard, header, paragraphs)
            {
            }

            /// <summary>
            /// Specialization constructor
            /// </summary>
            /// <param name="type"></param>
            /// <param name="header"></param>
            /// <param name="paragraphs"></param>
            protected Standard(CodeDomType type, SectionHeader header, Paragraphs paragraphs)
                : base((CodeElementType)type, header)
            {
                this.Paragraphs = paragraphs;
            }

            public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
            {
                return v.Visit(this, data);
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.SectionHeader != null)
                    yield return this.SectionHeader;
                if (this.UseStatement != null)
                    yield return this.UseStatement;
                if (this.Paragraphs != null)
                {
                    foreach (TypeCobol.DocumentModel.Dom.Paragraph p in this.Paragraphs)
                        yield return p;
                }
            }
        }

        public class Paragraph : TypeCobol.DocumentModel.Dom.Section
        {
            /// <summary>
            /// The ParagraphHeader
            /// </summary>
            public ParagraphHeader ParagraphHeader
            {
                get
                {
                    return (ParagraphHeader)base.Target;
                }
                set
                {
                    base.Target = value;
                }
            }

            /// <summary>
            /// List of Paragraphs.
            /// </summary>
            public Paragraphs Paragraphs
            {
                get;
                set;
            }

            public Paragraph(ParagraphHeader header, Paragraphs paragraphs)
                : base((CodeElementType)CodeDomType.SectionParagraph, header)
            {
                this.Paragraphs = paragraphs;
            }

            public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
            {
                return v.Visit(this, data);
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.ParagraphHeader != null)
                    yield return this.ParagraphHeader;
                if (this.Paragraphs != null)
                {
                    foreach (TypeCobol.DocumentModel.Dom.Paragraph p in this.Paragraphs)
                        yield return p;
                }
            }
        }

        /// <summary>
        /// A Section of Sentences
        /// </summary>
        public class Sentences : TypeCobol.DocumentModel.Dom.Section
        {
            /// <summary>
            /// All Sentences
            /// </summary>
            public TypeCobol.DocumentModel.Dom.Sentences All
            {
                get;
                set;
            }
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="sentences"></param>
            public Sentences(TypeCobol.DocumentModel.Dom.Sentences sentences) : base(CodeDomType.SectionSentences)
            {
                this.All = sentences;
            }

            public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
            {
                return v.Visit(this, data);
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.All != null)
                {
                    foreach (Sentence s in All)
                    {
                        yield return s;
                    }
                }
            }
        }
        /// <summary>
        /// A Section from Declaratives
        /// </summary>
        public class Declarative : Standard
        {
            /// <summary>
            /// This is used by a Declaration Section
            /// </summary>
            public new UseStatement UseStatement
            {
                get
                {
                    return base.UseStatement;
                }
                set
                {
                    base.UseStatement = value;
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="header"></param>
            /// <param name="use_stmt"></param>
            /// <param name="paragraphs"></param>
            public Declarative(SectionHeader header, UseStatement use_stmt, Paragraphs paragraphs)
                : base(CodeDomType.SectionDeclarative, header, paragraphs)
            {
                this.UseStatement = use_stmt;                
            }

            public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
            {
                return v.Visit(this, data);
            }
        }
    }

    /// <summary>
    /// The class List of Section
    /// </summary>
    public class Sections : List<Section>
    {
        public Sections()
        {
        }
    }

    /// <summary>
    /// COBOL Declaratives
    /// </summary>
    public class Declaratives : CodeElementGroup
    {
        /// <summary>
        /// The Declarative Header
        /// </summary>
        public DeclarativesHeader DeclarativesHeader
        {
            get
            {
                return (DeclarativesHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        /// <summary>
        /// All Section.Declarative elements.
        /// </summary>
        public Sections Sections
        {
            get;
            set;
        }

        /// <summary>
        /// End code element
        /// </summary>
        public DeclarativesEnd DeclarativesEnd
        {
            get;
            set;
        }
        /// <summary>
        /// Cosntructor
        /// </summary>
        /// <param name="header"></param>
        /// <param name="sections"></param>
        /// <param name="end"></param>
        public Declaratives(DeclarativesHeader header, Sections sections, DeclarativesEnd end)
            : base((CodeElementType)CodeDomType.Declaratives, header)
        {
            this.Sections = sections;
            this.DeclarativesEnd = end;
        }

        public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.DeclarativesHeader != null)
                yield return DeclarativesHeader;
            if (this.Sections != null)
            {
                foreach (Section.Standard s in Sections)
                    yield return s;
            }
            if (this.DeclarativesEnd != null)
                yield return DeclarativesEnd;
        }
    }
}
