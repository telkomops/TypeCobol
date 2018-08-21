using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes
{
    public class ArithmeticExpr
    {
        //TODO - finish implementation
        public class Operator
        {
            private readonly string description;
            private readonly string name;
            private readonly OperatorPosition pos;
            public readonly SqlOperator sqlOp;

            public Operator(string description, string name, OperatorPosition pos, SqlOperator sqlOp)
            {
                this.description = description;
                this.name = name;
                this.pos = pos;
                this.sqlOp = sqlOp;
            }


            public static readonly Operator MULTIPLY =
                new Operator("*", "multiply", OperatorPosition.BINARY_INFIX, SqlOperator.MULTIPLY);

            public static readonly Operator DIVIDE = new Operator("/", "divide", OperatorPosition.BINARY_INFIX,
                SqlOperator.DIVIDE);

            public static readonly Operator MOD = new Operator("%", "mod", OperatorPosition.BINARY_INFIX,
                SqlOperator.MOD);

            public static readonly Operator INT_DIVIDE = new Operator("DIV", "int_divide",
                OperatorPosition.BINARY_INFIX, SqlOperator.INT_DIVIDE);

            public static readonly Operator ADD = new Operator("+", "add", OperatorPosition.BINARY_INFIX,
                SqlOperator.ADD);

            public static readonly Operator SUBTRACT =
                new Operator("-", "subtract", OperatorPosition.BINARY_INFIX, SqlOperator.SUBTRACT);

            public static readonly Operator BITAND = new Operator("&", "bitand", OperatorPosition.BINARY_INFIX,
                SqlOperator.BITAND);

            public static readonly Operator BITOR = new Operator("|", "bitor", OperatorPosition.BINARY_INFIX,
                SqlOperator.BITOR);

            public static readonly Operator BITXOR = new Operator("^", "bitxor", OperatorPosition.BINARY_INFIX,
                SqlOperator.BITXOR);

            public static readonly Operator BITNOT = new Operator("~", "bitnot", OperatorPosition.UNARY_PREFIX,
                SqlOperator.BITNOT);

            public static readonly Operator FACTORIAL =
                new Operator("!", "factorial", OperatorPosition.UNARY_POSTFIX, SqlOperator.FACTORIAL);

        }
    }

    public enum OperatorPosition
    {
        BINARY_INFIX,
        UNARY_PREFIX,
        UNARY_POSTFIX,
    }

    public enum SqlOperator
    {
        MULTIPLY,
        DIVIDE,
        MOD,
        INT_DIVIDE,
        ADD,
        SUBTRACT,
        BITAND,
        BITOR,
        BITXOR,
        BITNOT,
        FACTORIAL
    }
}


