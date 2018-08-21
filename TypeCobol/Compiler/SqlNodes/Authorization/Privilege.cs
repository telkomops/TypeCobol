using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Authorization
{
    public class Privilege
    {
        private readonly List<ImpActions> actions;
        private readonly bool anyOf;

        public Privilege(List<ImpActions> action, bool any)
        {
            actions = action;
            anyOf = any;
        }

        public static readonly Privilege ALL = new Privilege(new List<ImpActions>() {ImpActions.ALL},false);
        public static readonly Privilege ALTER = new Privilege(new List<ImpActions>() { ImpActions.ALTER},false);
        public static readonly Privilege DROP = new Privilege(new List<ImpActions>() { ImpActions.DROP},false);
        public static readonly Privilege CREATE = new Privilege(new List<ImpActions>() { ImpActions.CREATE},false);
        public static readonly Privilege INSERT = new Privilege(new List<ImpActions>() { ImpActions.INSERT},false);
        public static readonly Privilege SELECT = new Privilege(new List<ImpActions>() { ImpActions.SELECT},false);
        public static readonly Privilege REFRESH = new Privilege(new List<ImpActions>() { ImpActions.REFRESH},false);
        public static readonly Privilege VIEW_METADATA = new Privilege(new List<ImpActions>() { ImpActions.INSERT, ImpActions.SELECT,ImpActions.REFRESH},true);
        public static readonly Privilege ANY = new Privilege(new List<ImpActions>()
        {
            ImpActions.SELECT,
            ImpActions.INSERT,
            ImpActions.ALTER,
            ImpActions.CREATE,
            ImpActions.DROP,
            ImpActions.REFRESH,
            ImpActions.ALL
        }, true);

        public Privilege(ImpActions action,bool any): this(new List<ImpActions>() {action}, any)
        {
        }

        public List<ImpActions> GetActions => actions;
        /*
         * Determines whether to check if the user has ANY the privileges defined in the
         * actions list or whether to check if the user has ALL of the privileges in the
         * actions list.
         */
        public bool GetAnyOf => anyOf;
    }
}
