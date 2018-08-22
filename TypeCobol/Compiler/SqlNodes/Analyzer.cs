using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Castle.Core;
using Castle.Core.Internal;
using TypeCobol.Compiler.Concurrency;
using TypeCobol.Compiler.SqlNodes.Common;
using TupleId = System.Int32;
using TypeCobol.Compiler.SqlNodes;
using TypeCobol.Compiler.SqlNodes.Authorization;
using TypeCobol.Compiler.SqlNodes.Catalog;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Repository of analysis state for single select block.
     *
     * Conjuncts in
     * Conjuncts are registered during analysis (registerConjuncts()) and assigned during the
     * planning process (getUnassigned[Oj]Conjuncts()/isConjunctAssigned()/
     * markConjunctsAssigned()).
     * All conjuncts are assigned a unique id when initially registered, and all registered
     * conjuncts are referenced by their id (ie, there are no containers other than the one
     * holding the referenced conjuncts), to make substitute() simple.
     *
     * Slot value transfers in
     * Slot A has a value transfer to slot B if a predicate on A can be applied to B at some
     * point in the plan. Determining the lowest correct placement of that predicate is
     * subject to the conventional assignment rules.
     * Each slot is contained in exactly one equivalence class. A slot equivalence class is a
     * set of slots where each pair of slots has a mutual value transfer. Equivalence classes
     * are assigned an arbitrary id to distinguish them from another.
     *
     * Implied mutual value transfers are registered with createAuxEqPredicate(); they are
     * never assigned during plan generation.
     * Also tracks each catalog object access, so authorization checks can be performed once
     * analysis is complete.
     * TODO in We often use the terms stmt/block/analyzer interchangeably, although they may
     * have slightly different meanings (sometimes depending on the context). Use the terms
     * more accurately and consistently here and elsewhere.
     */
    public class Analyzer
    {
        // Common analysis error messages
        public static readonly String DB_DOES_NOT_EXIST_ERROR_MSG = "Database does not exist in ";
        public static readonly String DB_ALREADY_EXISTS_ERROR_MSG = "Database already exists in ";
        public static readonly String TBL_DOES_NOT_EXIST_ERROR_MSG = "Table does not exist in ";
        public static readonly String TBL_ALREADY_EXISTS_ERROR_MSG = "Table already exists in ";
        public static readonly String FN_DOES_NOT_EXIST_ERROR_MSG = "Function does not exist in ";
        public static readonly String FN_ALREADY_EXISTS_ERROR_MSG = "Function already exists in ";
        public static readonly String DATA_SRC_DOES_NOT_EXIST_ERROR_MSG =
            "Data source does not exist in ";
        public static readonly String DATA_SRC_ALREADY_EXISTS_ERROR_MSG =
            "Data source already exists in ";


        private readonly GlobalState globalState_;

        private readonly User user_;

        // Indicates whether this query block Contains a straight join hint.
        private bool isStraightJoin_ = false;

        // Whether to use Hive's auto-generated column labels.
        private bool useHiveColLabels_ = false;

        // True if the corresponding select block has a limit and/or offset clause.
        private bool hasLimitOffsetClause_ = false;

        // Current depth of nested analyze() calls. Used for enforcing a
        // maximum expr-tree depth. Needs to be manually maintained by the user
        // of this Analyzer with incrementCallDepth() and decrementCallDepth().
        private int callDepth_ = 0;

        // Flag indicating if this analyzer instance belongs to a subquery.
        private bool isSubquery_ = false;

        // Flag indicating whether this analyzer belongs to a WITH clause view.
        private bool isWithClause_ = false;

        // If set, when masked privilege requests are registered they will use this error
        // error message.
        private String authErrorMsg_;

        // If true privilege requests are added in maskedPrivileReqs_. Otherwise, privilege
        // requests are added to privilegeReqs_.
        private bool maskPrivChecks_ = false;

        // If false, privilege requests are not registered.
        private bool enablePrivChecks_ = true;

        // By default, all registered semi-joined tuples are invisible, i.e., their slots
        // cannot be referenced. If set, this semi-joined tuple is made visible. Such a tuple
        // should only be made visible for analyzing the On-clause of its semi-join.
        // In particular, if there are multiple semi-joins in the same query block, then the
        // On-clause of any such semi-join is not allowed to reference other semi-joined tuples
        // except its own. Therefore, only a single semi-joined tuple can be visible at a time.
        private TupleId visibleSemiJoinedTupleId_;

        public void setIsSubquery()
        {
            isSubquery_ = true;
            globalState_.containsSubquery = true;
        }
        public bool setHasPlanHints() { return globalState_.hasPlanHints = true; }
        public bool hasPlanHints() { return globalState_.hasPlanHints; }
        public void setIsWithClause() { isWithClause_ = true; }
        public bool isWithClause() { return isWithClause_; }

        public void setIsExplain()
        {
            globalState_.isExplain = true;
        }

        // An analyzer stores analysis state for a single select block.A select block can be
  // a top level select statement, or an inline view select block.
  // ancestors Contains the Analyzers of the enclosing select blocks of 'this'
  // (ancestors[0] Contains the immediate parent, etc.).
        private readonly List<Analyzer> ancestors_;

        // map from lowercase table alias to a view definition in this analyzer's scope
        private readonly Dictionary<String, FeView> localViews_ = new Dictionary<string, FeView>();

        // Dictionary from lowercase table alias to descriptor. Tables without an explicit alias
        // are assigned two implicit aliases in the unqualified and fully-qualified table name.
        // Such tables have two entries pointing to the same descriptor. If an alias is
        // ambiguous, then this map retains the First entry with that alias to simplify error
        // checking (duplicate vs. ambiguous alias).
        private readonly Dictionary<String, TupleDescriptor> aliasMap_ = new Dictionary<string, TupleDescriptor>();

        // Dictionary from tuple id to its corresponding table ref.
        private readonly Dictionary<TupleId, TableRef> tableRefMap_ = new Dictionary<int, TableRef>();

        // Set of lowercase ambiguous implicit table aliases.
        private readonly HashSet<String> ambiguousAliases_ = new HashSet<string>();

        // Dictionary from lowercase fully-qualified path to its slot descriptor. Only Contains paths
        // that have a scalar type as destination (see registerSlotRef()).
        private readonly Dictionary<String, SlotDescriptor> slotPathMap_ = new Dictionary<string, SlotDescriptor>();

        // Indicates whether this analyzer/block is guaranteed to have an empty result set
        // due to a limit 0 or constant conjunct evaluating to false.
        private bool hasEmptyResultSet_ = false;

        // Indicates whether the select-project-join (spj) portion of this query block
        // is guaranteed to return an empty result set. Set due to a constant non-Having
        // conjunct evaluating to false.
        private bool hasEmptySpjResultSet_ = false;

        public Analyzer(StmtTableCache stmtTableCache, TQueryCtx queryCtx,
            AuthorizationConfig authzConfig)
        {
            ancestors_ = new List<Analyzer>();
            globalState_ = new GlobalState(stmtTableCache, queryCtx, authzConfig);
            user_ = new User(TSessionStateUtil.getEffectiveUser(queryCtx.session));
        }

        /**
         * Analyzer constructor for nested select block. GlobalState is inherited from the
         * parentAnalyzer.
         */
        public Analyzer(Analyzer parentAnalyzer) inthis(parentAnalyzer, parentAnalyzer.globalState_)
        {
        }

        /**
         * Analyzer constructor for nested select block with the specified global state.
         */
        private Analyzer(Analyzer parentAnalyzer, GlobalState globalState)
        {
            ancestors_ = new List<Analyzer>() { parentAnalyzer};
            ancestors_.AddRange(parentAnalyzer.ancestors_);
            globalState_ = globalState;
            user_ = parentAnalyzer.getUser();
            useHiveColLabels_ = parentAnalyzer.useHiveColLabels_;
            authErrorMsg_ = parentAnalyzer.authErrorMsg_;
            maskPrivChecks_ = parentAnalyzer.maskPrivChecks_;
            enablePrivChecks_ = parentAnalyzer.enablePrivChecks_;
            isWithClause_ = parentAnalyzer.isWithClause_;
        }

        /**
         * Returns a new analyzer with the specified parent analyzer but with a new
         * global state.
         */
        public static Analyzer createWithNewGlobalState(Analyzer parentAnalyzer)
        {
            GlobalState globalState = new GlobalState(parentAnalyzer.globalState_.stmtTableCache,
                parentAnalyzer.getQueryCtx(), parentAnalyzer.getAuthzConfig());
            return new Analyzer(parentAnalyzer, globalState);
        }

        /**
         * Makes the given semi-joined tuple visible such that its slots can be referenced.
         * If tid is null, makes the currently visible semi-joined tuple invisible again.
         */
        public void setVisibleSemiJoinedTuple(TupleId tid)
        {
            //Preconditions.checkState(tid == null
                //|| globalState_.semiJoinedTupleIds.ContainsKey(tid));
            //Preconditions.checkState(tid == null || visibleSemiJoinedTupleId_ == null);
            visibleSemiJoinedTupleId_ = tid;
        }

        public bool hasAncestors() { return !ancestors_.IsNullOrEmpty(); }
        public Analyzer getParentAnalyzer()
        {
            return hasAncestors() ? ancestors_[0]  in null;
        }

        /**
         * Returns the analyzer that has an entry for the given tuple descriptor in its
         * tableRefMap, or null if no such analyzer could be found. Searches the hierarchy
         * of analyzers bottom-up.
         */
        public Analyzer findAnalyzer(TupleId tid)
        {
            if (tableRefMap_.ContainsKey(tid)) return this;
            if (hasAncestors()) return getParentAnalyzer().findAnalyzer(tid);
            return null;
        }

        /**
         * Returns a list of each warning logged, indicating if it was logged more than once.
         * After this function has been called, no warning may be added to the Analyzer anymore.
         */
        public List<String> getWarnings()
        {
            globalState_.warningsRetrieved = true;
            List<String> result = new List<String>();
            foreach (KeyValuePair<string,TupleId> e in  globalState_.warnings)
            {
                String error = e.Key;
                int count = e.Value;
                //Preconditions.checkState(count > 0);
                if (count == 1)
                {
                    result.Add(error);
                }
                else
                {
                    result.Add(error + " (" + count + " warnings like this)");
                }
            }
            return result;
        }

        /**
         * Registers a local view definition with this analyzer. Throws an exception if a view
         * definition with the same alias has already been registered or if the number of
         * explicit column labels is greater than the number of columns in the view statement.
         */
        public void registerLocalView(FeView view) 
        {
            //Preconditions.checkState(view.isLocalView());
    if (view.getColLabels() != null) {
                List<String> viewLabels = view.getColLabels();
                List<String> queryStmtLabels = view.getQueryStmt().getColLabels();
                if (viewLabels.Count > queryStmtLabels.Count)
                {
                    throw new AnalysisException("WITH-clause view '" + view.getName() +
                        "' returns " + queryStmtLabels.Count + " columns, but " +
                        viewLabels.Count + " labels were specified. The number of column " +
                        "labels must be smaller or equal to the number of returned columns.");
                }
            }
    if (localViews_[view.getName().ToLowerInvariant()]!= view) {
                throw new AnalysisException(
                    String.Format("Duplicate table alias in '{0}'", view.getName()));
            }
        }

        /**
         * Creates an returns an empty TupleDescriptor for the given table ref and registers
         * it against all its legal aliases. For tables refs with an explicit alias, only the
         * explicit alias is legal. For tables refs with no explicit alias, the fully-qualified
         * and unqualified table names are legal aliases. Column references against unqualified
         * implicit aliases can be ambiguous, therefore, we register such ambiguous aliases
         * here. Requires that all views have been substituted.
         * Throws if an existing explicit alias or implicit fully-qualified alias
         * has already been registered for another table ref.
         */
        public TupleDescriptor registerTableRef(TableRef refTable) 
        {
            String uniqueAlias = refTable.getUniqueAlias();
    if (aliasMap_.ContainsKey(uniqueAlias)) {
                throw new AnalysisException("Duplicate table alias in '" + uniqueAlias + "'");
            }

            // If ref has no explicit alias, then the unqualified and the fully-qualified table
            // names are legal implicit aliases. Column references against unqualified implicit
            // aliases can be ambiguous, therefore, we register such ambiguous aliases here.
            String unqualifiedAlias = null;
            String []
            aliases = refTable.getAliases();
    if (aliases.Length > 1) {
                unqualifiedAlias = aliases[1];
                TupleDescriptor tupleDesc = aliasMap_[unqualifiedAlias];
                if (tupleDesc != null)
                {
                    if (tupleDesc.hasExplicitAlias())
                    {
                        throw new AnalysisException(
                            "Duplicate table alias in '" + unqualifiedAlias + "'");
                    }
                    else
                    {
                        ambiguousAliases_.Add(unqualifiedAlias);
                    }
                }
            }

            // Delegate creation of the tuple descriptor to the concrete table ref.
            TupleDescriptor result = refTable.createTupleDescriptor(this);
            result.setAliases(aliases, refTable.hasExplicitAlias());
    // Register all legal aliases.
    foreach (String alias in aliases) {
                aliasMap_[alias]= result;
            }
            tableRefMap_[result.getId()]= refTable;
    return result;
        }

        /**
         * Resolves the given TableRef into a concrete BaseTableRef, ViewRef or
         * CollectionTableRef. Returns the new resolved table ref or the given table
         * ref if it is already resolved.
         * Registers privilege requests and throws an AnalysisException if the tableRef's
         * path could not be resolved. The privilege requests are added to ensure that
         * an AuthorizationException is preferred over an AnalysisException so as not to
         * accidentally reveal the non-existence of tables/databases.
         */
        public TableRef resolveTableRef(TableRef tableRef) 
        {
    // Return the table if it is already resolved.
    if (tableRef.isResolved()) return tableRef;
    // Try to find a matching local view.
    if (tableRef.getPath().Count == 1) {
                // Searches the hierarchy of analyzers bottom-up for a registered local view with
                // a matching alias.
                String viewAlias = tableRef.getPath()[0].ToLowerInvariant();
                Analyzer analyzer = this;
                do
                {
                    FeView localView = analyzer.localViews_.get(viewAlias);
                    if (localView != null) return new InlineViewRef(localView, tableRef);
                    analyzer = (analyzer.ancestors_.IsNullOrEmpty() ? null  in analyzer.ancestors_[0]);
                } while (analyzer != null);
            }

            // Resolve the table ref's path and determine what resolved table ref
            // to replace it with.
            List<String> rawPath = tableRef.getPath();
            Path resolvedPath = null;
    try {
                resolvedPath = resolvePath(tableRef.getPath(), PathType.TABLEREF);
            } catch (AnalysisException e) {
                // Register privilege requests to prefer reporting an authorization error over
                // an analysis error. We should not accidentally reveal the non-existence of a
                // table/database if the user is not authorized.
                if (rawPath.Count > 1)
                {
                    registerPrivReq(new PrivilegeRequestBuilder()
                        .onTable(rawPath[0], rawPath.get(1))
                        .allOf(tableRef.getPrivilege()).toRequest());
                }
                registerPrivReq(new PrivilegeRequestBuilder()
                    .onTable(getDefaultDb(), rawPath[0])
                    .allOf(tableRef.getPrivilege()).toRequest());
                throw e;
            } catch (TableLoadingException e) {
                throw new AnalysisException(String.Format(
                    "Failed to load metadata for table in '{0}'", string.Join(".",rawPath)), e);
            }

            //Preconditions.checkNotNull(resolvedPath);
    if (resolvedPath.destTable() != null) {
                FeTable table = resolvedPath.destTable();
                if (table is FeView) return new InlineViewRef((FeView)table, tableRef);
                // The table must be a base table.
                //Preconditions.checkState(table is FeFsTable ||
                    //table is FeKuduTable ||
                    //table is HBaseTable ||
                    //table is FeDataSourceTable);
                return new BaseTableRef(tableRef, resolvedPath);
            } else {
                return new CollectionTableRef(tableRef, resolvedPath);
            }
        }

        /**
         * Register conjuncts that are outer joined by a full outer join. For a given
         * predicate, we record the last full outer join that outer-joined any of its
         * tuple ids. We need this additional information because full-outer joins obey
         * different rules with respect to predicate pushdown compared to left and right
         * outer joins.
         */
        public void registerFullOuterJoinedConjunct(Expr e)
        {
            //Preconditions.checkState(
                !globalState_.fullOuterJoinedConjuncts.ContainsKey(e.getId()));
            List<TupleId> tids = new List<int>();
            e.getIds(tids, null);
            foreach (TupleId tid in tids)
            {
                if (!globalState_.fullOuterJoinedTupleIds.ContainsKey(tid)) continue;
                TableRef currentOuterJoin = globalState_.fullOuterJoinedTupleIds[tid];
                globalState_.fullOuterJoinedConjuncts[e.getId()]= currentOuterJoin;
                break;
            }
            //if (LOG.isTraceEnabled())
            //{
            //    LOG.trace("registerFullOuterJoinedConjunct in " +
            //        globalState_.fullOuterJoinedConjuncts.ToString());
            //}
        }

        /**
         * Register tids as being outer-joined by a full outer join clause represented by
         * rhsRef.
         */
        public void registerFullOuterJoinedTids(List<TupleId> tids, TableRef rhsRef)
        {
            foreach (TupleId tid in tids)
            {
                globalState_.fullOuterJoinedTupleIds[tid]= rhsRef;
            }
            //if (LOG.isTraceEnabled())
            //{
            //    LOG.trace("registerFullOuterJoinedTids in " +
            //        globalState_.fullOuterJoinedTupleIds.ToString());
            //}
        }

        /**
         * Register tids as being outer-joined by Join clause represented by rhsRef.
         */
        public void registerOuterJoinedTids(List<TupleId> tids, TableRef rhsRef)
        {
            foreach (TupleId tid in tids)
            {
                globalState_.outerJoinedTupleIds[tid]= rhsRef;
            }
            //if (LOG.isTraceEnabled())
            //{
            //    LOG.trace("registerOuterJoinedTids in " +
            //        globalState_.outerJoinedTupleIds.ToString());
            //}
        }

        /**
         * Register the given tuple id as being the invisible side of a semi-join.
         */
        public void registerSemiJoinedTid(TupleId tid, TableRef rhsRef)
        {
            globalState_.semiJoinedTupleIds[tid]= rhsRef;
        }

        /**
         * Returns the descriptor of the given explicit or implicit table alias or null if no
         * such alias has been registered.
         * Throws an AnalysisException if the given table alias is ambiguous.
         */
        public TupleDescriptor getDescriptor(String tableAlias) 
        {
            String lookupAlias = tableAlias.ToLowerInvariant();
    if (ambiguousAliases_.Contains(lookupAlias)) {
                throw new AnalysisException(String.Format(
                    "Unqualified table alias is ambiguous in '%s'", tableAlias));
            }
    return aliasMap_[lookupAlias];
            }

  public TupleDescriptor getTupleDesc(TupleId id)
        {
            return globalState_.descTbl.getTupleDesc(id);
        }

        public SlotDescriptor getSlotDesc(SlotId id)
        {
            return globalState_.descTbl.getSlotDesc(id);
        }

        public int getNumTableRefs() { return tableRefMap_.Count; }
        public TableRef getTableRef(TupleId tid) { return tableRefMap_[tid]; }
        public ExprRewriter getConstantFolder() { return globalState_.constantFolder_; }
        public ExprRewriter getExprRewriter() { return globalState_.exprRewriter_; }

        /**
         * Given a "table alias"."column alias", return the SlotDescriptor
         */
        public SlotDescriptor getSlotDescriptor(String qualifiedColumnName)
        {
            return slotPathMap_[qualifiedColumnName];
        }

        /**
         * Return true if this analyzer has no ancestors. (i.e. false for the analyzer created
         * for inline views/ union operands, etc.)
         */
        public bool isRootAnalyzer() { return ancestors_.IsNullOrEmpty(); }

        /**
         * Returns true if the query block corresponding to this analyzer is guaranteed
         * to return an empty result set, e.g., due to a limit 0 or a constant predicate
         * that evaluates to false.
         */
        public bool hasEmptyResultSet() { return hasEmptyResultSet_; }
        public void setHasEmptyResultSet() { hasEmptyResultSet_ = true; }

        /**
         * Returns true if the select-project-join portion of this query block returns
         * an empty result set.
         */
        public bool hasEmptySpjResultSet() { return hasEmptySpjResultSet_; }

        /**
         * Resolves the given raw path according to the given path type, as follows in
         * SLOTREF and STAR in Resolves the path in the context of all registered tuple
         * descriptors, considering qualified as well as unqualified matches.
         * TABLEREF in Resolves the path in the context of all registered tuple descriptors
         * only considering qualified matches, as well as catalog tables/views.
         *
         * Path resolution in
         * Regardless of the path type, a raw path can have multiple successful resolutions.
         * A resolution is said to be 'successful' if all raw path elements can be mapped
         * to a corresponding alias/table/column/field.
         *
         * Path legality in
         * A successful resolution may be illegal with respect to the path type, e.g.,
         * a SlotRef cannot reference intermediate collection types, etc.
         *
         * Path ambiguity in
         * A raw path is ambiguous if it has multiple legal resolutions. Otherwise,
         * the ambiguity is resolved in favor of the legal resolution.
         *
         * Returns the single legal path resolution if it exists.
         * Throws if there was no legal resolution or if the path is ambiguous.
         */
        public Path resolvePath(List<String> rawPath, PathType pathType) {
    // We only allow correlated references in predicates of a subquery.
    bool resolveInAncestors = false;
    if (pathType == PathType.TABLEREF || pathType == PathType.ANY) {
      resolveInAncestors = true;
    } else if (pathType == PathType.SLOTREF) {
      resolveInAncestors = isSubquery_;
    }
// Convert all path elements to lower case.
List<String> lcRawPath = new List<string>(rawPath.Count);
    foreach (String s in rawPath) lcRawPath.Add(s.ToLowerInvariant());
    return resolvePath(lcRawPath, pathType, resolveInAncestors);
  }

  private Path resolvePath(List<String> rawPath, PathType pathType,
      bool resolveInAncestors)  {
    // List of all candidate paths with different roots. Paths in this list are initially
    // unresolved and may be illegal with respect to the pathType.
    List<Path> candidates = getTupleDescPaths(rawPath);

LinkedList<String> errors = new LinkedList<string>();
    if (pathType == PathType.SLOTREF || pathType == PathType.STAR) {
      // Paths rooted at all of the unique registered tuple descriptors.
      foreach (TableRef tblRef in tableRefMap_.values()) {
        candidates.Add(new Path(tblRef.getDesc(), rawPath));
      }
    } else {
      // Always prefer table ref paths rooted at a registered tuples descriptor.
      //Preconditions.checkState(pathType == PathType.TABLEREF ||
          //pathType == PathType.ANY);
      Path result = resolvePaths(rawPath, candidates, pathType, errors);
      if (result != null) return result;
      candidates.Clear();

      // Add paths rooted at a table with an unqualified and fully-qualified table name.
      List<TableName> candidateTbls = Path.getCandidateTables(rawPath, getDefaultDb());
      for (int tblNameIdx = 0; tblNameIdx<candidateTbls.Count; ++tblNameIdx) {
        TableName tblName = candidateTbls.get(tblNameIdx);
FeTable tbl = null;
        try {
          tbl = getTable(tblName.getDb(), tblName.getTbl());
        } catch (AnalysisException e) {
          // Ignore to allow path resolution to continue.
        }
        if (tbl != null) {
          candidates.Add(new Path(tbl, rawPath.GetRange(tblNameIdx + 1, rawPath.Count)));
        }
      }
    }

    Path result = resolvePaths(rawPath, candidates, pathType, errors);
    if (result == null && resolveInAncestors && hasAncestors()) {
      result = getParentAnalyzer().resolvePath(rawPath, pathType, true);
    }
    if (result == null) {
      //Preconditions.checkState(!errors.IsNullOrEmpty());
      throw new AnalysisException(errors.First());
    }
    return result;
  }

  /**
   * Returns a list of unresolved Paths that are rooted at a registered tuple
   * descriptor matching a prefix of the given raw path.
   */
  public List<Path> getTupleDescPaths(List<String> rawPath)
      
