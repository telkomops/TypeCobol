using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    public class StructField : SqlNodeType
    {

        protected readonly string name_;
        protected readonly SqlNodeType type_;
        protected readonly string comment_;
        protected int position_;  // in struct

        public StructField(string name, SqlNodeType type, string comment)
        {
            // Impala expects field names to be in lower case, but type strings stored in the HMS
            // are not guaranteed to be lower case.
            name_ = name.ToLowerInvariant();
            type_ = type;
            comment_ = comment;
        }

        public String getComment() { return comment_; }
        public String getName() { return name_; }
        public SqlNodeType getType() { return type_; }
        public int getPosition() { return position_; }
        public void setPosition(int position) { position_ = position; }

        public override string toSql(int depth)
        {
            string typeSql = (depth < SqlNodeType.MaxNestingDepth) ? type_.toSql(depth) : "...";
            StringBuilder sb = new StringBuilder(name_);
            if (type_ != null) sb.Append(":" + typeSql);
            if (comment_ != null) sb.Append(string.Format(" COMMENT '{0}'", comment_));
            return sb.ToString();
        }

        public override string prettyPrint(int lpad)
        {
            string leftPadding = new string(' ', lpad);
            StringBuilder sb = new StringBuilder(leftPadding + name_);
            if (type_ != null)
            {
                // Pass in the padding to make sure nested fields are aligned properly,
                // even if we then strip the top-level padding.
                string typeStr = type_.prettyPrint(lpad);
                typeStr = typeStr.Substring(lpad);
                sb.Append(":" + typeStr);
            }
            if (comment_ != null) sb.Append(string.Format(" COMMENT '%s'", comment_));
            return sb.ToString();
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
            if (!(other is StructField)) return false;
            StructField otherStructField = (StructField)other;
            return otherStructField.name_.Equals(name_) && otherStructField.type_.Equals(type_);
        }
    }
}
