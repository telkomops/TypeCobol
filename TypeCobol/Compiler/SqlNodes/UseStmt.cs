using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes
{
    public class UseStmt : StatementBase
    {
        private readonly string database;

        public UseStmt(string db)
        {
            database = db;
        }

        public string toSql()
        {
            return "USE" + database;
        }

        public override void reset()
        {
            throw new NotImplementedException();
        }


        public void analyze(Analyzer analyzer)
        {
            //if (database == Catalog.DEFAULT_DB)
            //{
            //    analyzer.getDb(database, Priviledge.ANY, true);
            //}
        }
    }
}