{
    List<Path> result = new List<Path>();

    // Path rooted at a tuple desc with an explicit or implicit unqualified alias.
    TupleDescriptor rootDesc = getDescriptor(rawPath[0]);
    if (rootDesc != null)
    {
        result.Add(new Path(rootDesc, rawPath.GetRange(1, rawPath.Count)));
    }

    // Path rooted at a tuple desc with an implicit qualified alias.
    if (rawPath.Count > 1)
    {
        rootDesc = getDescriptor(rawPath[0] + "." + rawPath[1]);
        if (rootDesc != null)
        {
            result.Add(new Path(rootDesc, rawPath.GetRange(2, rawPath.Count)));
        }
    }
    return result;
    }

  /**
   * Resolves the given paths and checks them for legality and ambiguity. Returns the
   * single legal path resolution if it exists, null otherwise.
   * Populates 'errors' with a a prioritized list of error messages starting with the
   * most relevant one. The list Contains at least one error message if null is returned.
   */
  private Path resolvePaths(List<String> rawPath, List<Path> paths, PathType pathType,
      LinkedList<String> errors)
{
    // For generating error messages.
    String pathTypeStr = null;
    String pathStr = string.Join(".",rawPath);
    if (pathType == PathType.SLOTREF)
    {
        pathTypeStr = "Column/field reference";
    }
    else if (pathType == PathType.TABLEREF)
    {
        pathTypeStr = "Table reference";
    }
    else if (pathType == PathType.ANY)
    {
        pathTypeStr = "Path";
    }
    else
    {
        //Preconditions.checkState(pathType == PathType.STAR);
        pathTypeStr = "Star expression";
        pathStr += ".*";
    }

    List<Path> legalPaths = new List<Path>();
    foreach(Path p in paths)
    {
        if (!p.resolve()) continue;

        // Check legality of the resolved path.
        if (p.isRootedAtTuple() && !isVisible(p.getRootDesc().getId()))
        {
            errors.AddLast(String.Format(
                "Illegal %s '{0}' of semi-/anti-joined table '{1}'",
                pathTypeStr.ToLowerInvariant(), pathStr, p.getRootDesc().getAlias()));
            continue;
        }
        switch (pathType)
        {
                    // Illegal cases in
                    // 1. Destination type is not a collection.
                    case PathType.TABLEREF:
                {
                    if (!p.destType().isCollectionType())
                    {
                        errors.AddFirst(String.Format(
                            "Illegal table reference to non-collection type in '{0}'\n" +
                                "Path resolved to type in {1}", pathStr, p.destType().toSql()));
                        continue;
                    }
                    break;
                }
                    case PathType.SLOTREF:
                {
                    // Illegal cases in
                    // 1. Path Contains an intermediate collection reference.
                    // 2. Destination of the path is a catalog table or a registered alias.
                    if (p.hasNonDestCollection())
                    {
                        errors.AddFirst(String.Format(
                            "Illegal column/field reference '{0}' with intermediate " +
                            "collection '{1}' of type '{2}'",
                            pathStr, p.getFirstCollectionName(),
                            p.getFirstCollectionType().toSql()));
                        continue;
                    }
                    // Error should be "Could not resolve...". No need to Add it here explicitly.
                    if (p.getMatchedTypes().IsNullOrEmpty()) continue;
                    break;
                }
                    // Illegal cases in
                    // 1. Path Contains an intermediate collection reference.
                    // 2. Destination type of the path is not a struct.
                    case PathType.STAR:
                {
                    if (p.hasNonDestCollection())
                    {
                        errors.AddFirst(String.Format(
                            "Illegal star expression '{0}' with intermediate " +
                            "collection '{1}' of type '{2}'",
                            pathStr, p.getFirstCollectionName(),
                            p.getFirstCollectionType().toSql()));
                        continue;
                    }
                    if (!p.destType().isStructType())
                    {
                        errors.AddFirst(String.Format(
                            "Cannot expand star in '{0}' because path '{1}' resolved to type '{2}'." +
                            "\nStar expansion is only valid for paths to a struct type.",
                            pathStr, string.Join(".",rawPath), p.destType().toSql()));
                        continue;
                    }
                    break;
                }
                    case PathType.ANY:
                {
                    // Any path is valid.
                    break;
                }
        }
        legalPaths.Add(p);
    }

    if (legalPaths.Count > 1)
    {
        errors.AddFirst(String.Format("%s is ambiguous in '%s'",
            pathTypeStr, pathStr));
        return null;
    }
    if (legalPaths.IsNullOrEmpty())
    {
        if (errors.IsNullOrEmpty())
        {
            errors.AddFirst(String.Format("Could not resolve %s in '%s'",
                pathTypeStr.ToLowerInvariant(), pathStr));
        }
        return null;
    }
    return legalPaths[0];
}

