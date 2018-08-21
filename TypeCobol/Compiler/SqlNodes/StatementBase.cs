using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public abstract class StatementBase : IParseNode
    {
        // True if this Stmt is the top level of an explain stmt.
        protected internal bool IsExplain = false;


        // Analyzer that was used to analyze this statement.
        protected internal Analyzer Analyzer;

        protected StatementBase()
        {

        }

        protected StatementBase(StatementBase cloneStatement)
        {
            IsExplain = cloneStatement.IsExplain;
            Analyzer = cloneStatement.Analyzer;
        }

        /**
         * Returns all table references in this statement and all its nested statements.
         * The TableRefs are collected depth-first in SQL-clause order.
         * Subclasses should override this method as necessary.
         */
        public void CollectTableRefs(List<TableRef> tblRefs)
        {
        }

        /**
       * Analyzes the statement and throws an AnalysisException if analysis fails. A failure
       * could be due to a problem with the statement or because one or more tables/views
       * were missing from the catalog.
       * It is up to the analysis() implementation to ensure the maximum number of missing
       * tables/views get collected in the Analyzer before failing analyze().
       */
        public void analyze(Analyzer analyzer)
        {
            if (IsAnalyzed()) return;
            if (IsExplain) analyzer.setIsExplain();
            this.Analyzer = analyzer;
        }

       
        /**
         * Returns the output column labels of this statement, if applicable, or an empty list
         * if not applicable (not all statements produce an output result set).
         * Subclasses must override this as necessary.
         */
        public List<string> GetColLabels()
        {
            return new List<string>();
        }

        /**
         * Returns the unresolved result expressions of this statement, if applicable, or an
         * empty list if not applicable (not all statements produce an output result set).
         * Subclasses must override this as necessary.
         */
        public List<Expr> getResultExprs()
        {
            return new List<Expr>();
        }

        public Analyzer GetAnalyzer => Analyzer;

        public bool IsAnalyzed()
        {
            return null == Analyzer;
        }

        /**
   * Casts the result expressions and derived members (e.g., destination column types for
   * CTAS) to the given types. No-op if this statement does not have result expressions.
   * Throws when casting fails. Subclasses may override this as necessary.
   */
        public void castResultExprs(List<SqlNodeType> types)
        {
            List<Expr> resultExprs = getResultExprs();
            if (resultExprs == null || resultExprs.Count != types.Count)
            {
                return;
            }

            for (int i = 0; i < types.Count; ++i)
            {
                //if (!resultExprs.get(i).getType().equals(types.get(i)))
                //{
                //    resultExprs.set(i, resultExprs.get(i).castTo(types.get(i)));
                //}
                if (resultExprs[i].GetType() != types[i])
                {
                    resultExprs[i] = (Expr)Convert.ChangeType(resultExprs[i], types[i]);
                }
            }
        }

        /**
         * Uses the given 'rewriter' to transform all Exprs in this statement according
         * to the rules specified in the 'rewriter'. Replaces the original Exprs with the
         * transformed ones in-place. Subclasses that have Exprs to be rewritten must
         * override this method. Valid to call after analyze().
         */
        //public void rewriteExprs(ExprRewriter rewriter)
        //{
        //    throw new InvalidOperationException(
        //        "rewriteExprs() not implemented for this stmt: " + this.GetType().Name);
        //}

        public string toSql()
        {
            return ToSql(false);
        }

/**
 * If rewritten is true, returns the rewritten SQL only if the statement was
 * rewritten. Otherwise, the original SQL will be returned instead. It is the caller's
 * responsibility to know if/when the statement was indeed rewritten.
 */
        public string ToSql(bool rewritten)
        {
            return "";
        }

        public void SetIsExplain()
        {
            IsExplain = true;
        }

        public bool isExplain()
        {
            return IsExplain;
        }

/**
 * Returns a deep copy of this node including its analysis state. Some members such as
 * tuple and slot descriptors are generally not deep copied to avoid potential
 * confusion of having multiple descriptor instances with the same id, although
 * they should be unique in the descriptor table.
 * TODO for 2.3: Consider also cloning table and slot descriptors for clarity,
 * or otherwise make changes to more provide clearly defined clone() semantics.
 */
        public StatementBase clone()
        {
            throw new NotImplementedException(
                "Clone() not implemented for " + GetType().Name);
        }

/**
 * Resets the internal analysis state of this node.
 * For easier maintenance, class members that need to be reset are grouped into
 * a 'section' clearly indicated by comments as follows:
 *
 * class SomeStmt extends StatementBase {
 *   ...
 *   /////////////////////////////////////////
 *   // BEGIN: Members that need to be reset()
 *
 *   <member declarations>
 *
 *   // END: Members that need to be reset()
 *   /////////////////////////////////////////
 *   ...
 * }
 *
 * In general, members that are set or modified during analyze() must be reset().
 * TODO: Introduce this same convention for Exprs, possibly by moving clone()/reset()
 * into the ParseNode interface for clarity.
 */
        public abstract void reset();

/**
 * Checks that 'srcExpr' is type compatible with 'dstCol' and returns a type compatible
 * expression by applying a CAST() if needed. Throws an AnalysisException if the types
 * are incompatible. 'dstTableName' is only used when constructing an AnalysisException
 * message.
 *
 * If strictDecimal is true, only consider casts that result in no loss of information
 * when casting between decimal types.
 */
        //TODO uncomment and find Column definition
        //protected Expr checkTypeCompatibility(string dstTableName, Column dstCol,
        //    Expr srcExpr, bool strictDecimal)
        //{
        //    Type dstColType = dstCol.GetType();
        //    Type srcExprType = srcExpr.GetType();
        //    Type compatType;
        //    // Trivially compatible, unless the type is complex.
        //    if (dstColType == srcExprType) return srcExpr;

        //    compatType = Catalog.TypeUtility.getAssignmentCompatibleType(dstColType, srcExprType, false, strictDecimal);

        //    if (compatType==null)
        //    {
        //        throw new AnalysisException(string.Format(
        //            "Target table '{0}' is incompatible with source expressions.\nExpression '{1}' " +
        //            "(type: {2}) is not compatible with column '{3}' (type: {4})",
        //            dstTableName, srcExpr.toSql(), srcExprType.toSql(), dstCol.getName(),
        //            dstColType.ToSql()));
        //    }

        //    if (compatType != dstColType && compatType != null)
        //    {
        //        throw new AnalysisException(string.Format(
        //            "Possible loss of precision for target table '{0}'.\nExpression '{1}' (type: " +
        //            "{2}) would need to be cast to %s for column '{3}'",
        //            dstTableName, srcExpr.toSql(), srcExprType.ToSql(), dstColType.ToSql(),
        //            dstCol.getName()));
        //    }

        //    return Convert.ChangeType(srcExpr,compatType);
        //}
    }

}
