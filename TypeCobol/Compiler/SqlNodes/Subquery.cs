using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Class representing a subquery. A Subquery consists of a QueryStmt and has
     * its own Analyzer context.
     */
    class Subquery : Expr
    {

        // The QueryStmt of the subquery.
        protected QueryStmt stmt_;

        // A subquery has its own analysis context
        protected Analyzer analyzer_;

        public Analyzer getAnalyzer()
        {
            return analyzer_;
        }

        public QueryStmt getStatement()
        {
            return stmt_;
        }

        /**
 * C'tor that initializes a Subquery from a QueryStmt.
 */
        public Subquery(QueryStmt queryStmt) : base()
        {
            //Preconditions.checkNotNull(queryStmt);
            stmt_ = queryStmt;
        }

        /**
         * Copy c'tor.
         */
        public Subquery(Subquery other) : base(other)
        {
            stmt_ = other.stmt_.clone();
            analyzer_ = other.analyzer_;
        }

        public override string ToSqlImpl()
        {
            return "(" + stmt_.toSql() + ")";
        }

        public override string DebugString()
        {
            return debugString();
        }

        /**
         * Returns true if the toSql() of the Subqueries is identical. May return false for
         * equivalent statements even due to minor syntactic differences like parenthesis.
         * TODO: Switch to a less restrictive implementation.
         */
        public override bool LocalEquals(Expr that)
        {
            return base.localEquals(that) &&
                   stmt_.toSql().Equals(((Subquery) that).stmt_.toSql());
        }

        public override void ResetAnalysisState()
        {
            resetAnalysisState();
        }

        public override Expr clone()
        {
            return new Subquery(this);
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
            getIdsHelper(tupleIds, slotIds);
        }

        protected override bool IsConstantImpl()
        {
            return false;
        }

        public override Expr IgnoreImplicitCast()
        {
            return ignoreImplicitCast();
        }

        /**
  * Analyzes the subquery in a child analyzer.
  */
        protected void analyzeImpl(Analyzer analyzer)
        {
            if (!(stmt_ is SelectStmt))
            {
                throw new AnalysisException("A subquery must contain a single select block: " +
                                            toSql());
            }

            // The subquery is analyzed with its own analyzer.
            analyzer_ = new Analyzer(analyzer);
            analyzer_.setIsSubquery();
            stmt_.analyze(analyzer_);
            // Check whether the stmt_ contains an illegal mix of un/correlated table refs.
            stmt_.getCorrelatedTupleIds(analyzer_);

            // Set the subquery type based on the types of the exprs in the
            // result list of the associated SelectStmt.
            List<Expr> stmtResultExprs = stmt_.getResultExprs();
            if (stmtResultExprs.Count == 1)
            {
                type_ = stmtResultExprs[0].getType();
                //Preconditions.checkState(!type_.isComplexType());
            }
            else
            {
                type_ = createStructTypeFromExprList();
            }

            // If the subquery can return many rows, do the cardinality check at runtime.
            if (!((SelectStmt) stmt_).returnsSingleRow()) stmt_.setIsRuntimeScalar(true);

            //Preconditions.checkNotNull(type_);
        }


        protected float computeEvalCost()
        {
            return UNKNOWN_COST;
        }

        /**
   * Check if the subquery's SelectStmt returns a single column of scalar type.
   */
        public bool returnsScalarColumn()
        {
            List<Expr> stmtResultExprs = stmt_.getResultExprs();
            if (stmtResultExprs.Count == 1 && stmtResultExprs[0].getType().isScalarType())
            {
                return true;
            }

            return false;
        }

        /**
         * Create a StrucType from the result expr list of a subquery's SelectStmt.
         */
        private StructType createStructTypeFromExprList()
        {
            List<Expr> stmtResultExprs = stmt_.getResultExprs();
            List<StructField> structFields = new List<StructField>();
            // Check if we have unique labels
            List<String> labels = stmt_.getColLabels();
            bool hasUniqueLabels = new HashSet<string>(labels).Count == labels.Count;

            // Construct a StructField from each expr in the select list
            for (int i = 0; i < stmtResultExprs.Count; ++i)
            {
                Expr expr = stmtResultExprs[i];
                String fieldName = null;
                // Check if the label meets the Metastore's requirements.
                if (MetastoreShim.validateName(labels[i]))
                {
                    fieldName = labels[i];
                    // Make sure the field names are unique.
                    if (!hasUniqueLabels)
                    {
                        fieldName = "_" + i + "_" + fieldName;
                    }
                }
                else
                {
                    // Use the expr ordinal to construct a StructField.
                    fieldName = "_" + i;
                }

                //Preconditions.checkNotNull(fieldName);
                structFields.Add(new StructField(fieldName, expr.getType(), null));
            }

            //Preconditions.checkState(structFields.Count != 0);
            return new StructType(structFields);
        }



    }
}
