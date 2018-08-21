using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    public abstract class GenericType
    {
        // Maximum nesting depth of a type. This limit was determined experimentally by
        // generating and scanning deeply nested Parquet and Avro files. In those experiments,
        // we exceeded the stack space in the scanner (which uses recursion for dealing with
        // nested types) at a nesting depth between 200 and 300 (200 worked, 300 crashed).
        public static int MaxNestingDepth = 100;
        /**
         * Pretty prints this type with lpad number of leading spaces. Used to implement
         * prettyPrint() with space-indented nested types.
         */
        public abstract string prettyPrint(int lpad);

        /**
         * Recursive helper for toSql() to be implemented by subclasses. Keeps track of the
         * nesting depth and terminates the recursion if MAX_NESTING_DEPTH is reached.
         */
        public abstract string toSql(int depth);

        public abstract bool isFullySpecifiedDecimal();
        public abstract bool isWildcardDecimal();
        public abstract bool isWildcardVarchar();
        public abstract bool isWildcardChar();

        // TODO: Handle complex types properly. Some instances may be fixed length.
        public abstract bool isFixedLengthType();

        /**
         * Returns true if Impala supports this type in the metdata. It does not mean we
         * can manipulate data of this type. For tables that contain columns with these
         * types, we can safely skip over them.
         */
        public abstract bool isSupported();

        /**
         * Indicates whether we support partitioning tables on columns of this type.
         */
        public abstract bool supportsTablePartitioning();
    }
}
