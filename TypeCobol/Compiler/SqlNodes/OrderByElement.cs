using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Combination of expr, ASC/DESC, and nulls ordering.
     */
    public class OrderByElement : IParseNode
    {
        private Expr expr_;

        private readonly bool isAsc_;

        // Represents the NULLs ordering specified: true when "NULLS FIRST", false when
        // "NULLS LAST", and null if not specified.
        private readonly bool? nullsFirstParam_;

        /**
         * Constructs the OrderByElement.
         *
         * 'nullsFirstParam' should be true if "NULLS FIRST", false if "NULLS LAST", or null if
         * the NULLs order was not specified.
         */
        public OrderByElement(Expr expr, bool isAsc, bool? nullsFirstParam) : base()
        {
            expr_ = expr;
            isAsc_ = isAsc;
            nullsFirstParam_ = nullsFirstParam;
        }

        /**
         * C'tor for cloning.
         */
        private OrderByElement(OrderByElement other)
        {
            expr_ = other.expr_.clone();
            isAsc_ = other.isAsc_;
            if (other.nullsFirstParam_ != null)
            {
                nullsFirstParam_ = other.nullsFirstParam_.Value;
            }
            else
            {
                nullsFirstParam_ = null;
            }
        }

        public Expr getExpr()
        {
            return expr_;
        }

        public void setExpr(Expr e)
        {
            expr_ = e;
        }

        public bool isAsc()
        {
            return isAsc_;
        }

        public bool? getNullsFirstParam()
        {
            return nullsFirstParam_;
        }

        public bool? nullsFirst()
        {
            return nullsFirst(nullsFirstParam_, isAsc_);
        }

        public void analyze(Analyzer analyzer)
        {
            throw new NotImplementedException();
        }

        public String toSql()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(expr_.toSql());
            strBuilder.Append(isAsc_ ? " ASC" : " DESC");
            // When ASC and NULLS LAST or DESC and NULLS FIRST, we do not print NULLS FIRST/LAST
            // because it is the default behavior and we want to avoid printing NULLS FIRST/LAST
            // whenever possible as it is incompatible with Hive (SQL compatibility with Hive is
            // important for views).
            if (nullsFirstParam_ != null)
            {
                if (isAsc_ && nullsFirstParam_.Value)
                {
                    // If ascending, nulls are last by default, so only Add if nulls first.
                    strBuilder.Append(" NULLS FIRST");
                }
                else if (!isAsc_ && !nullsFirstParam_.Value)
                {
                    // If descending, nulls are first by default, so only Add if nulls last.
                    strBuilder.Append(" NULLS LAST");
                }
            }

            return strBuilder.ToString();
        }

        public override bool Equals(Object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            OrderByElement o = (OrderByElement) obj;
            bool nullsFirstEqual =
                (nullsFirstParam_ == null) == (o.nullsFirstParam_ == null);
            if (nullsFirstParam_ != null && nullsFirstEqual)
            {
                nullsFirstEqual = nullsFirstParam_.Equals(o.nullsFirstParam_);
            }

            return expr_.Equals(o.expr_) && isAsc_ == o.isAsc_ && nullsFirstEqual;
        }

        public OrderByElement clone()
        {
            return new OrderByElement(this);
        }

        /**
         * Compute nullsFirst.
         *
         * @param nullsFirstParam True if "NULLS FIRST", false if "NULLS LAST", or null if
         *                        the NULLs order was not specified.
         * @param isAsc
         * @return Returns true if nulls are ordered first or false if nulls are ordered last.
         *         Independent of isAsc.
         */
        public static bool? nullsFirst(bool? nullsFirstParam, bool isAsc)
        {
            return nullsFirstParam == null ? !isAsc : nullsFirstParam;
        }

        /**
         * Returns a new list of order-by elements with the order by exprs of src substituted
         * according to smap. Preserves the other sort params from src.
         */
        public static List<OrderByElement> substitute(List<OrderByElement> src,
            ExprSubstitutionMap smap, Analyzer analyzer)
        {
            List<OrderByElement> result = new List<OrderByElement>(src.Count);
            foreach (OrderByElement element in src)
            {
                result.Add(new OrderByElement(element.getExpr().substitute(smap, analyzer, false),
                    element.isAsc_, element.nullsFirstParam_));
            }

            return result;
        }

        /**
         * Extracts the order-by exprs from the list of order-by elements and returns them.
         */
        public static List<Expr> getOrderByExprs(List<OrderByElement> src)
        {
            List<Expr> result = new List<Expr>(src.Count);
            foreach (OrderByElement element in src)
            {
                result.Add(element.getExpr());
            }

            return result;
        }

        /**
         * Returns a new list of OrderByElements with the same (cloned) expressions but the
         * ordering direction reversed (asc becomes desc, nulls first becomes nulls last, etc.)
         */
        public static List<OrderByElement> reverse(List<OrderByElement> src)
        {
            List<OrderByElement> result = new List<OrderByElement>(src.Count);
            for (int i = 0; i < src.Count; ++i)
            {
                OrderByElement element = src[i];
                OrderByElement reverseElement =
                    new OrderByElement(element.getExpr().clone(), !element.isAsc_,
                        !nullsFirst(element.nullsFirstParam_, element.isAsc_));
                result.Add(reverseElement);
            }

            return result;
        }
    }
}
