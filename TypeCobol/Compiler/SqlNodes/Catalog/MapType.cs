using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    /**
 * Describes a MAP type. MAP types have a scalar key and an arbitrarily-typed value.
 */
    public class MapType : SqlNodeType    
    {
        private readonly SqlNodeType keyType_;
        private readonly SqlNodeType valueType_;

        public MapType(SqlNodeType keyType, SqlNodeType valueType)
        {
            keyType_ = keyType;
            valueType_ = valueType;
        }

        public SqlNodeType getKeyType => keyType_;
        public SqlNodeType getValueType => valueType_;

        public override string toSql(int depth)
        {
            if (depth >= MaxNestingDepth) return "MAP<...>";
            return string.Format("MAP<{0},{1}>",
                keyType_.toSql(depth + 1), valueType_.toSql(depth + 1));
        }

        public override string prettyPrint(int lpad)
        {
            string leftPadding = new string(' ', lpad);
            if (valueType_.isScalarType()) return leftPadding + toSql();
            // Pass in the padding to make sure nested fields are aligned properly,
            // even if we then strip the top-level padding.
            string structStr = valueType_.prettyPrint(lpad);
            structStr = structStr.Substring(lpad);
            return string.Format("{0}MAP<{1},{2}>", leftPadding, keyType_.toSql(), structStr);
        }

        public override bool isFullySpecifiedDecimal()
        {
            return false;
        }

        public override bool isWildcardDecimal()
        {
            return false;
        }

        public override bool isWildcardVarchar()
        {
            return false;
        }

        public override bool isWildcardChar()
        {
            return false;
        }

        public override bool isFixedLengthType()
        {
            return false;
        }

        public override bool isSupported()
        {
            return true;
        }

        public override bool supportsTablePartitioning()
        {
            return false;
        }

        public override bool Equals(object other)
        {
            if (!(other is MapType)) return false;
            MapType otherMapType = (MapType)other;
            return otherMapType.keyType_.Equals(keyType_) &&
                   otherMapType.valueType_.Equals(valueType_);
        }
    }
}
