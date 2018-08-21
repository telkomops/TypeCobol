using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes
{
    public class CreateTableStmt : StatementBase
    {
        // Table parameters specified in a CREATE TABLE statement
        private readonly TableDef tableDef_;

        // Table owner. Set during analysis
        private string owner_;

        public CreateTableStmt(TableDef tableDef)
        {
            tableDef_ = tableDef;
        }

        public override void reset()
        {
            tableDef_.reset();
        }

        public CreateTableStmt clone()
        {
            return new CreateTableStmt(this.tableDef_);
        }

        //TODO - finish implementation
    }
}
