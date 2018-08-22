using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core;
using TypeCobol.Compiler.SqlNodes.Catalog;

namespace TypeCobol.Compiler.SqlNodes
{
    public class Predicate : Expr
    {
        public override string ToSqlImpl()
        {
            return base.toSql();
        }

        public override string DebugString()
        {
            return debugString();
        }

        public override bool LocalEquals(Expr that)
        {
            return localEquals(that);
        }

        public override void ResetAnalysisState()
        {
            resetAnalysisState();
        }

        public override Expr clone()
        {
            return clone();
        }

        public override bool IsBoundByTupleIds(List<int> tids)
        {
            return isBoundByTupleIds(tids);
        }

        public override bool IsBoundBySlotIds(List<SlotId> slotIds)
        {
            return isBoundBySlotIds(slotIds);
        }

        public override void GetIdsHelper(HashSet<int> tupleIds, HashSet<SlotId> slotIds)
        {
            GetIdsHelper(tupleIds,slotIds);
        }

        protected override bool IsConstantImpl()
        {
            return isConstantImpl();
        }

        public override Expr IgnoreImplicitCast()
        {
            return ignoreImplicitCast();
        }

        protected bool isEqJoinConjunct_;

        public Predicate():base()
        {
            isEqJoinConjunct_ = false;
        }

        /**
         * Copy c'tor used in clone().
         */
        protected Predicate(Predicate other):base(other)
        {
            isEqJoinConjunct_ = other.isEqJoinConjunct_;
        }

        public void setIsEqJoinConjunct(bool v) { isEqJoinConjunct_ = v; }

        protected void analyzeImpl(Analyzer analyzer)
        {
            type_ = SqlNodeType.BOOLEAN;
            // values: true/false/null
            numDistinctValues_ = 3;
        }

        /**
         * Returns true if one of the children is a slotref (possibly wrapped in a cast)
         * and the other children are all constant. Returns the slotref in 'slotRef' and
         * its child index in 'idx'.
         * This will pick up something like "col = 5", but not "2 * col = 10", which is
         * what we want.
         */
        public bool isSingleColumnPredicate(
            List<SlotRef> slotRefRef, List<int> idxRef)
        {
            // find slotref
            SlotRef slotRef = null;
            int i = 0;
            for (; i < children_.Count; ++i)
            {
                slotRef = getChild(i).unwrapSlotRef(false);
                if (slotRef != null) break;
            }
            if (slotRef == null) return false;

            // make sure everything else is constant
            for (int j = 0; j < children_.Count; ++j)
            {
                if (i == j) continue;
                if (!getChild(j).isConstant()) return false;
            }
            
            if (slotRefRef != null) slotRefRef=new List<SlotRef>() {slotRef};
            if (idxRef != null) idxRef=new List<int>(i);
            return true;
        }

        public static bool isEquivalencePredicate(Expr expr)
        {
            return (expr is BinaryPredicate)
                && ((BinaryPredicate)expr).getOp().isEquivalence();
        }

        /**
         * If predicate is of the form "<slotref> = <slotref>", returns both SlotRefs,
         * otherwise returns null.
         */
        public KeyValuePair<SlotId, SlotId>? getEqSlots() { return null; }

        /**
         * Returns the SlotRef bound by this Predicate.
         */
        public SlotRef getBoundSlot() { return null; }
    }
}
