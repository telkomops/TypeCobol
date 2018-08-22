using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUVienna.CS_CUP.Runtime;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;
using TypeCobol.Compiler.SqlParser;

namespace TypeCobol.Compiler.SqlNodes
{
    public class StringLiteral : LiteralExpr
    {
        private readonly string value_;

        // Indicates whether this value needs to be unescaped in toThrift().
        private readonly bool needsUnescaping_;

        public StringLiteral(string value) :
            this(value, ScalarType.STRING, true)
        {
        }

        public StringLiteral(string value, SqlNodeType type, bool needsUnescaping)
        {
            value_ = value;
            type_ = type;
            needsUnescaping_ = needsUnescaping;
        }

        /**
         * Copy c'tor used in clone().
         */
        protected StringLiteral(StringLiteral other) : base(other)
        {
            value_ = other.value_;
            needsUnescaping_ = other.needsUnescaping_;
        }


        public override bool LocalEquals(Expr that)
        {
            if (!base.LocalEquals(that)) return false;
            StringLiteral other = (StringLiteral) that;
            return needsUnescaping_ == other.needsUnescaping_ && value_.Equals(other.value_);
        }


        public override int GetHashCode()
        {
            return value_.GetHashCode();
        }

        public override string ToSqlImpl()
        {
            return "'" + getNormalizedValue() + "'";
        }

        //      @Override
        //protected void toThrift(TExprNode msg)
        //      {
        //          msg.node_type = TExprNodeType.STRING_LITERAL;
        //          string val = (needsUnescaping_) ? getUnescapedValue() : value_;
        //          msg.string_literal = new TStringLiteral(val);
        //      }

        public string getValue()
        {
            return value_;
        }

        public string getUnescapedValue()
        {
            // Unescape string exactly like Hive does. Hive's method assumes
            // quotes so we add them here to reuse Hive's code.
            return BaseSemanticAnalyzer.unescapeSQLString("'" + getNormalizedValue()
                                                              + "'");
        }

        /**
         *  string literals can come directly from the SQL of a query or from rewrites like
         *  constant folding. So this value normalization to a single-quoted string is necessary
         *  because we do not know whether single or double quotes are appropriate.
         *
         *  @return a normalized representation of the string value suitable for embedding in
         *          SQL as a single-quoted string literal.
         */
        private string getNormalizedValue()
        {
            int len = value_.Length;
            StringBuilder sb = new StringBuilder(len);
            for (int i = 0; i < len; ++i)
            {
                char currentChar = value_[i];
                if (currentChar == '\\' && (i + 1) < len)
                {
                    char nextChar = value_[i + 1];
                    // unescape an escaped double quote: remove back-slash in front of the quote.
                    if (nextChar == '"' || nextChar == '\'' || nextChar == '\\')
                    {
                        if (nextChar != '"')
                        {
                            sb.Append(currentChar);
                        }

                        sb.Append(nextChar);
                        ++i;
                        continue;
                    }

                    sb.Append(currentChar);
                }
                else if (currentChar == '\'')
                {
                    // escape a single quote: add back-slash in front of the quote.
                    sb.Append("\\\'");
                }
                else
                {
                    sb.Append(currentChar);
                }
            }

            return sb.ToString();
        }

        public string getStringValue()
        {
            return value_;
        }


        public override string DebugString()
        {
            return "value" + value_;
        }

        protected Expr uncheckedCastTo(SqlNodeType targetType)
        {
            //    Preconditions.checkState(targetType.isNumericType() || targetType.isDateType()
            //|| targetType.Equals(this.type_) || targetType.isStringType());
            if (targetType.Equals(this.type_))
            {
                return this;
            }
            else if (targetType.isStringType())
            {
                type_ = targetType;
            }
            else if (targetType.isNumericType())
            {
                return convertToNumber();
            }
            else if (targetType.isDateType())
            {
                // Let the BE do the cast so it is in Boost format
                return new CastExpr(targetType, this);
            }

            return this;
        }

        /**
         * Convert this string literal to numeric literal.
         *
         * @return new converted literal (not null)
         *         the type of the literal is determined by the lexical scanner
         * @throws AnalysisException
         *           if NumberFormatException occurs,
         *           or if floating point value is NaN or infinite
         */
        //TODO - recover sql tokens from exec statement
        public LiteralExpr convertToNumber()
        {
            StringReader reader = new StringReader(value_);
            SqlScanner.SqlScanner scanner = new SqlScanner.SqlScanner(reader);
            // For distinguishing positive and negative numbers.
            bool negative = false;
            Symbol sym;
            try
            {
                // We allow simple chaining of MINUS to recognize negative numbers.
                // Currently we can't handle string literals containing full fledged expressions
                // which are implicitly cast to a numeric literal.
                // This would require invoking the parser.
                sym = scanner.GetToken();
                while (sym.sym == SqlSymbols.SQL_OP_SUBTRACT)
                {
                    negative = !negative;
                    sym = scanner.next_token();
                }
            }
            catch (IOException e)
            {
                throw new AnalysisException("Failed to convert string literal to number.", e);
            }

            if (sym.sym == SqlSymbols.SQL_NUMERIC_OVERFLOW)
            {
                throw new AnalysisException("Number too large: " + value_);
            }

            if (sym.sym == SqlSymbols.SQL_INTEGER_LITERAL)
            {
                decimal val = (decimal) sym.value;
                if (negative) val = decimal.Negate(val);
                return new NumericLiteral(val);
            }

            if (sym.sym == SqlSymbols.SQL_DECIMAL_LITERAL)
            {
                decimal val = (decimal) sym.value;
                if (negative) val = decimal.Negate(val);
                return new NumericLiteral(val);
            }

            // Symbol is not an integer or floating point literal.
            throw new AnalysisException("Failed to convert string literal '"
                                        + value_ + "' to number.");
        }

        public int compareTo(LiteralExpr o)
        {
            int ret = base.compareTo(o);
            if (ret != 0) return ret;
            StringLiteral other = (StringLiteral) o;
            return value_.CompareTo(other.getStringValue());
        }

        public override Expr clone()
        {
            return new StringLiteral(this);
        }

    }
}
