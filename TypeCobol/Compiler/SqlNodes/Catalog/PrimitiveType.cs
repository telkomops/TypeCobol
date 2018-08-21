using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    public class PrimitiveType
    {
        private readonly string description;
        private readonly int slotSize;  // size of tuple slot for this type
        public readonly PTValues value;
        public PrimitiveType(string description, int slotSize, PTValues value)
        {
            this.description = description;
            this.slotSize = slotSize;
            this.value=value;
        }

        public int ordinal()
        {
            return (int) value;

        }

        public override string ToString()
        {
            return description;
        }

        public int getSlotSize() => slotSize;

        public static readonly PrimitiveType INVALID_TYPE = new PrimitiveType("INVALID_TYPE", -1, PTValues.INVALID_TYPE);
        // NULL_TYPE - used only in LiteralPredicate and NullLiteral to make NULLs compatible
        // with all other types.
        public static readonly PrimitiveType NULL_TYPE = new PrimitiveType("NULL_TYPE", 1,PTValues.NULL_TYPE);
        public static readonly PrimitiveType BOOLEAN = new PrimitiveType("BOOLEAN", 1,PTValues.BOOLEAN);
        public static readonly PrimitiveType TINYINT = new PrimitiveType("TINYINT", 1,PTValues.TINYINT);
        public static readonly PrimitiveType SMALLINT = new PrimitiveType("SMALLINT", 2,PTValues.SMALLINT);
        public static readonly PrimitiveType INT = new PrimitiveType("INT", 4,PTValues.INT);
        public static readonly PrimitiveType BIGINT = new PrimitiveType("BIGINT", 8,PTValues.BIGINT);
        public static readonly PrimitiveType FLOAT = new PrimitiveType("FLOAT", 4,PTValues.FLOAT);
        public static readonly PrimitiveType DOUBLE = new PrimitiveType("DOUBLE", 8,PTValues.DOUBLE);
        public static readonly PrimitiveType DATE = new PrimitiveType("DATE", 4,PTValues.DATE);
        public static readonly PrimitiveType DATETIME = new PrimitiveType("DATETIME", 8,PTValues.DATETIME);
        // The timestamp structure is 12 bytes, Aligning to 8 bytes makes it 16.
        public static readonly PrimitiveType TIMESTAMP = new PrimitiveType("TIMESTAMP", 16,PTValues.TIMESTAMP);
        // 8-byte pointer and 4-byte length indicator (12 bytes total).
        // Aligning to 8 bytes so 16 total.
        public static readonly PrimitiveType STRING = new PrimitiveType("STRING", 16,PTValues.STRING);
        public static readonly PrimitiveType VARCHAR = new PrimitiveType("VARCHAR", 16,PTValues.VARCHAR);

        // Unsupported scalar type.
        public static readonly PrimitiveType BINARY = new PrimitiveType("BINARY", -1,PTValues.BINARY);

        // For decimal at the highest precision, the BE uses 16 bytes.
        public static readonly PrimitiveType DECIMAL = new PrimitiveType("DECIMAL", 16,PTValues.DECIMAL);

        // Fixed length char array.
        public static readonly PrimitiveType CHAR = new PrimitiveType("CHAR", -1,PTValues.CHAR);

        // Fixed length binary array, stored inline in the tuple. Currently only used
        // internally for intermediate results of builtin aggregate functions. Not exposed
        // in SQL in any way.
        public static readonly PrimitiveType FIXED_UDA_INTERMEDIATE = new PrimitiveType("FIXED_UDA_INTERMEDIATE", -1,PTValues.FIXED_UDA_INTERMEDIATE);
        
        public static IEnumerable<PrimitiveType> Values
        {
            get
            {
                yield return INVALID_TYPE;
                yield return NULL_TYPE;
                yield return BOOLEAN;
                yield return TINYINT;
                yield return SMALLINT;
                yield return INT;
                yield return BIGINT;
                yield return FLOAT;
                yield return DOUBLE;
                yield return DATE;
                yield return DATETIME;
                yield return TIMESTAMP;
                yield return STRING;
                yield return VARCHAR;
                yield return BINARY;
                yield return DECIMAL;
                yield return CHAR;
                yield return FIXED_UDA_INTERMEDIATE;
            }
        }

        public enum PTValues
        {
            INVALID_TYPE,
            NULL_TYPE,
            BOOLEAN,
            TINYINT,
            SMALLINT,
            INT,
            BIGINT,
            FLOAT,
            DOUBLE,
            DATE,
            DATETIME,
            TIMESTAMP,
            STRING,
            VARCHAR,
            BINARY,
            DECIMAL,
            CHAR,
            FIXED_UDA_INTERMEDIATE
        }
    }
}                           