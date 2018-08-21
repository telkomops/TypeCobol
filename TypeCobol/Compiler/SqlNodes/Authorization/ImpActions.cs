using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes.Authorization
{
    public class ImpActions : BitFieldAction
    {
        private readonly BitFieldAction bitFieldAction;


        public ImpActions(string name, int code) : base(name, code)
        {
            bitFieldAction = new BitFieldAction(name,code);
        }

        public static readonly ImpActions SELECT = new ImpActions("select", 1);
        public static readonly ImpActions INSERT = new ImpActions("insert", 1<<2);
        public static readonly ImpActions ALTER = new ImpActions("alter", 1<<3);
        public static readonly ImpActions CREATE = new ImpActions("create", 1<<4);
        public static readonly ImpActions DROP = new ImpActions("drop", 1<<5);
        public static readonly ImpActions REFRESH = new ImpActions("refresh", 1<<6);
        public static readonly ImpActions ALL = new ImpActions("*", SELECT.GetCode|
                                                                    INSERT.GetCode|
                                                                    ALTER.GetCode|
                                                                    CREATE.GetCode|
                                                                    DROP.GetCode|
                                                                    REFRESH.GetCode);

        public int GetCode => bitFieldAction.getActionCode;
        public BitFieldAction GetBitFieldAction => bitFieldAction;
        public string GetValue => bitFieldAction.GetValue();
    }
}
