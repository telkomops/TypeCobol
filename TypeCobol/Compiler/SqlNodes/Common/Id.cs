using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes.Common
{
    public class Id<T>
    {
        protected readonly int id;
        public Id(int id)
        {
            this.id = id;
        }

        public int asInt => id;
        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;
            return (obj as Id<T>).id == id;
        }

        public int CompareTo(Id<T> cmp)
        {
            return id - cmp.id;
        }

        public List<T> asList()
        {
            List<T> list = new List<T> ();
            list.Add((T)(object)this);
            return list;
        }

        public static string printIds(List<T> ids)
        {
            List<string> l = new List<string>();
            foreach (T id in ids)
            {
                l.Add(id.ToString());
            }
            return "(" + string.Join(" ", l) + ")";
        }
    }
}
