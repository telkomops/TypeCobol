using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class TypeDef : IParseNode
    {
        private bool isAnalyzed_;
        private readonly SqlNodeType parsedType_;

        public void analyze(Analyzer analyzer)
        {
            if (isAnalyzed_) return;
            // Check the max nesting depth before calling the recursive analyze() to avoid
            // a stack overflow.
            if (parsedType_.exceedsMaxNestingDepth())
            {
                throw new AnalysisException(String.Format(
                    "SqlNodeType exceeds the maximum nesting depth of {0}s:\n{1}",
                    SqlNodeType.MaxNestingDepth, parsedType_.toSql()));
            }

            analyze(parsedType_, analyzer);
            isAnalyzed_ = true;
        }

        private void analyze(SqlNodeType type, Analyzer analyzer)
        {
            if (!type.isSupported())
            {
                throw new AnalysisException("Unsupported data type: " + type.toSql());
            }

            if (type.isScalarType())
            {
                analyzeScalarType((ScalarType) type, analyzer);
            }
            else if (type.isStructType())
            {
                analyzeStructType((StructType) type, analyzer);
            }
            else if (type.isArrayType())
            {
                ArrayType arrayType = (ArrayType) type;
                analyze(arrayType.getItemType(), analyzer);
            }
            else
            {
                //Preconditions.checkState(type.isMapType());
                //analyzeMapType((MapType)type, analyzer);
            }
        }

        private void analyzeScalarType(ScalarType scalarType, Analyzer analyzer)

        {
            PrimitiveType type = scalarType.getPrimitiveType();
            switch (type.value)
            {
                case PrimitiveType.PTValues.CHAR:
                case PrimitiveType.PTValues.VARCHAR:
                {
                    String name;
                    int maxLen;
                    if (type == PrimitiveType.VARCHAR)
                    {
                        name = "Varchar";
                        maxLen = ScalarType.MAX_VARCHAR_LENGTH;
                    }
                    else if (type == PrimitiveType.CHAR)
                    {
                        name = "Char";
                        maxLen = ScalarType.MAX_CHAR_LENGTH;
                    }
                    else
                    {
                        Preconditions.checkState(false);
                        return;
                    }

                    int len = scalarType.getLength();
                    if (len <= 0)
                    {
                        throw new AnalysisException(name + " size must be > 0: " + len);
                    }

                    if (scalarType.getLength() > maxLen)
                    {
                        throw new AnalysisException(
                            name + " size must be <= " + maxLen + ": " + len);
                    }
                }

                    break;
                case PrimitiveType.PTValues.DECIMAL:
                {
                    int precision = scalarType.decimalPrecision();
                    int scale = scalarType.decimalScale();
                    if (precision > ScalarType.MAX_PRECISION)
                    {
                        throw new AnalysisException("Decimal precision must be <= " +
                                                    ScalarType.MAX_PRECISION + ": " + precision);
                    }

                    if (precision == 0)
                    {
                        throw new AnalysisException("Decimal precision must be > 0: " + precision);
                    }

                    if (scale > precision)
                    {
                        throw new AnalysisException("Decimal scale (" + scale + ") must be <= " +
                                                    "precision (" + precision + ")");
                    }
                }
                    break;
                default: break;
            }
        }

        private void analyzeStructType(StructType structType, Analyzer analyzer)
        {
            // Check for duplicate field names.
            HashSet<String> fieldNames = new HashSet<string>();
            foreach (StructField f in structType.getFields())
            {
                analyze(f.getType(), analyzer);
                if (!fieldNames.Add(f.getName().ToLowerInvariant()))
                {
                    throw new AnalysisException(String.Format(
                        "Duplicate field name '{0}' in struct '{1}'", f.getName(), toSql()));
                }

                // Check whether the column name meets the Metastore's requirements.
                //if (!MetastoreShim.validateName(f.getName().ToLowerInvariant()))
                //{
                //    throw new AnalysisException("Invalid struct field name: " + f.getName());
                //}
            }
        }

        private void analyzeMapType(MapType mapType, Analyzer analyzer)

        {
            analyze(mapType.getKeyType(), analyzer);
            if (mapType.getKeyType().isComplexType())
            {
                throw new AnalysisException(
                    "Map type cannot have a complex-typed key: " + mapType.toSql());
            }

            analyze(mapType.getValueType(), analyzer);
        }

        public SqlNodeType getType()
        {
            return parsedType_;
        }


        public override String ToString()
        {
            return parsedType_.toSql();
        }


        public string toSql()
        {
            return parsedType_.toSql();
        }
    }
}
