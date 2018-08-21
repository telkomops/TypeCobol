using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class TypesUtil
    {

        // The sql standard specifies that the scale after division is incremented
        // by a system wide constant. Hive picked 4 so we will as well.
        // TODO: how did they pick this?
        static readonly int DECIMAL_DIVISION_SCALE_INCREMENT = 4;

        /**
         * [1-9] precision -> 4 bytes
         * [10-18] precision -> 8 bytes
         * [19-38] precision -> 16 bytes
         * TODO: Support 12 byte decimal?
         * For precision [20-28], we could support a 12 byte decimal but currently a 12
         * byte decimal in the BE is not implemented.
         */
        public static int getDecimalSlotSize(ScalarType SqlNodeType)
        {
            //Preconditions.checkState(SqlNodeType.isDecimal() && !SqlNodeType.isWildcardDecimal());
            if (SqlNodeType.decimalPrecision() <= 9) return 4;
            if (SqlNodeType.decimalPrecision() <= 18) return 8;
            return 16;
        }

        /**
         * Returns the decimal SqlNodeType that can hold t1 and t2 without loss of precision.
         * decimal(10, 2) && decimal(12, 2) -> decimal(12, 2)
         * decimal (10, 5) && decimal(12, 3) -> decimal(14, 5)
         * Either t1 or t2 can be a wildcard decimal (but not both).
         *
         * If strictDecimal is true, returns a SqlNodeType that results in no loss of information. If
         * this is not possible, returns INVLAID. For example,
         * decimal(38,0) && decimal(38,38) -> INVALID.
         */
        public static ScalarType getDecimalAssignmentCompatibleType(
            ScalarType t1, ScalarType t2, bool strictDecimal)
        {
            //Preconditions.checkState(t1.isDecimal());
            //Preconditions.checkState(t2.isDecimal());
            //Preconditions.checkState(!(t1.isWildcardDecimal() && t2.isWildcardDecimal()));
            if (t1.isWildcardDecimal()) return t2;
            if (t2.isWildcardDecimal()) return t1;

            //Preconditions.checkState(t1.isFullySpecifiedDecimal());
            //Preconditions.checkState(t2.isFullySpecifiedDecimal());
            if (t1.Equals(t2)) return t1;
            int s1 = t1.decimalScale();
            int s2 = t2.decimalScale();
            int p1 = t1.decimalPrecision();
            int p2 = t2.decimalPrecision();
            int digitsBefore = Math.Max(p1 - s1, p2 - s2);
            int digitsAfter = Math.Max(s1, s2);
            if (strictDecimal)
            {
                if (digitsBefore + digitsAfter > 38) return SqlNodeType.INVALID;
                return ScalarType.createDecimalType(digitsBefore + digitsAfter, digitsAfter);
            }

            return ScalarType.createClippedDecimalType(digitsBefore + digitsAfter, digitsAfter);
        }

        /**
         * Returns the necessary result SqlNodeType for t1 op t2. Throws an analysis exception
         * if the operation does not make sense for the types.
         */
        public static SqlNodeType getArithmeticResultType(SqlNodeType t1, SqlNodeType t2,
            ArithmeticExpr.Operator op, bool decimalV2)
        {
            //Preconditions.checkState(t1.isNumericType() || t1.isNull());
            //Preconditions.checkState(t2.isNumericType() || t2.isNull());

            if (t1.isNull() && t2.isNull()) return Catalog.SqlNodeType.NULL;

            if (t1.isDecimal() || t2.isDecimal())
            {
                if (t1.isNull()) return t2;
                if (t2.isNull()) return t1;

                // For multiplications involving at least one floating point SqlNodeType we cast decimal to
                // double in order to prevent decimals from overflowing.
                if (op == ArithmeticExpr.Operator.MULTIPLY &&
                    (t1.isFloatingPointType() || t2.isFloatingPointType()))
                {
                    return Catalog.SqlNodeType.DOUBLE;
                }

                t1 = ((ScalarType) t1).getMinResolutionDecimal();
                t2 = ((ScalarType) t2).getMinResolutionDecimal();
                //Preconditions.checkState(t1.isDecimal());
                //Preconditions.checkState(t2.isDecimal());
                return getDecimalArithmeticResultType(t1, t2, op, decimalV2);
            }

            SqlNodeType SqlNodeType = null;
            switch (op.sqlOp)
            {
                case SqlOperator.MULTIPLY:
                case SqlOperator.ADD:
                case SqlOperator.SUBTRACT:
                    // If one of the types is null, use the compatible SqlNodeType without promotion.
                    // Otherwise, promote the compatible SqlNodeType to the next higher resolution SqlNodeType,
                    // to ensure that that a <op> b won't overflow/underflow.
                    SqlNodeType compatibleType =
                        ScalarType.getAssignmentCompatibleType(t1, t2, false, false);
                    //Preconditions.checkState(compatibleType.isScalarType());
                    SqlNodeType = ((ScalarType) compatibleType).getNextResolutionType();
                    break;
                case SqlOperator.MOD:
                    SqlNodeType = ScalarType.getAssignmentCompatibleType(t1, t2, false, false);
                    break;
                case SqlOperator.DIVIDE:
                    SqlNodeType = SqlNodeType.DOUBLE;
                    break;
                default:
                    throw new AnalysisException("Invalid op: " + op);
            }

            //Preconditions.checkState(SqlNodeType.isValid());
            return SqlNodeType;
        }

        /**
         * Returns the result SqlNodeType for (t1 op t2) where t1 and t2 are both DECIMAL, depending
         * on whether DECIMAL version 1 or DECIMAL version 2 is enabled.
         *
         * TODO: IMPALA-4924: remove DECIMAL V1 code.
         */
        private static ScalarType getDecimalArithmeticResultType(SqlNodeType t1, SqlNodeType t2,
            ArithmeticExpr.Operator op, bool decimalV2)
        {
            if (decimalV2) return getDecimalArithmeticResultTypeV2(t1, t2, op);
            return getDecimalArithmeticResultTypeV1(t1, t2, op);
        }

        /**
         * Returns the result SqlNodeType for (t1 op t2) where t1 and t2 are both DECIMAL, used when
         * DECIMAL version 1 is enabled.
         *
         * These rules were mostly taken from the (pre Dec 2016) Hive / sql server rules with
         * some changes.
         * http://blogs.msdn.com/b/sqlprogrammability/archive/2006/03/29/564110.aspx
         * https://msdn.microsoft.com/en-us/library/ms190476.aspx
         *
         * Changes:
         *  - Multiply does not need +1 for the result precision.
         *  - Divide scale truncation is different. When the maximum precision is exceeded,
         *    unlike SQL server, we do not try to preserve at least a scale of 6.
         *  - For other operators, when maximum precision is exceeded, we clip the precision
         *    and scale at maximum precision rather tha reducing scale. Therefore, we
         *    sacrifice digits to the left of the decimal point before sacrificing digits to
         *    the right.
         */
        private static ScalarType getDecimalArithmeticResultTypeV1(SqlNodeType t1, SqlNodeType t2,
            ArithmeticExpr.Operator op)
        {
            //Preconditions.checkState(t1.isFullySpecifiedDecimal());
            //Preconditions.checkState(t2.isFullySpecifiedDecimal());
            ScalarType st1 = (ScalarType) t1;
            ScalarType st2 = (ScalarType) t2;
            int s1 = st1.decimalScale();
            int s2 = st2.decimalScale();
            int p1 = st1.decimalPrecision();
            int p2 = st2.decimalPrecision();
            int sMax = Math.Max(s1, s2);

            switch (op.sqlOp)
            {
                case SqlOperator.ADD:
                case SqlOperator.SUBTRACT:
                    return ScalarType.createClippedDecimalType(
                        sMax + Math.Max(p1 - s1, p2 - s2) + 1, sMax);
                case SqlOperator.MULTIPLY:
                    return ScalarType.createClippedDecimalType(p1 + p2, s1 + s2);
                case SqlOperator.DIVIDE:
                    int resultScale = Math.Max(DECIMAL_DIVISION_SCALE_INCREMENT, s1 + p2 + 1);
                    int resultPrecision = p1 - s1 + s2 + resultScale;
                    if (resultPrecision > ScalarType.MAX_PRECISION)
                    {
                        // In this case, the desired resulting precision exceeds the maximum and
                        // we need to truncate some way. We can either remove digits before or
                        // after the decimal and there is no right answer. This is an implementation
                        // detail and different databases will handle this differently.
                        // For simplicity, we will set the resulting scale to be the Max of the input
                        // scales and use the maximum precision.
                        resultScale = Math.Max(s1, s2);
                        resultPrecision = ScalarType.MAX_PRECISION;
                    }

                    return ScalarType.createClippedDecimalType(resultPrecision, resultScale);
                case SqlOperator.MOD:
                    return ScalarType.createClippedDecimalType(
                        Math.Min(p1 - s1, p2 - s2) + sMax, sMax);
                default:
                    throw new AnalysisException(
                        "Operation '" + op + "' is not allowed for decimal types.");
            }
        }

        /**
         * Returns the result SqlNodeType for (t1 op t2) where t1 and t2 are both DECIMAL, used when
         * DECIMAL version 2 is enabled.
         *
         * These rules are similar to (post Dec 2016) Hive / sql server rules.
         * http://blogs.msdn.com/b/sqlprogrammability/archive/2006/03/29/564110.aspx
         * https://msdn.microsoft.com/en-us/library/ms190476.aspx
         *
         * Changes:
         *  - There are slight difference with how precision/scale reduction occurs compared
         *    to SQL server when the desired precision is more than the maximum supported
         *    precision.  But an algorithm of reducing scale to a minimum of 6 is used.
         */
        private static ScalarType getDecimalArithmeticResultTypeV2(SqlNodeType t1, SqlNodeType t2,
            ArithmeticExpr.Operator op)
        {
           // Preconditions.checkState(t1.isFullySpecifiedDecimal());
           // Preconditions.checkState(t2.isFullySpecifiedDecimal());
            ScalarType st1 = (ScalarType) t1;
            ScalarType st2 = (ScalarType) t2;
            int s1 = st1.decimalScale();
            int s2 = st2.decimalScale();
            int p1 = st1.decimalPrecision();
            int p2 = st2.decimalPrecision();
            int resultScale;
            int resultPrecision;

            switch (op.sqlOp)
            {
                case SqlOperator.DIVIDE:
                    // Divide result always gets at least MIN_ADJUSTED_SCALE decimal places.
                    resultScale = Math.Max(ScalarType.MIN_ADJUSTED_SCALE, s1 + p2 + 1);
                    resultPrecision = p1 - s1 + s2 + resultScale;
                    break;
                case SqlOperator.MOD:
                    resultScale = Math.Max(s1, s2);
                    resultPrecision = Math.Min(p1 - s1, p2 - s2) + resultScale;
                    break;
                case SqlOperator.MULTIPLY:
                    resultScale = s1 + s2;
                    resultPrecision = p1 + p2 + 1;
                    break;
                case SqlOperator.ADD:
                case SqlOperator.SUBTRACT:
                    resultScale = Math.Max(s1, s2);
                    resultPrecision = Math.Max(p1 - s1, p2 - s2) + resultScale + 1;
                    break;
                default:
                    return null;
            }

            // Use the scale reduction technique when resultPrecision is too large.
            return ScalarType.createAdjustedDecimalType(resultPrecision, resultScale);
        }

/**
 * Computes the ColumnType that can represent 'v' with no loss of resolution.
 * The scale/precision in BigDecimal is not compatible with SQL decimal semantics
 * (much more like significant figures and exponent).
 * Returns null if the value cannot be represented.
 */
        public static SqlNodeType computeDecimalType(Decimal v)
        {
            // PlainString returns the string with no exponent. We walk it to compute
            // the digits before and after.
            // TODO: better way?
            String str = v.ToString();
            int digitsBefore = 0;
            int digitsAfter = 0;
            bool decimalFound = false;
            bool leadingZeros = true;
            foreach (var c in str)
            {
                switch (c)
                {
                    case '-':
                        continue;
                    case '.':
                        decimalFound = true;
                        continue;
                }

                if (decimalFound)
                {
                    ++digitsAfter;
                }
                else
                {
                    // Strip out leading 0 before the decimal point. We want "0.1" to
                    // be parsed as ".1" (1 digit instead of 2).
                    if (c == '0' && leadingZeros) continue;
                    leadingZeros = false;
                    ++digitsBefore;
                }
            }

            if (digitsAfter > ScalarType.MAX_SCALE) return null;
            if (digitsBefore + digitsAfter > ScalarType.MAX_PRECISION) return null;
            if (digitsBefore == 0 && digitsAfter == 0) digitsBefore = 1;
            return ScalarType.createDecimalType(digitsBefore + digitsAfter, digitsAfter);
        }
    }
}
