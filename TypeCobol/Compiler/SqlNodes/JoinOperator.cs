using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class JoinOperator
    {
        private SqlEnumerations.TJoinOp ThriftJoinOp { get; set; }

        private readonly string Description;

        private JoinOperator(string description, SqlEnumerations.TJoinOp joinOperator )
        {
            this.Description = description;
            ThriftJoinOp = joinOperator;

        }

        public override string ToString()
        {
            return Description;
        }

        public bool IsOuterJoin()
        {
            return ThriftJoinOp == SqlEnumerations.TJoinOp.LEFT_OUTER_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.RIGHT_OUTER_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.FULL_OUTER_JOIN;
        }

        public bool IsSemiJoin()
        {
            return ThriftJoinOp == SqlEnumerations.TJoinOp.LEFT_SEMI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.LEFT_ANTI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.RIGHT_SEMI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.RIGHT_ANTI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.NULL_AWARE_LEFT_ANTI_JOIN;
        }

        public bool IsLeftSemiJoin()
        {
            return ThriftJoinOp == SqlEnumerations.TJoinOp.LEFT_SEMI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.LEFT_ANTI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.NULL_AWARE_LEFT_ANTI_JOIN;
        }

        public bool IsRightSemiJoin()
        {
            return ThriftJoinOp == SqlEnumerations.TJoinOp.RIGHT_SEMI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.RIGHT_ANTI_JOIN;
        }

        public bool IsCrossJoin()
        {
            return ThriftJoinOp == SqlEnumerations.TJoinOp.CROSS_JOIN;
        }

        public bool IsNullAwareLeftAntiJoin()
        {
            return ThriftJoinOp == SqlEnumerations.TJoinOp.NULL_AWARE_LEFT_ANTI_JOIN;
        }

        public bool IsAntiJoin()
        {
            return ThriftJoinOp == SqlEnumerations.TJoinOp.LEFT_ANTI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.RIGHT_ANTI_JOIN ||
                   ThriftJoinOp == SqlEnumerations.TJoinOp.NULL_AWARE_LEFT_ANTI_JOIN;
        }

        public SqlEnumerations.TJoinOp Invert()
        {
            switch (ThriftJoinOp)
            {
                case SqlEnumerations.TJoinOp.LEFT_OUTER_JOIN:
                    return SqlEnumerations.TJoinOp.RIGHT_OUTER_JOIN;
                case SqlEnumerations.TJoinOp.LEFT_SEMI_JOIN:
                    return SqlEnumerations.TJoinOp.RIGHT_SEMI_JOIN;
                case SqlEnumerations.TJoinOp.LEFT_ANTI_JOIN:
                    return SqlEnumerations.TJoinOp.RIGHT_ANTI_JOIN;
                case SqlEnumerations.TJoinOp.RIGHT_OUTER_JOIN:
                    return SqlEnumerations.TJoinOp.LEFT_OUTER_JOIN;
                case SqlEnumerations.TJoinOp.RIGHT_SEMI_JOIN:
                    return SqlEnumerations.TJoinOp.LEFT_SEMI_JOIN;
                case SqlEnumerations.TJoinOp.RIGHT_ANTI_JOIN:
                    return SqlEnumerations.TJoinOp.LEFT_ANTI_JOIN;
                case SqlEnumerations.TJoinOp.NULL_AWARE_LEFT_ANTI_JOIN:
                    throw new InvalidOperationException();
                default:
                    return ThriftJoinOp;
            }
        }
    }
}
