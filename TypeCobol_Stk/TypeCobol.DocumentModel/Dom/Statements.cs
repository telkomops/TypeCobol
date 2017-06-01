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

        public class NextSentenceStatement : SingleStatement
        {
            public NextSentenceStatement(TypeCobol.Compiler.CodeElements.NextSentenceStatement singleStmt)
                : base(CodeElementType.NextSentenceStatement, singleStmt)
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
    /// The list of ConditionalExecutionStatement
    /// </summary>
    public class ConditionalExecutionStatements : List<ConditionalExecutionStatement>
    {
        public ConditionalExecutionStatements()
        {
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

    public class WhenConditionClause : ConditionalExecutionStatement
    {

        /// <summary>
        /// Is this a WhenSearchCondition ?
        /// </summary>
        public bool IsWhenSearchCondition
        {
            get
            {
                return Target is TypeCobol.Compiler.CodeElements.WhenSearchCondition;
            }
        }

        /// <summary>
        /// Is this a WhenCondition ?
        /// </summary>
        public bool IsWhenCondition
        {
            get
            {
                return Target is TypeCobol.Compiler.CodeElements.WhenSearchCondition;
            }
        }

        /// <summary>
        /// Getter on the WhenSearchCondition if any, null otherwise
        /// </summary>
        public TypeCobol.Compiler.CodeElements.WhenSearchCondition WhenSearchCondition
        {
            get
            {
                return IsWhenSearchCondition ? Target as TypeCobol.Compiler.CodeElements.WhenSearchCondition : null;
            }
            set
            {
                Target = value;
            }
        }

        /// <summary>
        /// Getter on the WhenCondition if any, null otherwise
        /// </summary>
        public TypeCobol.Compiler.CodeElements.WhenCondition WhenCondition
        {
            get
            {
                return IsWhenCondition ? Target as TypeCobol.Compiler.CodeElements.WhenCondition : null;
            }
            set
            {
                Target = value;
            }
        }

        /// <summary>
        /// When Condition Clause code element constructor
        /// </summary>
        /// <param name="conditional">The conditional code element</param>
        public WhenConditionClause(TypeCobol.Compiler.CodeElements.WhenSearchCondition conditional, Statements stmts)
            : base(CodeElementType.WhenSearchCondition, conditional, stmts)
        {
        }

        /// <summary>
        /// When Condition Clause code element constructor
        /// </summary>
        /// <param name="conditional">The conditional code element</param>
        public WhenConditionClause(TypeCobol.Compiler.CodeElements.WhenCondition conditional, Statements stmts)
            : base(CodeElementType.WhenCondition, conditional, stmts)
        {
        }

    }

    /// <summary>
    /// The list of WhenConditionClause
    /// </summary>
    public class WhenConditionClauses : List<WhenConditionClause>
    {
        public WhenConditionClauses()
        {
        }
    }

    public class WhenOtherClause : ConditionalExecutionStatement
    {

        /// <summary>
        /// Getter on the WhenOtherCondition
        /// </summary>
        public TypeCobol.Compiler.CodeElements.WhenOtherCondition WhenOtherCondition
        {
            get
            {
                return Target as TypeCobol.Compiler.CodeElements.WhenOtherCondition;
            }
            set
            {
                Target = value;
            }
        }

        /// <summary>
        /// When Other Clause code element constructor
        /// </summary>
        /// <param name="type">The Code Dom Type</param>
        /// <param name="conditional">The conditional code element</param>
        public WhenOtherClause(TypeCobol.Compiler.CodeElements.WhenOtherCondition conditional, Statements stmts)
            : base(CodeElementType.WhenOtherCondition, conditional, stmts)
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
            public TypeCobol.Compiler.CodeElements.AddStatement AddStatement
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

        /// <summary>
        /// Call Conditional Statement
        /// </summary>
        public class CallConditional : CompoundStatement
        {
            public TypeCobol.Compiler.CodeElements.CallStatement CallStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.CallStatement)Target;
                }

                set
                {
                    Target = value;
                }
            }

            /// <summary>
            /// Conditions: list of ExceptionCondition or OverflowCondition.On
            /// </summary>
            public ConditionalExecutionStatements Conditions
            {
                get;
                set;
            }

            public TypeCobol.Compiler.CodeElements.CallStatementEnd CallStatementEnd
            {
                get;
                set;
            }

            public CallConditional(TypeCobol.Compiler.CodeElements.CallStatement callStatement, ConditionalExecutionStatements conditions, TypeCobol.Compiler.CodeElements.CallStatementEnd csend = null)
                : base(CodeElementType.CallStatement, callStatement)
            {
                this.Conditions = conditions;
                this.CallStatementEnd = csend;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.CallStatement != null)
                    yield return this.CallStatement;
                if (this.Conditions != null)
                {
                    foreach (ConditionalExecutionStatement c in this.Conditions)
                        yield return c;
                }
            }
        }

        /// <summary>
        /// Compute Conditional Statement
        /// </summary>
        public class ComputeConditional : CompoundStatement
        {
            public TypeCobol.Compiler.CodeElements.ComputeStatement ComputeStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.ComputeStatement)Target;
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

            public TypeCobol.Compiler.CodeElements.ComputeStatementEnd ComputeStatementEnd
            {
                get;
                set;
            }

            public ComputeConditional(TypeCobol.Compiler.CodeElements.ComputeStatement ComputeStatement, SizeErrorConditions conditions, TypeCobol.Compiler.CodeElements.ComputeStatementEnd asend = null)
                : base(CodeElementType.ComputeStatement, ComputeStatement)
            {
                this.SizeErrorConditions = conditions;
                this.ComputeStatementEnd = asend;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.ComputeStatement != null)
                    yield return this.ComputeStatement;
                if (this.SizeErrorConditions != null)
                {
                    foreach (SizeErrorCondition sec in this.SizeErrorConditions)
                        yield return sec;
                }
            }
        }

        /// <summary>
        /// Delete Conditional Statement
        /// </summary>
        public class DeleteConditional : CompoundStatement
        {
            public TypeCobol.Compiler.CodeElements.DeleteStatement DeleteStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.DeleteStatement)Target;
                }

                set
                {
                    Target = value;
                }
            }

            public KeyConditions KeyConditions
            {
                get;
                set;
            }

            public TypeCobol.Compiler.CodeElements.DeleteStatementEnd DeleteStatementEnd
            {
                get;
                set;
            }

            public DeleteConditional(TypeCobol.Compiler.CodeElements.DeleteStatement DeleteStatement, KeyConditions conditions, TypeCobol.Compiler.CodeElements.DeleteStatementEnd end = null)
                : base(CodeElementType.DeleteStatement, DeleteStatement)
            {
                this.KeyConditions = conditions;
                this.DeleteStatementEnd = end;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.DeleteStatement != null)
                    yield return this.DeleteStatement;
                if (this.KeyConditions != null)
                {
                    foreach (KeyCondition c in this.KeyConditions)
                        yield return c;
                }
            }
        }

        /// <summary>
        /// Divide Conditional Statement
        /// </summary>
        public class DivideConditional : CompoundStatement
        {
            public TypeCobol.Compiler.CodeElements.DivideStatement DivideStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.DivideStatement)Target;
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

            public TypeCobol.Compiler.CodeElements.DivideStatementEnd DivideStatementEnd
            {
                get;
                set;
            }

            public DivideConditional(TypeCobol.Compiler.CodeElements.DivideStatement DivideStatement, SizeErrorConditions conditions, TypeCobol.Compiler.CodeElements.DivideStatementEnd asend = null)
                : base(CodeElementType.DivideStatement, DivideStatement)
            {
                this.SizeErrorConditions = conditions;
                this.DivideStatementEnd = asend;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.DivideStatement != null)
                    yield return this.DivideStatement;
                if (this.SizeErrorConditions != null)
                {
                    foreach (SizeErrorCondition sec in this.SizeErrorConditions)
                        yield return sec;
                }
            }
        }

        /// <summary>
        /// The Evaluate Statement
        /// </summary>
        public class EvaluateWith : CompoundStatement
        {
            public TypeCobol.Compiler.CodeElements.EvaluateStatement EvaluateStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.EvaluateStatement)Target;
                }

                set
                {
                    Target = value;
                }
            }

            /// <summary>
            /// WhenConditionClauses
            /// </summary>
            public WhenConditionClauses WhenConditionClauses
            {
                get;
                set;
            }

            /// <summary>
            /// WhenOtherClause
            /// </summary>
            public WhenOtherClause WhenOtherClause
            {
                get;
                set;
            }

            public TypeCobol.Compiler.CodeElements.EvaluateStatementEnd EvaluateStatementEnd
            {
                get;
                set;
            }

            /// <summary>
            /// Code Element constructor
            /// </summary>
            /// <param name="EvaluateStatement"></param>
            public EvaluateWith(TypeCobol.Compiler.CodeElements.EvaluateStatement EvaluateStatement)
                : this(EvaluateStatement, null, null, null)
            {
            }

            /// <summary>
            /// Code Element end constructor
            /// </summary>
            /// <param name="EvaluateStatement"></param>
            /// <param name="end"></param>
            public EvaluateWith(TypeCobol.Compiler.CodeElements.EvaluateStatement EvaluateStatement, TypeCobol.Compiler.CodeElements.EvaluateStatementEnd end)
                : this(EvaluateStatement, null, null, end)
            {
            }

            /// <summary>
            /// Code element When Condition Clauses constructor.
            /// </summary>
            /// <param name="EvaluateStatement"></param>
            /// <param name="whenCondClauses"></param>
            public EvaluateWith(TypeCobol.Compiler.CodeElements.EvaluateStatement EvaluateStatement, WhenConditionClauses whenCondClauses)
                : this(EvaluateStatement, whenCondClauses, null, null)
            {
            }

            /// <summary>
            /// Code element When Condition Clauses end constructor.
            /// </summary>
            /// <param name="EvaluateStatement"></param>
            /// <param name="whenCondClauses"></param>
            public EvaluateWith(TypeCobol.Compiler.CodeElements.EvaluateStatement EvaluateStatement,
                WhenConditionClauses whenCondClauses, TypeCobol.Compiler.CodeElements.EvaluateStatementEnd end)
                : this(EvaluateStatement, whenCondClauses, null, end)
            {
            }

            /// <summary>
            /// Code Element When Other Clause constructor.
            /// </summary>
            /// <param name="EvaluateStatement"></param>
            /// <param name="whenOtherClause"></param>
            public EvaluateWith(TypeCobol.Compiler.CodeElements.EvaluateStatement EvaluateStatement,
                WhenOtherClause whenOtherClause)
                : this(EvaluateStatement, null, whenOtherClause, null)
            {
            }

            /// <summary>
            /// Code Element When Other Clause end constructor.
            /// </summary>
            /// <param name="EvaluateStatement"></param>
            /// <param name="whenOtherClause"></param>
            /// <param name="end"></param>
            public EvaluateWith(TypeCobol.Compiler.CodeElements.EvaluateStatement EvaluateStatement,
                WhenOtherClause whenOtherClause,
                TypeCobol.Compiler.CodeElements.EvaluateStatementEnd end)
                : this(EvaluateStatement, null, whenOtherClause, end)
            {
            }

            /// <summary>
            /// Full Constructor
            /// </summary>
            /// <param name="EvaluateStatement"></param>
            /// <param name="whenCondClauses"></param>
            /// <param name="whenOtherClause"></param>
            /// <param name="end"></param>
            public EvaluateWith(TypeCobol.Compiler.CodeElements.EvaluateStatement EvaluateStatement, 
                WhenConditionClauses whenCondClauses,
                WhenOtherClause whenOtherClause,
                TypeCobol.Compiler.CodeElements.EvaluateStatementEnd end = null)
                : base(CodeElementType.EvaluateStatement, EvaluateStatement)
            {
                this.WhenConditionClauses = WhenConditionClauses;
                this.WhenOtherClause = WhenOtherClause;
                this.EvaluateStatementEnd = EvaluateStatementEnd;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.EvaluateStatement != null)
                    yield return this.EvaluateStatement;
                if (this.WhenConditionClauses != null)
                {
                    foreach (WhenConditionClause wcc in this.WhenConditionClauses)
                        yield return wcc;
                }
                if (this.EvaluateStatementEnd != null)
                    yield return this.EvaluateStatementEnd;
            }
        }

        /// <summary>
        /// The If Statement
        /// </summary>
        public class IfStatement : CompoundStatement
        {
            /// <summary>
            /// The If code element
            /// </summary>
            public TypeCobol.Compiler.CodeElements.IfStatement IfCondition
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.IfStatement)Target;
                }
                set
                {
                    Target = value;
                }
            }

            /// <summary>
            /// Then Statements
            /// </summary>
            public Statements ThenStatements
            {
                get;
                set;
            }

            /// <summary>
            /// The else code element
            /// </summary>
            public TypeCobol.Compiler.CodeElements.ElseCondition ElseCondition
            {
                get;
                set;
            }

            /// <summary>
            /// Else Statements
            /// </summary>
            public Statements ElseStatements
            {
                get;
                set;
            }

            /// <summary>
            /// If End Statement
            /// </summary>
            public TypeCobol.Compiler.CodeElements.IfStatementEnd IfStatementEnd
            {
                get;
                set;
            }

            /// <summary>
            /// If Then constructor
            /// </summary>
            /// <param name="ifStmt"></param>
            /// <param name="thenStmts"></param>
            public IfStatement(TypeCobol.Compiler.CodeElements.IfStatement ifStmt, Statements thenStmts)
                : this(ifStmt, thenStmts, null, null, null)
            {
            }

            /// <summary>
            /// If Then End Constructor
            /// </summary>
            /// <param name="ifStmt"></param>
            /// <param name="thenStmts"></param>
            /// <param name="ifEnd"></param>
            public IfStatement(TypeCobol.Compiler.CodeElements.IfStatement ifStmt, Statements thenStmts,
                TypeCobol.Compiler.CodeElements.IfStatementEnd ifEnd)
                : this(ifStmt, thenStmts, null, null, ifEnd)
            {
            }

            /// <summary>
            /// Full Constructor
            /// </summary>
            /// <param name="ifStmt"></param>
            /// <param name="thenStmts"></param>
            /// <param name="elseCond"></param>
            /// <param name="elseStmts"></param>
            /// <param name="ifEnd"></param>
            public IfStatement(TypeCobol.Compiler.CodeElements.IfStatement ifStmt, Statements thenStmts,
                TypeCobol.Compiler.CodeElements.ElseCondition elseCond, Statements elseStmts,
                TypeCobol.Compiler.CodeElements.IfStatementEnd ifEnd = null)
                : base(CodeElementType.IfStatement, ifStmt)
            {
                this.ThenStatements = thenStmts;
                this.ElseCondition = elseCond;
                this.ElseStatements = elseStmts;
                this.IfStatementEnd = ifEnd;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.IfCondition != null)
                    yield return this.IfCondition;
                if (this.ThenStatements != null)
                {
                    foreach (Statement stmt in this.ThenStatements)
                        yield return stmt;
                }
                if (this.ElseCondition != null)
                    yield return this.ElseCondition;
                if (this.ElseStatements != null)
                {
                    foreach (Statement stmt in this.ElseStatements)
                        yield return stmt;
                }
                if (this.IfStatementEnd != null)
                    yield return this.IfStatementEnd;
            }
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            if (Target != null)
                Target.Accept(v, data);
        }

        public class InvokeConditional : CompoundStatement
        {
            public TypeCobol.Compiler.CodeElements.InvokeStatement InvokeStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.InvokeStatement)Target;
                }

                set
                {
                    Target = value;
                }
            }

            public ExceptionConditions ExceptionConditions
            {
                get;
                set;
            }

            public TypeCobol.Compiler.CodeElements.InvokeStatementEnd InvokeStatementEnd
            {
                get;
                set;
            }

            public InvokeConditional(TypeCobol.Compiler.CodeElements.InvokeStatement InvokeStatement, ExceptionConditions conditions, TypeCobol.Compiler.CodeElements.InvokeStatementEnd end = null)
                : base(CodeElementType.InvokeStatement, InvokeStatement)
            {
                this.ExceptionConditions = conditions;
                this.InvokeStatementEnd = end;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.InvokeStatement != null)
                    yield return this.InvokeStatement;
                if (this.ExceptionConditions != null)
                {
                    foreach (ExceptionCondition sec in this.ExceptionConditions)
                        yield return sec;
                }
            }
        }

        /// <summary>
        /// Multiply Conditional Statement
        /// </summary>
        public class MultiplyConditional : CompoundStatement
        {
            public TypeCobol.Compiler.CodeElements.MultiplyStatement MultiplyStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.MultiplyStatement)Target;
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

            public TypeCobol.Compiler.CodeElements.MultiplyStatementEnd MultiplyStatementEnd
            {
                get;
                set;
            }

            public MultiplyConditional(TypeCobol.Compiler.CodeElements.MultiplyStatement MultiplyStatement, SizeErrorConditions conditions, TypeCobol.Compiler.CodeElements.MultiplyStatementEnd asend = null)
                : base(CodeElementType.MultiplyStatement, MultiplyStatement)
            {
                this.SizeErrorConditions = conditions;
                this.MultiplyStatementEnd = asend;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.MultiplyStatement != null)
                    yield return this.MultiplyStatement;
                if (this.SizeErrorConditions != null)
                {
                    foreach (SizeErrorCondition sec in this.SizeErrorConditions)
                        yield return sec;
                }
            }
        }

        /// <summary>
        /// The Perform Statement
        /// </summary>
        public class Perform : CompoundStatement
        {
            /// <summary>
            /// The PerformStatement code element
            /// </summary>
            public TypeCobol.Compiler.CodeElements.PerformStatement PerformStatement
            {
                get
                {
                    return PerformStatement;
                }
                set
                {
                    Target = value;
                }
            }

            /// <summary>
            /// All Statements
            /// </summary>
            public Statements Statements
            {
                get;
                set;
            }

            /// <summary>
            /// The PerformStatement end code element
            /// </summary>
            public TypeCobol.Compiler.CodeElements.PerformStatementEnd PerformStatementEnd
            {
                get;
                set;
            }

            /// <summary>
            /// Code element constructor
            /// </summary>
            /// <param name="PerformStatement"></param>
            public Perform(TypeCobol.Compiler.CodeElements.PerformStatement PerformStatement)
                : this(PerformStatement, null, null)
            {
            }

            /// <summary>
            /// Code Element end constructor.
            /// </summary>
            /// <param name="PerformStatement"></param>
            /// <param name="end"></param>
            public Perform(TypeCobol.Compiler.CodeElements.PerformStatement PerformStatement,
                TypeCobol.Compiler.CodeElements.PerformStatementEnd end)
                : this(PerformStatement, null, end)
            {
            }

            /// <summary>
            /// Full constructor
            /// </summary>
            /// <param name="PerformStatement"></param>
            /// <param name="stmts"></param>
            /// <param name="end"></param>
            public Perform(TypeCobol.Compiler.CodeElements.PerformStatement PerformStatement, 
                Statements stmts,
                TypeCobol.Compiler.CodeElements.PerformStatementEnd end = null)
                : base(CodeElementType.PerformStatement, PerformStatement)
            {
                this.Statements = stmts;
                this.PerformStatementEnd = end;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.PerformStatement != null)
                    yield return this.PerformStatement;
                if (this.Statements != null)
                {
                    foreach (Statement stmt in this.Statements)
                        yield return stmt;
                }
            }
        }

        /// <summary>
        /// Read Conditional Statement
        /// </summary>
        public class ReadConditional : CompoundStatement
        {
            public TypeCobol.Compiler.CodeElements.ReadStatement ReadStatement
            {
                get
                {
                    return (TypeCobol.Compiler.CodeElements.ReadStatement)Target;
                }

                set
                {
                    Target = value;
                }
            }

            /// <summary>
            /// Conditions: list of ExceptionCondition or KeyCondition
            /// </summary>
            public ConditionalExecutionStatements Conditions
            {
                get;
                set;
            }

            public TypeCobol.Compiler.CodeElements.ReadStatementEnd ReadStatementEnd
            {
                get;
                set;
            }

            public ReadConditional(TypeCobol.Compiler.CodeElements.ReadStatement callStatement, ConditionalExecutionStatements conditions, TypeCobol.Compiler.CodeElements.ReadStatementEnd csend = null)
                : base(CodeElementType.ReadStatement, callStatement)
            {
                this.Conditions = conditions;
                this.ReadStatementEnd = csend;
            }

            public override IEnumerator<CodeElement> GetEnumerator()
            {
                if (this.ReadStatement != null)
                    yield return this.ReadStatement;
                if (this.Conditions != null)
                {
                    foreach (ConditionalExecutionStatement c in this.Conditions)
                        yield return c;
                }
            }
        }

    }
}
