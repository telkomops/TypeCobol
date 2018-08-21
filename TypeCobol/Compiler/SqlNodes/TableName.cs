using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
 * Represents a table/view name that optionally includes its database (a fully qualified
 * table name). Analysis of this table name checks for validity of the database and
 * table name according to the Metastore's policy (see @MetaStoreUtils).
 * According to that definition, we can still use "invalid" table names for tables/views
 * that are not stored in the Metastore, e.g., for Inline Views or WITH-clause views.
 */
    public class TableName
    {
        private readonly string db_;
        private readonly string tbl_;

        public TableName(string db,string tbl)
        {
            db_ = db;
            tbl_ = tbl;
        }

        public string getDb() => db_;
        public string getTbl() => tbl_;
        public bool isExmpty() => string.IsNullOrEmpty(tbl_);

        /**
   * Returns true if this name has a non-empty database field and a non-empty
   * table name.
   */
        public bool isFullyQualified() => !string.IsNullOrEmpty(db_) && !string.IsNullOrEmpty(tbl_);

        public override string ToString()
        {
            if (string.IsNullOrEmpty(db_))
            {
                return tbl_;
            }
            return db_ + "." + tbl_;
        }

        public List<string> toPath()
        {
            List<string> result = new List<string>(2);
            if (!string.IsNullOrEmpty(db_)) result.Add(db_);
            result.Add(tbl_);
            return result;
        }
    }
}
