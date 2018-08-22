using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class NumericLiteral : LiteralExpr
    {
        // Use the java decimal (arbitrary scale/precision) to represent the value.
        // This object has notions of precision and scale but they do *not* match what
        // we need. decimal's precision is similar to significant figures and scale
        // is the exponent.
        // ".1" could be represented with an unscaled value = 1 and scale = 1 or
        // unscaled value = 100 and scale = 3. Manipulating the value_ (e.g. multiplying
        // it by 10) does not unnecessarily change the unscaled value. Special care
        // needs to be taken when converting between the big decimals unscaled value
        // and ours. (See getUnscaledValue()).
        // A decimal cannot represent special float values like NaN, infinity, or
        // negative zero.
        private decimal value_;

        // If true, this literal has been explicitly cast to a type and should not
        // be analyzed (which infers the type from value_).
        private bool explicitlyCast_;

        public NumericLiteral(decimal value)
        {
            value_ = value;
        }

        public NumericLiteral(String value, SqlNodeType t)
        {
            decimal val;
            try
            {
                val = Convert.ToDecimal(value);
            }
            catch (FormatException e)
            {
                throw new AnalysisException("invalid numeric literal: " + value, e);
            }

            value_ = val;
            this.analyze(null);
            if (type_.isDecimal() && t.isDecimal())
            {
                // Verify that the input decimal value is consistent with the specified
                // column type.
                ScalarType scalarType = (ScalarType) t;
                if (!scalarType.isSupertypeOf((ScalarType) type_))
                {
                    StringBuilder errMsg = new StringBuilder();
                    errMsg.Append("invalid ").Append(t);
                    errMsg.Append(" value: " + value);
                    throw new AnalysisException(errMsg.ToString());
                }
            }

            if (t.isFloatingPointType()) explicitlyCastToFloat(t);
        }

        /**
         * The versions of the ctor that take types assume the type is correct
         * and the NumericLiteral is created as analyzed with that type. The specified
         * type is preserved across substitutions and re-analysis.
         */
        public NumericLiteral(int value, SqlNodeType type)
        {
            value_ = new decimal(value);
            type_ = type;
            explicitlyCast_ = true;
            analysisDone();
        }

        public NumericLiteral(decimal value, SqlNodeType type)
        {
            value_ = value;
            type_ = type;
            explicitlyCast_ = true;
            analysisDone();
        }

        /**
         * Copy c'tor used in clone().
         */
        protected NumericLiteral(NumericLiteral other)
        {
            value_ = other.value_;
            explicitlyCast_ = other.explicitlyCast_;
        }

        /**
         * Returns true if 'v' can be represented by a NumericLiteral, false otherwise.
         * Special float values like NaN, infinity, and negative zero cannot be represented
         * by a NumericLiteral.
         */
        public static bool isValidLiteral(double v)
        {
            if (Double.IsNaN(v) || Double.IsInfinity(v)) return false;
            // Check for negative zero.
            if (v == 0 && 1.0 / v == Double.NegativeInfinity) return false;
            return true;
        }

        //@Override
        public String debugString()
        {
            return
                "value" + value_ +
                "type" + type_;
        }

        //@Override
        public bool localEquals(Expr that)
        {
            if (!base.localEquals(that)) return false;

            NumericLiteral tmp = (NumericLiteral) that;
            if (!tmp.value_.Equals(value_)) return false;
            // Analyzed Numeric literals of different types are distinct.
            if ((isAnalyzed() && tmp.isAnalyzed()) && (!getType().Equals(tmp.getType()))) return false;
            return true;
        }

        //@Override
        public override int GetHashCode()
        {
            return value_.GetHashCode();
        }

        //@Override
        public String toSqlImpl()
        {
            return getStringValue();
        }

        //@Override
        public String getStringValue()
        {
            return value_.ToString();
        }

        public double getDoubleValue()
        {
            return double.Parse(value_.ToString());
        }

        public long getLongValue()
        {
            return long.Parse(value_.ToString());
        }

        public long getIntValue()
        {
            return int.Parse(value_.ToString());
        }

        //      @Override
        //protected void toThrift(TExprNode msg)
        //      {
        //          switch (type_.getPrimitiveType())
        //          {
        //              case TINYINT:
        //              case SMALLINT:
        //              case INT:
        //              case BIGINT:
        //                  msg.node_type = TExprNodeType.INT_LITERAL;
        //                  msg.int_literal = new TIntLiteral(value_.longValue());
        //                  break;
        //              case FLOAT:
        //              case DOUBLE:
        //                  msg.node_type = TExprNodeType.FLOAT_LITERAL;
        //                  msg.float_literal = new TFloatLiteral(value_.doubleValue());
        //                  break;
        //              case DECIMAL:
        //                  msg.node_type = TExprNodeType.DECIMAL_LITERAL;
        //                  TDecimalLiteral literal = new TDecimalLiteral();
        //                  literal.setValue(getUnscaledValue().toByteArray());
        //                  msg.decimal_literal = literal;
        //                  break;
        //              default:
        //                  Preconditions.checkState(false);
        //          }
        //      }

        public decimal getValue()
        {
            return value_;
        }

        //@Override
        protected void analyzeImpl(Analyzer analyzer)
        {
            if (!explicitlyCast_)
            {
                // Compute the precision and scale from the decimal.
                type_ = TypesUtil.computeDecimalType(value_);
                if (type_ == null)
                {
                    Double d = double.Parse(value_.ToString());
                    if (double.IsInfinity(d))
                    {
                        throw new AnalysisException("Numeric literal '" + toSql() +
                                                    "' exceeds maximum range of doubles.");
                    }
                    else if (d == 0 && value_ != decimal.Zero)
                    {
                        throw new AnalysisException("Numeric literal '" + toSql() +
                                                    "' underflows minimum resolution of doubles.");
                    }

                    // Literal could not be stored in any of the supported decimal precisions and
                    // scale. Store it as a float/double instead.
                    float fvalue;
                    float.TryParse(value_.ToString(), out fvalue);
                    if (fvalue == double.Parse(value_.ToString()))
                    {
                        type_ = SqlNodeType.FLOAT;
                    }
                    else
                    {
                        type_ = SqlNodeType.DOUBLE;
                    }
                }
                else
                {
                    // Check for integer types.
                    //Preconditions.checkState(type_.isScalarType());
                    ScalarType scalarType = (ScalarType) type_;
                    if (scalarType.decimalScale() == 0)
                    {
                        if (value_.CompareTo(Byte.MaxValue) <= 0 &&
                            value_.CompareTo(Byte.MinValue) >= 0)
                        {
                            type_ = SqlNodeType.TINYINT;
                        }
                        else if (value_.CompareTo(short.MaxValue) <= 0 &&
                                 value_.CompareTo(short.MinValue) >= 0)
                        {
                            type_ = SqlNodeType.SMALLINT;
                        }
                        else if (value_.CompareTo(int.MaxValue) <= 0 &&
                                 value_.CompareTo(int.MinValue) >= 0)
                        {
                            type_ = SqlNodeType.INT;
                        }
                        else if (value_.CompareTo(long.MaxValue) <= 0 &&
                                 value_.CompareTo(long.MinValue) >= 0)
                        {
                            type_ = SqlNodeType.BIGINT;
                        }
                    }
                }
            }
        }

        /**
         * Explicitly cast this literal to 'targetType'. The targetType must be a
         * float point type.
         */
        public void explicitlyCastToFloat(SqlNodeType targetType)
        {
            //Preconditions.checkState(targetType.isFloatingPointType());
            type_ = targetType;
            explicitlyCast_ = true;
        }

        //@Override
        public Expr uncheckedCastTo(SqlNodeType targetType)
        {
            //Preconditions.checkState(targetType.isNumericType());
            // Implicit casting to decimals allows truncating digits from the left of the
            // decimal point (see TypesUtil). A literal that is implicitly cast to a decimal
            // with truncation is wrapped into a CastExpr so the BE can evaluate it and report
            // a warning. This behavior is consistent with casting/overflow of non-constant
            // exprs that return decimal.
            // IMPALA-1837: Without the CastExpr wrapping, such literals can exceed the max
            // expected byte size sent to the BE in toThrift().
            if (targetType.isDecimal())
            {
                ScalarType decimalType = (ScalarType) targetType;
                // analyze() ensures that value_ never exceeds the maximum scale and precision.
                //Preconditions.checkState(isAnalyzed());
                // Sanity check that our implicit casting does not allow a reduced precision or
                // truncating values from the right of the decimal point.
                //Preconditions.checkState(value_.precision() <= decimalType.decimalPrecision());
                //Preconditions.checkState(value_.scale() <= decimalType.decimalScale());
                //TODO - check what precision is used by java and convert next line
                int valLeftDigits = value_.precision() - value_.scale();
                int typeLeftDigits = decimalType.decimalPrecision() - decimalType.decimalScale();
                if (typeLeftDigits < valLeftDigits) return new CastExpr(targetType, this);
            }

            type_ = targetType;
            return this;
        }

        //@Override
        public void swapSign()
        {
            // swapping sign does not change the type
            value_ = decimal.Negate(value_);
        }

        //@Override
        public int CompareTo(LiteralExpr o)
        {
            int ret = base.compareTo(o);
            if (ret != 0) return ret;
            NumericLiteral other = (NumericLiteral) o;
            return value_.CompareTo(other.value_);
        }

        // Returns the unscaled value of this literal. decimal doesn't treat scale
        // the way we do. We need to pad it out with zeros or truncate as necessary.
        private int getUnscaledValue()
        {
            //Preconditions.checkState(type_.isDecimal());
            int result = value_.unscaledValue();
            int valueScale = value_.scale();
            // If valueScale is less than 0, it indicates the power of 10 to multiply the
            // unscaled value. This path also handles this case by padding with zeros.
            // e.g. unscaled value = 123, value scale = -2 means 12300.
            ScalarType decimalType = (ScalarType) type_;
            return result * (int)Math.Pow(10,decimalType.decimalScale() - valueScale));
        }

        //@Override
        public Expr clone()
        {
            return new NumericLiteral(this);
        }

        /**
         * Check overflow.
         */
        public static bool isOverflow(decimal value, SqlNodeType type)
        {
            switch (type.getPrimitiveType().value)
            {
                case PrimitiveType.PTValues.TINYINT:
                    return (value.CompareTo(Byte.MaxValue) > 0 ||
                            value.CompareTo(Byte.MinValue) < 0);
                case PrimitiveType.PTValues.SMALLINT:
                    return (value.CompareTo(short.MaxValue) > 0 ||
                            value.CompareTo(short.MinValue) < 0);
                case PrimitiveType.PTValues.INT:
                    return (value.CompareTo(int.MaxValue) > 0 ||
                            value.CompareTo(int.MinValue) < 0);
                case PrimitiveType.PTValues.BIGINT:
                    return (value.CompareTo(long.MaxValue) > 0 ||
                            value.CompareTo(long.MinValue) < 0);
                case PrimitiveType.PTValues.FLOAT:
                    return (value.CompareTo(float.MaxValue) > 0 ||
                            value.CompareTo(float.MinValue) < 0);
                case PrimitiveType.PTValues.DOUBLE:
                    return (value.CompareTo(Double.MaxValue) > 0 ||
                            value.CompareTo(Double.MinValue) < 0);
                case PrimitiveType.PTValues.DECIMAL:
                    return (TypesUtil.computeDecimalType(value) == null);
                default:
                    throw new AnalysisException("Overflow check on " + type + " isn't supported.");
            }
        }
    }
}