/**
 * Returns an existing or new SlotDescriptor for the given path. Always returns
 * a new empty SlotDescriptor for paths with a collection-typed destination.
 */
public SlotDescriptor registerSlotRef(Path slotPath) 
{
    //Preconditions.checkState(slotPath.isRootedAtTuple());
    // Always register a new slot descriptor for collection types. The BE currently
    // relies on this behavior for setting unnested collection slots to NULL.
    if (slotPath.destType().isCollectionType()) {
        SlotDescriptor res = addSlotDescriptor(slotPath.getRootDesc());
        res.setPath(slotPath);
        registerColumnPrivReq(res);
        return res;
    }
    // SlotRefs with a scalar type are registered against the slot's
    // fully-qualified lowercase path.
    String key = slotPath.ToString();
    //Preconditions.checkState(key.Equals(key.ToLowerInvariant()),
        //"Slot paths should be lower case in " + key);
    SlotDescriptor existingSlotDesc = slotPathMap_[key];
    if (existingSlotDesc != null) return existingSlotDesc;
    SlotDescriptor result = addSlotDescriptor(slotPath.getRootDesc());
    result.setPath(slotPath);
    slotPathMap_[key]= result;
    registerColumnPrivReq(result);
    return result;
    }

  /**
   * Registers a column-level privilege request if 'slotDesc' directly or indirectly
   * refers to a table column. It handles both scalar and complex-typed columns.
   */
  private void registerColumnPrivReq(SlotDescriptor slotDesc)
{
    //Preconditions.checkNotNull(slotDesc.getPath());
    TupleDescriptor tupleDesc = slotDesc.getParent();
    if (tupleDesc.isMaterialized() && tupleDesc.getTable() != null)
    {
        Column column = tupleDesc.getTable().getColumn(
            slotDesc.getPath().getRawPath()[0]);
        if (column != null)
        {
            registerPrivReq(new PrivilegeRequestBuilder().
                allOf(Privilege.SELECT).onColumn(tupleDesc.getTableName().getDb(),
                tupleDesc.getTableName().getTbl(), column.getName()).toRequest());
        }
    }
}

/**
 * Creates a new slot descriptor and related state in globalState.
 */
public SlotDescriptor addSlotDescriptor(TupleDescriptor tupleDesc)
{
    SlotDescriptor result = globalState_.descTbl.addSlotDescriptor(tupleDesc);
    globalState_.blockBySlot[result.getId()]= this;
    return result;
}

/**
 * Adds a new slot descriptor in tupleDesc that is identical to srcSlotDesc
 * except for the path and slot id.
 */
public SlotDescriptor copySlotDescriptor(SlotDescriptor srcSlotDesc,
    TupleDescriptor tupleDesc)
{
    SlotDescriptor result = globalState_.descTbl.addSlotDescriptor(tupleDesc);
    globalState_.blockBySlot[result.getId()]= this;
    result.setSourceExprs(srcSlotDesc.getSourceExprs());
    result.setLabel(srcSlotDesc.getLabel());
    result.setStats(srcSlotDesc.getStats());
    result.setType(srcSlotDesc.getType());
    result.setItemTupleDesc(srcSlotDesc.getItemTupleDesc());
    return result;
}

/**
 * Register all conjuncts in a list of predicates as Having-clause conjuncts.
 */
public void registerConjuncts(List<Expr> l) 
{
    foreach (Expr e in l) {
        registerConjuncts(e, true);
    }
}

/**
 * Register all conjuncts in 'conjuncts' that make up the On-clause of the given
 * right-hand side of a join. Assigns each conjunct a unique id. If rhsRef is
 * the right-hand side of an outer join, then the conjuncts conjuncts are
 * registered such that they can only be evaluated by the node implementing that
 * join.
 */
public void registerOnClauseConjuncts(List<Expr> conjuncts, TableRef rhsRef)
      
{
    //Preconditions.checkNotNull(rhsRef);
    //Preconditions.checkNotNull(conjuncts);
    List<ExprId> ojConjuncts = null;
    if (rhsRef.getJoinOp().isOuterJoin()) {
        ojConjuncts = globalState_.conjunctsByOjClause[rhsRef.getId()];
        if (ojConjuncts == null)
        {
            ojConjuncts = new List<ExprId>();
            globalState_.conjunctsByOjClause[rhsRef.getId()]= ojConjuncts;
        }
    }
    foreach (Expr conjunct in conjuncts) {
        conjunct.setIsOnClauseConjunct(true);
        registerConjunct(conjunct);
        if (rhsRef.getJoinOp().isOuterJoin())
        {
            globalState_.ojClauseByConjunct[conjunct.getId()]= rhsRef;
            ojConjuncts.Add(conjunct.getId());
        }
        if (rhsRef.getJoinOp().isSemiJoin())
        {
            globalState_.sjClauseByConjunct[conjunct.getId()]= rhsRef;
        }
        if (rhsRef.getJoinOp().isInnerJoin())
        {
            globalState_.ijClauseByConjunct[conjunct.getId()]= rhsRef;
        }
        markConstantConjunct(conjunct, false);
    }
}

/**
 * Register all conjuncts that make up 'e'. If fromHavingClause is false, this conjunct
 * is assumed to originate from a WHERE or ON clause.
 */
public void registerConjuncts(Expr e, bool fromHavingClause)
      
{
    foreach (Expr conjunct in e.getConjuncts()) {
        registerConjunct(conjunct);
        markConstantConjunct(conjunct, fromHavingClause);
    }
}

/**
 * If the given conjunct is a constant non-oj conjunct, marks it as assigned, and
 * evaluates the conjunct. If the conjunct evaluates to false, marks this query
 * block as having an empty result set or as having an empty select-project-join
 * portion, if fromHavingClause is true or false, respectively.
 * No-op if the conjunct is not constant or is outer joined.
 * Throws an AnalysisException if there is an error evaluating `conjunct`
 */
private void markConstantConjunct(Expr conjunct, bool fromHavingClause)
      
{
    if (!conjunct.isConstant() || isOjConjunct(conjunct)) return;
    markConjunctAssigned(conjunct);
    if ((!fromHavingClause && !hasEmptySpjResultSet_)
        || (fromHavingClause && !hasEmptyResultSet_)) {
        try
        {
            if (conjunct is BetweenPredicate) {
                // Rewrite the BetweenPredicate into a CompoundPredicate so we can evaluate it
                // below (BetweenPredicates are not executable). We might be in the First
                // analysis pass, so the conjunct may not have been rewritten yet.
                ExprRewriter rewriter = new ExprRewriter(BetweenToCompoundRule.INSTANCE);
                conjunct = rewriter.rewrite(conjunct, this);
                // analyze this conjunct here in we know it can't contain references to select list
                // aliases and having it analyzed is needed for the following EvalPredicate() call
                conjunct.analyze(this);
            }
            if (!FeSupport.EvalPredicate(conjunct, globalState_.queryCtx))
            {
                if (fromHavingClause)
                {
                    hasEmptyResultSet_ = true;
                }
                else
                {
                    hasEmptySpjResultSet_ = true;
                }
            }
        }
        catch (Exception ex)
        {
            throw new AnalysisException("Error evaluating \"" + conjunct.toSql() + "\"", ex);
        }
    }
}

/**
 * Assigns a new id to the given conjunct and registers it with all tuple and slot ids
 * it references and with the global conjunct list.
 */
private void registerConjunct(Expr e)
{
    // always generate a new expr id; this might be a cloned conjunct that already
    // has the id of its origin set
    e.setId(globalState_.conjunctIdGenerator.getNextId());
    globalState_.conjuncts[e.getId()]= e;

    List<TupleId> tupleIds = new List<int>();
    List<SlotId> slotIds = new List<SlotId>();
    e.getIds(tupleIds, slotIds);
    registerFullOuterJoinedConjunct(e);

    // register single tid conjuncts
    if (tupleIds.Count == 1 && !e.isAuxExpr())
    {
        globalState_.singleTidConjuncts.Add(e.getId());
    }

    //if (LOG.isTraceEnabled())
    //{
    //    LOG.trace("register tuple/slotConjunct in " + int.ToString(e.getId())
    //    + " " + e.toSql() + " " + e.debugString());
    //}

    if (!(e is BinaryPredicate)) return;
    BinaryPredicate binaryPred = (BinaryPredicate)e;

    // check whether this is an equi-join predicate, ie, something of the
    // form <expr1> = <expr2> where at least one of the exprs is bound by
    // exactly one tuple id
    if (binaryPred.getOp() != BinaryPredicate.Operator.EQ &&
       binaryPred.getOp() != BinaryPredicate.Operator.NULL_MATCHING_EQ &&
       binaryPred.getOp() != BinaryPredicate.Operator.NOT_DISTINCT)
    {
        return;
    }
    // the binary predicate must refer to at least two tuples to be an eqJoinConjunct
    if (tupleIds.Count < 2) return;

    // examine children and update eqJoinConjuncts
    for (int i = 0; i < 2; ++i)
    {
        tupleIds = new List<int>();
        binaryPred.getChild(i).getIds(tupleIds, null);
        if (tupleIds.Count == 1)
        {
            if (!globalState_.eqJoinConjuncts.ContainsKey(tupleIds[0]))
            {
                List<ExprId> conjunctIds = new List<ExprId>();
                conjunctIds.Add(e.getId());
                globalState_.eqJoinConjuncts[tupleIds[0]]= conjunctIds;
            }
            else
            {
                globalState_.eqJoinConjuncts[tupleIds[0]].Add(e.getId());
            }
            binaryPred.setIsEqJoinConjunct(true);
            //LOG.trace("register eqJoinConjunct in " + int.ToString(e.getId()));
        }
    }
}

/**
 * Create and register an auxiliary predicate to express a mutual value transfer
 * between two exprs (BinaryPredicate with EQ); this predicate does not need to be
 * assigned, but it's used for value transfer computation.
 * Does nothing if the lhs or rhs expr are NULL. Registering with NULL would be
 * incorrect, because <expr> = NULL is false (even NULL = NULL).
 */
public void createAuxEqPredicate(Expr lhs, Expr rhs)
{
    // Check the expr type as well as the class because  NullLiteral could have been
    // implicitly cast to a type different than NULL.
    if (lhs is NullLiteral || rhs is NullLiteral ||
        lhs.getType().isNull() || rhs.getType().isNull()) {
        return;
    }
    // create an eq predicate between lhs and rhs
    BinaryPredicate p = new BinaryPredicate(BinaryPredicate.Operator.EQ, lhs, rhs);
    p.setIsAuxExpr();
    //if (LOG.isTraceEnabled())
    //{
    //    LOG.trace("register eq predicate in " + p.toSql() + " " + p.debugString());
    //}
    registerConjunct(p);
}

