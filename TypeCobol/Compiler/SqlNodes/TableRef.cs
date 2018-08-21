using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using TypeCobol.Compiler.SqlNodes.Authorization;
using TypeCobol.Compiler.SqlNodes.Common;
//TupleId is defined as int in impala
using TupleId = System.Int32;

namespace TypeCobol.Compiler.SqlNodes
{
/**
* Superclass of all table references, including references to views, base tables
* (Hdfs, HBase or DataSource tables), and nested collections.Contains the join
* specification.An instance of a TableRef (and not a subclass thereof) represents
* an unresolved table reference that must be resolved during analysis.All resolved
* table references are subclasses of TableRef.
*
* The analysis of table refs follows a two-step process:
*
* 1. Resolution: A table ref's path is resolved and then the generic TableRef is
* replaced by a concrete table ref (a BaseTableRef, CollectionTabeRef or ViewRef)
* in the originating stmt and that is given the resolved path.This step is driven by
* Analyzer.resolveTableRef().
*
* 2. Analysis/registration: After resolution, the concrete table ref is analyzed
* to register a tuple descriptor for its resolved path and register other table-ref
* specific state with the analyzer(e.g., whether it is outer/semi joined, etc.).
*
* Therefore, subclasses of TableRef should never call the analyze() of its superclass.
*
* TODO for 2.3: The current TableRef class hierarchy and the related two-phase analysis
* feels convoluted and is hard to follow.We should reorganize the TableRef class
    * structure for clarity of analysis and avoid a table ref 'switching genders' in between
* resolution and registration.
*
* TODO for 2.3: Rename this class to CollectionRef and re-consider the naming and
* structure of all subclasses.
*/
    public class TableRef : IParseNode
    {
        // Path to a collection type. Not set for inline views.
        protected internal List<string> RawPath { get; set; }


        // Legal aliases of this table ref. Contains the explicit alias as its sole element if
        // there is one. Otherwise, contains the two implicit aliases. Implicit aliases are set
        // in the c'tor of the corresponding resolved table ref (subclasses of TableRef) during
        // analysis. By convention, for table refs with multiple implicit aliases, Aliases[0]
        // contains the fully-qualified implicit alias to ensure that Aliases[0] always
        // uniquely identifies this table ref regardless of whether it has an explicit alias.
        protected internal  string[] Aliases;

        // Indicates whether this table ref is given an explicit alias,
        protected internal bool HasExplicitAlias;

        // Analysis registers privilege and/or audit requests based on this privilege.
        protected readonly Privilege Priv;


        // Optional TABLESAMPLE clause. Null if not specified.
        protected internal TableSampleClause SampleParams=null;

        protected JoinOperator JoinOp;
        protected List<PlanHint> JoinHints;
        protected List<string> UsingColNames;

        protected List<PlanHint> TableHints;
        protected SqlEnumerations.TReplicaPreference ReplicaPreference;
        protected bool RandomReplica_;

        // Hinted distribution mode for this table ref; set after analyzeJoinHints()
        // TODO: Move join-specific members out of TableRef.
        private SqlEnumerations.DistributionMode DistrMode = SqlEnumerations.DistributionMode.NONE;

        /////////////////////////////////////////
        // BEGIN: Members that need to be reset()

        // Resolution of rawPath_ if applicable. Result of analysis.
        protected Path ResolvedPath;

        protected Expr OnClause;

        // the ref to the left of us, if we're part of a JOIN clause
        protected TableRef LeftTblRef;

        // true if this TableRef has been analyzed; implementing subclass should set it to true
        // at the end of analyze() call.
        protected bool IsAnalyzed;

        
        // Lists of table ref ids and materialized tuple ids of the full sequence of table
        // refs up to and including this one. These ids are cached during analysis because
        // we may alter the chain of table refs during plan generation, but we still rely
        // on the original list of ids for correct predicate assignment.
        // Populated in analyzeJoin().
        protected List<TupleId> allTableRefIds_ = new List<TupleId>();
        protected List<TupleId> allMaterializedTupleIds_ = new List<TupleId>();

        // All physical tuple ids that this table ref is correlated with:
        // Tuple ids of root descriptors from outer query blocks that this table ref
        // (if a CollectionTableRef) or contained CollectionTableRefs (if an InlineViewRef)
        // are rooted at. Populated during analysis.
        protected List<TupleId> correlatedTupleIds_ = new List<TupleId>();

        // analysis output
        protected TupleDescriptor desc_;

        // END: Members that need to be reset()
        /////////////////////////////////////////

        public void analyze(Analyzer analyzer)
        {
            throw new NotImplementedException();
        }

        public string toSql()
        {
            throw new NotImplementedException();
        }
    }
}
