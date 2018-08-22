using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Sharpen;
using Castle.Core.Internal;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;
using TupleId = System.Int32;


namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Root of the expr node hierarchy.
     *
     */
    public abstract class Expr : TreeNode<Expr>, IParseNode
    {
        // Limits on the number of expr children and the depth of an expr tree. These maximum
        // values guard against crashes due to stack overflows (IMPALA-432) and were
        // experimentally determined to be safe.
        public static readonly int EXPR_CHILDREN_LIMIT = 10000;

        // The expr depth limit is mostly due to our recursive implementation of clone().
        public static readonly int EXPR_DEPTH_LIMIT = 1000;

        // Name of the function that needs to be implemented by every Expr that
        // supports negation.
        private static readonly string NEGATE_FN = "negate";

        // To be used where we cannot come up with a better estimate (selectivity_ is -1).
        public static double DEFAULT_SELECTIVITY = 0.1;

        // The relative costs of different Exprs. These numbers are not intended as a precise
        // reflection of running times, but as simple heuristics for ordering Exprs from cheap
        // to expensive.
        // TODO(tmwarshall): Get these costs in a more principled way, eg. with a benchmark.
        public static readonly float ARITHMETIC_OP_COST = 1;
        public static readonly float BINARY_PREDICATE_COST = 1;
        public static readonly float VAR_LEN_BINARY_PREDICATE_COST = 5;
        public static readonly float CAST_COST = 1;
        public static readonly float COMPOUND_PREDICATE_COST = 1;
        public static readonly float FUNCTION_CALL_COST = 10;
        public static readonly float IS_NOT_EMPTY_COST = 1;
        public static readonly float IS_NULL_COST = 1;
        public static readonly float LIKE_COST = 10;
        public static readonly float LITERAL_COST = 1;
        public static readonly float SLOT_REF_COST = 1;
        public static readonly float TIMESTAMP_ARITHMETIC_COST = 5;
        public static readonly float UNKNOWN_COST = -1;

        // To be used when estimating the cost of Exprs of type string where we don't otherwise
        // have an estimate of how long the strings produced by that Expr are.
        public static readonly int DEFAULT_AVG_STRING_LENGTH = 5;

        // returns true if an Expr is a non-analytic aggregate.
        private static readonly Predicate<Expr> isAggregatePredicate_ =
            arg => arg is FunctionCallExpr &&
                   ((FunctionCallExpr) arg).isAggregateFunction();

        // Returns true if an Expr is a NOT CompoundPredicate.
        public static readonly Predicate<Expr> IS_NOT_PREDICATE =
            arg => arg is CompoundPredicate &&
                   ((CompoundPredicate) arg).getOp() == CompoundPredicate.Operator.NOT;


        // Returns true if an Expr is an OR CompoundPredicate.
        public static readonly Predicate<Expr> IS_OR_PREDICATE =
            arg => arg is CompoundPredicate &&
                   ((CompoundPredicate) arg).getOp() == CompoundPredicate.Operator.OR;

        // Returns true if an Expr is a scalar subquery
        public static readonly Predicate<Expr> IS_SCALAR_SUBQUERY =
            arg => arg.isScalarSubquery();


        // Returns true if an Expr is an aggregate function that returns non-null on
        // an empty set (e.g. count).
        public static readonly Predicate<Expr>
            NON_NULL_EMPTY_AGG = arg => arg is FunctionCallExpr &&
                                        ((FunctionCallExpr) arg).returnsNonNullOnEmpty();

        // Returns true if an Expr is a builtin aggregate function.
        public static readonly Predicate<Expr> IS_BUILTIN_AGG_FN =
            arg => arg is FunctionCallExpr &&
                   ((FunctionCallExpr) arg).getFnName().isBuiltin();

        // Returns true if an Expr is a user-defined aggregate function.
        public static readonly Predicate<Expr> IS_UDA_FN =
            arg => isAggregatePredicate_.Invoke(arg) &&
                   !((FunctionCallExpr) arg).getFnName().isBuiltin();


        public static readonly Predicate<Expr> IS_TRUE_LITERAL =
            arg => arg is BoolLiteral && ((BoolLiteral) arg).getValue();


        public static readonly Predicate<Expr> IS_FALSE_LITERAL =
            arg => arg is BoolLiteral && !((BoolLiteral) arg).getValue();


        public static readonly Predicate<Expr> IS_EQ_BINARY_PREDICATE =
            arg => BinaryPredicate.getEqSlots(arg) != null;


        public static readonly Predicate<Expr> IS_NOT_EQ_BINARY_PREDICATE =
            arg => arg is BinaryPredicate
                   && ((BinaryPredicate) arg).getOp() != BinaryPredicate.Operator.EQ
                   && ((BinaryPredicate) arg).getOp() != BinaryPredicate.Operator.NOT_DISTINCT;

        public static readonly Predicate<Expr> IS_BINARY_PREDICATE =
            arg => arg is BinaryPredicate;

        public static readonly Predicate<Expr> IS_EXPR_EQ_LITERAL_PREDICATE =
            arg => arg is BinaryPredicate
                   && ((BinaryPredicate) arg).getOp() == BinaryPredicate.Operator.EQ
                   && (((BinaryPredicate) arg).getChild(1).isLiteral());

        public static readonly Predicate<Expr>
            IS_NONDETERMINISTIC_BUILTIN_FN_PREDICATE =
                arg => arg is FunctionCallExpr
                       && ((FunctionCallExpr) arg).isNondeterministicBuiltinFn();


        public static readonly Predicate<Expr> IS_UDF_PREDICATE =
            arg => arg is FunctionCallExpr
                   && !((FunctionCallExpr) arg).getFnName().isBuiltin();


        // id that's unique across the entire query statement and is assigned by
        // Analyzer.registerConjuncts(); only assigned for the top-level terms of a
        // conjunction, and therefore null for most Exprs
        protected ExprId id_;

        // true if Expr is an auxiliary predicate that was generated by the plan generation
        // process to facilitate predicate propagation;
        // false if Expr originated with a query stmt directly
        private bool isAuxExpr_ = false;

        public SqlNodeType type_; // result of analysis

        protected bool isOnClauseConjunct_; // set by analyzer

        // Flag to indicate whether to wrap this expr's toSql() in parenthesis. Set by parser.
        // Needed for properly capturing expr precedences in the SQL string.
        protected bool printSqlInParens_ = false;

        // Estimated probability of a predicate evaluating to true. Set during analysis.
        // Between 0 and 1, or set to -1 if the selectivity could not be estimated.
        protected double selectivity_;

        // Estimated relative cost of evaluating this expression, including the costs of
        // its children. Set during analysis and used to sort conjuncts within a PlanNode.
        // Has a default value of -1 indicating unknown cost if the cost of this expression
        // or any of its children was not set, but it is required to be set for any
        // expression which may be part of a conjunct.
        protected float evalCost_;

        // estimated number of distinct values produced by Expr; invalid: -1
        // set during analysis
        protected long numDistinctValues_;

        // Cached value of IsConstant(), set during analyze() and valid if isAnalyzed_ is true.
        private bool isConstant_;

        // The function to call. This can either be a scalar or aggregate function.
        // Set in analyze().
        protected Function fn_;

        // True after analysis successfully completed. Protected by accessors isAnalyzed() and
        // analysisDone().
        private bool isAnalyzed_ = false;

        protected Expr()
        {
            type_ = SqlNodeType.INVALID;
            selectivity_ = -1.0;
            evalCost_ = -1.0f;
            numDistinctValues_ = -1;
        }

        /**
         * Copy c'tor used in clone().
         */
        protected Expr(Expr other)
        {
            id_ = other.id_;
            isAuxExpr_ = other.isAuxExpr_;
            type_ = other.type_;
            isAnalyzed_ = other.isAnalyzed_;
            isOnClauseConjunct_ = other.isOnClauseConjunct_;
            printSqlInParens_ = other.printSqlInParens_;
            selectivity_ = other.selectivity_;
            evalCost_ = other.evalCost_;
            numDistinctValues_ = other.numDistinctValues_;
            isConstant_ = other.isConstant_;
            fn_ = other.fn_;
            //children_ = Expr.cloneList(other.children_);
        }

        public bool isAnalyzed() => isAnalyzed_;


        public ExprId getId() => id_;


        public void setId(ExprId id)
        {
            id_ = id;
        }

        public SqlNodeType getType() => type_;


        public double getSelectivity() => selectivity_;


        public bool hasSelectivity() => selectivity_ >= 0;


        public float getCost() => evalCost_;


        public bool hasCost() => evalCost_ >= 0;


        public long getNumDistinctValues() => numDistinctValues_;


        public bool getPrintSqlInParens() => printSqlInParens_;


        public void setPrintSqlInParens(bool b)
        {
            printSqlInParens_ = b;
        }

        public bool isOnClauseConjunct()
        {
            return isOnClauseConjunct_;
        }

        public void setIsOnClauseConjunct(bool b)
        {
            isOnClauseConjunct_ = b;
        }

        public bool isAuxExpr()
        {
            return isAuxExpr_;
        }

        public void setIsAuxExpr()
        {
            isAuxExpr_ = true;
        }

        public Function getFn()
        {
            return fn_;
        }

        /**
         * Perform semantic analysis of node and all of its children.
         * Throws exception if any errors found.
         * @see ParseNode#analyze(Analyzer)
         */
        public void analyze(Analyzer analyzer)
        {
            if (isAnalyzed()) return;

            // Check the expr child limit.
            if (children_.Count > EXPR_CHILDREN_LIMIT)
            {
                string sql = toSql();
                string sqlSubstr = sql.Substring(0, Math.Min(80, sql.Length));
                throw new AnalysisException(string.Format("Exceeded the maximum number of child " +
                                                          "expressions ({0}).\nExpression has {1} children:\n{2}...",
                    EXPR_CHILDREN_LIMIT, children_.Count, sqlSubstr));
            }

            // analyzer may be null for certain literal constructions (e.g. IntLiteral).
            if (analyzer != null)
            {
                analyzer.incrementCallDepth();
                // Check the expr depth limit. Do not print the toSql() to not overflow the stack.
                if (analyzer.getCallDepth() > EXPR_DEPTH_LIMIT)
                {
                    throw new AnalysisException(string.Format("Exceeded the maximum depth of an " +
                                                              "expression tree ({0}).", EXPR_DEPTH_LIMIT));
                }
            }

            foreach (Expr child in children_)
            {
                child.analyze(analyzer);
            }

            if (analyzer != null) analyzer.decrementCallDepth();
            computeNumDistinctValues();

            // Do all the analysis for the expr subclass before marking the Expr analyzed.
            analyzeImpl(analyzer);
            evalCost_ = computeEvalCost();
            analysisDone();
        }


        public void analyzeNoThrow(Analyzer analyzer)
        {
            try
            {
                analyze(analyzer);
            }
            catch (AnalysisException e)
            {
                throw new InvalidOperationException(e);
            }
        }

        /**
         * Compute and return evalcost of this expr given the evalcost of all children has been
         * computed. Should be called bottom-up whenever the structure of subtree is modified.
         */
        //protected float computeEvalCost();

        protected void computeNumDistinctValues()
        {
            if (isConstant())
            {
                numDistinctValues_ = 1;
            }
            else
            {
                numDistinctValues_ = -1;

                // get the Max number of distinct values over all children of this node
                foreach (Expr child in children_)
                {
                    // A constant should not override a -1 from a SlotRef, so we only consider
                    // non-constant expressions. This is functionally similar to considering
                    // only the SlotRefs, except that it allows an Expr to override the values
                    // that come out of its children.
                    if (!child.isConstant())
                    {
                        numDistinctValues_ = Math.Max(numDistinctValues_, child.getNumDistinctValues());
                    }
                }
            }
        }

        /**
         * Collects the returns types of the child nodes in an array.
         */
        protected SqlNodeType[] collectChildReturnTypes()
        {
            SqlNodeType[] childTypes = new SqlNodeType[children_.Count];
            for (int i = 0; i < children_.Count; ++i)
            {
                childTypes[i] = children_[i].type_;
            }

            return childTypes;
        }

        /**
         * Looks up in the catalog the builtin for 'name' and 'argTypes'.
         * Returns null if the function is not found.
         */
        protected Function getBuiltinFunction(Analyzer analyzer, String name,
            SqlNodeType[] argTypes, Function.CompareMode mode)
        {
            FunctionName fnName = new FunctionName(BuiltinsDb.NAME, name);
            Function searchDesc = new Function(fnName, argTypes, SqlNodeType.INVALID, false);
            return analyzer.getCatalog().getFunction(searchDesc, mode);
        }

        /**
         * Generates the necessary casts for the children of this expr to call fn_.
         * child(0) is cast to the function's first argument, child(1) to the second etc.
         *
         * If ignoreWildcardDecimals is true, the function will not cast arguments that
         * are wildcard decimals. This is used for builtins where the cast is done within
         * the BE function.
         * Otherwise, if the function signature contains wildcard decimals, each wildcard child
         * argument will be cast to the highest resolution that can contain all of the child
         * wildcard arguments.
         * e.g. fn(decimal(*), decimal(*))
         *      called with fn(decimal(10,2), decimal(5,3))
         * both children will be cast to (11, 3).
         *
         * If strictDecimal is true, we will only consider casts between decimal types that
         * result in no loss of information. If it is not possible to come with such casts,
         * we will throw an exception.
         */
        protected void castForFunctionCall(
            bool ignoreWildcardDecimals, bool strictDecimal)
        {
            //Preconditions.checkState(fn_ != null);
            SqlNodeType[] fnArgs = fn_.getArgs();
            SqlNodeType resolvedWildcardType = getResolvedWildCardType(strictDecimal);
            if (resolvedWildcardType != null)
            {
                if (resolvedWildcardType.isNull())
                {
                    throw new AnalysisException(string.Format(
                        "Cannot resolve DECIMAL precision and scale from NULL type in {0} function.",
                        fn_.getFunctionName().getFunction()));
                }

                if (resolvedWildcardType.isInvalid() && !ignoreWildcardDecimals)
                {
                    StringBuilder argTypes = new StringBuilder();
                    foreach (var child in children_)
                    {
                        if (argTypes.Length > 0) argTypes.Append(", ");
                        SqlNodeType childType = child.type_;
                        argTypes.Append(childType.toSql());
                    }

                    throw new AnalysisException(string.Format(
                        "Cannot resolve DECIMAL types of the {0}({1}) function arguments. You need " +
                        "to wrap the arguments in a CAST.", fn_.getFunctionName().getFunction(),
                        argTypes.ToString()));
                }
            }

            for (int i = 0; i < children_.Count; ++i)
            {
                // For varargs, we must compare with the last type in fnArgs.argTypes.
                int ix = Math.Min(fnArgs.Length - 1, i);
                if (fnArgs[ix].isWildcardDecimal())
                {
                    if (children_[i].type_.isDecimal() && ignoreWildcardDecimals) continue;
                    if (children_[i].type_.isDecimal() || !ignoreWildcardDecimals)
                    {
                        //Preconditions.checkState(resolvedWildcardType != null);
                        //Preconditions.checkState(!resolvedWildcardType.isInvalid());
                        if (!children_[i].type_.Equals(resolvedWildcardType))
                        {
                            castChild(resolvedWildcardType, i);
                        }
                    }
                    else if (children_[i].type_.isNull())
                    {
                        castChild(ScalarType.createDecimalType(), i);
                    }
                    else
                    {
                        //Preconditions.checkState(children_[i].type_.isScalarType());
                        // It is safe to assign an arbitrary decimal here only if the backend function
                        // can handle it (in which case ignoreWildcardDecimals is true).
                        //Preconditions.checkState(ignoreWildcardDecimals);
                        castChild(((ScalarType) children_[i].type_).getMinResolutionDecimal(), i);
                    }
                }
                else if (!children_[i].type_.matchesType(fnArgs[ix]))
                {
                    castChild(fnArgs[ix], i);
                }
            }
        }

        /**
         * Returns the Max resolution type of all the wild card decimal types.
         * Returns null if there are no wild card types. If strictDecimal is enabled, will
         * return an invalid type if it is not possible to come up with a decimal type that
         * is guaranteed to not lose information.
         */
        SqlNodeType getResolvedWildCardType(bool strictDecimal)
        {
            SqlNodeType result = null;
            SqlNodeType[] fnArgs = fn_.getArgs();
            for (int i = 0; i < children_.Count; ++i)
            {
                // For varargs, we must compare with the last SqlNodeType in fnArgs.argTypes.
                int ix = Math.Min(fnArgs.Length - 1, i);
                if (!fnArgs[ix].isWildcardDecimal()) continue;

                SqlNodeType childType = children_[i].type_;
                //Preconditions.checkState(!childType.isWildcardDecimal(),
                //"Child expr should have been resolved.");
                //Preconditions.checkState(childType.isScalarType(),
                //"Function should not have resolved with a non-scalar child type.");
                if (result == null)
                {
                    ScalarType decimalType = (ScalarType) childType;
                    result = decimalType.getMinResolutionDecimal();
                }
                else
                {
                    result = SqlNodeType.getAssignmentCompatibleType(
                        result, childType, false, strictDecimal);
                }


                if (result != null && !result.isNull())
                {
                    result = ((ScalarType) result).getMinResolutionDecimal();
                    //Preconditions.checkState(result.isDecimal() || result.isInvalid());
                    //Preconditions.checkState(!result.isWildcardDecimal());
                }

                return result;
            }

            return result;
        }

        /**
         * Returns true if e is a CastExpr and the target type is a decimal.
         */
        private bool isExplicitCastToDecimal(Expr e)
        {
            if (!(e is CastExpr)) return false;
            CastExpr c = (CastExpr) e;
            return !c.isImplicit() && c.getType().isDecimal();
        }

        /**
         * Returns a clone of child with all decimal-typed NumericLiterals in it explicitly
         * cast to targetType.
         */
        private Expr convertDecimalLiteralsToFloat(Analyzer analyzer, Expr child,
            SqlNodeType targetType)
        {
            if (!targetType.isFloatingPointType() && !targetType.isIntegerType()) return child;
            if (targetType.isIntegerType()) targetType = SqlNodeType.DOUBLE;
            List<NumericLiteral> literals = new List<NumericLiteral>();
            //TODO - redo call to collect all from TreeNode
            //child.collectAll(Predicate is (NumericLiteral.class), literals);
            ExprSubstitutionMap smap = new ExprSubstitutionMap();
            foreach (NumericLiteral l in
                literals)
            {
                if (!l.getType().isDecimal()) continue;
                NumericLiteral castLiteral = (NumericLiteral) l.clone();
                castLiteral.explicitlyCastToFloat(targetType);
                smap.put(l, castLiteral);
            }

            return child.substitute(smap, analyzer, false);

        }


/**
 * DECIMAL_V1:
 * ----------
 * This function applies a heuristic that casts literal child exprs of this expr from
 * decimal to floating point in certain circumstances to reduce processing cost. In
 * earlier versions of Impala's decimal support, it was much slower than floating point
 * arithmetic. The original rationale for the automatic casting follows.
 *
 * Decimal has a higher processing cost than floating point and we should not pay
 * the cost if the user does not require the accuracy. For example:
 * "select float_col + 1.1" would start out with 1.1 as a decimal(2,1) and the
 * float_col would be promoted to a high accuracy decimal. This function will identify
 * this case and treat 1.1 as a float.
 * In the case of "decimal_col + 1.1", 1.1 would remain a decimal.
 * In the case of "float_col + cast(1.1 as decimal(2,1))", the result would be a
 * decimal.
 *
 * Another way to think about it is that DecimalLiterals are analyzed as returning
 * decimals (of the narrowest precision/scale) and we later convert them to a floating
 * point type according to a heuristic that attempts to guess what the user intended.
 *
 * DECIMAL_V2:
 * ----------
 * This function does nothing. All decimal numeric literals are interpreted as decimals
 * and the normal expression typing rules apply.
 */
        protected void convertNumericLiteralsFromDecimal(Analyzer analyzer)

        {
            //Preconditions.checkState(this is ArithmeticExpr ||
            //this is BinaryPredicate);
            // This heuristic conversion is not part of DECIMAL_V2.
            if (analyzer.getQueryOptions().isDecimal_v2()) return;
            if (children_.Count == 1) return; // Do not attempt to convert for unary ops
            //Preconditions.checkState(children_.Count == 2);
            SqlNodeType t0 = getChild(0).getType();
            SqlNodeType t1 = getChild(1).getType();
            bool c0IsConstantDecimal = getChild(0).isConstant() && t0.isDecimal();
            bool c1IsConstantDecimal = getChild(1).isConstant() && t1.isDecimal();
            if (c0IsConstantDecimal && c1IsConstantDecimal) return;
            if (!c0IsConstantDecimal && !c1IsConstantDecimal) return;

            // Only child(0) or child(1) is a const decimal. See if we can cast it to
            // the type of the other child.
            if (c0IsConstantDecimal && !isExplicitCastToDecimal(getChild(0)))
            {
                Expr c0 = convertDecimalLiteralsToFloat(analyzer, getChild(0), t1);
                setChild(0, c0);
            }

            if (c1IsConstantDecimal && !isExplicitCastToDecimal(getChild(1)))
            {
                Expr c1 = convertDecimalLiteralsToFloat(analyzer, getChild(1), t0);
                setChild(1, c1);
            }
        }


        /**
         * Helper function: analyze list of exprs
         */
        public static void analyze(List<Expr> exprs, Analyzer analyzer)
        {
            if (exprs == null) return;
            foreach (Expr expr in exprs)
            {
                expr.analyze(analyzer);
            }
        }


        public string toSql()
        {
            return (printSqlInParens_) ? "(" + toSqlImpl() + ")" : toSqlImpl();
        }

        private string toSqlImpl()
        {
            return string.Empty;
        }

        /**
         * Returns a SQL string representing this expr. Subclasses should override this method
         * instead of toSql() to ensure that parenthesis are properly added around the toSql().
         */
        public abstract String ToSqlImpl();



        /**
         * Returns the product of the given exprs' number of distinct values or -1 if any of
         * the exprs have an invalid number of distinct values.
         */
        public static long getNumDistinctValues(List<Expr> exprs)
        {
            if (exprs == null || exprs.IsNullOrEmpty()) return 0;
            long numDistinctValues = 1;
            foreach (Expr expr in
                exprs)
            {
                if (expr.getNumDistinctValues() == -1)
                {
                    numDistinctValues = -1;
                    break;
                }

                numDistinctValues *= expr.getNumDistinctValues();
            }

            return numDistinctValues;
        }


        public static Predicate<Expr> isAggregatePredicate()
        {
            return isAggregatePredicate_;
        }

        public bool isAggregate()
        {
            return isAggregatePredicate_.Invoke(this);
        }

        public List<string> childrenToSql()
        {
            List<string> result = new List<string>();
            foreach (Expr child in children_)
            {
                result.Add(child.toSql());
            }

            return result;
        }

        public abstract string DebugString();

        public string debugString()
        {
            return (id_ != null ? "exprid=" + id_.ToString() + " " : "") + debugString(children_);
        }

        public static string debugString(List<Expr> exprs)
        {
            if (exprs == null || exprs.IsNullOrEmpty()) return "";
            List<String> strings = new List<string>();
            foreach (Expr expr in exprs)
            {
                strings.Add(expr.debugString());
            }

            return string.Join(" ", strings);
        }

        public string toSql(List<Expr> exprs)
        {
            if (exprs == null || exprs.IsNullOrEmpty()) return "";
            List<String> strings = new List<string>();
            foreach (Expr expr in exprs)
            {
                strings.Add(expr.toSql());
            }

            return string.Join(" ", strings);
        }

        /**
         * Returns true if this expr matches 'that'. Two exprs match if:
         * 1. The tree structures ignoring implicit casts are the same.
         * 2. For every pair of corresponding SlotRefs, slotRefCmp.matches() returns true.
         * 3. For every pair of corresponding non-SlotRef exprs, localEquals() returns true.
         */
        public bool matches(Expr that, SlotRef.Comparator slotRefCmp)
        {
            if (that == null) return false;
            if (this is CastExpr && ((CastExpr) this).isImplicit())
            {
                return children_[0].matches(that, slotRefCmp);
            }

            if (that is CastExpr && ((CastExpr) that).isImplicit())
            {
                return matches(((CastExpr) that).children_[0], slotRefCmp);
            }

            if (this is SlotRef && that is SlotRef)
            {
                return slotRefCmp.matches((SlotRef) this, (SlotRef) that);
            }

            if (!localEquals(that)) return false;
            if (children_.Count != that.children_.Count) return false;
            for (int i = 0; i < children_.Count; ++i)
            {
                if (!children_[i].matches(that.children_[i], slotRefCmp)) return false;
            }

            return true;
        }

        /**
         * Local eq comparator. Returns true if this expr is equal to 'that' ignoring children.
         */
        protected bool localEquals(Expr that)
        {
            return GetType() == that.GetType() &&
                   (fn_ == null ? that.fn_ == null : fn_.Equals(that.fn_));
        }

        public abstract bool LocalEquals(Expr that);

        /**
         * Returns true if two expressions are equal. The equality comparison works on analyzed
         * as well as unanalyzed exprs by ignoring implicit casts.
         */
        public override bool Equals(Object obj)
        {
            return obj is Expr && matches((Expr) obj, SlotRef.SLOTREF_EQ_CMP);
        }

        /**
         * Return true if l1[i].Equals(l2[i]) for all i.
         */
        public static bool equalLists(List<Expr> l1, List<Expr> l2) => l1.SequenceEqual(l2);

        /**
         * Return true if l1 Equals l2 when both lists are interpreted as sets.
         * TODO: come up with something better than O(n^2)?
         */
        public static bool equalSets(List<Expr> l1, List<Expr> l2) => l1.SequenceEqual(l2) && l2.SequenceEqual(l1);


        /**
         * Return true if l1 is a subset of l2.
         */
        public static bool isSubset(List<Expr> l1, List<Expr> l2) => l2.All(l1.Contains);


        /**
         * Return the intersection of l1 and l2.
         */
        public static List<Expr> intersect(List<Expr> l1, List<Expr> l2)
        {
            List<Expr> result = new List<Expr>();
            foreach (Expr element in l1)
            {
                if (l2.Contains(element)) result.Add(element);
            }

            return result;
        }


        public override int GetHashCode()
        {
            if (id_ == null)
            {
                throw new NotSupportedException("Expr.hashCode() is not implemented");
            }
            else
            {
                return Convert.ToInt32(id_);
            }
        }

        /**
         * Gather conjuncts from this expr and return them in a list.
         * A conjunct is an expr that returns a bool, e.g., Predicates, function calls,
         * SlotRefs, etc. Hence, this method is placed here and not in Predicate.
         */
        public List<Expr> getConjuncts()
        {
            List<Expr> list = new List<Expr>();
            if (this is CompoundPredicate
                && ((CompoundPredicate) this).getOp() == CompoundPredicate.Operator.AND)
            {
                // TODO: we have to convert CompoundPredicate.AND to two expr trees for
                // conjuncts because NULLs are handled differently for CompoundPredicate.AND
                // and conjunct evaluation.  This is not optimal for jitted exprs because it
                // will result in two functions instead of one. Create a new CompoundPredicate
                // Operator (i.e. CONJUNCT_AND) with the right NULL semantics and use that
                // instead
                list.AddRange((getChild(0)).getConjuncts());
                list.AddRange((getChild(1)).getConjuncts());
            }
            else
            {
                list.Add(this);
            }

            return list;
        }

        /**
         * Returns an analyzed clone of 'this' with exprs substituted according to smap.
         * Removes implicit casts and analysis state while cloning/substituting exprs within
         * this tree, such that the returned result has minimal implicit casts and types.
         * Throws if analyzing the post-substitution expr tree failed.
         * If smap is null, this function is equivalent to clone().
         * If preserveRootType is true, the resulting expr tree will be cast if necessary to
         * the type of 'this'.
         */
        public Expr trySubstitute(ExprSubstitutionMap smap, Analyzer analyzer,
            bool preserveRootType)

        {
            Expr result = clone();
            // Return clone to avoid removing casts.
            if (smap == null) return result;
            result = result.substituteImpl(smap, analyzer);
            result.analyze(analyzer);
            if (preserveRootType && !type_.Equals(result.getType())) result = result.castTo(type_);
            return result;
        }

        /**
         * Returns an analyzed clone of 'this' with exprs substituted according to smap.
         * Removes implicit casts and analysis state while cloning/substituting exprs within
         * this tree, such that the returned result has minimal implicit casts and types.
         * Expects the analysis of the post-substitution expr to succeed.
         * If smap is null, this function is equivalent to clone().
         * If preserveRootType is true, the resulting expr tree will be cast if necessary to
         * the type of 'this'.
         */
        public Expr substitute(ExprSubstitutionMap smap, Analyzer analyzer,
            bool preserveRootType)
        {
            try
            {
                return trySubstitute(smap, analyzer, preserveRootType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed analysis after expr substitution.", e);
            }
        }


        public static List<Expr> trySubstituteList(IEnumerable<Expr> exprs,
            ExprSubstitutionMap smap, Analyzer analyzer, bool preserveRootTypes)

        {
            return exprs?.Select(e => e.trySubstitute(smap, analyzer, preserveRootTypes)).ToList();
        }

        public static List<Expr> substituteList(IEnumerable<Expr> exprs,
            ExprSubstitutionMap smap, Analyzer analyzer, bool preserveRootTypes)
        {
            try
            {
                return trySubstituteList(exprs, smap, analyzer, preserveRootTypes);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed analysis after expr substitution.", e);
            }
        }

        /**
         * Recursive method that performs the actual substitution for try/substitute() while
         * removing implicit casts. Resets the analysis state in all non-SlotRef expressions.
         * Exprs that have non-child exprs which should be affected by substitutions must
         * override this method and apply the substitution to such exprs as well.
         */
        protected Expr substituteImpl(ExprSubstitutionMap smap, Analyzer analyzer)
        {
            if (isImplicitCast()) return getChild(0).substituteImpl(smap, analyzer);
            if (smap != null)
            {
                Expr substExpr = smap[this];
                if (substExpr != null) return substExpr.clone();
            }

            for (int i = 0; i < children_.Count; ++i)
            {
                children_[i] = children_[i].substituteImpl(smap, analyzer);
            }

            // SlotRefs must remain analyzed to support substitution across query blocks. All
            // other exprs must be analyzed again after the substitution to add implicit casts
            // and for resolving their correct function signature.
            if (!(this is SlotRef)) resetAnalysisState();
            return this;
        }

        /**
         * Set the expr to be analyzed and computes isConstant_.
         */
        protected void analysisDone()
        {
            //Preconditions.checkState(!isAnalyzed_);
            // We need to compute the const-ness as the last step, since analysis may change
            // the result, e.g. by resolving function.
            isConstant_ = isConstantImpl();
            isAnalyzed_ = true;
        }

        /**
         * Resets the internal state of this expr produced by analyze().
         * Only modifies this expr, and not its child exprs.
         */
        protected void resetAnalysisState()
        {
            isAnalyzed_ = false;
        }

        public abstract void ResetAnalysisState();

        /**
         * Resets the internal analysis state of this expr tree. Removes implicit casts.
         */
        public Expr reset()
        {
            if (isImplicitCast()) return getChild(0).reset();
            foreach (var child in children_)
            {
                child.reset();
            }

            resetAnalysisState();
            return this;
        }

        public static List<Expr> resetList(List<Expr> l)
        {
            foreach (var elem in l)
            {
                elem.reset();
            }

            return l;
        }

        /**
         * Creates a deep copy of this expr including its analysis state. The method is
         *  in this class to force new Exprs to implement it.
         */

        public abstract Expr clone();

        /**
         * Create a deep copy of 'l'. The elements of the returned list are of the same
         * type as the input list.
         */
        public static List<Expr> cloneList(List<Expr> l)
        {
            //Preconditions.checkNotNull(l);
            List<Expr> result = new List<Expr>(l.Count);
            foreach (Expr element in l)
            {
                result.Add(element.clone());
            }

            return result;
        }

        /**
         * Removes duplicate exprs (according to Equals()).
         */
        public static void removeDuplicates(List<Expr> l)
        {
            if (l == null) return;
            List<Expr> origList = new List<Expr>(l);
            l.Clear();
            foreach (Expr expr in origList)
            {
                if (!l.Contains(expr))
                {
                    l.Add(expr);
                }
            }
        }

        /**
         * Return a new list without duplicate exprs (according to matches() using cmp).
         */
        public static List<Expr> removeDuplicates(List<Expr> l,
            SlotRef.Comparator cmp)
        {
            List<Expr> newList = new List<Expr>();
            foreach (Expr expr in l)
            {
                bool exists = false;
                foreach (Expr newExpr in newList)
                {
                    if (newExpr.matches(expr, cmp))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists) newList.Add(expr);
            }

            return newList;
        }

        /**
         * Removes constant exprs
         */
        public static void removeConstants(List<Expr> l)
        {
            if (l == null) return;
            IEnumerator<Expr> it = l.GetEnumerator();
            while (it.MoveNext())
            {
                Expr e = it.Current;
                if (e.isConstant())
                {
                    l.Remove(e);
                }
            }
        }

        // Arbitrary Max exprs considered for constant propagation due to O(n^2) complexity.
        private static readonly int CONST_PROPAGATION_EXPR_LIMIT = 200;

        /**
         * Propagates constant expressions of the form <slot ref> = <constant> to
         * other uses of slot ref in the given conjuncts; returns a BitSet with
         * bits set to true in all changed indices.  Only one round of substitution
         * is performed.  The candidates BitSet is used to determine which members of
         * conjuncts are considered for propagation.
         */
        private static BitSet propagateConstants(List<Expr> conjuncts, BitSet candidates,
            Analyzer analyzer)
        {
            //Preconditions.checkState(conjuncts.Count <= candidates.Count);
            BitSet changed = new BitSet(conjuncts.Count);
            for (int i = candidates.nextSetBit(0); i >= 0; i = candidates.nextSetBit(i + 1))
            {
                if (!(conjuncts[i] is BinaryPredicate)) continue;
                BinaryPredicate bp = (BinaryPredicate) conjuncts[i];
                if (bp.getOp() != BinaryPredicate.Operator.EQ) continue;
                SlotRef slotRef = bp.getBoundSlot();
                if (slotRef == null || !bp.getChild(1).isConstant()) continue;
                Expr subst = bp.getSlotBinding(slotRef.getSlotId());
                ExprSubstitutionMap smap = new ExprSubstitutionMap();
                smap.put(slotRef, subst);
                for (int j = 0; j < conjuncts.Count; ++j)
                {
                    // Don't rewrite with our own substitution!
                    if (j == i) continue;
                    Expr toRewrite = conjuncts[j];
                    Expr rewritten = toRewrite.substitute(smap, analyzer, true);
                    if (!rewritten.Equals(toRewrite))
                    {
                        conjuncts[j] = rewritten;
                        changed[j] = true;
                    }
                }
            }

            return changed;
        }

        /*
         * Propagates constants, performs expr rewriting and removes duplicates.
         * Returns false if a contradiction has been implied, true otherwise.
         * Catches and logs, but ignores any exceptions thrown during rewrite, which
         * will leave conjuncts intact and rewritten as far as possible until the
         * exception.
         */
        public static bool optimizeConjuncts(List<Expr> conjuncts, Analyzer analyzer)
        {
            //Preconditions.checkNotNull(conjuncts);
            try
            {
                //TODO - find a better implementation for BitSet n c#
                BitSet candidates = new BitSet(conjuncts.Count);
                candidates[0] = Math.Min(conjuncts.Count, CONST_PROPAGATION_EXPR_LIMIT);
                int transfers = 0;

                // Constant propagation may make other slots constant, so repeat the process
                // until there are no more changes.
                while (!candidates.IsNullOrEmpty())
                {
                    BitSet changed = propagateConstants(conjuncts, candidates, analyzer);
                    candidates.Clear();
                    int pruned = 0;
                    for (int i = changed.nextSetBit(0); i >= 0; i = changed.nextSetBit(i + 1))
                    {
                        // When propagating constants, we may de-normalize expressions, so we
                        // must normalize binary predicates.  Any additional rules will be
                        // applied by the rewriter.
                        int index = i - pruned;
                        //Preconditions.checkState(index >= 0);
                        ExprRewriter rewriter = analyzer.getExprRewriter();
                        Expr rewritten = rewriter.rewrite(conjuncts[index], analyzer);
                        // Re-analyze to add implicit casts and update cost
                        rewritten.reset();
                        rewritten.analyze(analyzer);
                        if (!rewritten.isConstant())
                        {
                            conjuncts[index] = rewritten;
                            if (++transfers < CONST_PROPAGATION_EXPR_LIMIT) candidates[index] = true;
                            continue;
                        }

                        // Remove constant bool literal expressions.  N.B. - we may have
                        // expressions determined to be constant which can not yet be discarded
                        // because they can't be evaluated if expr rewriting is turned off.
                        if (rewritten is NullLiteral ||
                            Expr.IS_FALSE_LITERAL.Invoke(rewritten))
                        {
                            conjuncts.Clear();
                            conjuncts.Add(rewritten);
                            return false;
                        }

                        if (Expr.IS_TRUE_LITERAL.Invoke(rewritten))
                        {
                            pruned++;
                            conjuncts.Remove(index);
                        }
                    }
                }
            }
            catch (AnalysisException )
            {
                //TODO - log exception
                //LOG.warn("Not able to analyze after rewrite: " + e.ToString() + " conjuncts: " +
                //         Expr.debugString(conjuncts));
            }

            conjuncts = conjuncts.Distinct().ToList();
            return true;
        }

        /**
         * Returns true if expr is fully bound by tid, otherwise false.
         */
        public bool isBound(TupleId tid)
        {
            return isBoundByTupleIds(new List<int>(tid));
        }

        /**
         * Returns true if expr is fully bound by tids, otherwise false.
         */
        public bool isBoundByTupleIds(List<TupleId> tids)
        {
            foreach (Expr child in children_)
            {
                if (!child.isBoundByTupleIds(tids)) return false;
            }

            return true;
        }

        public abstract bool IsBoundByTupleIds(List<TupleId> tids);
        /**
         * Returns true if expr is fully bound by slotIds, otherwise false.
         */
        public bool isBoundBySlotIds(List<SlotId> slotIds)
        {
            foreach (Expr child in children_)
            {
                if (!child.isBoundBySlotIds(slotIds)) return false;
            }

            return true;
        }

        public abstract bool IsBoundBySlotIds(List<SlotId> slotIds);

        public static Expr getFirstBoundChild(Expr expr, List<TupleId> tids)
        {
            foreach (Expr child in expr.getChildren())
            {
                if (child.isBoundByTupleIds(tids)) return child;
            }

            return null;
        }

        public void getIds(List<TupleId> tupleIds, List<SlotId> slotIds)
        {
            HashSet<TupleId> tupleIdSet = new HashSet<int>();
            HashSet<SlotId> slotIdSet = new HashSet<SlotId>();
            getIdsHelper(tupleIdSet, slotIdSet);
            tupleIds?.AddRange(tupleIdSet);
            slotIds?.AddRange(slotIdSet);
        }

        protected void getIdsHelper(HashSet<TupleId> tupleIds, HashSet<SlotId> slotIds)
        {
            foreach (Expr child in children_)
            {
                child.getIdsHelper(tupleIds, slotIds);
            }
        }

        public abstract void GetIdsHelper(HashSet<TupleId> tupleIds, HashSet<SlotId> slotIds);

        public static void getIds(List<Expr> exprs,
            List<TupleId> tupleIds, List<SlotId> slotIds)
        {
            if (exprs == null) return;
            foreach (Expr e in exprs)
            {
                e.getIds(tupleIds, slotIds);
            }
        }

        /**
         * @return true if this is an instance of LiteralExpr
         */
        public bool isLiteral()
        {
            return this is LiteralExpr;
        }


        /**
         * Returns true if this expression should be treated as constant. I.e. if the frontend
         * and backend should assume that two evaluations of the expression within a query will
         * return the same value. Examples of constant expressions include:
         * - Literal values like 1, "foo", or NULL
         * - Deterministic operators applied to constant arguments, e.g. 1 + 2, or
         *   concat("foo", "bar")
         * - Functions that should be always return the same value within a query but may
         *   return different values for different queries. E.g. now(), which we want to
         *   evaluate only once during planning.
         * May incorrectly return true if the expression is not analyzed.
         * TODO: isAnalyzed_ should be a precondition for isConstant(), since it is not always
         * possible to correctly determine const-ness before analysis (e.g. see
         * FunctionCallExpr.isConstant()).
         */
        public bool isConstant()
        {
            return isAnalyzed_ ? isConstant_ : isConstantImpl();
        }

        /**
         * Implements isConstant() - computes the value without using 'isConstant_'.
         */
        protected bool isConstantImpl()
        {
            foreach (Expr expr in children_)
            {
                if (!expr.isConstant()) return false;
            }

            return true;
        }

        protected abstract bool IsConstantImpl();

        /**
         * @return true if this expr is either a null literal or a cast from
         * a null literal.
         */
        public bool isNullLiteral()
        {
            if (this is NullLiteral) return true;
            return this is CastExpr && children_[0].isNullLiteral();
            //Preconditions.checkState(children_.Count == 1);
        }

        /**
         * Return true if this expr is a scalar subquery.
         */
        public bool isScalarSubquery()
        {
            //Preconditions.checkState(isAnalyzed_);
            if (!(this is Subquery)) return false;
            Subquery subq = (Subquery) this;
            SelectStmt stmt = (SelectStmt) subq.getStatement();
            return stmt.returnsSingleRow() && getType().isScalarType();
        }

        /**
         * Checks whether this expr returns a bool type or NULL type.
         * If not, throws an AnalysisException with an appropriate error message using
         * 'name' as a prefix. For example, 'name' could be "WHERE clause".
         * The error message only contains this.toSql() if printExpr is true.
         */
        public void checkReturnsBool(String name, bool printExpr)
        {
            if (!type_.isBoolean() && !type_.isNull())
            {
                throw new AnalysisException(
                    string.Format("{0}{1} requires return type 'bool'. " +
                                  "Actual type is '{2}'.", name, (printExpr) ? " '" + toSql() + "'" : "",
                        type_.ToString()));
            }
        }

        /**
         * Casts this expr to a specific target type. It checks the validity of the cast and
         * calls uncheckedCastTo().
         * @param targetType
         *          type to be cast to
         * @return cast expression, or converted literal,
         *         should never return null
         * @
         *           when an invalid cast is asked for, for example,
         *           failure to convert a string literal to a date literal
         */
        public Expr castTo(SqlNodeType targetType)
        {
            SqlNodeType type = SqlNodeType.getAssignmentCompatibleType(this.type_, targetType, false, false);
            //Preconditions.checkState(type.isValid(), "cast %s to %s", this.type_, targetType);
            // If the targetType is NULL_TYPE then ignore the cast because NULL_TYPE
            // is compatible with all types and no cast is necessary.
            if (targetType.isNull()) return this;
            if (!targetType.isDecimal())
            {
                // requested cast must be to assignment-compatible type
                // (which implies no loss of precision)
                //Preconditions.checkArgument(targetType.Equals(type),
                //"targetType=" + targetType + " type=" + type);
            }

            return uncheckedCastTo(targetType);
        }

        /**
         * Create an expression equivalent to 'this' but returning targetType;
         * possibly by inserting an implicit cast,
         * or by returning an altogether new expression
         * or by returning 'this' with a modified return type'.
         * @param targetType
         *          type to be cast to
         * @return cast expression, or converted literal,
         *         should never return null
         * @
         *           when an invalid cast is asked for, for example,
         *           failure to convert a string literal to a date literal
         */
        protected Expr uncheckedCastTo(SqlNodeType targetType)
        {
            return new CastExpr(targetType, this);
        }

        /**
         * Add a cast expression above child.
         * If child is a literal expression, we attempt to
         * convert the value of the child directly, and not insert a cast node.
         * @param targetType
         *          type to be cast to
         * @param childIndex
         *          index of child to be cast
         */
        public void castChild(SqlNodeType targetType, int childIndex)
        {
            Expr child = getChild(childIndex);
            Expr newChild = child.castTo(targetType);
            setChild(childIndex, newChild);
        }


        /**
         * Convert child to to targetType, possibly by inserting an implicit cast, or by
         * returning an altogether new expression, or by returning 'this' with a modified
         * return type'.
         * @param targetType
         *          type to be cast to
         * @param childIndex
         *          index of child to be cast
         */
        protected void uncheckedCastChild(SqlNodeType targetType, int childIndex)

        {
            Expr child = getChild(childIndex);
            Expr newChild = child.uncheckedCastTo(targetType);
            setChild(childIndex, newChild);
        }

        /**
         * Returns child expr if this expr is an implicit cast, otherwise returns 'this'.
         */
        public Expr ignoreImplicitCast()
        {
            if (isImplicitCast()) return getChild(0).ignoreImplicitCast();
            return this;
        } 

        public abstract Expr IgnoreImplicitCast();

        /**
         * Returns true if 'this' is an implicit cast expr.
         */
        public bool isImplicitCast()
        {
            return this is CastExpr && ((CastExpr) this).isImplicit();
        }

        public override string ToString()
        {
            return this.GetType().ToString() +
                   "id" + id_ +
                   "type" + type_ +
                   "sel" + selectivity_ +
                   "evalCost" + evalCost_ +
                   "#distinct" + numDistinctValues_;
        }

        /**
         * If 'this' is a SlotRef or a Cast that wraps a SlotRef, returns that SlotRef.
         * Otherwise returns null.
         */
        public SlotRef unwrapSlotRef(bool implicitOnly)
        {
            Expr unwrappedExpr = unwrapExpr(implicitOnly);
            if (unwrappedExpr is SlotRef) return (SlotRef) unwrappedExpr;
            return null;
        }

        /**
         * Returns the first child if this Expr is a CastExpr. Otherwise, returns 'this'.
         */
        public Expr unwrapExpr(bool implicitOnly)
        {
            if (this is CastExpr
                && (!implicitOnly || ((CastExpr) this).isImplicit()))
            {
                return children_[0];
            }

            return this;
        }

        /**
         * Returns the descriptor of the scan slot that directly or indirectly produces
         * the values of 'this' SlotRef. Traverses the source exprs of intermediate slot
         * descriptors to resolve materialization points (e.g., aggregations).
         * Returns null if 'e' or any source expr of 'e' is not a SlotRef or cast SlotRef.
         */
        public SlotDescriptor findSrcScanSlot()
        {
            SlotRef slotRef = unwrapSlotRef(false);
            if (slotRef == null) return null;
            SlotDescriptor slotDesc = slotRef.getDesc();
            if (slotDesc.isScanSlot()) return slotDesc;
            if (slotDesc.getSourceExprs().Count == 1)
            {
                return slotDesc.getSourceExprs()[0].findSrcScanSlot();
            }

            // No known source expr, or there are several source exprs meaning the slot is
            // has no single source table.
            return null;
        }

        /**
         * Pushes negation to the individual operands of a predicate
         * tree rooted at 'root'.
         */
        public static Expr pushNegationToOperands(Expr root)
        {
            //Preconditions.checkNotNull(root);
            if (Expr.IS_NOT_PREDICATE.Invoke(root))
            {
                try
                {
                    // Make sure we call function 'negate' only on classes that support it,
                    // otherwise we may recurse infinitely.
                    MethodInfo m = typeof(Expr).GetMethod(NEGATE_FN);
                    return pushNegationToOperands(root.getChild(0).negate());
                }
                catch (ArgumentNullException)
                {
                    // The 'negate' function is not implemented. Break the recursion.
                    return root;
                }
            }

            if (root is CompoundPredicate)
            {
                Expr left = pushNegationToOperands(root.getChild(0));
                Expr right = pushNegationToOperands(root.getChild(1));
                CompoundPredicate compoundPredicate =
                    new CompoundPredicate(((CompoundPredicate) root).getOp(), left, right);
                compoundPredicate.setPrintSqlInParens(root.getPrintSqlInParens());
                return compoundPredicate;
            }

            return root;
        }

        /**
         * Negates a bool Expr.
         */
        public Expr negate()
        {
            //Preconditions.checkState(type_.getPrimitiveType() == PrimitiveType.bool);
            return new CompoundPredicate(CompoundPredicate.Operator.NOT, this, null);
        }

        /**
         * Returns the subquery of an expr. Returns null if this expr does not contain
         * a subquery.
         *
         * TODO: Support predicates with more that one subqueries when we implement
         * the independent subquery evaluation.
         */
        public Subquery getSubquery()
        {
            if (!contains(Subquery)) return null;
            List<Subquery> subqueries = new List<Subquery>();
            collect(Subquery, subqueries);
            //Preconditions.checkState(subqueries.Count == 1);
            return subqueries.FirstOrDefault();
        }

        /**
         * Returns true iff all of this Expr's children have their costs set.
         */
        protected bool hasChildCosts()
        {
            foreach (Expr child in children_)
            {
                if (!child.hasCost()) return false;
            }

            return true;
        }

        /**
         * Computes and returns the sum of the costs of all of this Expr's children.
         */
        protected float getChildCosts()
        {
            float cost = 0;
            foreach (Expr child in children_) cost += child.getCost();
            return cost;
        }

        /**
         * Returns the average Length of the values produced by an Expr
         * of type string. Returns a default for unknown lengths.
         */
        protected static double getAvgStringLength(Expr e)
        {
            //Preconditions.checkState(e.getType().isStringType());
            //Preconditions.checkState(e.isAnalyzed_);

            SlotRef slRef = e.unwrapSlotRef(false);
            if (slRef != null)
            {
                if (slRef.getDesc() != null && slRef.getDesc().getStats().getAvgSize() > 0)
                {
                    return slRef.getDesc().getStats().getAvgSize();
                }
                else
                {
                    return DEFAULT_AVG_STRING_LENGTH;
                }
            }
            else if (e is StringLiteral)
            {
                return ((StringLiteral) e).getValue().Length;
            }
            else
            {
                // TODO(tmarshall): Extend this to support other string Exprs, such as
                // function calls that return string.
                return DEFAULT_AVG_STRING_LENGTH;
            }
        }

        /**
         * Generates a comma-separated string from the toSql() string representations of
         * 'exprs'.
         */
        public static string listToSql(List<Expr> exprs)
        {
            List<string> replies = new List<string>();
            foreach (Expr expr in exprs)
            {
                replies.Add(expr.toSql());
            }

            return string.Join(",", replies);
        }
    }
}