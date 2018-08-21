using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Common
{
    public class BitFieldAction
    {
        private readonly string name;
        private readonly int code;

        public BitFieldAction(string name,int code)
        {
            this.name = name;
            this.code = code;
        }

        public int getActionCode => code;

        public bool implies(BitFieldAction that)
        {
            if (that != null)
            {
                return (code & that.code) == that.code;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (null == obj)
            {
                return false;
            }

            if (!typeof(BitFieldAction).IsInstanceOfType(obj))
            {
                return false;
            }

            BitFieldAction that = (BitFieldAction) obj;
            return code == that.code && name.Equals(that.name);
        }

        public override int GetHashCode()
        {
            return code + name.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }

        public string GetValue()
        {
            return name;
        }
    }
}
