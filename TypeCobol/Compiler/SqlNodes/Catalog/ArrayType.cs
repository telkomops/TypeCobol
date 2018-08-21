using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    /**
 * Describes an ARRAY type.
 */
    public class ArrayType : SqlNodeType
    {
        private readonly SqlNodeType _itemType;

        public ArrayType(SqlNodeType itemType)
        {
            this._itemType = itemType;
        }

        public SqlNodeType GetItemType => _itemType;  

        public override string toSql(int depth)
        {
            if (depth >= MaxNestingDepth) return "ARRAY<...>";
            return string.Format("ARRAY<{0}>", _itemType.toSql(depth + 1));
        }

        public override string prettyPrint(int lpad)
        {
            string leftPadding = new string(' ', lpad);
            if (_itemType.isScalarType()) return leftPadding + toSql(0);
            // Pass in the padding to make sure nested fields are aligned properly,
            // even if we then strip the top-level padding.
            string structStr = _itemType.prettyPrint(lpad);
            structStr = structStr.Substring(lpad);
            return string.Format("{0}ARRAY<{1}>", leftPadding, structStr);
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
            if (!(other is ArrayType)) return false;
            ArrayType otherArrayType = (ArrayType)other;
            return otherArrayType._itemType.Equals(_itemType);
        }
    }
}
