using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Common
{
    public abstract class IdGenerator<T> : Id<T>
    {
        public IdGenerator(int id) : base(id)
        {
        }

        protected int nextId = 0;
        public abstract T GetNextId();
        public abstract T GetMaxId();
    }
}