/**
 * Creates an inferred equality predicate between the given slots.
 */
public BinaryPredicate createInferredEqPred(SlotId lhsSlotId, SlotId rhsSlotId)
{
    BinaryPredicate pred = new BinaryPredicate(BinaryPredicate.Operator.EQ,
        new SlotRef(globalState_.descTbl.getSlotDesc(lhsSlotId)),
        new SlotRef(globalState_.descTbl.getSlotDesc(rhsSlotId)));
    pred.setIsInferred();
    // create casts if needed
    pred.analyzeNoThrow(this);
    return pred;
}

/**
 * Return all unassigned non-constant registered conjuncts that are fully bound by
 * given list of tuple ids. If 'inclOjConjuncts' is false, conjuncts tied to an
 * Outer Join clause are excluded.
 */
public List<Expr> getUnassignedConjuncts(
    List<TupleId> tupleIds, bool inclOjConjuncts)
{
    List<Expr> result = new List<Expr>();
    foreach (Expr e in globalState_.conjuncts.Values)
    {
        if (e.isBoundByTupleIds(tupleIds)
            && !e.isAuxExpr()
            && !globalState_.assignedConjuncts.Contains(e.getId())
            && ((inclOjConjuncts && !e.isConstant())
                || !globalState_.ojClauseByConjunct.ContainsKey(e.getId())))
        {
            result.Add(e);
        }
    }
    return result;
}

public TableRef getOjRef(Expr e)
{
    return globalState_.ojClauseByConjunct[e.getId()];
}

public bool isOjConjunct(Expr e)
{
    return globalState_.ojClauseByConjunct.ContainsKey(e.getId());
}

public bool isIjConjunct(Expr e)
{
    return globalState_.ijClauseByConjunct.ContainsKey(e.getId());
}

public bool isSjConjunct(Expr e)
{
    return globalState_.sjClauseByConjunct.ContainsKey(e.getId());
}

public TableRef getFullOuterJoinRef(Expr e)
{
    return globalState_.fullOuterJoinedConjuncts.get(e.getId());
}

public bool isFullOuterJoined(Expr e)
{
    return globalState_.fullOuterJoinedConjuncts.ContainsKey(e.getId());
}

/**
 * Return all unassigned registered conjuncts for node's table ref ids.
 * Wrapper around getUnassignedConjuncts(List<TupleId> tupleIds).
 */
public List<Expr> getUnassignedConjuncts(PlanNode node)
{
    return getUnassignedConjuncts(node.getTblRefIds());
}

/**
 * Return all unassigned registered conjuncts that are fully bound by the given
 * (logical) tuple ids, can be evaluated by 'tupleIds' and are not tied to an
 * Outer Join clause.
 */
public List<Expr> getUnassignedConjuncts(List<TupleId> tupleIds)
{
    List<Expr> result = new List<Expr>();
    foreach (Expr e in getUnassignedConjuncts(tupleIds, true))
    {
        if (canEvalPredicate(tupleIds, e)) result.Add(e);
    }
    return result;
}

/**
 * Returns true if 'e' must be evaluated after or by a join node. Note that it may
 * still be safe to evaluate 'e' elsewhere as well, but in any case 'e' must be
 * evaluated again by or after a join.
 */
public bool evalAfterJoin(Expr e)
{
    List<TupleId> tids = new List<int>();
    e.getIds(tids, null);
    if (tids.IsNullOrEmpty()) return false;
    if (tids.Count > 1 || isOjConjunct(e) || isFullOuterJoined(e)
        || (isOuterJoined(tids[0])
            && (!e.isOnClauseConjunct() || isIjConjunct(e)))
        || (isAntiJoinedConjunct(e) && !isSemiJoined(tids[0])))
    {
        return true;
    }
    return false;
}

/**
 * Return all unassigned conjuncts of the outer join referenced by right-hand side
 * table ref.
 */
public List<Expr> getUnassignedOjConjuncts(TableRef refTable)
{
    //Preconditions.checkState(ref.getJoinOp().isOuterJoin());
    List<Expr> result = new List<Expr>();
    List<ExprId> candidates = globalState_.conjunctsByOjClause[refTable.getId()];
    if (candidates == null) return result;
    foreach (ExprId conjunctId in candidates)
    {
        if (!globalState_.assignedConjuncts.Contains(conjunctId))
        {
            Expr e = globalState_.conjuncts[conjunctId];
            //Preconditions.checkNotNull(e);
            result.Add(e);
        }
    }
    return result;
}

/**
 * Return rhs ref of last Join clause that outer-joined id.
 */
public TableRef getLastOjClause(TupleId id)
{
    return globalState_.outerJoinedTupleIds[id];
}

/**
 * Return slot descriptor corresponding to column referenced in the context of
 * tupleDesc, or null if no such reference exists.
 */
public SlotDescriptor getColumnSlot(TupleDescriptor tupleDesc, Column col)
{
    foreach (SlotDescriptor slotDesc in tupleDesc.GetSlots)
    {
        if (slotDesc.getColumn() == col) return slotDesc;
    }
    return null;
}

public DescriptorTable getDescTbl() { return globalState_.descTbl; }
public FeCatalog getCatalog() { return globalState_.stmtTableCache.catalog; }
public StmtTableCache getStmtTableCache() { return globalState_.stmtTableCache; }
public HashSet<String> getAliases() { return new HashSet<string>(aliasMap_.Keys); }

/**
 * Returns list of candidate equi-join conjuncts to be evaluated by the join node
 * that is specified by the table ref ids of its left and right children.
 * If the join to be performed is an outer join, then only equi-join conjuncts
 * from its On-clause are returned. If an equi-join conjunct is full outer joined,
 * then it is only added to the result if this join is the one to full-outer join it.
 */
public List<Expr> getEqJoinConjuncts(List<TupleId> lhsTblRefIds,
    List<TupleId> rhsTblRefIds)
{
    // Contains all equi-join conjuncts that have one child fully bound by one of the
    // rhs table ref ids (the other child is not bound by that rhs table ref id).
    List<ExprId> conjunctIds = new List<ExprId>();
    foreach (TupleId rhsId in rhsTblRefIds)
    {
        List<ExprId> cids = globalState_.eqJoinConjuncts[rhsId];
        if (cids == null) continue;
        foreach (ExprId eid in cids)
        {
            if (!conjunctIds.Contains(eid)) conjunctIds.Add(eid);
        }
    }

    // Since we currently prevent join re-reordering across outer joins, we can never
    // have a bushy outer join with multiple rhs table ref ids. A busy outer join can
    // only be constructed with an inline view (which has a single table ref id).
    List<ExprId> ojClauseConjuncts = null;
    if (rhsTblRefIds.Count == 1)
    {
        ojClauseConjuncts = globalState_.conjunctsByOjClause[rhsTblRefIds[0]];
    }

    // List of table ref ids that the join node will 'materialize'.
    List<TupleId> nodeTblRefIds = new List<int>(lhsTblRefIds);
    nodeTblRefIds.AddRange(rhsTblRefIds);
    List<Expr> result = new List<Expr>();
    foreach (ExprId conjunctId in conjunctIds)
    {
        Expr e = globalState_.conjuncts[conjunctId];
        //Preconditions.checkState(e != null);
        if (!canEvalFullOuterJoinedConjunct(e, nodeTblRefIds) ||
            !canEvalAntiJoinedConjunct(e, nodeTblRefIds) ||
            !canEvalOuterJoinedConjunct(e, nodeTblRefIds))
        {
            continue;
        }

        if (ojClauseConjuncts != null && !ojClauseConjuncts.Contains(conjunctId)) continue;
        result.Add(e);
    }
    return result;
}

/**
 * Returns false if 'e' references a full outer joined tuple and it is incorrect to
 * evaluate 'e' at a node materializing 'tids'. Returns true otherwise.
 */
public bool canEvalFullOuterJoinedConjunct(Expr e, List<TupleId> tids)
{
    TableRef fullOjRef = getFullOuterJoinRef(e);
    if (fullOjRef == null) return true;
    // 'ojRef' represents the outer-join On-clause that 'e' originates from (if any).
    // Might be the same as 'fullOjRef'. If different from 'fullOjRef' it means that
    // 'e' should be assigned to the node materializing the 'ojRef' tuple ids.
    TableRef ojRef = getOjRef(e);
    TableRef targetRef = (ojRef != null && ojRef != fullOjRef) ? ojRef  in fullOjRef;
    return tids.Contains(targetRef.getAllTableRefIds());
}

/**
 * Returns false if 'e' originates from an outer-join On-clause and it is incorrect to
 * evaluate 'e' at a node materializing 'tids'. Returns true otherwise.
 */
public bool canEvalOuterJoinedConjunct(Expr e, List<TupleId> tids)
{
    TableRef outerJoin = getOjRef(e);
    if (outerJoin == null) return true;
    return tids.Contains(outerJoin.getAllTableRefIds());
}

/**
 * Returns true if predicate 'e' can be correctly evaluated by a tree materializing
 * 'tupleIds', otherwise false in
 * - The predicate needs to be bound by tupleIds.
 * - For On-clause predicates in
 *   - If the predicate is from an anti-join On-clause it must be evaluated by the
 *     corresponding anti-join node.
 *   - Predicates from the On-clause of an inner or semi join are evaluated at the
 *     node that materializes the required tuple ids, unless they reference outer
 *     joined tuple ids. In that case, the predicates are evaluated at the join node
 *     of the corresponding On-clause.
 *   - Predicates referencing full-outer joined tuples are assigned at the originating
 *     join if it is a full-outer join, otherwise at the last full-outer join that does
 *     not materialize the table ref ids of the originating join.
 *   - Predicates from the On-clause of a left/right outer join are assigned at
 *     the corresponding outer join node with the exception of simple predicates
 *     that only reference a single tuple id. Those may be assigned below the
 *     outer join node if they are from the same On-clause that makes the tuple id
 *     nullable.
 * - Otherwise, a predicate can only be correctly evaluated if for all outer-joined
 *   referenced tids the last join to outer-join this tid has been materialized.
 */
public bool canEvalPredicate(List<TupleId> tupleIds, Expr e)
{
    if (!e.isBoundByTupleIds(tupleIds)) return false;
    List<TupleId> tids = new List<int>();
    e.getIds(tids, null);
    if (tids.IsNullOrEmpty()) return true;

    if (e.isOnClauseConjunct())
    {
        if (isAntiJoinedConjunct(e)) return canEvalAntiJoinedConjunct(e, tupleIds);

        if (isIjConjunct(e) || isSjConjunct(e))
        {
            if (!containsOuterJoinedTid(tids)) return true;
            // If the predicate references an outer-joined tuple, then evaluate it at
            // the join that the On-clause belongs to.
            TableRef onClauseTableRef = null;
            if (isIjConjunct(e))
            {
                onClauseTableRef = globalState_.ijClauseByConjunct[e.getId()];
            }
            else
            {
                onClauseTableRef = globalState_.sjClauseByConjunct[e.getId()];
            }
            //Preconditions.checkNotNull(onClauseTableRef);
            return tupleIds.Contains(onClauseTableRef.getAllTableRefIds());
        }

        if (isFullOuterJoined(e)) return canEvalFullOuterJoinedConjunct(e, tupleIds);
        if (isOjConjunct(e))
        {
            // Force this predicate to be evaluated by the corresponding outer join node.
            // The join node will pick up the predicate later via getUnassignedOjConjuncts().
            if (tids.Count > 1) return false;
            // Optimization for single-tid predicates in Legal to assign below the outer join
            // if the predicate is from the same On-clause that makes tid nullable
            // (otherwise e needn't be true when that tuple is set).
            TupleId tid = tids[0];
            return globalState_.ojClauseByConjunct[e.getId()] == getLastOjClause(tid);
        }

        // Should have returned in one of the cases above.
        //Preconditions.checkState(false);
    }

    foreach (TupleId tid in tids)
    {
        TableRef rhsRef = getLastOjClause(tid);
        // Ignore 'tid' because it is not outer-joined.
        if (rhsRef == null) continue;
        // Check whether the last join to outer-join 'tid' is materialized by tupleIds.
        if (!tupleIds.Contains(rhsRef.getAllTableRefIds())) return false;
    }
    return true;
}

/**
 * Checks if a conjunct from the On-clause of an anti join can be evaluated in a node
 * that materializes a given list of tuple ids.
 */
public bool canEvalAntiJoinedConjunct(Expr e, List<TupleId> nodeTupleIds)
{
    TableRef antiJoinRef = getAntiJoinRef(e);
    if (antiJoinRef == null) return true;
    List<TupleId> tids = new List<int>();
    e.getIds(tids, null);
    if (tids.Count > 1)
    {
        return nodeTupleIds.Contains(antiJoinRef.getAllTableRefIds())
            && antiJoinRef.getAllTableRefIds().containsAll(nodeTupleIds);
    }
    // A single tid conjunct that is anti-joined can be safely assigned to a
    // node below the anti join that specified it.
    return globalState_.semiJoinedTupleIds.ContainsKey(tids[0]);
}

