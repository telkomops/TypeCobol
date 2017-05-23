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
            : base(type)
        {
            base.Target = singleStatement;
        }
    }

    public class SingleStatement : Statement
    {
	    public class ContinueStatement : Statement
        {
            ContinueStatement(TypeCobol.Compiler.CodeElements.ContinueStatement singleStmt) : base(CodeElementType.ContinueStatement, singleStmt)
            {
            }
        }
	    public class EntryStatement : Statement
		{
			public EntryStatement(TypeCobol.Compiler.CodeElements.EntryStatement singleStmt) : base(CodeElementType.EntryStatement, singleStmt)
			{
			}
		}

    // -- arithmetic --
	    public class AddStatement : Statement
		{
			public AddStatement(TypeCobol.Compiler.CodeElements.AddStatement singleStmt) : base(CodeElementType.AddStatement, singleStmt)
			{
			}
		}
	    public class ComputeStatement : Statement
		{
			public ComputeStatement(TypeCobol.Compiler.CodeElements.ComputeStatement singleStmt) : base(CodeElementType.ComputeStatement, singleStmt)
			{
			}
		}
	    public class DivideStatement : Statement
		{
			public DivideStatement(TypeCobol.Compiler.CodeElements.DivideStatement singleStmt) : base(CodeElementType.DivideStatement, singleStmt)
			{
			}
		}
	    public class MultiplyStatement : Statement
		{
			public MultiplyStatement(TypeCobol.Compiler.CodeElements.MultiplyStatement singleStmt) : base(CodeElementType.MultiplyStatement, singleStmt)
			{
			}
		}
	    public class SubtractStatement : Statement
		{
			public SubtractStatement(TypeCobol.Compiler.CodeElements.SubtractStatement singleStmt) : base(CodeElementType.SubtractStatement, singleStmt)
			{
			}
		}
    // -- data movement --
	    public class AcceptStatement : Statement
		{
			public AcceptStatement(TypeCobol.Compiler.CodeElements.AcceptStatement singleStmt) : base(CodeElementType.AcceptStatement, singleStmt)
			{
			}
		} // (DATE, DAY, DAY-OF-WEEK, TIME)
	    public class InitializeStatement : Statement
		{
			public InitializeStatement(TypeCobol.Compiler.CodeElements.InitializeStatement singleStmt) : base(CodeElementType.InitializeStatement, singleStmt)
			{
			}
		}
	    public class InspectStatement : Statement
		{
			public InspectStatement(TypeCobol.Compiler.CodeElements.InspectStatement singleStmt) : base(CodeElementType.InspectStatement, singleStmt)
			{
			}
		}
	    public class MoveStatement : Statement
		{
			public MoveStatement(TypeCobol.Compiler.CodeElements.MoveStatement singleStmt) : base(CodeElementType.MoveStatement, singleStmt)
			{
			}
		}
	    public class SetStatement  : Statement
		{
			public SetStatement(TypeCobol.Compiler.CodeElements.SetStatement singleStmt) : base(CodeElementType.SetStatement, singleStmt)
			{
			}
		}// "table-handling" too
	    public class StringStatement : Statement
		{
			public StringStatement(TypeCobol.Compiler.CodeElements.StringStatement singleStmt) : base(CodeElementType.StringStatement, singleStmt)
			{
			}
		}
	    public class UnstringStatement : Statement
		{
			public UnstringStatement(TypeCobol.Compiler.CodeElements.UnstringStatement singleStmt) : base(CodeElementType.UnstringStatement, singleStmt)
			{
			}
		}
	    public class XmlGenerateStatement : Statement
		{
			public XmlGenerateStatement(TypeCobol.Compiler.CodeElements.XmlGenerateStatement singleStmt) : base(CodeElementType.XmlGenerateStatement, singleStmt)
			{
			}
		}
	    public class XmlParseStatement : Statement
		{
			public XmlParseStatement(TypeCobol.Compiler.CodeElements.XmlParseStatement singleStmt) : base(CodeElementType.XmlParseStatement, singleStmt)
			{
			}
		}
    // -- ending --
	    public class StopStatement  : Statement
		{
			public StopStatement(TypeCobol.Compiler.CodeElements.StopStatement singleStmt) : base(CodeElementType.StopStatement, singleStmt)
			{
			}
		}// RUN
	    public class ExitMethodStatement : Statement
		{
			public ExitMethodStatement(TypeCobol.Compiler.CodeElements.ExitMethodStatement singleStmt) : base(CodeElementType.ExitMethodStatement, singleStmt)
			{
			}
		}
	    public class ExitProgramStatement : Statement
		{
			public ExitProgramStatement(TypeCobol.Compiler.CodeElements.ExitProgramStatement singleStmt) : base(CodeElementType.ExitProgramStatement, singleStmt)
			{
			}
		}
	    public class GobackStatement : Statement
		{
			public GobackStatement(TypeCobol.Compiler.CodeElements.GobackStatement singleStmt) : base(CodeElementType.GobackStatement, singleStmt)
			{
			}
		}
    // -- input-output --
	    public class CloseStatement : Statement
		{
			public CloseStatement(TypeCobol.Compiler.CodeElements.CloseStatement singleStmt) : base(CodeElementType.CloseStatement, singleStmt)
			{
			}
		}
	    public class DeleteStatement : Statement
		{
			public DeleteStatement(TypeCobol.Compiler.CodeElements.DeleteStatement singleStmt) : base(CodeElementType.DeleteStatement, singleStmt)
			{
			}
		}
	    public class DisplayStatement : Statement
		{
			public DisplayStatement(TypeCobol.Compiler.CodeElements.DisplayStatement singleStmt) : base(CodeElementType.DisplayStatement, singleStmt)
			{
			}
		}
	    public class OpenStatement : Statement
		{
			public OpenStatement(TypeCobol.Compiler.CodeElements.OpenStatement singleStmt) : base(CodeElementType.OpenStatement, singleStmt)
			{
			}
		}
	    public class ReadStatement : Statement
		{
			public ReadStatement(TypeCobol.Compiler.CodeElements.ReadStatement singleStmt) : base(CodeElementType.ReadStatement, singleStmt)
			{
			}
		}
	    public class RewriteStatement : Statement
		{
			public RewriteStatement(TypeCobol.Compiler.CodeElements.RewriteStatement singleStmt) : base(CodeElementType.RewriteStatement, singleStmt)
			{
			}
		}
	    public class StartStatement : Statement
		{
			public StartStatement(TypeCobol.Compiler.CodeElements.StartStatement singleStmt) : base(CodeElementType.StartStatement, singleStmt)
			{
			}
		}
    //	StopStatement // literal
	    public class WriteStatement : Statement
		{
			public WriteStatement(TypeCobol.Compiler.CodeElements.WriteStatement singleStmt) : base(CodeElementType.WriteStatement, singleStmt)
			{
			}
		}
    // -- ordering --
	    public class MergeStatement : Statement
		{
			public MergeStatement(TypeCobol.Compiler.CodeElements.MergeStatement singleStmt) : base(CodeElementType.MergeStatement, singleStmt)
			{
			}
		}
	    public class ReleaseStatement : Statement
		{
			public ReleaseStatement(TypeCobol.Compiler.CodeElements.ReleaseStatement singleStmt) : base(CodeElementType.ReleaseStatement, singleStmt)
			{
			}
		}
	    public class ReturnStatement : Statement
		{
			public ReturnStatement(TypeCobol.Compiler.CodeElements.ReturnStatement singleStmt) : base(CodeElementType.ReturnStatement, singleStmt)
			{
			}
		}
	    public class SortStatement : Statement
		{
			public SortStatement(TypeCobol.Compiler.CodeElements.SortStatement singleStmt) : base(CodeElementType.SortStatement, singleStmt)
			{
			}
		}
    // -- procedure-branching --
	    public class AlterStatement : Statement
		{
			public AlterStatement(TypeCobol.Compiler.CodeElements.AlterStatement singleStmt) : base(CodeElementType.AlterStatement, singleStmt)
			{
			}
		}
	    public class ExitStatement : Statement
		{
			public ExitStatement(TypeCobol.Compiler.CodeElements.ExitStatement singleStmt) : base(CodeElementType.ExitStatement, singleStmt)
			{
			}
		}
	    public class GotoStatement : Statement
		{
			public GotoStatement(TypeCobol.Compiler.CodeElements.GotoStatement singleStmt) : base(CodeElementType.GotoStatement, singleStmt)
			{
			}
		}
	    public class PerformProcedureStatement : Statement
		{
			public PerformProcedureStatement(TypeCobol.Compiler.CodeElements.PerformProcedureStatement singleStmt) : base(CodeElementType.PerformProcedureStatement, singleStmt)
			{
			}
		}
    // -- program or method linkage --
	    public class CallStatement : Statement
		{
			public CallStatement(TypeCobol.Compiler.CodeElements.CallStatement singleStmt) : base(CodeElementType.CallStatement, singleStmt)
			{
			}
		}
    /*
	    public class ProcedureStyleCall : Statement
		{
			public ProcedureStyleCall(TypeCobol.Compiler.CodeElements.ProcedureStyleCall singleStmt) : base(CodeElementType.ProcedureStyleCall, singleStmt)
			{
			}
		}
        */
	    public class CancelStatement : Statement
		{
			public CancelStatement(TypeCobol.Compiler.CodeElements.CancelStatement singleStmt) : base(CodeElementType.CancelStatement, singleStmt)
			{
			}
		}
	    public class InvokeStatement : Statement
		{
			public InvokeStatement(TypeCobol.Compiler.CodeElements.InvokeStatement singleStmt) : base(CodeElementType.InvokeStatement, singleStmt)
			{
			}
		}
    // -- DB2 & CICS integration --
	    public class ExecStatement : Statement
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
    /// A List of Statements
    /// </summary>
    public class Statements : List<Statement> 
    {
        public Statements()
        {
        }
    }

}
