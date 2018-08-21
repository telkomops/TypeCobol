using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using TypeCobol.Compiler.SqlNodes.Common;
using TupleId = System.Int32;

namespace TypeCobol.Compiler.SqlNodes
{
    public class Analyzer
    {

        private readonly GlobalState globalState;

        public void setIsExplain()
        {
            globalState.isExplain = true;
        }
    }

    internal class GlobalState
    {
        public readonly TQueryCtx queryCtx;
        public readonly AuthorizationConfig authzConfig;
        public readonly DescriptorTable descTbl = new DescriptorTable();
        public readonly IdGenerator<ExprId> conjunctIdGenerator = ExprId.createGenerator();
        public readonly ColumnLineageGraph lineageGraph;

        // True if we are analyzing an explain request. Should be set before starting
        // analysis.
        public bool isExplain;

        // Indicates whether the query has plan hints.
        public bool hasPlanHints = false;

        // True if at least one of the analyzers belongs to a subquery.
        public bool containsSubquery = false;

        // all registered conjuncts (Dictionary from expr id to conjunct). We use a LinkedHashDictionary to
        // preserve the order in which conjuncts are added.
        public readonly LinkedHashDictionary<ExprId, Expr> conjuncts = Dictionarys.newLinkedHashDictionary();

        // all registered conjuncts bound by a single tuple id; used in getBoundPredicates()
        public readonly List<ExprId> singleTidConjuncts = new List<ExprId>();

        // eqJoinConjuncts[tid] contains all conjuncts of the form
        // "<lhs> = <rhs>" in which either lhs or rhs is fully bound by tid
        // and the other side is not bound by tid (ie, predicates that express equi-join
        // conditions between two tablerefs).
        // A predicate such as "t1.a = t2.b" has two entries, one for 't1' and
        // another one for 't2'.
        public readonly Dictionary<TupleId, List<ExprId>> eqJoinConjuncts = new Dictionary<TupleId, List<ExprId>>();

        // set of conjuncts that have been assigned to some PlanNode
        public Set<ExprId> assignedConjuncts =
            Collections.newSetFromDictionary(new IdentityHashDictionary<ExprId, bool>());

        // Dictionary from outer-joined tuple id, i.e., one that is nullable,
        // to the last Join clause (represented by its rhs table ref) that outer-joined it
        public readonly Dictionary<TupleId, TableRef> outerJoinedTupleIds = new Dictionary<TupleId, TableRef>();

        // Dictionary of registered conjunct to the last full outer join (represented by its
        // rhs table ref) that outer joined it.
        public readonly Dictionary<ExprId, TableRef> fullOuterJoinedConjuncts = new Dictionary<ExprId, TableRef>();

        // Dictionary of full-outer-joined tuple id to the last full outer join that outer-joined it
        public readonly Dictionary<TupleId, TableRef> fullOuterJoinedTupleIds = new Dictionary<TupleId, TableRef>();

        // Dictionary from semi-joined tuple id, i.e., one that is invisible outside the join's
        // On-clause, to its Join clause (represented by its rhs table ref). An anti-join is
        // a kind of semi-join, so anti-joined tuples are also registered here.
        public readonly Dictionary<TupleId, TableRef> semiJoinedTupleIds = new Dictionary<TupleId, TableRef>();

        // Dictionary from right-hand side table-ref id of an outer join to the list of
        // conjuncts in its On clause. There is always an entry for an outer join, but the
        // corresponding value could be an empty list. There is no entry for non-outer joins.
        public readonly Dictionary<TupleId, List<ExprId>> conjunctsByOjClause = new Dictionary<TupleId, List<ExprId>>();

        // Dictionary from registered conjunct to its containing outer join On clause (represented
        // by its right-hand side table ref); this is limited to conjuncts that can only be
        // correctly evaluated by the originating outer join, including constant conjuncts
        public readonly Dictionary<ExprId, TableRef> ojClauseByConjunct = new Dictionary<ExprId, TableRef>();

        // Dictionary from registered conjunct to its containing semi join On clause (represented
        // by its right-hand side table ref)
        public readonly Dictionary<ExprId, TableRef> sjClauseByConjunct = new Dictionary<ExprId, TableRef>();