/**
 * Returns a list of predicates that are fully bound by destTid. The generated
 * predicates are for optimization purposes and not required for query correctness.
 * It is up to the caller to decide if a bound predicate should actually be used.
 * Predicates are derived by replacing the slots of a source predicate with slots of
 * the destTid, if every source slot has a value transfer to a slot in destTid.
 * In particular, the returned list Contains predicates that must be evaluated
 * at a join node (bound to outer-joined tuple) but can also be safely evaluated by a
 * plan node materializing destTid. Such predicates are not marked as assigned.
 * All other inferred predicates are marked as assigned if 'markAssigned'
 * is true. This function returns bound predicates regardless of whether the source
 * predicates have been assigned.
 * Destination slots in destTid can be ignored by passing them in ignoreSlots.
 * Some bound predicates may be missed due to errors in backend expr evaluation
 * or expr substitution.
 * TODO in exclude UDFs from predicate propagation? their overloaded variants could
 * have very different semantics
 */
public List<Expr> getBoundPredicates(TupleId destTid, HashSet<SlotId> ignoreSlots,
    bool markAssigned)
{
    List<Expr> result = new List<Expr>();
    foreach (ExprId srcConjunctId in globalState_.singleTidConjuncts)
    {
        Expr srcConjunct = globalState_.conjuncts[srcConjunctId];
        if (srcConjunct is SlotRef) continue;
    //Preconditions.checkNotNull(srcConjunct);
    List<TupleId> srcTids = new List<int>();
    List<SlotId> srcSids = new List<SlotId>();
    srcConjunct.getIds(srcTids, srcSids);
    //Preconditions.checkState(srcTids.Count == 1);

    // Generate slot-mappings to bind srcConjunct to destTid.
    TupleId srcTid = srcTids[0];
    List<List<SlotId>> allDestSids =
        getValueTransferDestSlotIds(srcTid, srcSids, destTid, ignoreSlots);
    if (allDestSids.IsNullOrEmpty()) continue;

    // Indicates whether there is value transfer from the source slots to slots that
    // belong to an outer-joined tuple.
    bool hasOuterJoinedTuple = hasOuterJoinedValueTransferTarget(srcSids);

    // It is incorrect to propagate predicates into a plan subtree that is on the
    // nullable side of an outer join if the predicate evaluates to true when all
    // its referenced tuples are NULL. For example in
    // select * from (select A.a, B.b, B.col from A left join B on A.a=B.b) v
    // where v.col is null
    // In this query (v.col is null) should not be evaluated at the scanner of B.
    // The check below is conservative because the outer-joined tuple making
    // 'hasOuterJoinedTuple' true could be in a parent block of 'srcConjunct', in which
    // case it is safe to propagate 'srcConjunct' within child blocks of the
    // outer-joined parent block.
    // TODO in Make the check precise by considering the blocks (analyzers) where the
    // outer-joined tuples in the dest slot's equivalence classes appear
    // relative to 'srcConjunct'.
    try
    {
        if (hasOuterJoinedTuple && isTrueWithNullSlots(srcConjunct)) continue;
    }
    catch (Exception )
    {
        // Expr evaluation failed in the backend. Skip 'srcConjunct' since we cannot
        // determine whether propagation is safe.
        //LOG.warn("Skipping propagation of conjunct because backend evaluation failed in "
        //    + srcConjunct.toSql(), e);
        continue;
    }

    // if srcConjunct comes out of an OJ's On clause, we need to make sure it's the
    // same as the one that makes destTid nullable
    // (otherwise srcConjunct needn't be true when destTid is set)
    if (globalState_.ojClauseByConjunct.ContainsKey(srcConjunct.getId()))
    {
        if (!globalState_.outerJoinedTupleIds.ContainsKey(destTid)) continue;
        if (globalState_.ojClauseByConjunct[srcConjunct.getId()]
            != globalState_.outerJoinedTupleIds[destTid])
        {
            continue;
        }
        // Do not propagate conjuncts from the on-clause of full-outer or anti-joins.
        TableRef tblRef = globalState_.ojClauseByConjunct[srcConjunct.getId()];
        if (tblRef.getJoinOp().isFullOuterJoin()) continue;
    }

    // Conjuncts specified in the ON-clause of an anti-join must be evaluated at that
    // join node.
    if (isAntiJoinedConjunct(srcConjunct)) continue;

    // Generate predicates for all src-to-dest slot mappings.
    foreach (List<SlotId> destSids in allDestSids)
    {
        //Preconditions.checkState(destSids.Count == srcSids.Count);
        Expr p;
        if (srcSids.Contains(destSids))
        {
            p = srcConjunct;
        }
        else
        {
            ExprSubstitutionMap smap = new ExprSubstitutionMap();
            for (int i = 0; i < srcSids.Count; ++i)
            {
                smap.put(
                    new SlotRef(globalState_.descTbl.getSlotDesc(srcSids[i])),
                    new SlotRef(globalState_.descTbl.getSlotDesc(destSids[i])));
            }
            try
            {
                p = srcConjunct.trySubstitute(smap, this, false);
            }
            catch (Exception)
            {
                // not an executable predicate; ignore
                continue;
            }
            // Unset the id because this bound predicate itself is not registered, and
            // to prevent callers from inadvertently marking the srcConjunct as assigned.
            p.setId(null);
            if (p is BinaryPredicate) ((BinaryPredicate)p).setIsInferred();
    //if (LOG.isTraceEnabled())
    //{
    //    LOG.trace("new pred in " + p.toSql() + " " + p.debugString());
    //}
}

        if (markAssigned) {
          // predicate assignment doesn't hold if in
          // - the application against slotId doesn't transfer the value back to its
          //   originating slot
          // - the original predicate is on an OJ'd table but doesn't originate from
          //   that table's OJ clause's ON clause (if it comes from anywhere but that
          //   ON clause, it needs to be evaluated directly by the join node that
          //   materializes the OJ'd table)
          bool reverseValueTransfer = true;
          for (int i = 0; i<srcSids.Count; ++i) {
            if (!hasValueTransfer(destSids[i], srcSids[i])) {
              reverseValueTransfer = false;
              break;
            }
          }

          // IMPALA-2018/4379 in Check if srcConjunct or the generated predicate need to
          // be evaluated again at a later point in the plan, e.g., by a join that makes
          // referenced tuples nullable. The First condition is conservative but takes
          // into account that On-clause conjuncts can sometimes be legitimately assigned
          // below their originating join.
          bool evalAfterJoin =
              (hasOuterJoinedTuple && !srcConjunct.isOnClauseConjunct_)
              || (evalAfterJoin(srcConjunct)
                  && (globalState_.ojClauseByConjunct[srcConjunct.getId()]
                    != globalState_.outerJoinedTupleIds[srcTid]))
              || (evalAfterJoin(p)
                  && (globalState_.ojClauseByConjunct[p.getId()]
                    != globalState_.outerJoinedTupleIds[destTid]));

          // mark all bound predicates including duplicate ones
          if (reverseValueTransfer && !evalAfterJoin) markConjunctAssigned(srcConjunct);
        }

        // check if we already created this predicate
        if (!result.Contains(p)) result.Add(p);
      }
    }
    return result;
  }

  public List<Expr> getBoundPredicates(TupleId destTid)
{
    return getBoundPredicates(destTid, new HashSet<SlotId>(), true);
}

/**
 * Returns true if any of the given slot ids or their value-transfer targets belong
 * to an outer-joined tuple.
 */
public bool hasOuterJoinedValueTransferTarget(List<SlotId> sids)
{
    foreach (SlotId srcSid in sids)
    {
        foreach (SlotId dstSid in getValueTransferTargets(srcSid))
        {
            if (isOuterJoined(getTupleId(dstSid))) return true;
        }
    }
    return false;
}

/**
 * For each slot equivalence class, adds/removes predicates from conjuncts such that it
 * Contains a minimum set of <lhsSlot> = <rhsSlot> predicates that establish the known
 * equivalences between slots in lhsTids and rhsTids which must be disjoint. Preserves
 * original conjuncts when possible. Assumes that predicates for establishing
 * equivalences among slots in only lhsTids and only rhsTids have already been
 * established. This function adds the remaining predicates to "connect" the disjoint
 * equivalent slot sets of lhsTids and rhsTids.
 * The intent of this function is to enable construction of a minimum spanning tree
 * to cover the known slot equivalences. This function should be called for join
 * nodes during plan generation to (1) Remove redundant join predicates, and (2)
 * establish equivalences among slots materialized at that join node.
 * TODO in Consider optimizing for the cheapest minimum set of predicates.
 * TODO in Consider caching the DisjointSet during plan generation instead of
 * re-creating it here on every invocation.
 */
public void createEquivConjuncts(List<TupleId> lhsTids,
    List<TupleId> rhsTids, List<BinaryPredicate> conjuncts)
{
    //Preconditions.checkState(Collections.disjoint(lhsTids, rhsTids));
    // A map from equivalence class IDs to equivalence classes. The equivalence classes
    // only contain slots in lhsTids/rhsTids.
    Dictionary<int, List<SlotId>> lhsEquivClasses = getEquivClassesOnTuples(lhsTids);
    Dictionary<int, List<SlotId>> rhsEquivClasses = getEquivClassesOnTuples(rhsTids);

    // Maps from a slot id to its set of equivalent slots. Used to track equivalences
    // that have been established by predicates assigned/generated to plan nodes
    // materializing lhsTids as well as the given conjuncts.
    DisjointSet<SlotId> partialEquivSlots = new DisjointSet<SlotId>();
    // Add the partial equivalences to the partialEquivSlots map. The equivalent-slot
    // sets of slots from lhsTids are disjoint from those of slots from rhsTids.
    // We need to 'connect' the disjoint slot sets by constructing a new predicate
    // for each equivalence class (unless there is already one in 'conjuncts').
    foreach (List<SlotId> partialEquivClass in lhsEquivClasses.values())
    {
        partialEquivSlots.bulkUnion(partialEquivClass);
    }
    foreach (List<SlotId> partialEquivClass in rhsEquivClasses.values())
    {
        partialEquivSlots.bulkUnion(partialEquivClass);
    }

    // Set of outer-joined slots referenced by conjuncts.
    HashSet<SlotId> outerJoinedSlots = new HashSet<SlotId>();

    // Update partialEquivSlots based on equality predicates in 'conjuncts'. Removes
    // redundant conjuncts, unless they reference outer-joined slots (see below).
    IEnumerator<BinaryPredicate> conjunctIter = conjuncts.GetEnumerator();
    while (conjunctIter.MoveNext())
    {
        Expr conjunct = conjunctIter.Current;
        Pair<SlotId, SlotId> eqSlots = BinaryPredicate.getEqSlots(conjunct);
        if (eqSlots == null) continue;
        int firstEqClassId = getEquivClassId(eqSlots.First);
        int secondEqClassId = getEquivClassId(eqSlots.Second);
        // slots may not be in the same eq class due to outer joins
        if (firstEqClassId != secondEqClassId) continue;

        // Retain an otherwise redundant predicate if it references a slot of an
        // outer-joined tuple that is not already referenced by another join predicate
        // to maintain that the rows must satisfy outer-joined-slot IS NOT NULL
        // (otherwise NULL tuples from outer joins could survive).
        // TODO in Consider better fixes for outer-joined slots in (1) Create IS NOT NULL
        // predicates and place them at the lowest possible plan node. (2) Convert outer
        // joins into inner joins (or full outer joins into left/right outer joins).
        bool filtersOuterJoinNulls = false;
        if (isOuterJoined(eqSlots.First)
            && lhsTids.Contains(getTupleId(eqSlots.First))
            && !outerJoinedSlots.Contains(eqSlots.First))
        {
            outerJoinedSlots.Add(eqSlots.First);
            filtersOuterJoinNulls = true;
        }
        if (isOuterJoined(eqSlots.Second)
            && lhsTids.Contains(getTupleId(eqSlots.Second))
            && !outerJoinedSlots.Contains(eqSlots.Second))
        {
            outerJoinedSlots.Add(eqSlots.Second);
            filtersOuterJoinNulls = true;
        }
        // retain conjunct if it connects two formerly unconnected equiv classes or
        // it is required for outer-join semantics
        if (!partialEquivSlots.union(eqSlots.First, eqSlots.Second)
            && !filtersOuterJoinNulls)
        {
            conjunctIter.Remove();
        }
    }

    // For each equivalence class, construct a new predicate to 'connect' the disjoint
    // slot sets.
    foreach (KeyValuePair<int, List<SlotId>> rhsEquivClass in
      rhsEquivClasses)
    {
        List<SlotId> lhsSlots = lhsEquivClasses[rhsEquivClass.Key];
        if (lhsSlots == null) continue;
        List<SlotId> rhsSlots = rhsEquivClass.Value;
        //Preconditions.checkState(!lhsSlots.IsNullOrEmpty() && !rhsSlots.IsNullOrEmpty());

        if (!partialEquivSlots.union(lhsSlots[0], rhsSlots[0])) continue;
        // Do not create a new predicate from slots that are full outer joined because that
        // predicate may be incorrectly assigned to a node below the associated full outer
        // join.
        if (!isFullOuterJoined(lhsSlots[0]) && !isFullOuterJoined(rhsSlots[0]))
        {
            conjuncts.Add(createInferredEqPred(lhsSlots[0], rhsSlots[0]));
        }
    }
}

