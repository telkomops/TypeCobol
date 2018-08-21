using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    public class ScalarType : SqlNodeType
    {
        private readonly PrimitiveType type_;

        // Used for fixed-length types parameterized by size, i.e. CHAR, VARCHAR and
        // FIXED_UDA_INTERMEDIATE.
        private int len_;

        // Only used if type is DECIMAL. -1 (for both) is used to represent a
        // decimal with any precision and scale.
        // It is invalid to have one by -1 and not the other.
        // TODO: we could use that to store DECIMAL(8,*), indicating a decimal
        // with 8 digits of precision and any valid ([0-8]) scale.
        private int precision_;
        private int scale_;

        // SQL allows the engine to pick the default precision. We pick the largest
        // precision that is supported by the smallest decimal type in the BE (4 bytes).
        public static readonly int DEFAULT_PRECISION = 9;
        public static readonly int DEFAULT_SCALE = 0; // SQL standard

        // Longest supported VARCHAR and CHAR, chosen to match Hive.
        public static readonly int MAX_VARCHAR_LENGTH = (1 << 16) - 1; // 65535
        public static readonly int MAX_CHAR_LENGTH = (1 << 8) - 1; // 255

        // Hive, mysql, sql server standard.
        public static readonly int MAX_PRECISION = 38;
        public static readonly int MAX_SCALE = MAX_PRECISION;
        public static readonly int MIN_ADJUSTED_SCALE = 6;

        public ScalarType(PrimitiveType type)
        {
            this.type_ = type;
            
        }

        public static ScalarType createType(PrimitiveType type)
        {
            switch (type.value)
            {
                case PrimitiveType.PTValues.INVALID_TYPE: return INVALID;
                case PrimitiveType.PTValues.NULL_TYPE: return NULL;
                case PrimitiveType.PTValues.BOOLEAN: return BOOLEAN;
                case PrimitiveType.PTValues.SMALLINT: return SMALLINT;
                case PrimitiveType.PTValues.TINYINT: return TINYINT;
                case PrimitiveType.PTValues.INT: return INT;
                case PrimitiveType.PTValues.BIGINT: return BIGINT;
                case PrimitiveType.PTValues.FLOAT: return FLOAT;
                case PrimitiveType.PTValues.DOUBLE: return DOUBLE;
                case PrimitiveType.PTValues.STRING: return STRING;
                case PrimitiveType.PTValues.VARCHAR: return createVarcharType();
                case PrimitiveType.PTValues.BINARY: return BINARY;
                case PrimitiveType.PTValues.TIMESTAMP: return TIMESTAMP;
                case PrimitiveType.PTValues.DATE: return DATE;
                case PrimitiveType.PTValues.DATETIME: return DATETIME;
                case PrimitiveType.PTValues.DECIMAL: return (ScalarType)createDecimalType();
                default:
                    return NULL;
            }
        }

        public static ScalarType createCharType(int len)
        {
            ScalarType type = new ScalarType(PrimitiveType.CHAR);
            type.len_ = len;
            return type;
        }

        public static ScalarType createFixedUdaIntermediateType(int len)
        {
            ScalarType type = new ScalarType(PrimitiveType.FIXED_UDA_INTERMEDIATE);
            type.len_ = len;
            return type;
        }

        public static ScalarType createDecimalType() { return DEFAULT_DECIMAL; }

        public static ScalarType createDecimalType(int precision)
        {
            return createDecimalType(precision, DEFAULT_SCALE);
        }

        /**
         * Returns a DECIMAL type with the specified precision and scale.
         */
        public static ScalarType createDecimalType(int precision, int scale)
        {
            // Enforced by parser
            if (precision <= 0 || scale <= 0)
            {
                return null;
            }
            ScalarType type = new ScalarType(PrimitiveType.DECIMAL);
            type.precision_ = precision;
            type.scale_ = scale;
            return type;
        }

        /**
  * Returns a DECIMAL wildcard type (i.e. precision and scale hasn't yet been resolved).
  */
        public static ScalarType createWildCardDecimalType()
        {
            ScalarType type = new ScalarType(PrimitiveType.DECIMAL);
            type.precision_ = -1;
            type.scale_ = -1;
            return type;
        }

        /**
         * Returns a DECIMAL type with the specified precision and scale, but truncating the
         * precision to the max storable precision (i.e. removes digits from before the
         * decimal point).
         */
        public static ScalarType createClippedDecimalType(int precision, int scale)
        {
            // Enforced by parser
            if (precision <= 0 || scale <= 0)
            {
                return null;
            }
            ScalarType type = new ScalarType(PrimitiveType.DECIMAL);
            type.precision_ = Math.Min(precision, MAX_PRECISION);
            type.scale_ = Math.Min(type.precision_, scale);
            return type;
        }

        /**
         * Returns a DECIMAL type with the specified precision and scale. When the given
         * precision exceeds the max storable precision, reduce both precision and scale but
         * preserve at least MIN_ADJUSTED_SCALE for scale (unless the desired scale was less).
         */
        public static ScalarType createAdjustedDecimalType(int precision, int scale)
        {
            // Enforced by parser
            if (precision <= 0 || scale <= 0)
            {
                return null;
            }
            if (precision > MAX_PRECISION)
            {
                int minScale = Math.Min(scale, MIN_ADJUSTED_SCALE);
                int delta = precision - MAX_PRECISION;
                precision = MAX_PRECISION;
                scale = Math.Max(scale - delta, minScale);
            }
            ScalarType type = new ScalarType(PrimitiveType.DECIMAL);
            type.precision_ = precision;
            type.scale_ = scale;
            return type;
        }

        public static ScalarType createVarcharType(int len)
        {
            // length checked in analysis
            ScalarType type = new ScalarType(PrimitiveType.VARCHAR);
            type.len_ = len;
            return type;
        }

        public static ScalarType createVarcharType()
        {
            return DEFAULT_VARCHAR;
        }

        public override string ToString()
        {
            if (type_ == PrimitiveType.CHAR)
            {
                if (isWildcardChar()) return "CHAR(*)";
                return "CHAR(" + len_ + ")";
            }
            else if (type_ == PrimitiveType.DECIMAL)
            {
                if (isWildcardDecimal()) return "DECIMAL(*,*)";
                return "DECIMAL(" + precision_ + "," + scale_ + ")";
            }
            else if (type_ == PrimitiveType.VARCHAR)
            {
                if (isWildcardVarchar()) return "VARCHAR(*)";
                return "VARCHAR(" + len_ + ")";
            }
            else if (type_ == PrimitiveType.FIXED_UDA_INTERMEDIATE)
            {
                return "FIXED_UDA_INTERMEDIATE(" + len_ + ")";
            }
            return type_.ToString();
        }

        public override string toSql(int depth)
        {
            if (depth >= MaxNestingDepth) return "...";
            if (type_ == PrimitiveType.BINARY)
            {
                return type_.ToString();
            }
            else
            {
                if (type_ == PrimitiveType.VARCHAR || type_ == PrimitiveType.CHAR)
                {
                    return type_.ToString() + "(" + len_ + ")";
                }
                else
                {
                    if (type_ == PrimitiveType.DECIMAL)
                    {
                        return string.Format("{0}({1},{2})", type_.ToString(), precision_, scale_);
                    }

                }
            }
            return type_.ToString();
        }

        public override string prettyPrint(int lpad)
        {
            return new string(' ', lpad) + toSql(0);
        }


        //public void toThrift(TColumnType container)
        //      {
        //          TTypeNode node = new TTypeNode();
        //          container.types.add(node);
        //          switch (type_)
        //          {
        //              case VARCHAR:
        //              case CHAR:
        //              case FIXED_UDA_INTERMEDIATE:
        //                  {
        //                      node.setType(TTypeNodeType.SCALAR);
        //                      TScalarType scalarType = new TScalarType();
        //                      scalarType.setType(type_.toThrift());
        //                      scalarType.setLen(len_);
        //                      node.setScalar_type(scalarType);
        //                      break;
        //                  }
        //              case DECIMAL:
        //                  {
        //                      node.setType(TTypeNodeType.SCALAR);
        //                      TScalarType scalarType = new TScalarType();
        //                      scalarType.setType(type_.toThrift());
        //                      scalarType.setScale(scale_);
        //                      scalarType.setPrecision(precision_);
        //                      node.setScalar_type(scalarType);
        //                      break;
        //                  }
        //              default:
        //                  {
        //                      node.setType(TTypeNodeType.SCALAR);
        //                      TScalarType scalarType = new TScalarType();
        //                      scalarType.setType(type_.toThrift());
        //                      node.setScalar_type(scalarType);
        //                      break;
        //                  }
        //          }
        //      }

        public int decimalPrecision()
        {
            if (type_ != PrimitiveType.DECIMAL)
            {
                return -1;
            }
            return precision_;
        }

        public int decimalScale()
        {
            if (type_ != PrimitiveType.DECIMAL)
            {
                return -1;
            }
            return scale_;
        }

        public PrimitiveType getPrimitiveType() { return type_; }
        public int ordinal() { return (int)type_.value; }
        public int getLength() { return len_; }

        public override bool isWildcardDecimal()
        {
            return type_ == PrimitiveType.DECIMAL && precision_ == -1 && scale_ == -1;
        }

        public override bool isWildcardVarchar()
        {
            return type_ == PrimitiveType.VARCHAR && len_ == -1;
        }

        public override bool isWildcardChar()
        {
            return type_ == PrimitiveType.CHAR && len_ == -1;
        }

        /**
         *  Returns true if this type is a fully specified (not wild card) decimal.
         */
        public override bool isFullySpecifiedDecimal()
        {
            if (!isDecimal()) return false;
            if (isWildcardDecimal()) return false;
            if (precision_ <= 0 || precision_ > MAX_PRECISION) return false;
            if (scale_ < 0 || scale_ > precision_) return false;
            return true;
        }

        public override bool isFixedLengthType()
        {
            return type_ == PrimitiveType.BOOLEAN || type_ == PrimitiveType.TINYINT
                || type_ == PrimitiveType.SMALLINT || type_ == PrimitiveType.INT
                || type_ == PrimitiveType.BIGINT || type_ == PrimitiveType.FLOAT
                || type_ == PrimitiveType.DOUBLE || type_ == PrimitiveType.DATE
                || type_ == PrimitiveType.DATETIME || type_ == PrimitiveType.TIMESTAMP
                || type_ == PrimitiveType.CHAR || type_ == PrimitiveType.DECIMAL
                || type_ == PrimitiveType.FIXED_UDA_INTERMEDIATE;
        }

        public override bool isSupported()
        {
            return isValid() && !getUnsupportedTypes().Contains(this);
        }


        public override bool supportsTablePartitioning()
        {
            if (!isSupported() || isComplexType() || type_ == PrimitiveType.TIMESTAMP)
            {
                return false;
            }
            return true;
        }

        public int getSlotSize()
        {
            if (type_ == PrimitiveType.CHAR || type_ == PrimitiveType.FIXED_UDA_INTERMEDIATE)
            {
                return len_;

            }

            if (type_ == PrimitiveType.DECIMAL) {
                return TypesUtil.getDecimalSlotSize(this);
            }
            return type_.getSlotSize();
        }

        /**
         * Returns true if this object is of type t.
         * Handles wildcard types. That is, if t is the wildcard type variant
         * of 'this', returns true.
         */
        public bool matchesType<T>(T t)
        {
            if (Equals(t)) return true;
            if (!isScalarType(t)) return false;
            ScalarType scalarType = (ScalarType)(object)t;
            if (type_ == PrimitiveType.VARCHAR && scalarType.isWildcardVarchar())
            {
                return !isWildcardVarchar();
            }
            if (type_ == PrimitiveType.CHAR && scalarType.isWildcardChar())
            {
                return !isWildcardVarchar();
            }
            if (isDecimal() && scalarType.isWildcardDecimal())
            {
                return !isWildcardVarchar();
            }
            return false;
        }


        public override bool Equals(object o)
        {
            if (!(o is ScalarType)) return false;
            ScalarType other = (ScalarType) o;
            if (type_ != other.type_) return false;
            if (type_ == PrimitiveType.CHAR || type_ == PrimitiveType.FIXED_UDA_INTERMEDIATE)
            {
                return len_ == other.len_;
            }

            if (type_ == PrimitiveType.VARCHAR) return len_ == other.len_;
            if (type_ == PrimitiveType.DECIMAL)
            {
                return precision_ == other.precision_ && scale_ == other.scale_;
            }

            return true;
        }

        public SqlNodeType getMaxResolutionType()
        {
            if (isIntegerType())
            {
                return ScalarType.BIGINT;
                // Timestamps get summed as DOUBLE for AVG.
            }
            else if (isFloatingPointType() || type_ == PrimitiveType.TIMESTAMP)
            {
                return ScalarType.DOUBLE;
            }
            else if (isNull())
            {
                return ScalarType.NULL;
            }
            else if (isDecimal())
            {
                if (scale_ >= MAX_PRECISION)
                {
                    return null;
                }
                return createDecimalType(MAX_PRECISION, scale_);
            }
            else
            {
                return ScalarType.INVALID;
            }
        }

        public ScalarType getNextResolutionType()
        {
            if(!isNumericType() && !isNull())
            {
                return null;
            }
            if (type_ == PrimitiveType.DOUBLE || type_ == PrimitiveType.BIGINT || isNull())
            {
                return this;
            }
            else if (type_ == PrimitiveType.DECIMAL)
            {
                if (scale_ >= MAX_PRECISION)
                {
                    return null;
                }
                return createDecimalType(MAX_PRECISION, scale_);
            }
            return null; // to erase once the next statement is translated
            //return createType(PrimitiveType.Values[type_.ordinal() + 1]); //TODO search how to translate to c#
        }

        /**
         * Returns the smallest decimal type that can safely store this type. Returns
         * INVALID if this type cannot be stored as a decimal.
         */
        public ScalarType getMinResolutionDecimal()
        {
            switch (type_.value)
            {
                case PrimitiveType.PTValues.NULL_TYPE: return NULL;
                case PrimitiveType.PTValues.DECIMAL: return this;
                case PrimitiveType.PTValues.TINYINT: return createDecimalType(3);
                case PrimitiveType.PTValues.SMALLINT: return createDecimalType(5);
                case PrimitiveType.PTValues.INT: return createDecimalType(10);
                case PrimitiveType.PTValues.BIGINT: return createDecimalType(19);
                case PrimitiveType.PTValues.FLOAT: return createDecimalType(MAX_PRECISION, 9);
                case PrimitiveType.PTValues.DOUBLE: return createDecimalType(MAX_PRECISION, 17);
                default: return ScalarType.INVALID;
            }
        }

        /**
         * Returns true if this decimal type is a supertype of the other decimal type.
         * e.g. (10,3) is a supertype of (3,3) but (5,4) is not a supertype of (3,0).
         * To be a super type of another decimal, the number of digits before and after
         * the decimal point must be greater or equal.
         */
        public bool isSupertypeOf(ScalarType o)
        {
            if (!isDecimal() || !o.isDecimal())
            {
                return false;
            }
            if (isWildcardDecimal()) return true;
            if (o.isWildcardDecimal()) return false;
            return scale_ >= o.scale_ && precision_ - scale_ >= o.precision_ - o.scale_;
        }

        /**
         * Return type t such that values from both t1 and t2 can be assigned to t.
         * Returns INVALID_TYPE if there is no such type or if any of t1 and t2
         * is INVALID_TYPE.
         *
         * If strictDecimal is true, only return types that result in no loss of information
         * when both inputs are decimal.
         * If strict is true, only return types that result in no loss of information
         * when at least one of the inputs is not decimal.
         */
        public static ScalarType getAssignmentCompatibleType(ScalarType t1,
            ScalarType t2, bool strict, bool strictDecimal)
        {
            if (!t1.isValid() || !t2.isValid()) return INVALID;
            if (t1.Equals(t2)) return t1;
            if (t1.isNull()) return t2;
            if (t2.isNull()) return t1;

            if (t1.type_ == PrimitiveType.VARCHAR || t2.type_ == PrimitiveType.VARCHAR)
            {
                if (t1.type_ == PrimitiveType.STRING || t2.type_ == PrimitiveType.STRING)
                {
                    return STRING;
                }
                if (t1.isStringType() && t2.isStringType())
                {
                    return createVarcharType(Math.Max(t1.len_, t2.len_));
                }
                return INVALID;
            }

            if (t1.type_ == PrimitiveType.CHAR || t2.type_ == PrimitiveType.CHAR)
            {
                //Preconditions.checkState(t1.type_ != PrimitiveType.VARCHAR);
                //Preconditions.checkState(t2.type_ != PrimitiveType.VARCHAR);
                if (t1.type_ == PrimitiveType.STRING || t2.type_ == PrimitiveType.STRING)
                {
                    return STRING;
                }
                if (t1.type_ == PrimitiveType.CHAR && t2.type_ == PrimitiveType.CHAR)
                {
                    return createCharType(Math.Max(t1.len_, t2.len_));
                }
                return INVALID;
            }

            if (t1.isDecimal() || t2.isDecimal())
            {
                // The case of decimal and float/double must be handled carefully. There are two
                // modes: strict and non-strict. In non-strict mode, we convert to the floating
                // point type, since it can contain a larger range of values than any decimal (but
                // has lower precision in some parts of its range), so it is generally better.
                // In strict mode, we avoid conversion in either direction because there are also
                // decimal values (e.g. 0.1) that cannot be exactly represented in binary
                // floating point.
                // TODO: it might make sense to promote to double in many cases, but this would
                // require more work elsewhere to avoid breaking things, e.g. inserting decimal
                // literals into float columns.
                if (t1.isFloatingPointType()) return strict ? INVALID : t1;
                if (t2.isFloatingPointType()) return strict ? INVALID : t2;

                // Allow casts between decimal and numeric types by converting
                // numeric types to the containing decimal type.
                ScalarType t1Decimal = t1.getMinResolutionDecimal();
                ScalarType t2Decimal = t2.getMinResolutionDecimal();
                if (t1Decimal.isInvalid() || t2Decimal.isInvalid()) return SqlNodeType.INVALID;

                
                if (t1Decimal.isDecimal()&& t2Decimal.isDecimal()&& t1Decimal.Equals(t2Decimal))
                {
                    //Preconditions.checkState(!(t1.isDecimal() && t2.isDecimal()));
                    // The containing decimal type for a non-decimal type is always an exclusive
                    // upper bound, therefore the decimal has higher precision.
                    return t1Decimal;
                }
                if (t1Decimal.isSupertypeOf(t2Decimal)) return t1;
                if (t2Decimal.isSupertypeOf(t1Decimal)) return t2;
                return TypesUtil.getDecimalAssignmentCompatibleType(
                    t1Decimal, t2Decimal, strictDecimal);
            }

            PrimitiveType smallerType =
                (t1.type_.ordinal() < t2.type_.ordinal() ? t1.type_ : t2.type_);
            PrimitiveType largerType =
                (t1.type_.ordinal() > t2.type_.ordinal() ? t1.type_ : t2.type_);
            PrimitiveType result = null;
            if (strict)
            {
                result = strictCompatibilityMatrix[smallerType.ordinal(),largerType.ordinal()];
            }
            if (result == null)
            {
                result = compatibilityMatrix[smallerType.ordinal(),largerType.ordinal()];
            }

            return result == null ? null : createType(result);
        }

      

        /**
         * Returns true t1 can be implicitly cast to t2, false otherwise.
         *
         * If strictDecimal is true, only consider casts that result in no loss of information
         * when casting between decimal types.
         * If strict is true, only consider casts that result in no loss of information when
         * casting between any two types other than both decimals.
         */
        public static bool isImplicitlyCastable(ScalarType t1, ScalarType t2,
            bool strict, bool strictDecimal)
        {
            return getAssignmentCompatibleType(t1, t2, strict, strictDecimal).matchesType(t2);
        }

    }
}
