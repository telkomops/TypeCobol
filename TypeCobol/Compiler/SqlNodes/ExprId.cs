using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class ExprId : IdGenerator<ExprId>
    {
        public ExprId(int id) : base(id)
        {
        }

        public override ExprId GetNextId()
        {
            return new ExprId(nextId++);
        }

        public override ExprId GetMaxId()
        {
            return new ExprId(nextId - 1);
        }
    }
}