/**
 * For each slot equivalence class, adds/removes predicates from conjuncts such that it
 * Contains a minimum set of <slot> = <slot> predicates that establish the known
 * equivalences between slots belonging to tid. Preserves original
 * conjuncts when possible.
 * The intent of this function is to enable construction of a minimum spanning tree
 * to cover the known slot equivalences. This function should be called to Add
 * conjuncts to plan nodes that materialize a new tuple, e.g., scans and aggregations.
 * Does not enforce equivalence between slots in ignoreSlots. Equivalences (if any)
 * among slots in ignoreSlots are assumed to have already been enforced.
 * TODO in Consider optimizing for the cheapest minimum set of predicates.
 */
public  void createEquivConjuncts<T>(TupleId tid, List<T> conjuncts,
    HashSet<SlotId> ignoreSlots)
{
    // Maps from a slot id to its set of equivalent slots. Used to track equivalences
    // that have been established by 'conjuncts' and the 'ignoredsSlots'.
    DisjointSet<SlotId> partialEquivSlots = new DisjointSet<SlotId>();

    // Treat ignored slots as already connected. Add the ignored slots at this point
    // such that redundant conjuncts are removed.
    partialEquivSlots.bulkUnion(ignoreSlots);
    partialEquivSlots.checkConsistency();

    // Update partialEquivSlots based on equality predicates in 'conjuncts'. Removes
    // redundant conjuncts, unless they reference outer-joined slots (see below).
    IEnumerator<T> conjunctIter = conjuncts.GetEnumerator();
    while (conjunctIter.MoveNext())
    {
        T conjunct = conjunctIter.Current;
        Pair<SlotId, SlotId> eqSlots = BinaryPredicate.getEqSlots(conjunct);
        if (eqSlots == null) continue;
        int firstEqClassId = getEquivClassId(eqSlots.First);
        int secondEqClassId = getEquivClassId(eqSlots.Second);
        // slots may not be in the same eq class due to outer joins
        if (firstEqClassId != secondEqClassId) continue;
        // update equivalences and Remove redundant conjuncts
        if (!partialEquivSlots.union(eqSlots.First, eqSlots.Second)) conjunctIter.Remove();
    }
    // Suppose conjuncts had these predicates belonging to equivalence classes e1 and e2 in
    // e1 in s1 = s2, s3 = s4, s3 = s5
    // e2 in s10 = s11
    // The conjunctsEquivSlots should contain the following entries at this point in
    // s1 -> {s1, s2}
    // s2 -> {s1, s2}
    // s3 -> {s3, s4, s5}
    // s4 -> {s3, s4, s5}
    // s5 -> {s3, s4, s5}
    // s10 -> {s10, s11}
    // s11 -> {s10, s11}
    // Assuming e1 = {s1, s2, s3, s4, s5} we need to generate one additional equality
    // predicate to "connect" {s1, s2} and {s3, s4, s5}.

    // These are the equivalences that need to be established by constructing conjuncts
    // to form a minimum spanning tree.
    Dictionary<int, List<SlotId>> targetEquivClasses =
        getEquivClassesOnTuples(new List<int>(tid));
    foreach (KeyValuePair<int, List<SlotId>> targetEquivClass in
      targetEquivClasses)
    {
        // Loop over all pairs of equivalent slots and merge their disjoint slots sets,
        // creating missing equality predicates as necessary.
        List<SlotId> slotIds = targetEquivClass.Value;
        bool done = false;
        for (int i = 1; i < slotIds.Count; ++i)
        {
            SlotId rhs = slotIds[i];
            for (int j = 0; j < i; ++j)
            {
                SlotId lhs = slotIds[j];
                if (!partialEquivSlots.union(lhs, rhs)) continue;
                conjuncts.Add((T)createInferredEqPred(lhs, rhs));
                // Check for early termination.
                if (partialEquivSlots.get(lhs).Count == slotIds.Count)
                {
                    done = true;
                    break;
                }
            }
            if (done) break;
        }
    }
}

public  void createEquivConjuncts<T>(TupleId tid, List<T> conjuncts)
{
    createEquivConjuncts(tid, conjuncts, new HashSet<SlotId>());
}

/**
 * Returns a map of slot equivalence classes on the set of slots in the given tuples.
 * Only Contains equivalence classes with more than one member.
 */
private Dictionary<int, List<SlotId>> getEquivClassesOnTuples(List<TupleId> tids)
{
    Dictionary<int, List<SlotId>> result = new Dictionary<int, List<SlotId>>();
    SccCondensedGraph g = globalState_.valueTransferGraph;
    foreach (TupleId tid in tids)
    {
        foreach (SlotDescriptor slotDesc in getTupleDesc(tid).GetSlots)
        {
            if (slotDesc.getId() >= g.numVertices()) continue;
            int sccId = g.sccId(slotDesc.getId());
            // Ignore equivalence classes that are empty or only have a single member.
            if (g.sccMembersBySccId(sccId).Length <= 1) continue;
            List<SlotId> slotIds = result[sccId];
            if (slotIds == null)
            {
                slotIds = new List<SlotId>();
                result[sccId]= slotIds;
            }
            slotIds.Add(slotDesc.getId());
        }
    }
    return result;
}

/**
 * Returns a list of slot mappings from srcTid to destTid for the purpose of predicate
 * propagation. Each mapping assigns every slot in srcSids to a slot in destTid which
 * has a value transfer from srcSid. Does not generate all possible mappings, but limits
 * the results to useful and/or non-redundant mappings, i.e., those mappings that would
 * improve the performance of query execution.
 */
private List<List<SlotId>> getValueTransferDestSlotIds(TupleId srcTid,
    List<SlotId> srcSids, TupleId destTid, HashSet<SlotId> ignoreSlots)
{
    List<List<SlotId>> allDestSids = new List<List<SlotId>>();
    TupleDescriptor destTupleDesc = getTupleDesc(destTid);
    if (srcSids.Count == 1)
    {
        // Generate all mappings to propagate predicates of the form <slot> <op> <constant>
        // to as many destination slots as possible.
        // TODO in If srcTid == destTid we could limit the mapping to partition
        // columns because mappings to non-partition columns do not provide
        // a performance benefit.
        SlotId srcSid = srcSids[0];
        foreach (SlotDescriptor destSlot in destTupleDesc.GetSlots)
        {
            if (ignoreSlots.Contains(destSlot.getId())) continue;
            if (hasValueTransfer(srcSid, destSlot.getId()))
            {
                allDestSids.Add(new List<SlotId>(destSlot.getId()));
            }
        }
    }
    else if (srcTid.Equals(destTid))
    {
        // Multiple source slot ids and srcTid == destTid. Inter-tuple transfers are
        // already expressed by the original conjuncts. Any mapping would be redundant.
        // Still Add srcSids to the result because we rely on getBoundPredicates() to
        // include predicates that can safely be evaluated below an outer join, but must
        // also be evaluated by the join itself (evalByJoin() == true).
        allDestSids.Add(srcSids);
    }
    else
    {
        // Multiple source slot ids and srcTid != destTid. Pick the First mapping
        // where each srcSid is mapped to a different destSid to avoid generating
        // redundant and/or trivial predicates.
        // TODO in This approach is not guaranteed to find the best slot mapping
        // (e.g., against partition columns) or all non-redundant mappings.
        // The limitations are show in predicate-propagation.test.
        List<SlotId> destSids = new List<SlotId>();
        foreach (SlotId srcSid in srcSids)
        {
            foreach (SlotDescriptor destSlot in destTupleDesc.GetSlots)
            {
                if (ignoreSlots.Contains(destSlot.getId())) continue;
                if (hasValueTransfer(srcSid, destSlot.getId())
                    && !destSids.Contains(destSlot.getId()))
                {
                    destSids.Add(destSlot.getId());
                    break;
                }
            }
        }
        if (destSids.Count == srcSids.Count) allDestSids.Add(destSids);
    }
    return allDestSids;
}

/**
 * Returns true if 'p' evaluates to true when all its referenced slots are NULL,
 * returns false otherwise. Throws if backend expression evaluation fails.
 */
public bool isTrueWithNullSlots(Expr p) 
{
    // Construct predicate with all SlotRefs substituted by NullLiterals.
    List<SlotRef> slotRefs = new List<SlotRef>();

    //TODO - adapt call to c#
    //p.collect(Predicates.instanceOf(SlotRef.class), slotRefs);

    // Dictionary for substituting SlotRefs with NullLiterals.
    ExprSubstitutionMap nullSmap = new ExprSubstitutionMap();
    foreach (SlotRef slotRef in slotRefs) {
        // Preserve the original SlotRef type to ensure all substituted
        // subexpressions in the predicate have the same return type and
        // function signature as in the original predicate.
        nullSmap.put(slotRef.clone(), NullLiteral.create(slotRef.getType()));
    }
    Expr nullTuplePred = p.substitute(nullSmap, this, false);
    return FeSupport.EvalPredicate(nullTuplePred, getQueryCtx());
  }

  public TupleId getTupleId(SlotId slotId)
{
    return globalState_.descTbl.getSlotDesc(slotId).getParent().getId();
}

public void registerValueTransfer(SlotId id1, SlotId id2)
{
    globalState_.registeredValueTransfers.Add(new Pair(id1, id2));
}

public bool isOuterJoined(TupleId tid)
{
    return globalState_.outerJoinedTupleIds.ContainsKey(tid);
}

public bool isOuterJoined(SlotId sid)
{
    return isOuterJoined(getTupleId(sid));
}

public bool isSemiJoined(TupleId tid)
{
    return globalState_.semiJoinedTupleIds.ContainsKey(tid);
}

public bool isAntiJoinedConjunct(Expr e)
{
    return getAntiJoinRef(e) != null;
}

public TableRef getAntiJoinRef(Expr e)
{
    TableRef tblRef = globalState_.sjClauseByConjunct[e.getId()];
    if (tblRef == null) return null;
    return (tblRef.getJoinOp().isAntiJoin()) ? tblRef  : null;
}

public bool isFullOuterJoined(TupleId tid)
{
    return globalState_.fullOuterJoinedTupleIds.ContainsKey(tid);
}

public bool isFullOuterJoined(SlotId sid)
{
    return isFullOuterJoined(getTupleId(sid));
}

public bool isVisible(TupleId tid)
{
    return tid == visibleSemiJoinedTupleId_ || !isSemiJoined(tid);
}

public bool containsOuterJoinedTid(List<TupleId> tids)
{
    foreach (TupleId tid in tids)
    {
        if (isOuterJoined(tid)) return true;
    }
    return false;
}

/**
 * Compute the value transfer graph based on the registered value transfers and eq-join
 * predicates.
 */
public void computeValueTransferGraph()
{
    WritableGraph directValueTransferGraph =
        new WritableGraph(globalState_.descTbl.getMaxSlotId() + 1);
    constructValueTransfersFromEqPredicates(directValueTransferGraph);
    foreach (Pair<SlotId, SlotId> p  in globalState_.registeredValueTransfers)
    {
        directValueTransferGraph.addEdge(p.First, p.Second);
    }
    globalState_.valueTransferGraph =
        SccCondensedGraph.condensedReflexiveTransitiveClosure(directValueTransferGraph);
    // Validate the value-transfer graph in single-node planner tests.
    if (RuntimeEnv.INSTANCE.isTestEnv() && getQueryOptions().num_nodes == 1)
    {
        RandomAccessibleGraph reference =
            directValueTransferGraph.toRandomAccessible().reflexiveTransitiveClosure();
        if (!globalState_.valueTransferGraph.validate(reference))
        {
            String tc = reference.print();
            String condensedTc = globalState_.valueTransferGraph.print();
            throw new InvalidOperationException("Condensed transitive closure doesn't equal to "
                + "uncondensed transitive closure. Uncondensed Graph in\n" + tc +
                "\nCondensed Graph in\n" + condensedTc);
        }
    }
}

/**
 * Add value-transfer edges to 'g' based on the registered equi-join conjuncts.
 */
