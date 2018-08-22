using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Map of expression substitutions: lhs[i] gets substituted with rhs[i].
     * To support expression substitution across query blocks, rhs exprs must already be
     * analyzed when added to this map. Otherwise, analysis of a SlotRef may fail after
     * substitution, e.g., because the table it refers to is in a different query block
     * that is not visible.
     * See Expr.substitute() and related functions for details on the actual substitution.
     */
    public sealed class ExprSubstitutionMap
    {
        private List<Expr> lhs_; // left-hand side
        private List<Expr> rhs_; // right-hand side

        public ExprSubstitutionMap():
            this(new List<Expr>(), new List<Expr>())
        { 
        }

        public ExprSubstitutionMap(List<Expr> lhs, List<Expr> rhs)
        {
            lhs_ = lhs;
            rhs_ = rhs;
        }

        /**
         * Add an expr mapping. The rhsExpr must be analyzed to support correct substitution
         * across query blocks. It is not required that the lhsExpr is analyzed.
         */
        public void put(Expr lhsExpr, Expr rhsExpr)
        {
            //Preconditions.checkState(rhsExpr.isAnalyzed(), "Rhs expr must be analyzed.");
            lhs_.Add(lhsExpr);
            rhs_.Add(rhsExpr);
        }

        /**
         * Returns the expr mapped to lhsExpr or null if no mapping to lhsExpr exists.
         */
        public Expr get(Expr lhsExpr)
        {
            for (int i = 0; i < lhs_.Count; ++i)
            {
                if (lhsExpr.Equals(lhs_[i])) return rhs_[i];
            }
            return null;
        }

        /**
         * Returns true if the smap contains a mapping for lhsExpr.
         */
        public bool containsMappingFor(Expr lhsExpr)
        {
            return lhs_.Contains(lhsExpr);
        }

        /**
         * Return a map  which is equivalent to applying f followed by g,
         * i.e., g(f()).
         * Always returns a non-null map.
         */
        public static ExprSubstitutionMap compose(ExprSubstitutionMap f, ExprSubstitutionMap g,
            Analyzer analyzer)
        {
            if (f == null && g == null) return new ExprSubstitutionMap();
            if (f == null) return g;
            if (g == null) return f;
            ExprSubstitutionMap result = new ExprSubstitutionMap();
            // f's substitution targets need to be substituted via g
            result.lhs_ = Expr.cloneList(f.lhs_);
            result.rhs_ = Expr.substituteList(f.rhs_, g, analyzer, false);

            // substitution maps are cumulative: the combined map contains all
            // substitutions from f and g.
            for (int i = 0; i < g.lhs_.Count; i++)
            {
                // If f contains expr1->fn(expr2) and g contains expr2->expr3,
                // then result must contain expr1->fn(expr3).
                // The check before adding to result.lhs is to ensure that cases
                // where expr2.Equals(expr1) are handled correctly.
                // For example f: count(*) -> zeroifnull(count(*))
                // and g: count(*) -> slotref
                // result.lhs must only have: count(*) -> zeroifnull(slotref) from f above,
                // and not count(*) -> slotref from g as well.
                if (!result.lhs_.Contains(g.lhs_[i]))
                {
                    result.lhs_.Add(g.lhs_[i].clone());
                    result.rhs_.Add(g.rhs_[i].clone());
                }
            }

            result.verify();
            return result;
        }

        /**
         * Returns the union of two substitution maps. Always returns a non-null map.
         */
        public static ExprSubstitutionMap combine(ExprSubstitutionMap f,
            ExprSubstitutionMap g)
        {
            if (f == null && g == null) return new ExprSubstitutionMap();
            if (f == null) return g;
            if (g == null) return f;
            ExprSubstitutionMap result = new ExprSubstitutionMap();
            result.lhs_ = new List<Expr>(f.lhs_);
            result.lhs_.AddRange(g.lhs_);
            result.rhs_ = new List<Expr>(f.rhs_);
            result.rhs_.AddRange(g.rhs_);
            result.verify();
            return result;
        }

        public void substituteLhs(ExprSubstitutionMap lhsSmap, Analyzer analyzer)
        {
            lhs_ = Expr.substituteList(lhs_, lhsSmap, analyzer, false);
        }

        public List<Expr> getLhs() { return lhs_; }
        public List<Expr> getRhs() { return rhs_; }

        public int size() { return lhs_.Count; }

        public String DebugString()
        {
            //Preconditions.checkState(lhs_.size() == rhs_.size());
            List<String> output = new List<string>();
            for (int i = 0; i < lhs_.Count; ++i)
            {
                output.Add(lhs_[i].toSql() + ":" + rhs_[i].toSql());
                output.Add("(" + lhs_[i].debugString() + ":" + rhs_[i].debugString() + ")");
            }
            return "smap(" + string.Join(" ",output) + ")";
        }

        /**
         * Verifies the internal state of this smap: Checks that the lhs_ has no duplicates,
         * and that all rhs exprs are analyzed.
         */
        private void verify()
        {
            for (int i = 0; i < lhs_.Count; ++i)
            {
                for (int j = i + 1; j < lhs_.Count; ++j)
                {
                    if (lhs_[i].Equals(lhs_[j]))
                    {
                        DebugString();
                        //if (LOG.isTraceEnabled())
                        //{
                        //    LOG.trace("verify: smap=" + this.DebugString());
                        //}
                        //Preconditions.checkState(false);
                    }
                }
                //Preconditions.checkState(rhs_.get(i).isAnalyzed());
            }
        }

        public void clear()
        {
            lhs_.Clear();
            rhs_.Clear();
        }
        
        public ExprSubstitutionMap clone()
        {
            return new ExprSubstitutionMap(Expr.cloneList(lhs_), Expr.cloneList(rhs_));
        }
    }
}