        // Dictionary from registered conjunct to its containing inner join On clause (represented
        // by its right-hand side table ref)
        public readonly Dictionary<ExprId, TableRef> ijClauseByConjunct = new Dictionary<ExprId, TableRef>();

        // Dictionary from slot id to the analyzer/block in which it was registered
        public readonly Dictionary<SlotId, Analyzer> blockBySlot = new Dictionary<SlotId, Analyzer>();

        // Tracks all privilege requests on catalog objects.
        private readonly HashSet<PrivilegeRequest> privilegeReqs = new HashSet<PrivilegeRequest>();

        // List of PrivilegeRequest to custom authorization failure error message.
        // Tracks all privilege requests on catalog objects that need a custom
        // error message returned to avoid exposing existence of catalog objects.
        private readonly List<Tuple<PrivilegeRequest, String>> maskedPrivilegeReqs =
            new EditableList<Tuple<PrivilegeRequest, string>>();

        // accesses to catalog objects
        // TODO: This can be inferred from privilegeReqs. They should be coalesced.
        public HashSet<TAccessEvent> accessEvents = new HashSet<TAccessEvent>();

        // Tracks all warnings (e.g. non-fatal errors) that were generated during analysis.
        // These are passed to the backend and eventually propagated to the shell. Dictionarys from
        // warning message to the number of times that warning was logged (in order to avoid
        // duplicating the same warning over and over).
        public readonly Dictionary<String, int> warnings =
            new Dictionary<String, int>();

        // Tracks whether the warnings have been retrieved from this analyzer. If set to true,
        // adding new warnings will result in an error. This helps to make sure that no
        // warnings are added which will not be displayed.
        public bool warningsRetrieved = false;

        // The SCC-condensed graph representation of all slot value transfers.
        private SccCondensedGraph valueTransferGraph;

        private readonly List<Tuple<SlotId, SlotId>> registeredValueTransfers =
            new EditableList<Tuple<SlotId, SlotId>>();

        // Bidirectional Dictionary between Integer index and TNetworkAddress.
        // Decreases the size of the scan range locations.
        private readonly List<TNetworkAddress> hostIndex = new List<TNetworkAddress>();

        // Cache of statement-relevant table metadata populated before analysis.
        private readonly StmtTableCache stmtTableCache;

        // Expr rewriter for folding constants.
        private readonly ExprRewriter constantFolder_ =
            new ExprRewriter(FoldConstantsRule.INSTANCE);

        // Expr rewriter for normalizing and rewriting expressions.
        private readonly ExprRewriter exprRewriter_;

        public GlobalState(StmtTableCache stmtTableCache, TQueryCtx queryCtx,
            AuthorizationConfig authzConfig)
        {
            this.stmtTableCache = stmtTableCache;
            this.queryCtx = queryCtx;
            this.authzConfig = authzConfig;
            this.lineageGraph = new ColumnLineageGraph();
            List<ExprRewriteRule> rules = new List<ExprRewriteRule>();
            // BetweenPredicates must be rewritten to be executable. Other non-essential
            // expr rewrites can be disabled via a query option. When rewrites are enabled
            // BetweenPredicates should be rewritten first to help trigger other rules.
            rules.Add(BetweenToCompoundRule.INSTANCE);
            // Binary predicates must be rewritten to a canonical form for both Kudu predicate
            // pushdown and Parquet row group pruning based on min/max statistics.
            rules.Add(NormalizeBinaryPredicatesRule.INSTANCE);
            if (queryCtx.getClient_request().getQuery_options().enable_expr_rewrites)
            {
                rules.Add(FoldConstantsRule.INSTANCE);
                rules.Add(NormalizeExprsRule.INSTANCE);
                rules.Add(ExtractCommonConjunctRule.INSTANCE);
                // Relies on FoldConstantsRule and NormalizeExprsRule.
                rules.Add(SimplifyConditionalsRule.INSTANCE);
                rules.Add(EqualityDisjunctsToInRule.INSTANCE);
                rules.Add(NormalizeCountStarRule.INSTANCE);
                rules.Add(SimplifyDistinctFromRule.INSTANCE);
                rules.Add(RemoveRedundantStringCast.INSTANCE);
            }

            exprRewriter_ = new ExprRewriter(rules);
        }
    }

}