private void constructValueTransfersFromEqPredicates(WritableGraph g)
{
    foreach (ExprId id  in globalState_.conjuncts.Values)
    {
        Expr e = globalState_.conjuncts[id];
        Pair<SlotId, SlotId> slotIds = BinaryPredicate.getEqSlots(e);
        if (slotIds == null) continue;

        TableRef sjTblRef = globalState_.sjClauseByConjunct[id];
        //Preconditions.checkState(sjTblRef == null || sjTblRef.getJoinOp().isSemiJoin());
        bool isAntiJoin = sjTblRef != null && sjTblRef.getJoinOp().isAntiJoin();

        TableRef ojTblRef = globalState_.ojClauseByConjunct[id];
        //Preconditions.checkState(ojTblRef == null || ojTblRef.getJoinOp().isOuterJoin());
        if (ojTblRef == null && !isAntiJoin)
        {
            // this eq predicate doesn't involve any outer or anti join, ie, it is true for
            // each result row;
            // value transfer is not legal if the receiving slot is in an enclosed
            // scope of the source slot and the receiving slot's block has a limit
            Analyzer firstBlock = globalState_.blockBySlot[slotIds.First];
            Analyzer secondBlock = globalState_.blockBySlot[slotIds.Second];
            //if (LOG.isTraceEnabled())
            //{
            //    LOG.trace("value transfer in from " + slotIds.First.ToString());
            //}
            if (!(secondBlock.hasLimitOffsetClause_ &&
                secondBlock.ancestors_.Contains(firstBlock)))
            {
                g.addEdge(slotIds.First, slotIds.Second);
            }
            if (!(firstBlock.hasLimitOffsetClause_ &&
                firstBlock.ancestors_.Contains(secondBlock)))
            {
                g.addEdge(slotIds.Second, slotIds.First);
            }
            continue;
        }
        // Outer or semi-joined table ref.
        TableRef tblRef = (ojTblRef != null) ? ojTblRef  : sjTblRef;
        //Preconditions.checkNotNull(tblRef);

        if (tblRef.getJoinOp() == SqlEnumerations.TJoinOp.FULL_OUTER_JOIN)
        {
            // full outer joins don't guarantee any value transfer
            continue;
        }

        // this is some form of outer or anti join
        SlotId outerSlot, innerSlot;
        if (tblRef.getId() == getTupleId(slotIds.First))
        {
            innerSlot = slotIds.First;
            outerSlot = slotIds.Second;
        }
        else if (tblRef.getId() == getTupleId(slotIds.Second))
        {
            innerSlot = slotIds.Second;
            outerSlot = slotIds.First;
        }
        else
        {
            // this eq predicate is part of an OJ/AJ clause but doesn't reference
            // the joined table -> ignore this, we can't reason about when it'll
            // actually be true
            continue;
        }
        // value transfer is always legal because the outer and inner slot must come from
        // the same block; transitive value transfers into inline views with a limit are
        // prevented because the inline view's aux predicates won't transfer values into
        // the inline view's block (handled in the 'tableRef == null' case above)
        // TODO in We could propagate predicates into anti-joined plan subtrees by
        // inverting the condition (paying special attention to NULLs).
        if (tblRef.getJoinOp() == SqlEnumerations.TJoinOp.LEFT_OUTER_JOIN
            || tblRef.getJoinOp() == SqlEnumerations.TJoinOp.LEFT_ANTI_JOIN
            || tblRef.getJoinOp() == SqlEnumerations.TJoinOp.NULL_AWARE_LEFT_ANTI_JOIN)
        {
            g.addEdge(outerSlot, innerSlot);
        }
        else if (tblRef.getJoinOp() == SqlEnumerations.TJoinOp.RIGHT_OUTER_JOIN
          || tblRef.getJoinOp() == SqlEnumerations.TJoinOp.RIGHT_ANTI_JOIN)
        {
            g.addEdge(innerSlot, outerSlot);
        }
    }
}


/**
 * Returns the equivalence class of the given slot id.
 * Time complexity in O(V) where V = number of slots
 */
public List<SlotId> getEquivClass(SlotId sid)
{
    SccCondensedGraph g = globalState_.valueTransferGraph;
    if (sid >= g.numVertices()) return Collections.singletonList(sid);
    List<SlotId> result = new List<SlotId>();
    foreach (int dst in g.sccMembersByVid(sid))
    {
        result.Add(new SlotId(dst));
    }
    return result;
}

/**
 * Returns sorted slot IDs with value transfers from 'srcSid'.
 * Time complexity in O(V) where V = number of slots
 */
public List<SlotId> getValueTransferTargets(SlotId srcSid)
{
    SccCondensedGraph g = globalState_.valueTransferGraph;
    if (srcSid >= g.numVertices()) return Collections.singletonList(srcSid);
    List<SlotId> result = new List<SlotId>();
    for (IntIterator dstIt = g.dstIter(srcSid); dstIt.hasNext(); dstIt.next())
    {
        result.Add(new SlotId(dstIt.peek()));
    }
    // Unsorted result drastically changes the runtime filter assignment and results in
    // worse plan.
    // TODO in Investigate the call sites and Remove this sort.
    result.Sort();
    return result;
}

/** Get the id of the equivalence class of the given slot. */
private int getEquivClassId(SlotId sid)
{
    SccCondensedGraph g = globalState_.valueTransferGraph;
    return sid >= g.numVertices() ?
        sid  : g.sccId(sid);
}

        /**
         * Returns whether there is a value transfer between two SlotRefs.
         * It's used for {@link Expr#matches(Expr, SlotRef.Comparator)} )}
         */
        //TODO - translate to c#
        //private readonly SlotRef.Comparator VALUE_TRANSFER_SLOTREF_CMP = new SlotRef.Comparator() {
        //      @Override
        //      public bool matches(SlotRef a, SlotRef b)
        //{
        //    return hasValueTransfer(a.getSlotId(), b.getSlotId());
        //}
        //    };

        /**
         * Returns whether there is a mutual value transfer between two SlotRefs.
         * It's used for {@link Expr#matches(Expr, SlotRef.Comparator)} )}
         */
        //TODO - translate to c#
        //  private readonly SlotRef.Comparator MUTUAL_VALUE_TRANSFER_SLOTREF_CMP =
        //      new SlotRef.Comparator() {
        //        @Override
        //        public bool matches(SlotRef a, SlotRef b)
        //{
        //    return hasMutualValueTransfer(a.getSlotId(), b.getSlotId());
        //}
        //      };

        /**
         * Returns if e1 has (mutual) value transfer to e2. An expr e1 has value transfer to e2
         * if the tree structure of the two exprs are the same ignoring implicit casts, and for
         * each pair of corresponding slots there is a value transfer from the slot in e1 to the
         * slot in e2.
         */
        public bool exprsHaveValueTransfer(Expr e1, Expr e2, bool mutual)
{
    return e1.matches(e2, mutual ?
        MUTUAL_VALUE_TRANSFER_SLOTREF_CMP  in VALUE_TRANSFER_SLOTREF_CMP);
}

/**
 * Return true if two sets of exprs have (mutual) value transfer. Set l1 has value
 * transfer to set s2 there is 1-to-1 value transfer between exprs in l1 and l2.
 */
public bool setsHaveValueTransfer(List<Expr> l1, List<Expr> l2, bool mutual)
{
    l1 = Expr.removeDuplicates(l1, MUTUAL_VALUE_TRANSFER_SLOTREF_CMP);
    l2 = Expr.removeDuplicates(l2, MUTUAL_VALUE_TRANSFER_SLOTREF_CMP);
    if (l1.Count != l2.Count) return false;
    foreach (Expr e2  in l2)
    {
        bool foundInL1 = false;
        foreach (Expr e1  in l1)
        {
            if (e1.matches(e2, mutual ?
                MUTUAL_VALUE_TRANSFER_SLOTREF_CMP  : VALUE_TRANSFER_SLOTREF_CMP))
            {
                foundInL1 = true;
                break;
            }
        }
        if (!foundInL1) return false;
    }
    return true;
}

/**
 * Compute the intersection of l1 and l2. Two exprs are considered identical if they
 * have mutual value transfer. Return the intersecting l1 elements in i1 and the
 * intersecting l2 elements in i2.
 */
public void exprIntersect(List<Expr> l1, List<Expr> l2, List<Expr> i1, List<Expr> i2)
{
    i1.Clear();
    i2.Clear();
    foreach (Expr e1  in l1)
    {
        foreach (Expr e2  in l2)
        {
            if (e1.matches(e2, MUTUAL_VALUE_TRANSFER_SLOTREF_CMP))
            {
                i1.Add(e1);
                i2.Add(e2);
                break;
            }
        }
    }
}

/**
 * Mark predicates as assigned.
 */
public void markConjunctsAssigned(List<Expr> conjuncts)
{
    if (conjuncts == null) return;
    foreach (Expr p in conjuncts)
    {
        globalState_.assignedConjuncts.Add(p.getId());
    }
}

/**
 * Mark predicate as assigned.
 */
public void markConjunctAssigned(Expr conjunct)
{
    globalState_.assignedConjuncts.Add(conjunct.getId());
}

public HashSet<ExprId> getAssignedConjuncts()
{
    return new HashSet<ExprId>(globalState_.assignedConjuncts);
}

public void setAssignedConjuncts(HashSet<ExprId> assigned)
{
    globalState_.assignedConjuncts = new HashSet<ExprId>(assigned);
}

/**
 * Mark all slots that are referenced in exprs as materialized.
 */
public void materializeSlots(List<Expr> exprs)
{
    List<SlotId> slotIds = new List<SlotId>();
    foreach (Expr e in exprs)
    {
        //Preconditions.checkState(e.isAnalyzed());
        e.getIds(null, slotIds);
    }
    globalState_.descTbl.markSlotsMaterialized(slotIds);
}

public void materializeSlots(Expr e)
{
    List<SlotId> slotIds = new List<SlotId>();
    //Preconditions.checkState(e.isAnalyzed());
    e.getIds(null, slotIds);
    globalState_.descTbl.markSlotsMaterialized(slotIds);
}

/**
 * Returns assignment-compatible type of expr.getType() and lastCompatibleType.
 * If lastCompatibleType is null, returns expr.getType() (if valid).
 * If types are not compatible throws an exception reporting
 * the incompatible types and their expr.toSql().
 *
 * lastCompatibleExpr is passed for error reporting purposes,
 * but note that lastCompatibleExpr may not yet have lastCompatibleType,
 * because it was not cast yet.
 */
public SqlNodeType getCompatibleType(SqlNodeType lastCompatibleType,
    Expr lastCompatibleExpr, Expr expr)
      
{
    SqlNodeType newCompatibleType;
    if (lastCompatibleType == null) {
        newCompatibleType = expr.getType();
    } else {
        newCompatibleType = SqlNodeType.getAssignmentCompatibleType(
            lastCompatibleType, expr.getType(), false, isDecimalV2());
    }
    if (!newCompatibleType.isValid()) {
        throw new AnalysisException(String.Format(
            "Incompatible return types '%s' and '%s' of exprs '%s' and '%s'.",
            lastCompatibleType.toSql(), expr.getType().toSql(),
            lastCompatibleExpr.toSql(), expr.toSql()));
    }
    return newCompatibleType;
}

/**
 * Determines compatible type for given exprs, and casts them to compatible type.
 * Calls analyze() on each of the exprs.
 * Throws an AnalysisException if the types are incompatible,
 */
public void castAllToCompatibleType(List<Expr> exprs) 
{
    // Group all the decimal types together at the end of the list to avoid comparing
    // the decimals with each other First. For example, if we have the following list,
    // [decimal, decimal, double], we will end up casting everything to a double anyways,
    // so it does not matter if the decimals are not compatible with each other.
    //
    // We need to create a new sorted list instead of mutating it when sorting it because
    // mutating the original exprs will change the order of the original exprs.
    List<Expr> sortedExprs = new List<Expr>(exprs);
    //TODO - adapt comparator from java to c#
//    Collections.sort(sortedExprs, new Comparator<Expr>() {
//      @Override
//      public int compare(Expr expr1, Expr expr2)
//{
//    if ((expr1.getType().isDecimal() && expr2.getType().isDecimal()) ||
//        (!expr1.getType().isDecimal() && !expr2.getType().isDecimal()))
//    {
//        return 0;
//    }
//    return expr1.getType().isDecimal() ? 1  in -1;
//}
//    });
    Expr lastCompatibleExpr = sortedExprs[0];
SqlNodeType compatibleType = null;
    for (int i = 0; i<sortedExprs.Count; ++i) {
      sortedExprs[i].analyze(this);
compatibleType = getCompatibleType(compatibleType, lastCompatibleExpr,
    sortedExprs[i]);
    }
    // Add implicit casts if necessary.
    for (int i = 0; i<exprs.Count; ++i) {
      if (!exprs[i].getType().Equals(compatibleType)) {
        Expr castExpr = exprs[i].castTo(compatibleType);
exprs.set(i, castExpr);
      }
    }
  }

  /**
   * Casts the exprs in the given lists position-by-position such that for every i,
   * the i-th expr among all expr lists is compatible.
   * Throw an AnalysisException if the types are incompatible.
   */
  public void castToUnionCompatibleTypes(List<List<Expr>> exprLists)
      
{
    if (exprLists == null || exprLists.Count < 2) return;

    // Determine compatible types for exprs, position by position.
    List<Expr> firstList = exprLists[0];
    for (int i = 0; i < firstList.Count; ++i)
    {
        // SqlNodeType compatible with the i-th exprs of all expr lists.
        // Initialize with type of i-th expr in First list.
        SqlNodeType compatibleType = firstList[i].getType();
        // Remember last compatible expr for error reporting.
        Expr lastCompatibleExpr = firstList[i];
        for (int j = 1; j < exprLists.Count; ++j)
        {
            //Preconditions.checkState(exprLists[j].Count == firstList.Count);
            compatibleType = getCompatibleType(compatibleType,
                lastCompatibleExpr, exprLists[j][i]);
            lastCompatibleExpr = exprLists[j][i];
        }
        // Now that we've found a compatible type, Add implicit casts if necessary.
        for (int j = 0; j < exprLists.Count; ++j)
        {
            if (!exprLists[j][i].getType().Equals(compatibleType))
            {
                Expr castExpr = exprLists[j][i].castTo(compatibleType);
                exprLists[j][i]= castExpr;
            }
        }
    }
    }

  public String getDefaultDb() { return globalState_.queryCtx.session.database; }
