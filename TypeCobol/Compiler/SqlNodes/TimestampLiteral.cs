using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Represents a literal timestamp. Its value is a 16-byte array that corresponds to a
     * raw BE TimestampValue, e.g., in a slot. In addition, it stores the string
     * representation of the timestamp value to avoid converting the raw bytes on the Java
     * side. Such a conversion could potentially be inconsistent with what the BE would
     * produce, so it's better to defer to a single source of truth (the BE implementation).
     *
     * Literal timestamps can currently only be created via constant folding. There is no
     * way to directly specify a literal timestamp from SQL.
     */
    public class TimestampLiteral : LiteralExpr
    {
        private readonly byte[] value_;
        private readonly String strValue_;

        public TimestampLiteral(byte[] value, String strValue)
        {
            //Preconditions.checkState(value.Length == SqlNodeType.TIMESTAMP.getSlotSize());
            value_ = value;
            strValue_ = strValue;
            type_ = SqlNodeType.TIMESTAMP;
        }

        /**
         * Copy c'tor used in clone.
         */
        protected TimestampLiteral(TimestampLiteral other) : base(other)
        {
            value_ = other.value_;
            strValue_ = other.strValue_;
        }

        public override bool LocalEquals(Expr that)
        {
            return base.localEquals(that) &&
                   value_.Equals(((TimestampLiteral) that).value_);
        }

        public override int GetHashCode()
        {
            return value_.GetHashCode();
        }

        public override String ToSqlImpl()
        {
            // ANSI Timestamp Literal format.
            return "TIMESTAMP '" + getStringValue() + "'";
        }

        public String getStringValue()
        {
            return strValue_;
        }

        public byte[] getValue()
        {
            return value_;
        }

        //@Override
        //protected void toThrift(TExprNode msg)
        //{
        //    msg.node_type = TExprNodeType.TIMESTAMP_LITERAL;
        //    msg.timestamp_literal = new TTimestampLiteral();
        //    msg.timestamp_literal.setValue(value_);
        //}

        protected Expr uncheckedCastTo(SqlNodeType targetType)
        {
            if (targetType.Equals(type_))
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
            TimestampLiteral other = (TimestampLiteral) o;
            return strValue_.CompareTo(other.strValue_);
        }

        public override Expr clone()
        {
            return new TimestampLiteral(this);
        }

    }
}
