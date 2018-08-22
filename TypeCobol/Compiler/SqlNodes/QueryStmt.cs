using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;
using TupleId = System.Int32;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Abstract base class for any statement that returns results
     * via a list of result expressions, for example a
     * SelectStmt or UnionStmt. Also maintains a map of expression substitutions
     * for replacing expressions from ORDER BY or GROUP BY clauses with
     * their corresponding result expressions.
     * Used for sharing members/methods and some of the analysis code, in particular the
     * analysis of the ORDER BY and LIMIT clauses.
     *
     */
    public abstract class QueryStmt : StatementBase
    {
        /////////////////////////////////////////
        // BEGIN: Members that need to be reset()

        protected WithClause withClause_;

        protected List<OrderByElement> orderByElements_;
        protected LimitElement limitElement_;

        // For a select statment:
        // original list of exprs in select clause (star-expanded, ordinals and
        // aliases substituted, agg output substituted)
        // For a union statement:
        // list of slotrefs into the tuple materialized by the union.
        protected List<Expr> resultExprs_ = new List<Expr>();

        // For a select statment: select list exprs resolved to base tbl refs
        // For a union statement: same as resultExprs
        protected List<Expr> baseTblResultExprs_ = new List<Expr>();

        /**
         * Map of expression substitutions for replacing aliases
         * in "order by" or "group by" clauses with their corresponding result expr.
         */
        protected readonly ExprSubstitutionMap aliasSmap_;

        /**
         * Select list item alias does not have to be unique.
         * This list contains all the non-unique aliases. For example,
         *   select int_col a, string_col a from alltypessmall;
         * Both columns are using the same alias "a".
         */
        protected readonly List<Expr> ambiguousAliasList_;

        protected SortInfo sortInfo_;

        // evaluateOrderBy_ is true if there is an order by clause that must be evaluated.
        // False for nested query stmts with an order-by clause without offset/limit.
        // sortInfo_ is still generated and used in analysis to ensure that the order-by clause
        // is well-formed.
        protected bool evaluateOrderBy_;

        /////////////////////////////////////////
        // END: Members that need to be reset()

        // Contains the post-analysis toSql() string before rewrites. I.e. table refs are
        // resolved and fully qualified, but no rewrites happened yet. This string is showed
        // to the user in some cases in order to display a statement that is very similar
        // to what was originally issued.
        protected String origSqlString_ = null;

        // If true, we need a runtime check on this statement's result to check if it
        // returns a single row.
        protected bool isRuntimeScalar_ = false;

        QueryStmt(List<OrderByElement> orderByElements, LimitElement limitElement)
        {
            orderByElements_ = orderByElements;
            sortInfo_ = null;
            limitElement_ = limitElement == null ? new LimitElement(null, null) : limitElement;
            aliasSmap_ = new ExprSubstitutionMap();
            ambiguousAliasList_ = new List<Expr>();
        }

        /**
        * Returns all table references in the FROM clause of this statement and all statements
        * nested within FROM clauses.
        */
        public void collectFromClauseTableRefs(List<TableRef> tblRefs)
        {
            collectTableRefs(tblRefs, true);
        }

        public void collectTableRefs(List<TableRef> tblRefs)
        {
            collectTableRefs(tblRefs, false);
        }

        /**
         * Helper for collectFromClauseTableRefs() and collectTableRefs().
         * If 'fromClauseOnly' is true only collects table references in the FROM clause,
         * otherwise all table references.
         */
        protected void collectTableRefs(List<TableRef> tblRefs, bool fromClauseOnly)
        {
            if (!fromClauseOnly && withClause_ != null)
            {
                foreach (View v in withClause_.getViews())
                {
                    v.getQueryStmt().collectTableRefs(tblRefs, fromClauseOnly);
                }
            }
        }

        public void analyze(Analyzer analyzer)
        {
            if (isAnalyzed()) return;
            base.analyze(analyzer);
            analyzeLimit(analyzer);
            if (hasWithClause()) withClause_.analyze(analyzer);
        }

        /**
         * Returns a list containing all the materialized tuple ids that this stmt is
         * correlated with (i.e., those tuple ids from outer query blocks that TableRefs
         * inside this stmt are rooted at).
         *
         * Throws if this stmt contains an illegal mix of un/correlated table refs.
         * A statement is illegal if it contains a TableRef correlated with a parent query
         * block as well as a table ref with an absolute path (e.g. a BaseTabeRef). Such a
         * statement would generate a Subplan containing a base table scan (very expensive),
         * and should therefore be avoided.
         *
         * In other words, the following cases are legal:
         * (1) only uncorrelated table refs
         * (2) only correlated table refs
         * (3) a mix of correlated table refs and table refs rooted at those refs
         *     (the statement is 'self-contained' with respect to correlation)
         */
        public List<TupleId> getCorrelatedTupleIds(Analyzer analyzer)
        {
            // Correlated tuple ids of this stmt.
            List<TupleId> correlatedTupleIds = new List<int>();
            // First correlated and absolute table refs. Used for error detection/reporting.
            // We pick the first ones for simplicity. Choosing arbitrary ones is equally valid.
            TableRef correlatedRef = null;
            TableRef absoluteRef = null;
            // Materialized tuple ids of the table refs checked so far.
            HashSet<TupleId> tblRefIds = new HashSet<int>();

            List<TableRef> tblRefs = new List<TableRef>();
            collectTableRefs(tblRefs, true);
            foreach (TableRef tblRef in tblRefs)
            {
                if (absoluteRef == null && !tblRef.isRelative()) absoluteRef = tblRef;
                if (tblRef.isCorrelated())
                {
                    // Check if the correlated table ref is rooted at a tuple descriptor from within
                    // this query stmt. If so, the correlation is contained within this stmt
                    // and the table ref does not conflict with absolute refs.
                    CollectionTableRef t = (CollectionTableRef) tblRef;
                    Preconditions.checkState(t.getResolvedPath().isRootedAtTuple());
                    // This check relies on tblRefs being in depth-first order.
                    if (!tblRefIds.contains(t.getResolvedPath().getRootDesc().getId()))
                    {
                        if (correlatedRef == null) correlatedRef = tblRef;
                        correlatedTupleIds.Add(t.getResolvedPath().getRootDesc().getId());
                    }
                }

                if (correlatedRef != null && absoluteRef != null)
                {
                    throw new AnalysisException(String.Format(
                        "Nested query is illegal because it contains a table reference '{0}' " +
                        "correlated with an outer block as well as an uncorrelated one '{1}':\n{2}",
                        correlatedRef.tableRefToSql(), absoluteRef.tableRefToSql(), toSql()));
                }

                tblRefIds.Add(tblRef.getId());
            }

            return correlatedTupleIds;
        }

        private void analyzeLimit(Analyzer analyzer)
        {
            if (limitElement_.getOffsetExpr() != null && !hasOrderByClause())
            {
                throw new AnalysisException("OFFSET requires an ORDER BY clause: " +
                                            limitElement_.toSql().trim());
            }

            limitElement_.analyze(analyzer);
        }

        /**
         * Creates sortInfo by resolving aliases and ordinals in the orderingExprs.
         * If the query stmt is an inline view/union operand, then order-by with no
         * limit with offset is not allowed, since that requires a sort and merging-exchange,
         * and subsequent query execution would occur on a single machine.
         * Sets evaluateOrderBy_ to false for ignored order-by w/o limit/offset in nested
         * queries.
         */
        protected void createSortInfo(Analyzer analyzer)
        {
            // not computing order by
            if (orderByElements_ == null)
            {
                evaluateOrderBy_ = false;
                return;
            }

            List<Expr> orderingExprs = new List<Expr>();
            List<Boolean> isAscOrder = new List<bool>();
            List<Boolean> nullsFirstParams = new List<bool>();

            // extract exprs
            foreach (OrderByElement orderByElement in orderByElements_)
            {
                //TODO - adapt predicate call
                //if (orderByElement.getExpr().contains(Predicates.instanceOf(Subquery.class))) {
                //    throw new AnalysisException(
                //        "Subqueries are not supported in the ORDER BY clause.");
                //}
                // create copies, we don't want to modify the original parse node, in case
                // we need to print it
                orderingExprs.Add(orderByElement.getExpr().clone());
                isAscOrder.Add(Boolean.valueOf(orderByElement.isAsc()));
                nullsFirstParams.Add(orderByElement.getNullsFirstParam());
            }

            substituteOrdinalsAndAliases(orderingExprs, "ORDER BY", analyzer);

            if (!analyzer.isRootAnalyzer() && hasOffset() && !hasLimit())
            {
                throw new AnalysisException("Order-by with offset without limit not supported" +
                                            " in nested queries.");
            }

            sortInfo_ = new SortInfo(orderingExprs, isAscOrder, nullsFirstParams);
            // order by w/o limit and offset in inline views, union operands and insert statements
            // are ignored.
            if (!hasLimit() && !hasOffset() && !analyzer.isRootAnalyzer())
            {
                evaluateOrderBy_ = false;
                // Return a warning that the order by was ignored.
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.Append("Ignoring ORDER BY clause without LIMIT or OFFSET: ");
                strBuilder.Append("ORDER BY ");
                strBuilder.Append(orderByElements_[0].toSql());
                for (int i = 1; i < orderByElements_.Count; ++i)
                {
                    strBuilder.Append(", ").Append(orderByElements_[i].toSql());
                }

                strBuilder.Append(".\nAn ORDER BY appearing in a view, subquery, union operand, ");
                strBuilder.Append("or an insert/ctas statement has no effect on the query result ");
                strBuilder.Append("unless a LIMIT and/or OFFSET is used in conjunction ");
                strBuilder.Append("with the ORDER BY.");
                analyzer.addWarning(strBuilder.ToString());
            }
            else
            {
                evaluateOrderBy_ = true;
            }
        }

        /**
         * Create a tuple descriptor for the single tuple that is materialized, sorted and
         * output by the exec node implementing the sort. Done by materializing slot refs in
         * the order-by and result expressions. Those SlotRefs in the ordering and result exprs
         * are substituted with SlotRefs into the new tuple. This simplifies sorting logic for
         * total (no limit) sorts.
         * Done after analyzeAggregation() since ordering and result exprs may refer to the
         * outputs of aggregation.
         */
        protected void createSortTupleInfo(Analyzer analyzer)
        {
            //Preconditions.checkState(evaluateOrderBy_);
            foreach (Expr orderingExpr in sortInfo_.getSortExprs())
            {
                if (orderingExpr.getType().isComplexType())
                {
                    throw new AnalysisException(String.Format("ORDER BY expression '{0}' with " +
                                                              "complex type '{1}' is not supported.",
                        orderingExpr.toSql(),
                        orderingExpr.getType().toSql()));
                }
            }

            sortInfo_.createSortTupleInfo(resultExprs_, analyzer);

            ExprSubstitutionMap smap = sortInfo_.getOutputSmap();
            for (int i = 0; i < smap.size(); ++i)
            {
                if (!(smap.getLhs()[i] is SlotRef)
                    || !(smap.getRhs()[i] is SlotRef))
                {
                    continue;
                }

                SlotRef inputSlotRef = (SlotRef) smap.getLhs()[i];
                SlotRef outputSlotRef = (SlotRef) smap.getRhs()[i];
                if (hasLimit())
                {
                    analyzer.registerValueTransfer(
                        inputSlotRef.getSlotId(), outputSlotRef.getSlotId());
                }
                else
                {
                    analyzer.createAuxEqPredicate(outputSlotRef, inputSlotRef);
                }
            }

            substituteResultExprs(smap, analyzer);
        }

/**
 * Substitutes an ordinal or an alias. An ordinal is an integer NumericLiteral
 * that refers to a select-list expression by ordinal. An alias is a SlotRef
 * that matches the alias of a select-list expression (tracked by 'aliasMap_').
 * We should substitute by ordinal or alias but not both to avoid an incorrect
 * double substitution.
 * Returns clone() of 'expr' if it is not an ordinal, nor an alias.
 * The returned expr is analyzed regardless of whether substitution was performed.
 */
        protected Expr substituteOrdinalOrAlias(Expr expr, String errorPrefix, Analyzer analyzer)
        {
            Expr substituteExpr = trySubstituteOrdinal(expr, errorPrefix, analyzer);
            if (substituteExpr != null) return substituteExpr;
            if (ambiguousAliasList_.Contains(expr))
            {
                throw new AnalysisException("Column '" + expr.toSql() +
                                            "' in " + errorPrefix + " clause is ambiguous");
            }

            if (expr is SlotRef)
            {
                substituteExpr = expr.trySubstitute(aliasSmap_, analyzer, false);
            }
            else
            {
                expr.analyze(analyzer);
                substituteExpr = expr;
            }

            return substituteExpr;
        }

/**
 * Substitutes top-level ordinals and aliases. Does not substitute ordinals and
 * aliases in subexpressions.
 * Modifies the 'exprs' list in-place.
 * The 'exprs' are all analyzed after this function regardless of whether
 * substitution was performed.
 */
        protected void substituteOrdinalsAndAliases(List<Expr> exprs, String errorPrefix,
            Analyzer analyzer)
        {
            for (int i = 0; i < exprs.Count; ++i)
            {
                exprs[i] = substituteOrdinalOrAlias(exprs[i], errorPrefix, analyzer);
            }
        }

// Attempt to replace an expression of form "<number>" with the corresponding
// select list items.  Return null if not an ordinal expression.
        private Expr trySubstituteOrdinal(Expr expr, String errorPrefix,
            Analyzer analyzer)
        {
            if (!(expr is NumericLiteral)) return null;
            expr.analyze(analyzer);
            if (!expr.getType().isIntegerType()) return null;
            long pos = ((NumericLiteral) expr).getLongValue();
            if (pos < 1)
            {
                throw new AnalysisException(
                    errorPrefix + ": ordinal must be >= 1: " + expr.toSql());
            }

            if (pos > resultExprs_.Count)
            {
                throw new AnalysisException(
                    errorPrefix + ": ordinal exceeds number of items in select list: "
                                + expr.toSql());
            }

            // Create copy to protect against accidentally shared state.
            return resultExprs_[(int) pos - 1].clone();
        }

        /**
         * Returns the materialized tuple ids of the output of this stmt.
         * Used in case this stmt is part of an @InlineViewRef,
         * since we need to know the materialized tupls ids of a TableRef.
         * This call must be idempotent because it may be called more than once for Union stmt.
         * TODO: The name of this function has become outdated due to analytics
         * producing logical (non-materialized) tuples. Re-think and clean up.
         */
        public abstract void getMaterializedTupleIds(List<TupleId> tupleIdList);

        public List<Expr> getResultExprs()
        {
            return resultExprs_;
        }

        public void setWithClause(WithClause withClause)
        {
            this.withClause_ = withClause;
        }

        public bool hasWithClause()
        {
            return withClause_ != null;
        }

        public WithClause getWithClause()
        {
            return withClause_;
        }

        public bool hasOrderByClause()
        {
            return orderByElements_ != null;
        }

        public bool hasLimit()
        {
            return limitElement_.getLimitExpr() != null;
        }

        public String getOrigSqlString()
        {
            return origSqlString_;
        }

        public bool isRuntimeScalar()
        {
            return isRuntimeScalar_;
        }

        public void setIsRuntimeScalar(bool isRuntimeScalar)
        {
            isRuntimeScalar_ = isRuntimeScalar;
        }

        public long getLimit()
        {
            return limitElement_.getLimit();
        }

        public bool hasOffset()
        {
            return limitElement_.getOffsetExpr() != null;
        }

        public long getOffset()
        {
            return limitElement_.getOffset();
        }

        public SortInfo getSortInfo()
        {
            return sortInfo_;
        }

        public bool evaluateOrderBy()
        {
            return evaluateOrderBy_;
        }

        public List<Expr> getBaseTblResultExprs()
        {
            return baseTblResultExprs_;
        }

        public void setLimit(long limit)
        {
            //Preconditions.checkState(limit >= 0);
            long newLimit = hasLimit() ? Math.Min(limit, getLimit()) : limit;
            limitElement_ = new LimitElement(new NumericLiteral(newLimit,
                SqlNodeType.BIGINT), limitElement_.getOffsetExpr());
        }

        /**
         * Mark all slots that need to be materialized for the execution of this stmt.
         * This excludes slots referenced in resultExprs (it depends on the consumer of
         * the output of the stmt whether they'll be accessed) and single-table predicates
         * (the PlanNode that materializes that tuple can decide whether evaluating those
         * predicates requires slot materialization).
         * This is called prior to plan tree generation and allows tuple-materializing
         * PlanNodes to compute their tuple's mem layout.
         */
        public abstract void materializeRequiredSlots(Analyzer analyzer);

/**
 * Mark slots referenced in exprs as materialized.
 */
        protected void materializeSlots(Analyzer analyzer, List<Expr> exprs)
        {
            List<SlotId> slotIds = new List<SlotId>();
            foreach (Expr e in exprs)
            {
                e.getIds(null, slotIds);
            }

            analyzer.getDescTbl().markSlotsMaterialized(slotIds);
        }

/**
 * Substitutes the result expressions with smap. Preserves the original types of
 * those expressions during the substitution.
 */
        public void substituteResultExprs(ExprSubstitutionMap smap, Analyzer analyzer)
        {
            resultExprs_ = Expr.substituteList(resultExprs_, smap, analyzer, true);
        }

        public DataSink createDataSink()
        {
            return new PlanRootSink();
        }

        public List<OrderByElement> cloneOrderByElements()
        {
            if (orderByElements_ == null) return null;
            List<OrderByElement> result =
                new List<OrderByElement>(orderByElements_.Count);
            foreach (OrderByElement o in orderByElements_)
            {
                result.Add(o.clone());
            }
            return result;
        }

        public WithClause cloneWithClause()
        {
            return withClause_ != null ? withClause_.clone() : null;
        }

/**
 * C'tor for cloning.
 */
        protected QueryStmt(QueryStmt other) : base(other)
        {
            withClause_ = other.cloneWithClause();
            orderByElements_ = other.cloneOrderByElements();
            limitElement_ = other.limitElement_.clone();
            resultExprs_ = Expr.cloneList(other.resultExprs_);
            baseTblResultExprs_ = Expr.cloneList(other.baseTblResultExprs_);
            aliasSmap_ = other.aliasSmap_.clone();
            ambiguousAliasList_ = Expr.cloneList(other.ambiguousAliasList_);
            sortInfo_ = (other.sortInfo_ != null) ? other.sortInfo_.clone() : null;
            analyzer_ = other.analyzer_;
            evaluateOrderBy_ = other.evaluateOrderBy_;
            origSqlString_ = other.origSqlString_;
            isRuntimeScalar_ = other.isRuntimeScalar_;
        }

        public override void reset()
        {
            if (orderByElements_ != null)
            {
                foreach (OrderByElement o in 
                orderByElements_) o.getExpr().reset();
            }

            limitElement_.reset();
            resultExprs_.Clear();
            baseTblResultExprs_.Clear();
            aliasSmap_.clear();
            ambiguousAliasList_.Clear();
            sortInfo_ = null;
            evaluateOrderBy_ = false;
        }

        public abstract QueryStmt clone();


    }
}
