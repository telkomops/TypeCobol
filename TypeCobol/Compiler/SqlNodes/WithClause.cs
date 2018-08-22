using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Representation of the WITH clause that may appear before a query statement or insert
     * statement. A WITH clause contains a list of named view definitions that may be
     * referenced in the query statement that follows it.
     *
     * Scoping rules:
     * A WITH-clause view is visible inside the query statement that it belongs to.
     * This includes inline views and nested WITH clauses inside the query statement.
     *
     * Each WITH clause establishes a new analysis scope. A WITH-clause view definition
     * may refer to views from the same WITH-clause appearing to its left, and to all
     * WITH-clause views from outer scopes.
     *
     * References to WITH-clause views are resolved inside out, i.e., a match is found by
     * first looking in the current scope and then in the enclosing scope(s).
     *
     * Views defined within the same WITH-clause may not use the same alias.
     */
    public class WithClause : IParseNode
    {
        /////////////////////////////////////////
        // BEGIN: Members that need to be reset()

        private readonly List<View> views_;

        // END: Members that need to be reset()
        /////////////////////////////////////////

        public WithClause(List<View> views)
        {
            //Preconditions.checkNotNull(views);
            //Preconditions.checkState(!views.isEmpty());
            views_ = views;
        }

        public void analyze(Analyzer analyzer)
        {
            // Create a new analyzer for the WITH clause with a new global state (IMPALA-1357)
            // but a child of 'analyzer' so that the global state for 'analyzer' is not polluted
            // during analysis of the WITH clause. withClauseAnalyzer is a child of 'analyzer' so
            // that local views registered in parent blocks are visible here.
            Analyzer withClauseAnalyzer = Analyzer.createWithNewGlobalState(analyzer);
            withClauseAnalyzer.setIsWithClause();
            if (analyzer.isExplain()) withClauseAnalyzer.setIsExplain();
            foreach (View view in views_)
            {
                Analyzer viewAnalyzer = new Analyzer(withClauseAnalyzer);
                view.getQueryStmt().analyze(viewAnalyzer);
                // Register this view so that the next view can reference it.
                withClauseAnalyzer.registerLocalView(view);
            }
            // Register all local views with the analyzer.
            foreach (FeView localView in withClauseAnalyzer.getLocalViews().Values)
            {
                analyzer.registerLocalView(localView);
            }
            // Record audit events because the resolved table references won't generate any
            // when a view is referenced.
            analyzer.getAccessEvents().AddRange(withClauseAnalyzer.getAccessEvents());

            // Register all privilege requests made from the root analyzer.
            foreach (PrivilegeRequest req in withClauseAnalyzer.getPrivilegeReqs())
            {
                analyzer.registerPrivReq(req);
            }
        }

        /**
  * C'tor for cloning.
  */
        private WithClause(WithClause other)
        {
            //Preconditions.checkNotNull(other);
            views_ = new List<View>;
            foreach (View view in other.views_)
            {
                views_.Add(new View(view.getName(), view.getQueryStmt().clone(),
                    view.getOriginalColLabels()));
            }
        }

        public void reset()
        {
            foreach (View view in views_)
                view.getQueryStmt().reset();
        }
        
        public WithClause clone() { return new WithClause(this); }


        public string toSql()
        {
            return toSql(false);
        }


        public String toSql(bool rewritten)
        {
            List<String> viewStrings = new List<string>();
            foreach (View view in views_)
            {
                // Enclose the view alias and explicit labels in quotes if Hive cannot parse it
                // without quotes. This is needed for view compatibility between Impala and Hive.
                String aliasSql = ToSqlUtils.getIdentSql(view.getName());
                if (view.getColLabels() != null)
                {
                    aliasSql += "(" + string.Join(", ",
                                    ToSqlUtils.getIdentSqlList(view.getOriginalColLabels())) + ")";
                }
                viewStrings.Add(aliasSql + " AS (" + view.getQueryStmt().toSql(rewritten) + ")");
            }
            return "WITH " + string.Join(",",viewStrings);
        }

        public List<View> getViews() { return views_; }
    }
}