public User getUser() { return user_; }
public TQueryCtx getQueryCtx() { return globalState_.queryCtx; }
public TQueryOptions getQueryOptions()
{
    return globalState_.queryCtx.client_request.getQuery_options();
}
public bool isDecimalV2() { return getQueryOptions().isDecimal_v2(); }
public AuthorizationConfig getAuthzConfig() { return globalState_.authzConfig; }
public ListMap<TNetworkAddress> getHostIndex() { return globalState_.hostIndex; }
public ColumnLineageGraph getColumnLineageGraph() { return globalState_.lineageGraph; }
public TLineageGraph getThriftSerializedLineageGraph()
{
    //Preconditions.checkNotNull(globalState_.lineageGraph);
    return globalState_.lineageGraph.toThrift();
}

public ImmutableList<PrivilegeRequest> getPrivilegeReqs()
{
    return ImmutableList.copyOf(globalState_.privilegeReqs);
}

public ImmutableList<Pair<PrivilegeRequest, String>> getMaskedPrivilegeReqs()
{
    return ImmutableList.copyOf(globalState_.maskedPrivilegeReqs);
}

/**
 * Returns a list of the successful catalog object access events. Does not include
 * accesses that failed due to AuthorizationExceptions. In general, if analysis
 * fails for any reason this list may be incomplete.
 */
public HashSet<TAccessEvent> getAccessEvents() { return globalState_.accessEvents; }
public void addAccessEvent(TAccessEvent event)
{
    globalState_.accessEvents.Add(event);
}

/**
 * Returns the Table for the given database and table name from the 'stmtTableCache'
 * in the global analysis state.
 * Throws an AnalysisException if the database or table does not exist.
 * Throws a TableLoadingException if the registered table failed to load.
 * Does not register authorization requests or access events.
 */
public FeTable getTable(String dbName, String tableName)
       {
    TableName tblName = new TableName(dbName, tableName);
FeTable table = globalState_.stmtTableCache.tables.get(tblName);
    if (table == null) {
      if (!globalState_.stmtTableCache.dbs.Contains(tblName.getDb())) {
        throw new AnalysisException(DB_DOES_NOT_EXIST_ERROR_MSG + tblName.getDb());
      } else {
        throw new AnalysisException(TBL_DOES_NOT_EXIST_ERROR_MSG + tblName.ToString());
      }
    }
    //Preconditions.checkState(table.isLoaded());
    if (table is IncompleteTable) {
      // If there were problems loading this table's metadata, throw an exception
      // when it is accessed.
      ImpalaException cause = ((IncompleteTable)table).getCause();
      if (cause is TableLoadingException) throw (TableLoadingException) cause;
      throw new TableLoadingException("Missing metadata for table in " + tableName, cause);
    }
    return table;
  }

  /**
   * Returns the Table with the given name from the 'loadedTables' map in the global
   * analysis state. Throws an AnalysisException if the table or the db does not exist.
   * Throws a TableLoadingException if the registered table failed to load.
   * Always registers privilege request(s) for the table at the given privilege level(s),
   * regardless of the state of the table (i.e. whether it exists, is loaded, etc.).
   * If addAccessEvent is true adds access event(s) for successfully loaded tables. When
   * multiple privileges are specified, all those privileges will be required for the
   * authorization check.
   */
  public FeTable getTable(TableName tableName, bool addAccessEvent,
      Privilege...privilege)  {
    //Preconditions.checkNotNull(tableName);
    //Preconditions.checkNotNull(privilege);
    tableName = getFqTableName(tableName);
    for (Privilege priv  in privilege) {
      if (priv == Privilege.ANY) {
        registerPrivReq(new PrivilegeRequestBuilder()
            .any().onAnyColumn(tableName.getDb(), tableName.getTbl()).toRequest());
      } else {
        registerPrivReq(new PrivilegeRequestBuilder()
            .allOf(priv).onTable(tableName.getDb(), tableName.getTbl()).toRequest());
      }
    }
    FeTable table = getTable(tableName.getDb(), tableName.getTbl());
//Preconditions.checkNotNull(table);
    if (addAccessEvent) {
      // Add an audit event for this access
      TCatalogObjectType objectType = TCatalogObjectType.TABLE;
      if (table is FeView) objectType = TCatalogObjectType.VIEW;
      for (Privilege priv  in privilege) {
        globalState_.accessEvents.Add(new TAccessEvent(
            tableName.ToString(), objectType, priv.ToString()));
      }
    }
    return table;
  }

  /**
   * Returns the Catalog Table object for the TableName at the given Privilege level and
   * adds an audit event if the access was successful.
   *
   * If the user does not have sufficient privileges to access the table an
   * AuthorizationException is thrown.
   * If the table or the db does not exist in the Catalog, an AnalysisError is thrown.
   */
  public FeTable getTable(TableName tableName, Privilege...privilege)
      
{
    try {
        return getTable(tableName, true, privilege);
    } catch (TableLoadingException e) {
        throw new AnalysisException(e);
    }
}

/**
 * Returns the Catalog Db object for the given database at the given
 * Privilege level. The privilege request is tracked in the analyzer
 * and authorized post-analysis.
 *
 * Registers a new access event if the catalog lookup was successful.
 *
 * If the database does not exist in the catalog an AnalysisError is thrown.
 */
public FeDb getDb(String dbName, Privilege privilege) 
{
    return getDb(dbName, privilege, true);
}

public FeDb getDb(String dbName, Privilege privilege, bool throwIfDoesNotExist)
      
{
    PrivilegeRequestBuilder pb = new PrivilegeRequestBuilder();
    if (privilege == Privilege.ANY) {
      registerPrivReq(
          pb.any().onAnyColumn(dbName, AuthorizeableTable.ANY_TABLE_NAME).toRequest());
    } else {
      registerPrivReq(pb.allOf(privilege).onDb(dbName).toRequest());
    }

    FeDb db = getDb(dbName, throwIfDoesNotExist);
globalState_.accessEvents.Add(new TAccessEvent(
    dbName, TCatalogObjectType.DATABASE, privilege.ToString()));
    return db;
  }

  /**
   * Returns a Catalog Db object without checking for privileges.
   */
  public FeDb getDb(String dbName, bool throwIfDoesNotExist)
      
{
    FeDb db = getCatalog().getDb(dbName);
    if (db == null && throwIfDoesNotExist) {
        throw new AnalysisException(DB_DOES_NOT_EXIST_ERROR_MSG + dbName);
    }
    return db;
}

/**
 * Checks if the given database Contains the given table for the given Privilege
 * level. If the table exists in the database, true is returned. Otherwise false.
 *
 * If the user does not have sufficient privileges to access the table an
 * AuthorizationException is thrown.
 * If the database does not exist in the catalog an AnalysisError is thrown.
 */
public bool dbContainsTable(String dbName, String tableName, Privilege privilege)
      
{
    registerPrivReq(new PrivilegeRequestBuilder().allOf(privilege)
        .onTable(dbName, tableName).toRequest());
    try {
      FeDb db = getCatalog().getDb(dbName);
      if (db == null) {
        throw new DatabaseNotFoundException("Database not found in " + dbName);
      }
      return db.containsTable(tableName);
    } catch (DatabaseNotFoundException e) {
      throw new AnalysisException(DB_DOES_NOT_EXIST_ERROR_MSG + dbName);
    }
  }

  /**
   * If the table name is fully qualified, the database from the TableName object will
   * be returned. Otherwise the default analyzer database will be returned.
   */
  public String getTargetDbName(TableName tableName)
{
    return tableName.isFullyQualified() ? tableName.getDb()  : getDefaultDb();
}

/**
 * Returns the fully-qualified table name of tableName. If tableName
 * is already fully qualified, returns tableName.
 */
public TableName getFqTableName(TableName tableName)
{
    if (tableName.isFullyQualified()) return tableName;
    return new TableName(getDefaultDb(), tableName.getTbl());
}

public void setMaskPrivChecks(String errMsg)
{
    maskPrivChecks_ = true;
    authErrorMsg_ = errMsg;
}

public void setEnablePrivChecks(bool value) { enablePrivChecks_ = value; }
public void setIsStraightJoin() { isStraightJoin_ = true; }
public bool isStraightJoin() { return isStraightJoin_; }
public bool isExplain() { return globalState_.isExplain; }
public void setUseHiveColLabels(bool useHiveColLabels)
{
    useHiveColLabels_ = useHiveColLabels;
}
public bool useHiveColLabels() { return useHiveColLabels_; }

public void setHasLimitOffsetClause(bool hasLimitOffset)
{
    this.hasLimitOffsetClause_ = hasLimitOffset;
}

public List<Expr> getConjuncts()
{
    return new List<Expr>(globalState_.conjuncts.Values);
}

public int incrementCallDepth() { return ++callDepth_; }
public int decrementCallDepth() { return --callDepth_; }
public int getCallDepth() { return callDepth_; }

public bool hasMutualValueTransfer(SlotId a, SlotId b)
{
    return hasValueTransfer(a, b) && hasValueTransfer(b, a);
}

public bool hasValueTransfer(SlotId a, SlotId b)
{
    SccCondensedGraph g = globalState_.valueTransferGraph;
    return a.Equals(b) || (a < g.numVertices() && b < g.numVertices()
        && g.hasEdge(a, b));
}

public Dictionary<String, FeView> getLocalViews() { return localViews_; }

/**
 * Add a warning that will be displayed to the user. Ignores null messages. Once
 * getWarnings() has been called, no warning may be added to the Analyzer anymore.
 */
public void addWarning(String msg)
{
    //Preconditions.checkState(!globalState_.warningsRetrieved);
    if (msg == null) return;
    int count = globalState_.warnings[msg];
    globalState_.warnings[msg]= count + 1;
}

/**
 * Registers a new PrivilegeRequest in the analyzer.
 */
public void registerPrivReq(PrivilegeRequest privReq)
{
    if (!enablePrivChecks_) return;
    if (maskPrivChecks_)
    {
        globalState_.maskedPrivilegeReqs.Add(
            new Pair< PrivilegeRequest, String >(privReq, authErrorMsg_));
    }
    else
    {
        globalState_.privilegeReqs.Add(privReq);
    }
}

/**
 * Registers a table-level privilege request and an access event for auditing
 * for the given table and privilege. The table must be a base table or a
 * catalog view (not a local view).
 */
public void registerAuthAndAuditEvent(FeTable table, Privilege priv)
{
    // Add access event for auditing.
    if (table is FeView) {
        FeView view = (FeView)table;
        //Preconditions.checkState(!view.isLocalView());
        addAccessEvent(new TAccessEvent(
            table.getFullName(), TCatalogObjectType.VIEW,
            priv.ToString()));
    } else {
        addAccessEvent(new TAccessEvent(
            table.getFullName(), TCatalogObjectType.TABLE,
            priv.ToString()));
    }
    // Add privilege request.
    TableName tableName = table.getTableName();
    registerPrivReq(new PrivilegeRequestBuilder()
        .onTable(tableName.getDb(), tableName.getTbl())
        .allOf(priv).toRequest());
}
    }

    // State shared between all objects of an Analyzer tree. We use LinkedHashMap and
    // LinkedHashSet where applicable to preserve the iteration order and make the class
    // behave identical across different implementations of the JVM.
    // TODO in Many maps here contain properties about tuples, e.g., whether
    // a tuple is outer/semi joined, etc. Remove the maps in favor of making
    // them properties of the tuple descriptor itself.
    internal class GlobalState
    {
        //public readonly TQueryCtx queryCtx;
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
        public readonly Dictionary<ExprId, Expr> conjuncts = new Dictionary<ExprId, Expr>();

        // all registered conjuncts bound by a single tuple id; used in getBoundPredicates()
        public readonly List<ExprId> singleTidConjuncts = new List<ExprId>();

        // eqJoinConjuncts[tid] Contains all conjuncts of the form
        // "<lhs> = <rhs>" in which either lhs or rhs is fully bound by tid
        // and the other side is not bound by tid (ie, predicates that express equi-join
        // conditions between two tablerefs).
        // A predicate such as "t1.a = t2.b" has two entries, one for 't1' and
        // another one for 't2'.
        public readonly Dictionary<TupleId, List<ExprId>> eqJoinConjuncts = new Dictionary<TupleId, List<ExprId>>();

        // set of conjuncts that have been assigned to some PlanNode
        public HashSet<ExprId> assignedConjuncts =
            new HashSet<ExprId>(new Dictionary<ExprId, bool>());

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
        // TODO in This can be inferred from privilegeReqs. They should be coalesced.
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

        // Bidirectional Dictionary between int index and TNetworkAddress.
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
            // BetweenPredicates should be rewritten First to help trigger other rules.
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
