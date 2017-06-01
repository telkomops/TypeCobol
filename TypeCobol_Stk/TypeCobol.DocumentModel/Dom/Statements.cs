using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom
{
    /// <summary>
    /// Base class for all Statements, basically a Statement is considered as a group of
    /// CodeElement.
    /// </summary>
    public abstract class Statement : CodeElementGroup
    {
        /// <summary>
        /// Code Dom Type constructor
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        public Statement(CodeDomType type)
            : base(type)
        {
        }

        /// <summary>
        /// Code Dom Type constructor of a Single Statement
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        public Statement(CodeElementType type, CodeElement singleStatement)
            : base(type, singleStatement)
        {
        }
    }

    /// <summary>
    /// A List of Statements
    /// </summary>
    public class Statements : List<Statement> 
    {
        public Statements()
        {
        }        
    }

    public class SingleStatement : Statement
    {
        /// <summary>
        /// Code Dom Type constructor of a Single Statement
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        public SingleStatement(CodeElementType type, CodeElement singleStatement)
            : base(type, singleStatement)
        {
        }

	    public class ContinueStatement : SingleStatement
        {
            public ContinueStatement(TypeCobol.Compiler.CodeElements.ContinueStatement singleStmt) : base(CodeElementType.ContinueStatement, singleStmt)
            {
            }
        }
	    public class EntryStatement : SingleStatement
		{
			public EntryStatement(TypeCobol.Compiler.CodeElements.EntryStatement singleStmt) : base(CodeElementType.EntryStatement, singleStmt)
			{
			}
		}

    // -- arithmetic --
	    public class AddStatement : SingleStatement
		{
			public AddStatement(TypeCobol.Compiler.CodeElements.AddStatement singleStmt) : base(CodeElementType.AddStatement, singleStmt)
			{
			}
		}
	    public class ComputeStatement : SingleStatement
		{
			public ComputeStatement(TypeCobol.Compiler.CodeElements.ComputeStatement singleStmt) : base(CodeElementType.ComputeStatement, singleStmt)
			{
			}
		}
	    public class DivideStatement : SingleStatement
		{
			public DivideStatement(TypeCobol.Compiler.CodeElements.DivideStatement singleStmt) : base(CodeElementType.DivideStatement, singleStmt)
			{
			}
		}
	    public class MultiplyStatement : SingleStatement
		{
			public MultiplyStatement(TypeCobol.Compiler.CodeElements.MultiplyStatement singleStmt) : base(CodeElementType.MultiplyStatement, singleStmt)
			{
			}
		}
	    public class SubtractStatement : SingleStatement
		{
			public SubtractStatement(TypeCobol.Compiler.CodeElements.SubtractStatement singleStmt) : base(CodeElementType.SubtractStatement, singleStmt)
			{
			}
		}
    // -- data movement --
	    public class AcceptStatement : SingleStatement
		{
			public AcceptStatement(TypeCobol.Compiler.CodeElements.AcceptStatement singleStmt) : base(CodeElementType.AcceptStatement, singleStmt)
			{
			}
		} // (DATE, DAY, DAY-OF-WEEK, TIME)
	    public class InitializeStatement : SingleStatement
		{
			public InitializeStatement(TypeCobol.Compiler.CodeElements.InitializeStatement singleStmt) : base(CodeElementType.InitializeStatement, singleStmt)
			{
			}
		}
	    public class InspectStatement : SingleStatement
		{
			public InspectStatement(TypeCobol.Compiler.CodeElements.InspectStatement singleStmt) : base(CodeElementType.InspectStatement, singleStmt)
			{
			}
		}
	    public class MoveStatement : SingleStatement
		{
			public MoveStatement(TypeCobol.Compiler.CodeElements.MoveStatement singleStmt) : base(CodeElementType.MoveStatement, singleStmt)
			{
			}
		}
	    public class SetStatement  : SingleStatement
		{
			public SetStatement(TypeCobol.Compiler.CodeElements.SetStatement singleStmt) : base(CodeElementType.SetStatement, singleStmt)
			{
			}
		}// "table-handling" too
	    public class StringStatement : SingleStatement
		{
			public StringStatement(TypeCobol.Compiler.CodeElements.StringStatement singleStmt) : base(CodeElementType.StringStatement, singleStmt)
			{
			}
		}
	    public class UnstringStatement : SingleStatement
		{
			public UnstringStatement(TypeCobol.Compiler.CodeElements.UnstringStatement singleStmt) : base(CodeElementType.UnstringStatement, singleStmt)
			{
			}
		}
	    public class XmlGenerateStatement : SingleStatement
		{
			public XmlGenerateStatement(TypeCobol.Compiler.CodeElements.XmlGenerateStatement singleStmt) : base(CodeElementType.XmlGenerateStatement, singleStmt)
			{
			}
		}
	    public class XmlParseStatement : SingleStatement
		{
			public XmlParseStatement(TypeCobol.Compiler.CodeElements.XmlParseStatement singleStmt) : base(CodeElementType.XmlParseStatement, singleStmt)
			{
			}
		}
    // -- ending --
	    public class StopStatement  : SingleStatement
		{
			public StopStatement(TypeCobol.Compiler.CodeElements.StopStatement singleStmt) : base(CodeElementType.StopStatement, singleStmt)
			{
			}
		}// RUN
	    public class ExitMethodStatement : SingleStatement
		{
			public ExitMethodStatement(TypeCobol.Compiler.CodeElements.ExitMethodStatement singleStmt) : base(CodeElementType.ExitMethodStatement, singleStmt)
			{
			}
		}
	    public class ExitProgramStatement : SingleStatement
		{
			public ExitProgramStatement(TypeCobol.Compiler.CodeElements.ExitProgramStatement singleStmt) : base(CodeElementType.ExitProgramStatement, singleStmt)
			{
			}
		}
	    public class GobackStatement : SingleStatement
		{
			public GobackStatement(TypeCobol.Compiler.CodeElements.GobackStatement singleStmt) : base(CodeElementType.GobackStatement, singleStmt)
			{
			}
		}
    // -- input-output --
	    public class CloseStatement : SingleStatement
		{
			public CloseStatement(TypeCobol.Compiler.CodeElements.CloseStatement singleStmt) : base(CodeElementType.CloseStatement, singleStmt)
			{
			}
		}
	    public class DeleteStatement : SingleStatement
		{
			public DeleteStatement(TypeCobol.Compiler.CodeElements.DeleteStatement singleStmt) : base(CodeElementType.DeleteStatement, singleStmt)
			{
			}
		}
	    public class DisplayStatement : SingleStatement
		{
			public DisplayStatement(TypeCobol.Compiler.CodeElements.DisplayStatement singleStmt) : base(CodeElementType.DisplayStatement, singleStmt)
			{
			}
		}
	    public class OpenStatement : SingleStatement
		{
			public OpenStatement(TypeCobol.Compiler.CodeElements.OpenStatement singleStmt) : base(CodeElementType.OpenStatement, singleStmt)
			{
			}
		}
	    public class ReadStatement : SingleStatement
		{
			public ReadStatement(TypeCobol.Compiler.CodeElements.ReadStatement singleStmt) : base(CodeElementType.ReadStatement, singleStmt)
			{
			}
		}
	    public class RewriteStatement : SingleStatement
		{
			public RewriteStatement(TypeCobol.Compiler.CodeElements.RewriteStatement singleStmt) : base(CodeElementType.RewriteStatement, singleStmt)
			{
			}
		}
	    public class StartStatement : SingleStatement
		{
			public StartStatement(TypeCobol.Compiler.CodeElements.StartStatement singleStmt) : base(CodeElementType.StartStatement, singleStmt)
			{
			}
		}
    //	StopStatement // literal
	    public class WriteStatement : SingleStatement
		{
			public WriteStatement(TypeCobol.Compiler.CodeElements.WriteStatement singleStmt) : base(CodeElementType.WriteStatement, singleStmt)
			{
			}
		}
    // -- ordering --
	    public class MergeStatement : SingleStatement
		{
			public MergeStatement(TypeCobol.Compiler.CodeElements.MergeStatement singleStmt) : base(CodeElementType.MergeStatement, singleStmt)
			{
			}
		}
	    public class ReleaseStatement : SingleStatement
		{
			public ReleaseStatement(TypeCobol.Compiler.CodeElements.ReleaseStatement singleStmt) : base(CodeElementType.ReleaseStatement, singleStmt)
			{
			}
		}
	    public class ReturnStatement : SingleStatement
		{
			public ReturnStatement(TypeCobol.Compiler.CodeElements.ReturnStatement singleStmt) : base(CodeElementType.ReturnStatement, singleStmt)
			{
			}
		}
	    public class SortStatement : SingleStatement
		{
			public SortStatement(TypeCobol.Compiler.CodeElements.SortStatement singleStmt) : base(CodeElementType.SortStatement, singleStmt)
			{
			}
		}
    // -- procedure-branching --
	    public class AlterStatement : SingleStatement
		{
			public AlterStatement(TypeCobol.Compiler.CodeElements.AlterStatement singleStmt) : base(CodeElementType.AlterStatement, singleStmt)
			{
			}
		}
	    public class ExitStatement : SingleStatement
		{
			public ExitStatement(TypeCobol.Compiler.CodeElements.ExitStatement singleStmt) : base(CodeElementType.ExitStatement, singleStmt)
			{
			}
		}
	    public class GotoStatement : SingleStatement
		{
			public GotoStatement(TypeCobol.Compiler.CodeElements.GotoStatement singleStmt) : base(CodeElementType.GotoStatement, singleStmt)
			{
			}
		}
	    public class PerformProcedureStatement : SingleStatement
		{
			public PerformProcedureStatement(TypeCobol.Compiler.CodeElements.PerformProcedureStatement singleStmt) : base(CodeElementType.PerformProcedureStatement, singleStmt)
			{
			}
		}
    // -- program or method linkage --
	    public class CallStatement : SingleStatement
		{
			public CallStatement(TypeCobol.Compiler.CodeElements.CallStatement singleStmt) : base(CodeElementType.CallStatement, singleStmt)
			{
			}
		}
	    public class ProcedureStyleCall : SingleStatement
		{
            public ProcedureStyleCall(TypeCobol.Compiler.CodeElements.ProcedureStyleCallStatement singleStmt)
                : base(CodeElementType.ProcedureStyleCall, singleStmt)
			{
			}
		}
        
	    public class CancelStatement : SingleStatement
		{
			public CancelStatement(TypeCobol.Compiler.CodeElements.CancelStatement singleStmt) : base(CodeElementType.CancelStatement, singleStmt)
			{
			}
		}
	    public class InvokeStatement : SingleStatement
		{
			public InvokeStatement(TypeCobol.Compiler.CodeElements.InvokeStatement singleStmt) : base(CodeElementType.InvokeStatement, singleStmt)
			{
			}
		}
    // -- DB2 & CICS integration --
	    public class ExecStatement : SingleStatement
		{
			public ExecStatement(TypeCobol.Compiler.CodeElements.ExecStatement singleStmt) : base(CodeElementType.ExecStatement, singleStmt)
			{
			}
		}

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            if (Target != null)
                Target.Accept(v, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (Target != null)
                yield return Target;
        }
    }

    /// <summary>
    /// Base class of  Conditional execution of statements
    /// </summary>
    public abstract class ConditionalExecutionStatement : CodeElementGroup
    {
        /// <summary>
        /// The Statements
        /// </summary>
        public Statements Statements
        {
            get;
            set;
        }

        /// <summary>
        /// Type/Conditional code element statements constructor
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        /// <param name="conditional">The conditional code element</param>
        /// <param name="stmts">Statements</param>
        protected ConditionalExecutionStatement(CodeElementType type, CodeElement conditional, Statements stmts)
            : base(type, conditional)
        {
            this.Statements = stmts;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            if (Target != null)
                Target.Accept(v, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (Target != null)
                yield return Target;
            if (this.Statements != null)
            {
                foreach(Statement stmt in this.Statements)
                    yield return stmt;
            }
        }
    }

    /// <summary>
    /// End Conditional execution Statement
    /// </summary>
    public class EndCondition : ConditionalExecutionStatement
    {
                /// <summary>
        /// Type/Conditional code element constructor
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        /// <param name="conditional">The conditional code element</param>
        public EndCondition(CodeElementType type, CodeElement conditional, Statements stmts)
            : base(type, conditional, stmts)
        {
        }
        public class At : EndCondition
        {
            public At(TypeCobol.Compiler.CodeElements.AtEndCondition at, Statements stmts)
                : base(CodeElementType.AtEndCondition, at, stmts)
            {
            }
        }

        public class NotAt : EndCondition
        {
            public NotAt(TypeCobol.Compiler.CodeElements.NotAtEndCondition not_at, Statements stmts)
                : base(CodeElementType.NotAtEndCondition, not_at, stmts)
            {
            }
        }
    }

    /// <summary>
    /// The list of EndCondition
    /// </summary>
    public class EndConditions : List<EndCondition>
    {
        public EndConditions()
        {
        }
    }

    /// <summary>
    /// Exception Conditional execution Statement
    /// </summary>
    public class ExceptionCondition : ConditionalExecutionStatement
    {
        /// <summary>
        /// Type/Conditional code element constructor
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        /// <param name="conditional">The conditional code element</param>
        public ExceptionCondition(CodeElementType type, CodeElement conditional, Statements stmts)
            : base(type, conditional, stmts)
        {
        }
        public class On : ExceptionCondition
        {
            public On(TypeCobol.Compiler.CodeElements.OnExceptionCondition on, Statements stmts)
                : base(CodeElementType.OnExceptionCondition, on, stmts)
            {
            }
        }

        public class NotOn : ExceptionCondition
        {
            public NotOn(TypeCobol.Compiler.CodeElements.NotOnExceptionCondition not_on, Statements stmts)
                : base(CodeElementType.NotOnExceptionCondition, not_on, stmts)
            {
            }
        }
    }

    /// <summary>
    /// The list of ExceptionCondition
    /// </summary>
    public class ExceptionConditions : List<ExceptionCondition>
    {
        public ExceptionConditions()
        {
        }
    }

    /// <summary>
    /// Key Conditional execution Statement
    /// </summary>
    public class KeyCondition : ConditionalExecutionStatement
    {
        /// <summary>
        /// Type/Conditional code element constructor
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        /// <param name="conditional">The conditional code element</param>
        public KeyCondition(CodeElementType type, CodeElement conditional, Statements stmts)
            : base(type, conditional, stmts)
        {
        }
        public class Invalid : KeyCondition
        {
            public Invalid(TypeCobol.Compiler.CodeElements.InvalidKeyCondition invalid, Statements stmts)
                : base(CodeElementType.InvalidKeyCondition, invalid, stmts)
            {
            }
        }

        public class NotInvalid : KeyCondition
        {
            public NotInvalid(TypeCobol.Compiler.CodeElements.NotInvalidKeyCondition not_invalid, Statements stmts)
                : base(CodeElementType.NotInvalidKeyCondition, not_invalid, stmts)
            {
            }
        }
    }

    /// <summary>
    /// The list of KeyCondition
    /// </summary>
    public class KeyConditions : List<KeyCondition>
    {
        public KeyConditions()
        {
        }
    }

    /// <summary>
    /// Overflow Conditional execution Statement
    /// </summary>
    public class OverflowCondition : ConditionalExecutionStatement
    {
        /// <summary>
        /// Type/Conditional code element constructor
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        /// <param name="conditional">The conditional code element</param>
        public OverflowCondition(CodeElementType type, CodeElement conditional, Statements stmts)
            : base(type, conditional, stmts)
        {
        }
        public class On : OverflowCondition
        {
            public On(TypeCobol.Compiler.CodeElements.OnOverflowCondition on, Statements stmts)
                : base(CodeElementType.OnOverflowCondition, on, stmts)
            {
            }
        }

        public class NotOn : OverflowCondition
        {
            public NotOn(TypeCobol.Compiler.CodeElements.NotOnOverflowCondition not_on, Statements stmts)
                : base(CodeElementType.NotOnOverflowCondition, not_on, stmts)
            {
            }
        }
    }

    /// <summary>
    /// The list of OverflowCondition
    /// </summary>
    public class OverflowConditions : List<OverflowCondition>
    {
        public OverflowConditions()
        {
        }
    }

    /// <summary>
    /// SizeError Conditional execution Statement
    /// </summary>
    public class SizeErrorCondition : ConditionalExecutionStatement
    {
        /// <summary>
        /// Type/Conditional code element constructor
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        /// <param name="conditional">The conditional code element</param>
        public SizeErrorCondition(CodeElementType type, CodeElement conditional, Statements stmts)
            : base(type, conditional, stmts)
        {
        }
        public class On : SizeErrorCondition
        {
            public On(TypeCobol.Compiler.CodeElements.OnSizeErrorCondition on, Statements stmts)
                : base(CodeElementType.OnOverflowCondition, on, stmts)
            {
            }
        }

        public class NotOn : SizeErrorCondition
        {
            public NotOn(TypeCobol.Compiler.CodeElements.NotOnSizeErrorCondition not_on, Statements stmts)
                : base(CodeElementType.NotOnOverflowCondition, not_on, stmts)
            {
            }
        }
    }

    /// <summary>
    /// The list of SizeErrorCondition
    /// </summary>
    public class SizeErrorConditions : List<SizeErrorCondition>
    {
        public SizeErrorConditions()
        {
        }
    }

    /// <summary>
    /// Statements with Optional body
    /// </summary>
    public abstract class CompoundStatement : Statement
    {        
        /// <summary>
        /// Code Dom Type constructor of a Single Statement
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        public CompoundStatement(CodeElementType type, CodeElement singleStatement)
            : base(type, singleStatement)
        {
        }

        /// <summary>
        /// Add Conditional Statement
        /// </summary>
        public class AddConditional : CompoundStatement
        {
            TypeCobol.Compiler.CodeElements.AddStatement AddStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.AddStatement)Target;
                }

                set
                {
                    Target = value;
                }
            }

            public SizeErrorConditions SizeErrorConditions
            {
                get;
                set;
            }

            public TypeCobol.Compiler.CodeElements.AddStatementEnd AddStatementEnd
            {
                get;
                set;
            }

            public AddConditional(TypeCobol.Compiler.CodeElements.AddStatement addStatement, SizeErrorConditions conditions, TypeCobol.Compiler.CodeElements.AddStatementEnd asend = null)
                : base(CodeElementType.AddStatement, addStatement)
            {
                this.SizeErrorConditions = conditions;
                this.AddStatementEnd = asend;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.AddStatement != null)
                    yield return this.AddStatement;
                if (this.SizeErrorConditions != null)
                {
                    foreach (SizeErrorCondition sec in this.SizeErrorConditions)
                        yield return sec;
                }
            }
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            if (Target != null)
                Target.Accept(v, data);
        }
    }
}
