using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class SlotId : IdGenerator<SlotId>
    {
        public SlotId(int id) : base(id)
        {
        }

        public override SlotId GetMaxId()
        {
            return new SlotId(base.nextId++);
        }

        public override SlotId GetNextId()
        {
            return new SlotId(base.nextId-1);
        }
    }
}
