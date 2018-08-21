using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
 * Return value of the grammar production that parses function
 * parameters. These parameters can be for scalar or aggregate functions.
 */
    public class FunctionParams
    {
        private readonly bool isStar_;
        private bool isDistinct_;
        private bool isIgnoreNulls_;
        private readonly List<Expr> exprs_;

        // c'tor for non-star params
        public FunctionParams(bool isDistinct, bool isIgnoreNulls, List<Expr> exprs)
        {
            this.isStar_ = false;
            this.isDistinct_ = isDistinct;
            this.isIgnoreNulls_ = isIgnoreNulls;
            this.exprs_ = exprs;
        }

        // c'tor for non-star, non-ignore-nulls params
        public FunctionParams(bool isDistinct, List<Expr> exprs) : this(isDistinct, false, exprs)
        {
        }

        // c'tor for non-star, non-distinct, non-ignore-nulls params
        public FunctionParams(List<Expr> exprs) : this(false, false, exprs)
        { 
        }

        public static FunctionParams createStarParam()
        {
            return new FunctionParams();
        }

        public bool isStar() { return isStar_; }
        public bool isDistinct() { return isDistinct_; }
        public void setIsDistinct(bool v) { isDistinct_ = v; }
        public bool isIgnoreNulls() { return isIgnoreNulls_; }
        public void setIsIgnoreNulls(bool b) { isIgnoreNulls_ = b; }
        public List<Expr> exprs() { return exprs_; }
        public int size() { return exprs_?.Count ?? 0; }

        // c'tor for <agg>(*)
        private FunctionParams()
        {
            exprs_ = null;
            isStar_ = true;
            isDistinct_ = false;
            isIgnoreNulls_ = false;
        }
    }
}
