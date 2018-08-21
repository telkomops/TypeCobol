using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    public class StructType : SqlNodeType
    {
        private readonly Dictionary<string, StructField> fieldMap_ = new Dictionary<string, StructField>();
        private readonly List<StructField> fields_;
        
        public StructType(List<StructField> fields)
        {
            fields_ = fields;
            for (int i = 0; i < fields_.Count; i++)
            {
                fields_[i].setPosition(i);
                fieldMap_.Add(fields_[i].getName().ToLowerInvariant(), fields_[i]);
            }
        }

        public StructType()
        {
            fields_ = new List<StructField>();
        }

        public void addField(StructField field)
        {
            field.setPosition(fields_.Count + 1);
            fields_.Add(field);
            fieldMap_.Add(field.getName().ToLowerInvariant(), field);
        }

        public List<StructField> getFields() { return fields_; }

        public StructField getField(string fieldName)
        {
            StructField searchedvalue;
            fieldMap_.TryGetValue(fieldName.ToLowerInvariant(),out searchedvalue);
            return searchedvalue;
        }

        public void clearFields()
        {
            fields_.Clear();
            fieldMap_.Clear();
        }

        public override string toSql(int depth)
        {
            if (depth >= MaxNestingDepth) return "STRUCT<...>";
            List<string> fieldsSql = new List<string>();
            foreach (StructField f in fields_) fieldsSql.Add(f.toSql(depth + 1));
            return string.Format("STRUCT<{0}>", string.Join(",",fieldsSql));
        }

        public override string prettyPrint(int lpad)
        {
            String leftPadding = new string(' ', lpad);
            List<string> fieldsSql = new List<string>();
            foreach (StructField f in fields_) fieldsSql.Add(f.prettyPrint(lpad + 2));
           return string.Format("{0}STRUCT<\n{1}\n{2}>",
                leftPadding, string.Join(",\n",fieldsSql), leftPadding);
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
            if (!(other is StructType)) return false;
            StructType otherStructType = (StructType)other;
            return otherStructType.getFields().Equals(fields_);
        }
    }
}
