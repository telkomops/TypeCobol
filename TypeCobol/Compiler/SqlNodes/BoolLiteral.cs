using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class BoolLiteral : LiteralExpr
    {
        private readonly bool value_;

        public BoolLiteral(bool value)
        {
            this.value_ = value;
            type_ = SqlNodeType.BOOLEAN;
        }

        public BoolLiteral(String value)
        {
            type_ = SqlNodeType.BOOLEAN;
            if (value.ToLowerInvariant().Equals("true"))
            {
                this.value_ = true;
            }
            else if (value.ToLowerInvariant().Equals("false"))
            {
                this.value_ = false;
            }
            else
            {
                throw new AnalysisException("invalid BOOLEAN literal: " + value);
            }
        }

        /**
         * Copy c'tor used in clone.
         */
        protected BoolLiteral(BoolLiteral other):base(other)
        {
            value_ = other.value_;
        }

        public override string DebugString()
        {
            return "value" + value_;
        }


        public override bool LocalEquals(Expr that)
        {
            return base.LocalEquals(that) && ((BoolLiteral) that).value_ == value_;
        }


        public override int GetHashCode()
        {
            return value_ ? 1 : 0;
        }

        public bool getValue()
        {
            return value_;
        }

        public override string ToSqlImpl()
        {
            return getStringValue();
        }

        public String getStringValue()
        {
            return value_ ? "TRUE" : "FALSE";
        }

        protected Expr uncheckedCastTo(SqlNodeType targetType)
        {
            if (targetType.Equals(this.type_))
            {
                return this;
            }
            else
            {
                return new CastExpr(targetType, this);
            }
        }

        public int compareTo(LiteralExpr o)
        {
            int ret = base.compareTo(o);
            if (ret != 0) return ret;
            BoolLiteral other = (BoolLiteral) o;
            if (value_ && !other.getValue()) return 1;
            if (!value_ && other.getValue()) return -1;
            return 0;
        }

        public Expr clone()
        {
            return new BoolLiteral(this);
        }
    }
}
