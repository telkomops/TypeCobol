using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    public abstract class SqlNodeType
    {
        // Maximum nesting depth of a type. This limit was determined experimentally by
        // generating and scanning deeply nested Parquet and Avro files. In those experiments,
        // we exceeded the stack space in the scanner (which uses recursion for dealing with
        // nested types) at a nesting depth between 200 and 300 (200 worked, 300 crashed).
        public static int MaxNestingDepth = 100;

        // Static constant types for scalar types that don't require additional information.
        public static readonly ScalarType INVALID = new ScalarType(PrimitiveType.INVALID_TYPE);
        public static readonly ScalarType NULL = new ScalarType(PrimitiveType.NULL_TYPE);
        public static readonly ScalarType BOOLEAN = new ScalarType(PrimitiveType.BOOLEAN);
        public static readonly ScalarType TINYINT = new ScalarType(PrimitiveType.TINYINT);
        public static readonly ScalarType SMALLINT = new ScalarType(PrimitiveType.SMALLINT);
        public static readonly ScalarType INT = new ScalarType(PrimitiveType.INT);
        public static readonly ScalarType BIGINT = new ScalarType(PrimitiveType.BIGINT);
        public static readonly ScalarType FLOAT = new ScalarType(PrimitiveType.FLOAT);
        public static readonly ScalarType DOUBLE = new ScalarType(PrimitiveType.DOUBLE);
        public static readonly ScalarType STRING = new ScalarType(PrimitiveType.STRING);
        public static readonly ScalarType BINARY = new ScalarType(PrimitiveType.BINARY);
        public static readonly ScalarType TIMESTAMP = new ScalarType(PrimitiveType.TIMESTAMP);
        public static readonly ScalarType DATE = new ScalarType(PrimitiveType.DATE);
        public static readonly ScalarType DATETIME = new ScalarType(PrimitiveType.DATETIME);

        public static readonly ScalarType DEFAULT_DECIMAL = ScalarType.createDecimalType(ScalarType.DEFAULT_PRECISION,
            ScalarType.DEFAULT_SCALE);

        public static readonly ScalarType DECIMAL = ScalarType.createWildCardDecimalType();
        public static readonly ScalarType DEFAULT_VARCHAR = ScalarType.createVarcharType(-1);
        public static readonly ScalarType VARCHAR = ScalarType.createVarcharType(-1);
        public static readonly ScalarType CHAR = ScalarType.createCharType(-1);
        public static readonly ScalarType FIXED_UDA_INTERMEDIATE = ScalarType.createFixedUdaIntermediateType(-1);

        private static readonly List<ScalarType> IntegerTypes= new List<ScalarType> { TINYINT, SMALLINT, INT, BIGINT };
        private static readonly List<ScalarType> NumericTypes = new List<ScalarType> { TINYINT, SMALLINT, INT, BIGINT, FLOAT, DOUBLE, DECIMAL };
        private static readonly List<ScalarType> SupportedTypes = new List<ScalarType>
        {
            NULL,
            BOOLEAN,
            TINYINT,
            SMALLINT,
            INT,
            BIGINT,
            FLOAT,
            DOUBLE,
            STRING,
            VARCHAR,
            CHAR,
            TIMESTAMP,
            DECIMAL
        };
        private static readonly List<ScalarType> UnsupportedTypes = new List<ScalarType> { BINARY, DATE, DATETIME };

        public static IEnumerable<ScalarType> Values
        {
            get
            {
                yield return INVALID;
                yield return NULL;
                yield return BOOLEAN;
                yield return TINYINT;
                yield return SMALLINT;
                yield return INT;
                yield return BIGINT;
                yield return FLOAT;
                yield return DOUBLE;
                yield return STRING;
                yield return BINARY;
                yield return TIMESTAMP;
                yield return DATE;
                yield return DATETIME;
                yield return DEFAULT_DECIMAL;
                yield return DEFAULT_VARCHAR;
                yield return DECIMAL;
                yield return VARCHAR;
                yield return CHAR;
                yield return FIXED_UDA_INTERMEDIATE;
            }
        }

        


        public static List<ScalarType> getIntegerTypes()
        {
            return IntegerTypes;
        }

        public static List<ScalarType> getNumericTypes()
        {
            return NumericTypes;
        }

        public static List<ScalarType> getSupportedTypes()
        {
            return SupportedTypes;
        }

        public static List<ScalarType> getUnsupportedTypes()
        {
            return UnsupportedTypes;
        }

        /**
         * The output of this is stored directly in the hive metastore as the column type.
         * The string must match exactly.
         */
        public string toSql()
        {
            return toSql(0);
        }

        /**
         * Recursive helper for toSql() to be implemented by subclasses. Keeps track of the
         * nesting depth and terminates the recursion if MAX_NESTING_DEPTH is reached.
         */
        public abstract string toSql(int depth);

        /**
         * Same as toSql() but adds newlines and spaces for better readability of nested types.
         */
        public string prettyPrint()
        {
            return prettyPrint(0);
        }

        /**
         * Pretty prints this type with lpad number of leading spaces. Used to implement
         * prettyPrint() with space-indented nested types.
         */
        public abstract string prettyPrint(int lpad);

        public bool isInvalid()
        {
            return isScalarType(PrimitiveType.INVALID_TYPE);
        }

        public bool isValid()
        {
            return !isInvalid();
        }

        public bool isNull()
        {
            return isScalarType(PrimitiveType.NULL_TYPE);
        }

        public bool isBoolean()
        {
            return isScalarType(PrimitiveType.BOOLEAN);
        }

        public bool isTimestamp()
        {
            return isScalarType(PrimitiveType.TIMESTAMP);
        }

        public bool isDecimal()
        {
            return isScalarType(PrimitiveType.DECIMAL);
        }

        public abstract bool isFullySpecifiedDecimal();
        public abstract bool isWildcardDecimal();
        public abstract bool isWildcardVarchar();
        public abstract bool isWildcardChar();

        public bool isStringType()
        {
            return isScalarType(PrimitiveType.STRING) || isScalarType(PrimitiveType.VARCHAR) ||
                   isScalarType(PrimitiveType.CHAR);
        }

        public bool isScalarType()
        {
            return this is ScalarType;
        }

        public static bool isScalarType<T>(T type)
        {
            return type is ScalarType;
        }

        public bool isScalarType(PrimitiveType t)
        {
            return isScalarType() && ((ScalarType) this).getPrimitiveType() == t;
        }

        public bool isFixedPointType()
        {
            return isScalarType(PrimitiveType.TINYINT) || isScalarType(PrimitiveType.SMALLINT) ||
                   isScalarType(PrimitiveType.INT) || isScalarType(PrimitiveType.BIGINT) ||
                   isScalarType(PrimitiveType.DECIMAL);
        }

        public bool isFloatingPointType()
        {
            return isScalarType(PrimitiveType.FLOAT) || isScalarType(PrimitiveType.DOUBLE);
        }

        public bool isIntegerType()
        {
            return isScalarType(PrimitiveType.TINYINT) || isScalarType(PrimitiveType.SMALLINT)
                                                       || isScalarType(PrimitiveType.INT) ||
                                                       isScalarType(PrimitiveType.BIGINT);
        }

        // TODO: Handle complex types properly. Some instances may be fixed length.
        public abstract bool isFixedLengthType();

        public bool isNumericType()
        {
            return isFixedPointType() || isFloatingPointType() || isDecimal();
        }

        public bool isDateType()
        {
            return isScalarType(PrimitiveType.DATE) || isScalarType(PrimitiveType.DATETIME)
                                                    || isScalarType(PrimitiveType.TIMESTAMP);
        }

        public bool isComplexType()
        {
            return isStructType() || isCollectionType();
        }

        public bool isCollectionType()
        {
            return isMapType() || isArrayType();
        }

        public bool isMapType()
        {
            return this is MapType;
        }

        public bool isArrayType()
        {
            return this is ArrayType;
        }

        public bool isStructType()
        {
            return this is StructType;
        }

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

        public PrimitiveType getPrimitiveType()
        {
            return PrimitiveType.INVALID_TYPE;
        }

        /**
         * Returns the size in bytes of the fixed-length portion that a slot of this type
         * occupies in a tuple.
         */
        public int getSlotSize()
        {
            // 8-byte pointer and 4-byte length indicator (12 bytes total).
            // Per struct alignment rules, there is an extra 4 bytes of padding to align to 8
            // bytes so 16 bytes total.
            if (isCollectionType()) return 16;
            throw new InvalidOperationException("getSlotSize() not implemented for type " + toSql());
        }

        //public TColumnType toThrift()
        //{
        //    TColumnType container = new TColumnType();
        //    container.setTypes(new List<TTypeNode>());
        //    toThrift(container);
        //    return container;
        //}

        ///**
        // * Subclasses should override this method to add themselves to the thrift container.
        // */
        //public abstract void toThrift(TColumnType container);

        /**
         * Returns true if this type is equal to t, or if t is a wildcard variant of this
         * type. Subclasses should override this as appropriate. The default implementation
         * here is to avoid special-casing logic in callers for concrete types.
         */
        public bool matchesType(SqlNodeType t)
        {
            return false;
        }

        /**
         * Gets the ColumnType from the given FieldSchema by using Impala's SqlParser.
         * Returns null if the FieldSchema could not be parsed.
         * The type can either be:
         *   - Supported by Impala, in which case the type is returned.
         *   - A type Impala understands but is not yet implemented (e.g. date), the type is
         *     returned but type.IsSupported() returns false.
         *   - A type Impala can't understand at all in which case null is returned.
         */
        public static SqlNodeType parseColumnType(string typeStr)
        {
            // Wrap the type string in a CREATE TABLE stmt and use Impala's Parser
            // to get the ColumnType.
            // Pick a table name that can't be used.
            string stmt = string.Format("CREATE TABLE $DUMMY ($DUMMY {0})", typeStr);
            SqlScanner.SqlScanner input = new SqlScanner.SqlScanner(new StringReader(stmt));
            SqlParser.SqlParser parser = new SqlParser.SqlParser(input);
            CreateTableStmt createTableStmt;
            try
            {
                Object o = parser.parse().value;
                if (!(o is CreateTableStmt))
                {
                    // Should never get here.
                    throw new InvalidOperationException("Couldn't parse create table stmt.");
                }

                createTableStmt = (CreateTableStmt) o;
                if (createTableStmt.getColumnDefs().isEmpty())
                {
                    // Should never get here.
                    throw new InvalidOperationException("Invalid create table stmt.");
                }
            }
            catch (Exception)
            {
                return null;
            }

            TypeDef typeDef = createTableStmt.getColumnDefs().get(0).getTypeDef();
            return typeDef.getType();
        }

        /**
         * Returns true if t1 can be implicitly cast to t2 according to Impala's casting rules.
         * Implicit casts are always allowed when no loss of information would result (i.e.
         * every value of t1 can be represented exactly by a value of t2). Implicit casts are
         * allowed in certain other cases such as casting numeric types to floating point types
         * and converting strings to timestamps.
         *
         * If strictDecimal is true, only consider casts that result in no loss of information
         * when casting between decimal types.
         * If strict is true, only consider casts that result in no loss of information when
         * casting between any two types other than both decimals.
         *
         * TODO: Support casting of non-scalar types.
         */
        public static bool isImplicitlyCastable(
            SqlNodeType t1, SqlNodeType t2, bool strict, bool strictDecimal)
        {
            if (t1.isScalarType() && t2.isScalarType())
            {
                return ScalarType.isImplicitlyCastable(
                    (ScalarType) t1, (ScalarType) t2, strict, strictDecimal);
            }

            return false;
        }

        /**
         * Return type t such that values from both t1 and t2 can be assigned to t without an
         * explicit cast. If strict, does not consider conversions that would result in loss
         * of precision (e.g. converting decimal to float). Returns INVALID_TYPE if there is
         * no such type or if any of t1 and t2 is INVALID_TYPE.
         *
         * If strictDecimal is true, only consider casts that result in no loss of information
         * when casting between decimal types.
         * If strict is true, only consider casts that result in no loss of information when
         * casting between any two types other than both decimals.
         *
         *
         * TODO: Support non-scalar types.
         */
        public static SqlNodeType getAssignmentCompatibleType(
            SqlNodeType t1, SqlNodeType t2, bool strict, bool strictDecimal)
        {
            if (t1.isScalarType() && t2.isScalarType())
            {
                return ScalarType.getAssignmentCompatibleType(
                    (ScalarType) t1, (ScalarType) t2, strict, strictDecimal);
            }

            return ScalarType.INVALID;
        }

        /**
         * Returns true if this type exceeds the MAX_NESTING_DEPTH, false otherwise.
         */
        public bool exceedsMaxNestingDepth()
        {
            return exceedsMaxNestingDepth(0);
        }

        /**
         * Helper for exceedsMaxNestingDepth(). Recursively computes the max nesting depth,
         * terminating early if MAX_NESTING_DEPTH is reached. Returns true if this type
         * exceeds the MAX_NESTING_DEPTH, false otherwise.
         *
         * Examples of types and their nesting depth:
         * INT --> 1
         * STRUCT<f1:INT> --> 2
         * STRUCT<f1:STRUCT<f2:INT>> --> 3
         * ARRAY<INT> --> 2
         * ARRAY<STRUCT<f1:INT>> --> 3
         * MAP<string,INT> --> 2
         * MAP<string,STRUCT<f1:INT>> --> 3
         */
        private bool exceedsMaxNestingDepth(int d)
        {
            if (d >= MaxNestingDepth) return true;
            if (isStructType())
            {
                StructType structType = (StructType) (object) this;
                foreach (var f in structType.getFields())
                {
                    if (f.getType().exceedsMaxNestingDepth(d + 1)) return true;
                }
            }
            else if (isArrayType())
            {
                ArrayType arrayType = (ArrayType) this;
                if (arrayType.GetItemType.exceedsMaxNestingDepth(d + 1)) return true;
            }
            else if (isMapType())
            {
                MapType mapType = (MapType) this;
                if (mapType.getValueType.exceedsMaxNestingDepth(d + 1)) return true;
            }

            return false;
        }

        //public static List<TColumnType> toThrift(Type[] types)
        //{
        //    return toThrift(Lists.newList(types));
        //}

        //public static List<TColumnType> toThrift(List<Type> types)
        //{
        //    List<TColumnType> result = Lists.newList();
        //    for (Type t: types)
        //    {
        //        result.add(t.toThrift());
        //    }
        //    return result;
        //}

        //public static Type fromThrift(TColumnType thrift)
        //{
        //    Preconditions.checkState(thrift.types.size() > 0);
        //    Tuple<Type, int> t = fromThrift(thrift, 0);
        //    Preconditions.checkState(t.second.equals(thrift.getTypesSize()));
        //    return t.first;
        //}

        /**
         * Constructs a ColumnType rooted at the TTypeNode at nodeIdx in TColumnType.
         * Returned pair: The resulting ColumnType and the next nodeIdx that is not a child
         * type of the result.
         */
        //protected static Pair<Type, int> fromThrift(TColumnType col, int nodeIdx)
        //{
        //    TTypeNode node = col.getTypes().get(nodeIdx);
        //    Type type = null;
        //    switch (node.getType())
        //    {
        //        case SCALAR:
        //            {
        //                Preconditions.checkState(node.isSetScalar_type());
        //                TScalarType scalarType = node.getScalar_type();
        //                if (scalarType.getType() == TPrimitiveType.CHAR)
        //                {
        //                    Preconditions.checkState(scalarType.isSetLen());
        //                    type = ScalarType.createCharType(scalarType.getLen());
        //                }
        //                else if (scalarType.getType() == TPrimitiveType.VARCHAR)
        //                {
        //                    Preconditions.checkState(scalarType.isSetLen());
        //                    type = ScalarType.createVarcharType(scalarType.getLen());
        //                }
        //                else if (scalarType.getType() == TPrimitiveType.DECIMAL)
        //                {
        //                    Preconditions.checkState(scalarType.isSetPrecision()
        //                        && scalarType.isSetScale());
        //                    type = ScalarType.createDecimalType(scalarType.getPrecision(),
        //                        scalarType.getScale());
        //                }
        //                else
        //                {
        //                    type = ScalarType.createType(
        //                        PrimitiveType.fromThrift(scalarType.getType()));
        //                }
        //                ++nodeIdx;
        //                break;
        //            }
        //        case ARRAY:
        //            {
        //                Preconditions.checkState(nodeIdx + 1 < col.getTypesSize());
        //                Pair<Type, int> childType = fromThrift(col, nodeIdx + 1);
        //                type = new ArrayType(childType.first);
        //                nodeIdx = childType.second;
        //                break;
        //            }
        //        case MAP:
        //            {
        //                Preconditions.checkState(nodeIdx + 2 < col.getTypesSize());
        //                Pair<Type, int> keyType = fromThrift(col, nodeIdx + 1);
        //                Tuple<Type, int> valueType = fromThrift(col, keyType.second);
        //                type = new MapType(keyType.first, valueType.first);
        //                nodeIdx = valueType.second;
        //                break;
        //            }
        //        case STRUCT:
        //            {
        //                Preconditions.checkState(nodeIdx + node.getStruct_fieldsSize() < col.getTypesSize());
        //                List<StructField> structFields = Lists.newList();
        //                ++nodeIdx;
        //                for (int i = 0; i < node.getStruct_fieldsSize(); ++i)
        //                {
        //                    TStructField thriftField = node.getStruct_fields().get(i);
        //                    string name = thriftField.getName();
        //                    string comment = null;
        //                    if (thriftField.isSetComment()) comment = thriftField.getComment();
        //                    Tuple<Type, int> res = fromThrift(col, nodeIdx);
        //                    nodeIdx = res.second.intValue();
        //                    structFields.add(new StructField(name, res.first, comment));
        //                }
        //                type = new StructType(structFields);
        //                break;
        //            }
        //    }
        //    return new Tuple<Type, int>(type, nodeIdx);
        //}

        /**
         * JDBC data type description
         * Returns the column size for this type.
         * For numeric data this is the maximum precision.
         * For character data this is the length in characters.
         * For datetime types this is the length in characters of the string representation
         * (assuming the maximum allowed precision of the fractional seconds component).
         * For binary data this is the length in bytes.
         * Null is returned for for data types where the column size is not applicable.
         */
        public int getColumnSize()
        {
            if (!isScalarType()) return -1;
            if (isNumericType()) return getPrecision()??-1;
            ScalarType t = (ScalarType) this;
            switch (t.getPrimitiveType().value)
            {
                case PrimitiveType.PTValues.STRING:
                    return int.MaxValue;
                case PrimitiveType.PTValues.TIMESTAMP:
                    return 29;
                case PrimitiveType.PTValues.CHAR:
                case PrimitiveType.PTValues.VARCHAR:
                case PrimitiveType.PTValues.FIXED_UDA_INTERMEDIATE:
                    return t.getLength();
                default:
                    return -1;
            }
        }

        /**
         * JDBC data type description
         * For numeric types, returns the maximum precision for this type.
         * For non-numeric types, returns null.
         */
        public int? getPrecision()
        {
            if (!isScalarType()) return null;
            ScalarType t = (ScalarType) this;
            switch (t.getPrimitiveType().value)
            {
                case PrimitiveType.PTValues.TINYINT:
                    return 3;
                case PrimitiveType.PTValues.SMALLINT:
                    return 5;
                case PrimitiveType.PTValues.INT:
                    return 10;
                case PrimitiveType.PTValues.BIGINT:
                    return 19;
                case PrimitiveType.PTValues.FLOAT:
                    return 7;
                case PrimitiveType.PTValues.DOUBLE:
                    return 15;
                case PrimitiveType.PTValues.DECIMAL:
                    return t.decimalPrecision();
                default:
                    return null;
            }
        }

        /**
         * JDBC data type description
         * Returns the number of fractional digits for this type, or null if not applicable.
         * For timestamp/time types, returns the number of digits in the fractional seconds
         * component.
         */
        public int? getDecimalDigits()
        {
            if (!isScalarType()) return null;
            ScalarType t = (ScalarType) this;
            switch (t.getPrimitiveType().value)
            {
                case PrimitiveType.PTValues.BOOLEAN:
                case PrimitiveType.PTValues.TINYINT:
                case PrimitiveType.PTValues.SMALLINT:
                case PrimitiveType.PTValues.INT:
                case PrimitiveType.PTValues.BIGINT:
                    return 0;
                case PrimitiveType.PTValues.FLOAT:
                    return 7;
                case PrimitiveType.PTValues.DOUBLE:
                    return 15;
                case PrimitiveType.PTValues.TIMESTAMP:
                    return 9;
                case PrimitiveType.PTValues.DECIMAL:
                    return t.decimalScale();
                default:
                    return null;
            }
        }

        /**
         * JDBC data type description
         * For numeric data types, either 10 or 2. If it is 10, the values in COLUMN_SIZE
         * and DECIMAL_DIGITS give the number of decimal digits allowed for the column.
         * For example, a DECIMAL(12,5) column would return a NUM_PREC_RADIX of 10,
         * a COLUMN_SIZE of 12, and a DECIMAL_DIGITS of 5; a FLOAT column could return
         * a NUM_PREC_RADIX of 10, a COLUMN_SIZE of 15, and a DECIMAL_DIGITS of NULL.
         * If it is 2, the values in COLUMN_SIZE and DECIMAL_DIGITS give the number of bits
         * allowed in the column. For example, a FLOAT column could return a RADIX of 2,
         * a COLUMN_SIZE of 53, and a DECIMAL_DIGITS of NULL. NULL is returned for data
         * types where NUM_PREC_RADIX is not applicable.
         */
        public int? getNumPrecRadix()
        {
            if (!isScalarType()) return null;
            ScalarType t = (ScalarType) this;
            switch (t.getPrimitiveType().value)
            {
                case PrimitiveType.PTValues.TINYINT:
                case PrimitiveType.PTValues.SMALLINT:
                case PrimitiveType.PTValues.INT:
                case PrimitiveType.PTValues.BIGINT:
                case PrimitiveType.PTValues.FLOAT:
                case PrimitiveType.PTValues.DOUBLE:
                case PrimitiveType.PTValues.DECIMAL:
                    return 10;
                default:
                    // everything else (including bool and string) is null
                    return null;
            }
        }

        /**
         * JDBC data type description
         * Returns the java SQL type enum
         */
        public TypeCode getDotNetSqlType()
        {
            //TODO - find better match
            //if (isStructType()) return java.sql.Types.STRUCT;
            //// Both MAP and ARRAY are reported as ARRAY, since there is no better matching
            //// Java SQL type. This behavior is consistent with Hive.
            //if (isCollectionType()) return java.sql.Types.ARRAY;

            if (isScalarType())
                throw new InvalidOperationException("Invalid non-scalar type: " + toSql());
            ScalarType t = (ScalarType) this;
            switch (t.getPrimitiveType().value)
            {
                case PrimitiveType.PTValues.NULL_TYPE: return Type.GetTypeCode(typeof(Nullable));
                case PrimitiveType.PTValues.BOOLEAN: return Type.GetTypeCode(typeof(bool));
                case PrimitiveType.PTValues.TINYINT: return Type.GetTypeCode(typeof(byte));
                case PrimitiveType.PTValues.SMALLINT: return Type.GetTypeCode(typeof(Int16));
                case PrimitiveType.PTValues.INT: return Type.GetTypeCode(typeof(int));
                case PrimitiveType.PTValues.BIGINT: return Type.GetTypeCode(typeof(Int64));
                case PrimitiveType.PTValues.FLOAT:
                case PrimitiveType.PTValues.DOUBLE: return Type.GetTypeCode(typeof(double));
                case PrimitiveType.PTValues.TIMESTAMP: return Type.GetTypeCode(typeof(byte[]));
                case PrimitiveType.PTValues.STRING:
                case PrimitiveType.PTValues.CHAR:
                case PrimitiveType.PTValues.VARCHAR: return Type.GetTypeCode(typeof(string));
                case PrimitiveType.PTValues.BINARY: return Type.GetTypeCode(typeof(byte[]));
                case PrimitiveType.PTValues.DECIMAL: return Type.GetTypeCode(typeof(decimal));
                case PrimitiveType.PTValues.FIXED_UDA_INTERMEDIATE:
                    return Type.GetTypeCode(typeof(byte[]));

                default:
                    return 0;
            }
        }

        /**
         * Matrix that records "smallest" assignment-compatible type of two types
         * (INVALID_TYPE if no such type exists, ie, if the input types are fundamentally
         * incompatible). A value of any of the two types could be assigned to a slot
         * of the assignment-compatible type. For strict compatibility, this can be done
         * without any loss of precision. For non-strict compatibility, there may be loss of
         * precision, e.g. if converting from BIGINT to FLOAT.
         *
         * We chose not to follow MySQL's type casting behavior as described here:
         * http://dev.mysql.com/doc/refman/5.0/en/type-conversion.html
         * for the following reasons:
         * conservative casting in arithmetic exprs: TINYINT + TINYINT -> BIGINT
         * comparison of many types as double: INT < FLOAT -> comparison as DOUBLE
         * special cases when dealing with dates and timestamps.
         */
        protected static PrimitiveType[,] compatibilityMatrix;

        /**
         * If we are checking in strict mode, any non-null entry in this matrix overrides
         * compatibilityMatrix. If the entry is null, the entry in compatibility matrix
         * is valid.
         */
        protected static PrimitiveType[,] strictCompatibilityMatrix;

        public SqlNodeType()
        {
            compatibilityMatrix = new
                PrimitiveType[PrimitiveType.Values.Count(), PrimitiveType.Values.Count()];
            strictCompatibilityMatrix = new
                PrimitiveType[PrimitiveType.Values.Count(),PrimitiveType.Values.Count()];

            var i = 0;
            foreach (var value in PrimitiveType.Values)
            {

                // Each type is compatible with itself.
                compatibilityMatrix[i,i] = value;
                // BINARY is not supported.
                compatibilityMatrix[BINARY.ordinal(),i] = PrimitiveType.INVALID_TYPE;
                compatibilityMatrix[i,BINARY.ordinal()] = PrimitiveType.INVALID_TYPE;

                // FIXED_UDA_INTERMEDIATE cannot be cast to/from another type
                if (i != FIXED_UDA_INTERMEDIATE.ordinal())
                {
                    compatibilityMatrix[FIXED_UDA_INTERMEDIATE.ordinal(),i] =
                        PrimitiveType.INVALID_TYPE;
                    compatibilityMatrix[i,FIXED_UDA_INTERMEDIATE.ordinal()] =
                        PrimitiveType.INVALID_TYPE;

                }

                i++;
            }

            compatibilityMatrix[BOOLEAN.ordinal(),TINYINT.ordinal()] = PrimitiveType.TINYINT;
            compatibilityMatrix[BOOLEAN.ordinal(),SMALLINT.ordinal()] = PrimitiveType.SMALLINT;
            compatibilityMatrix[BOOLEAN.ordinal(),INT.ordinal()] = PrimitiveType.INT;
            compatibilityMatrix[BOOLEAN.ordinal(),BIGINT.ordinal()] = PrimitiveType.BIGINT;
            compatibilityMatrix[BOOLEAN.ordinal(),FLOAT.ordinal()] = PrimitiveType.FLOAT;
            compatibilityMatrix[BOOLEAN.ordinal(),DOUBLE.ordinal()] = PrimitiveType.DOUBLE;
            compatibilityMatrix[BOOLEAN.ordinal(),DATE.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BOOLEAN.ordinal(),DATETIME.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BOOLEAN.ordinal(),TIMESTAMP.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BOOLEAN.ordinal(),STRING.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BOOLEAN.ordinal(),VARCHAR.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BOOLEAN.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            compatibilityMatrix[TINYINT.ordinal(),SMALLINT.ordinal()] = PrimitiveType.SMALLINT;
            compatibilityMatrix[TINYINT.ordinal(),INT.ordinal()] = PrimitiveType.INT;
            compatibilityMatrix[TINYINT.ordinal(),BIGINT.ordinal()] = PrimitiveType.BIGINT;
            // 8 bit int fits in mantissa of both float and double.
            compatibilityMatrix[TINYINT.ordinal(),FLOAT.ordinal()] = PrimitiveType.FLOAT;
            compatibilityMatrix[TINYINT.ordinal(),DOUBLE.ordinal()] = PrimitiveType.DOUBLE;
            compatibilityMatrix[TINYINT.ordinal(),DATE.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[TINYINT.ordinal(),DATETIME.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[TINYINT.ordinal(),TIMESTAMP.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[TINYINT.ordinal(), STRING.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[TINYINT.ordinal(),VARCHAR.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[TINYINT.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            compatibilityMatrix[SMALLINT.ordinal(),INT.ordinal()] = PrimitiveType.INT;
            compatibilityMatrix[SMALLINT.ordinal(),BIGINT.ordinal()] = PrimitiveType.BIGINT;
            // 16 bit int fits in mantissa of both float and double.
            compatibilityMatrix[SMALLINT.ordinal(),FLOAT.ordinal()] = PrimitiveType.FLOAT;
            compatibilityMatrix[SMALLINT.ordinal(),DOUBLE.ordinal()] = PrimitiveType.DOUBLE;
            compatibilityMatrix[SMALLINT.ordinal(),DATE.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[SMALLINT.ordinal(),DATETIME.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[SMALLINT.ordinal(),TIMESTAMP.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[SMALLINT.ordinal(), STRING.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[SMALLINT.ordinal(),VARCHAR.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[SMALLINT.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            compatibilityMatrix[INT.ordinal(),BIGINT.ordinal()] = PrimitiveType.BIGINT;
            // 32 bit int fits only mantissa of double.
            // TODO: arguably we should promote INT + FLOAT to DOUBLE to avoid loss of precision,
            // but we depend on it remaining FLOAT for some use cases, e.g.
            // "insert into tbl (float_col) select int_col + float_col from ..."
            compatibilityMatrix[INT.ordinal(),FLOAT.ordinal()] = PrimitiveType.FLOAT;
            strictCompatibilityMatrix[INT.ordinal(),FLOAT.ordinal()] = PrimitiveType.DOUBLE;
            compatibilityMatrix[INT.ordinal(),DOUBLE.ordinal()] = PrimitiveType.DOUBLE;
            compatibilityMatrix[INT.ordinal(),DATE.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[INT.ordinal(),DATETIME.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[INT.ordinal(),TIMESTAMP.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[INT.ordinal(), STRING.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[INT.ordinal(),VARCHAR.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[INT.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            // 64 bit int does not fit in mantissa of double or float.
            // TODO: arguably we should always promote BIGINT + FLOAT to double here to keep as
            // much precision as possible, but we depend on this implicit cast for some use
            // cases, similarly to INT + FLOAT.
            compatibilityMatrix[BIGINT.ordinal(),FLOAT.ordinal()] = PrimitiveType.FLOAT;
            strictCompatibilityMatrix[BIGINT.ordinal(),FLOAT.ordinal()] = PrimitiveType.DOUBLE;
            // TODO: we're breaking the definition of strict compatibility for BIGINT + DOUBLE,
            // but this forces function overloading to consider the DOUBLE overload first.
            compatibilityMatrix[BIGINT.ordinal(),DOUBLE.ordinal()] = PrimitiveType.DOUBLE;
            compatibilityMatrix[BIGINT.ordinal(),DATE.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BIGINT.ordinal(),DATETIME.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BIGINT.ordinal(),TIMESTAMP.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BIGINT.ordinal(), STRING.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BIGINT.ordinal(),VARCHAR.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[BIGINT.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            compatibilityMatrix[FLOAT.ordinal(),DOUBLE.ordinal()] = PrimitiveType.DOUBLE;
            compatibilityMatrix[FLOAT.ordinal(),DATE.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[FLOAT.ordinal(),DATETIME.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[FLOAT.ordinal(),TIMESTAMP.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[FLOAT.ordinal(), STRING.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[FLOAT.ordinal(),VARCHAR.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[FLOAT.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            compatibilityMatrix[DOUBLE.ordinal(),DATE.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DOUBLE.ordinal(),DATETIME.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DOUBLE.ordinal(),TIMESTAMP.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DOUBLE.ordinal(), STRING.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DOUBLE.ordinal(),VARCHAR.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DOUBLE.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            compatibilityMatrix[DATE.ordinal(),DATETIME.ordinal()] = PrimitiveType.DATETIME;
            compatibilityMatrix[DATE.ordinal(),TIMESTAMP.ordinal()] = PrimitiveType.TIMESTAMP;
            compatibilityMatrix[DATE.ordinal(), STRING.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DATE.ordinal(),VARCHAR.ordinal()] = PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DATE.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            compatibilityMatrix[DATETIME.ordinal(),TIMESTAMP.ordinal()] =
                PrimitiveType.TIMESTAMP;
            compatibilityMatrix[DATETIME.ordinal(), STRING.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DATETIME.ordinal(),VARCHAR.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[DATETIME.ordinal(),CHAR.ordinal()] =
                PrimitiveType.INVALID_TYPE;

            // We can convert some but not all string values to timestamps.
            compatibilityMatrix[TIMESTAMP.ordinal(), STRING.ordinal()] =
                PrimitiveType.TIMESTAMP;
            strictCompatibilityMatrix[TIMESTAMP.ordinal(), STRING.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[TIMESTAMP.ordinal(),VARCHAR.ordinal()] =
                PrimitiveType.INVALID_TYPE;
            compatibilityMatrix[TIMESTAMP.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            compatibilityMatrix[STRING.ordinal(),VARCHAR.ordinal()] = PrimitiveType.STRING;
            compatibilityMatrix[STRING.ordinal(),CHAR.ordinal()] = PrimitiveType.STRING;

            compatibilityMatrix[VARCHAR.ordinal(),CHAR.ordinal()] = PrimitiveType.INVALID_TYPE;

            //TODO - analyze how to translate to c#
            // Check all of the necessary entries that should be filled.
            //for (int i = 0; i < PrimitiveType.Values.Count(); ++i)
            //{
            //    for (int j = i; j < PrimitiveType.Values.Count(); ++j)
            //    {
            //        PrimitiveType t1 = PrimitiveType.Values[i];
            //        PrimitiveType t2 = PrimitiveType.Values[j];
            //        // DECIMAL, NULL, and INVALID_TYPE  are handled separately.
            //        if (t1 == PrimitiveType.INVALID_TYPE ||
            //            t2 == PrimitiveType.INVALID_TYPE) continue;
            //        if (t1 == PrimitiveType.NULL_TYPE || t2 == PrimitiveType.NULL_TYPE) continue;
            //        if (t1 == PrimitiveType.DECIMAL || t2 == PrimitiveType.DECIMAL) continue;
            //        Preconditions.checkNotNull(compatibilityMatrix[i,j]);
            //    }
            //}
        }
    }
}
