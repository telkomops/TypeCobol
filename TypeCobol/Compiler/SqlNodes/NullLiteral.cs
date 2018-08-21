using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;

namespace TypeCobol.Compiler.SqlNodes
{
    public class NullLiteral : LiteralExpr
    {
        public NullLiteral()
        {
            type_ = SqlNodeType.NULL;
        }

        /**
         * Copy c'tor used in clone().
         */
        protected NullLiteral(NullLiteral other) :
            base(other)
        {

        }

        /**
         * Returns an analyzed NullLiteral of the specified type.
         */
        public static NullLiteral create(SqlNodeType type)
        {
            NullLiteral l = new NullLiteral();
            l.analyzeNoThrow(null);
            l.uncheckedCastTo(type);
            return l;
        }

        
        public override int GetHashCode() { return 0; }

        
        public override string ToSqlImpl() { return getStringValue(); }
        
        public override string DebugString()
        {
            return base.DebugString();
        }
        
        public string getStringValue() { return "NULL"; }
        
        protected Expr uncheckedCastTo(SqlNodeType targetType)
        {
            //Preconditions.checkState(targetType.isValid());
            type_ = targetType;
            return this;
        }

        //@Override
        //protected void toThrift(TExprNode msg)
        //{
        //    msg.node_type = TExprNodeType.NULL_LITERAL;
        //}
        
        public Expr clone() { return new NullLiteral(this); }
        
        public override void ResetAnalysisState()
        {
            base.ResetAnalysisState();
            type_ = SqlNodeType.NULL;
        }
    }
}
