using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using TypeCobol.Compiler.SqlNodes;
using TypeCobol.Compiler.SqlNodes.Authorization;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    class FunctionCallExpr : Expr
    {
        private readonly FunctionName fnName_;
        private readonly FunctionParams params_;
        private bool isAnalyticFnCall_ = false;
        private bool isInternalFnCall_ = false;

        // Non-null iff this is an aggregation function that executes the Merge() step. This
        // is an analyzed clone of the FunctionCallExpr that executes the Update() function
        // feeding into this Merge(). This is stored so that we can access the types of the
        // original input argument exprs. Note that the nullness affects the behaviour of
        // resetAnalysisState(), which is used during expr substitution.
        private FunctionCallExpr mergeAggInputFn_;

        // Printed in toSqlImpl(), if set. Used for merge agg fns.
        private String label_;

        public FunctionCallExpr(String functionName, List<Expr> parameters) :
            this(new FunctionName(functionName), new FunctionParams(false, parameters))
        {
        }

        public FunctionCallExpr(FunctionName fnName, List<Expr> parameters) : this(fnName,
            new FunctionParams(false, parameters))
        {
        }

        public FunctionCallExpr(FunctionName fnName, FunctionParams parameters) : this
            (fnName, parameters, null)
        {
        }

        private FunctionCallExpr(FunctionName fnName, FunctionParams parameters,
            FunctionCallExpr mergeAggInputFn)
        {
            fnName_ = fnName;
            params_ = parameters;
            mergeAggInputFn_ =
                (FunctionCallExpr) mergeAggInputFn?.clone();
            if (parameters.exprs() != null) children_ = new List<Expr>(parameters.exprs());
        }

        /**
         * Returns an Expr that evaluates the function call <fnName>(<params>). The returned
         * Expr is not necessarily a FunctionCallExpr (example: DECODE())
         */
        public static Expr createExpr(FunctionName fnName, FunctionParams parameters)
        {
            FunctionCallExpr functionCallExpr = new FunctionCallExpr(fnName, parameters);
            if (functionNameEqualsBuiltin(fnName, "decode"))
            {
                return new CaseExpr(functionCallExpr);
            }

            if (functionNameEqualsBuiltin(fnName, "nvl2"))
            {
                List<Expr> plist = new List<Expr>(parameters.exprs());
                if (!plist.IsNullOrEmpty())
                {
                    plist[0] = new IsNullPredicate(plist[0], true));
                }

                return new FunctionCallExpr("if", plist);
            }

            // nullif(x, y) -> if(x DISTINCT FROM y, x, NULL)
            if (functionNameEqualsBuiltin(fnName, "nullif") && parameters.   Count == 2)
            {
                return new FunctionCallExpr("if", Lists.newArrayList(
                    new BinaryPredicate(BinaryPredicate.Operator.DISTINCT_FROM, parameters.exprs()[0],
                        parameters.exprs()[1]), // x IS DISTINCT FROM y
                    parameters.exprs()[0], // x
                    new NullLiteral() // NULL
                ));
            }

            return functionCallExpr;
        }

        /** Returns true if fnName is a built-in with given name. */
        private static bool functionNameEqualsBuiltin(FunctionName fnName, String name)
        {
            return fnName.getFnNamePath().Count == 1
                   && fnName.getFnNamePath()[0].Equals(name, StringComparison.OrdinalIgnoreCase)
                   || fnName.getFnNamePath().Count == 2
                   && fnName.getFnNamePath()[0].Equals(BuiltinsDb.NAME)
                   && fnName.getFnNamePath()[1].Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        /**
         * Returns a new function call expr on the given parameters for performing the merge()
         * step of the given aggregate function.
         */
        public static FunctionCallExpr createMergeAggCall(
            FunctionCallExpr agg, List<Expr> parameters)
        {
            //Preconditions.checkState(agg.isAnalyzed());
            //Preconditions.checkState(agg.isAggregateFunction());
            // If the input aggregate function is already a merge aggregate function (due to
            // 2-phase aggregation), its input types will be the intermediate value types. The
            // original input argument exprs are in 'agg.mergeAggInputFn_' so use it instead.
            FunctionCallExpr mergeAggInputFn = agg.isMergeAggFn() ? agg.mergeAggInputFn_ : agg;
            FunctionCallExpr result = new FunctionCallExpr(
                agg.fnName_, new FunctionParams(false, parameters), mergeAggInputFn);
            // Inherit the function object from 'agg'.
            result.fn_ = agg.fn_;
            result.type_ = agg.type_;
            // Set an explicit label based on the input agg.
            if (agg.isMergeAggFn())
            {
                result.label_ = agg.label_;
            }
            else
            {
                // fn(input) becomes fn:merge(input).
                result.label_ = agg.toSql().Replace(agg.fnName_.ToString(),
                    agg.fnName_.ToString() + ":merge");
            }

            //Preconditions.checkState(!result.type_.isWildcardDecimal());
            return result;
        }

        /**
         * Copy c'tor used in clone().
         */
        protected FunctionCallExpr(FunctionCallExpr other)
        {
            //   super(other);
            fnName_ = other.fnName_;
            isAnalyticFnCall_ = other.isAnalyticFnCall_;
            isInternalFnCall_ = other.isInternalFnCall_;
            mergeAggInputFn_ = other.mergeAggInputFn_ == null
                ? null
                : (FunctionCallExpr) other.mergeAggInputFn_.clone();
            // Clone the params in a way that keeps the children_ and the params.exprs()
            // in sync. The children have already been cloned in the super c'tor.
            if (other.params_.isStar())
            {
                //Preconditions.checkState(children_.isEmpty());
                params_ = FunctionParams.createStarParam();
            }
            else
            {
                params_ = new FunctionParams(other.params_.isDistinct(),
                    other.params_.isIgnoreNulls(), children_);
            }

            label_ = other.label_;
        }

        public bool isMergeAggFn()
        {
            return mergeAggInputFn_ != null;
        }

        public override void ResetAnalysisState()
        {
            base.resetAnalysisState();
            // Resolving merge agg functions after substitution may fail e.g., if the
            // intermediate agg type is not the same as the output type. Preserve the original
            // fn_ such that analyze() hits the special-case code for merge agg fns that
            // handles this case.
            if (!isMergeAggFn()) fn_ = null;
        }

        public override bool LocalEquals(Expr that)
        {
            if (!base.localEquals(that)) return false;
            FunctionCallExpr o = (FunctionCallExpr) that;
            return fnName_.Equals(o.fnName_) &&
                   params_.isDistinct() == o.params_.isDistinct() &&
                   params_.isIgnoreNulls() == o.params_.isIgnoreNulls() &&
                   params_.isStar() == o.params_.isStar();
        }


        public override String ToSqlImpl()
        {
            if (label_ != null) return label_;
            // Merge agg fns should have an explicit label.
            //Preconditions.checkState(!isMergeAggFn());
            StringBuilder sb = new StringBuilder();
            sb.Append(fnName_).Append("(");
            if (params_.isStar()) sb.Append("*");
            if (params_.isDistinct()) sb.Append("DISTINCT ");
            sb.Append(string.Join(", ", childrenToSql()));
            if (params_.isIgnoreNulls()) sb.Append(" IGNORE NULLS");
            sb.Append(")");
            return sb.ToString();
        }


        public override String DebugString()
        {
            return "name" + fnName_
                          + "isStar" + params_.isStar()
                          + "isDistinct" + params_.isDistinct()
                          + "isIgnoreNulls" + params_.isIgnoreNulls()
                          + base.debugString();
        }

        public FunctionParams getParams()
        {
            return params_;
        }

        public bool isScalarFunction()
        {
            //Preconditions.checkNotNull(fn_);
            return fn_ is ScalarFunction;
        }

        public SqlNodeType getReturnType()
        {
            //Preconditions.checkNotNull(fn_);
            return fn_.getReturnType();
        }

        /**
         * Returns true if this is a call to a non-analytic aggregate function.
         */
        public bool isAggregateFunction()
        {
            //Preconditions.checkNotNull(fn_);
            return fn_ is AggregateFunction && !isAnalyticFnCall_;
        }

        /**
         * Returns true if this is a call to an aggregate function that returns
         * non-null on an empty input (e.g. count).
         */
        public bool returnsNonNullOnEmpty()
        {
            //Preconditions.checkNotNull(fn_);
            return fn_ is AggregateFunction &&
                   ((AggregateFunction) fn_).returnsNonNullOnEmpty();
        }

        public bool isDistinct()
        {
            //Preconditions.checkState(isAggregateFunction());
            return params_.isDistinct();
        }

        public bool ignoresDistinct()
        {
            //Preconditions.checkState(isAggregateFunction());
            return ((AggregateFunction) fn_).ignoresDistinct();
        }

        public FunctionName getFnName()
        {
            return fnName_;
        }

        public void setIsAnalyticFnCall(bool v)
        {
            isAnalyticFnCall_ = v;
        }

        public void setIsInternalFnCall(bool v)
        {
            isInternalFnCall_ = v;
        }

        static bool isNondeterministicBuiltinFnName(String fnName)
        {
            return fnName.Equals("rand", StringComparison.OrdinalIgnoreCase) ||
                   fnName.Equals("random", StringComparison.OrdinalIgnoreCase) ||
                   fnName.Equals("uuid", StringComparison.OrdinalIgnoreCase);
        }

        /**
         * Returns true if function is a non-deterministic builtin function, i.e. for a fixed
         * input, it may not always produce the same output for every invocation.
         * Functions that use randomness or variable runtime state are non-deterministic.
         * This only applies to builtin functions, and does not provide any information
         * about user defined functions.
         */
        public bool isNondeterministicBuiltinFn()
        {
            String fnName = fnName_.getFunction();
            return isNondeterministicBuiltinFnName(fnName);
        }


        protected override bool IsConstantImpl()
        {
            // TODO: we can't correctly determine const-ness before analyzing 'fn_'. We should
            // rework logic so that we do not call this function on unanalyzed exprs.
            // Aggregate functions are never constant.
            if (fn_ is AggregateFunction) return false;

            String fnName = fnName_.getFunction();
            if (fnName == null)
            {
                // This expr has not been analyzed yet, get the function name from the path.
                List<String> path = fnName_.getFnNamePath();
                fnName = path[path.Count - 1];
            }

            // Non-deterministic functions are never constant.
            if (isNondeterministicBuiltinFnName(fnName))
            {
                return false;
            }

            // Sleep is a special function for testing.
            return !fnName.Equals("sleep", StringComparison.OrdinalIgnoreCase) && base.isConstantImpl();
        }

        public override Expr IgnoreImplicitCast()
        {
            throw new NotImplementedException();
        }

        // Provide better error message for some aggregate builtins. These can be
        // a bit more user friendly than a generic function not found.
        // TODO: should we bother to do this? We could also improve the general
        // error messages. For example, listing the alternatives.
        protected String getFunctionNotFoundError(SqlNodeType[] argTypes)
        {
            if (fnName_.isBuiltin())
            {
                // Some custom error message for builtins
                if (params_.isStar())
                {
                    return "'*' can only be used in conjunction with COUNT";
                }

                if (fnName_.getFunction().Equals("count", StringComparison.OrdinalIgnoreCase))
                {
                    if (!params_.isDistinct() && argTypes.Length > 1)
                    {
                        return "COUNT must have DISTINCT for multiple arguments: " + toSql();
                    }
                }

                if (fnName_.getFunction().Equals("sum", StringComparison.OrdinalIgnoreCase))
                {
                    return "SUM requires a numeric parameter: " + toSql();
                }

                if (fnName_.getFunction().Equals("avg", StringComparison.OrdinalIgnoreCase))
                {
                    return "AVG requires a numeric or timestamp parameter: " + toSql();
                }
            }

            String[] argTypesSql = new String[argTypes.Length];
            for (int i = 0; i < argTypes.Length; ++i)
            {
                argTypesSql[i] = argTypes[i].toSql();
            }

            return String.Format(
                "No matching function with signature: {0}({1}).",
                fnName_, params_.isStar() ? "*" : string.Join(", ", argTypesSql));
        }

        /**
         * Builtins that return decimals are specified as the wildcard decimal(decimal(*,*))
         * and the specific decimal can only be determined based on the inputs. We currently
         * don't have a mechanism to specify this with the UDF interface. Until we add
         * that (i.e. allowing UDFs to participate in the planning phase), we will
         * manually resolve the wildcard types for the few functions that need it.
         * This can only be called for functions that return wildcard decimals and the first
         * argument is a wildcard decimal.
         * TODO: this prevents UDFs from using wildcard decimals and is in general not scalable.
         * We should add a prepare_fn() to UDFs for doing this.
         */
        private SqlNodeType resolveDecimalReturnType(Analyzer analyzer)
        {
            //Preconditions.checkState(type_.isWildcardDecimal());
            //Preconditions.checkState(fn_.getBinaryType() == TFunctionBinaryType.BUILTIN);
            //Preconditions.checkState(children_.Count > 0);

            // Find first decimal input (some functions, such as if(), begin with non-decimal
            // arguments).
            ScalarType childType = null;
            foreach (Expr child in children_)
            {
                if (child.type_.isDecimal())
                {
                    childType = (ScalarType) child.type_;
                    break;
                }
            }

            //Preconditions.checkState(childType != null && !childType.isWildcardDecimal());
            SqlNodeType returnType = childType;

            if (fnName_.getFunction().Equals("sum", StringComparison.OrdinalIgnoreCase))
            {
                return childType.getMaxResolutionType();
            }

            int digitsBefore = childType.decimalPrecision() - childType.decimalScale();
            int digitsAfter = childType.decimalScale();
            if (fnName_.getFunction().Equals("avg", StringComparison.OrdinalIgnoreCase) &&
                analyzer.getQueryOptions().isDecimal_v2())
            {
                // AVG() always gets at least MIN_ADJUSTED_SCALE decimal places since it performs
                // an implicit divide. The output type isn't always the same as SUM()/COUNT().
                // Scale is set the same as MS SQL Server, which takes the max of the input scale
                // and MIN_ADJUST_SCALE. For precision, MS SQL always sets it to 38. We choose to
                // trim it down to the size that's needed because the absolute value of the result
                // is less than the absolute value of the largest input. Using a smaller precision
                // allows for better DECIMAL types to be chosen for the overall expression when
                // AVG() is a subexpression. For DECIMAL_V1, we set the output type to be the same
                // as the input type.
                int resultScale = Math.Max(ScalarType.MIN_ADJUSTED_SCALE, digitsAfter);
                int resultPrecision = digitsBefore + resultScale;
                return ScalarType.createAdjustedDecimalType(resultPrecision, resultScale);
            }
            else if (fnName_.getFunction().Equals("ceil", StringComparison.OrdinalIgnoreCase) ||
                     fnName_.getFunction().Equals("ceiling", StringComparison.OrdinalIgnoreCase) ||
                     fnName_.getFunction().Equals("floor") ||
                     fnName_.getFunction().Equals("dfloor"))
            {
                // These functions just return with scale 0 but can trigger rounding. We need
                // to increase the precision by 1 to handle that.
                ++digitsBefore;
                digitsAfter = 0;
            }
            else if (fnName_.getFunction().Equals("truncate", StringComparison.OrdinalIgnoreCase) ||
                     fnName_.getFunction().Equals("dtrunc", StringComparison.OrdinalIgnoreCase) ||
                     fnName_.getFunction().Equals("trunc", StringComparison.OrdinalIgnoreCase) ||
                     fnName_.getFunction().Equals("round", StringComparison.OrdinalIgnoreCase) ||
                     fnName_.getFunction().Equals("dround", StringComparison.OrdinalIgnoreCase))
            {
                if (children_.Count > 1)
                {
                    // The second argument to these functions is the desired scale, otherwise
                    // the default is 0.
                    //Preconditions.checkState(children_.Count == 2);
                    if (children_[1].isNullLiteral())
                    {
                        analyzer.addWarning(fnName_.getFunction() +
                         "() cannot be called with a NULL second argument.");
                    }

                    if (!children_[1].isConstant())
                    {
                        // We don't allow calling truncate or round with a non-constant second
                        // (desired scale) argument. e.g. select round(col1, col2). This would
                        // mean we don't know the scale of the resulting type and would need some
                        // kind of dynamic type handling which is not yet possible. This seems like
                        // a reasonable restriction.
                        analyzer.addWarning(fnName_.getFunction() +
                         "() must be called with a constant second argument.");
                    }

                    NumericLiteral scaleLiteral = (NumericLiteral) LiteralExpr.create(
                        children_[1], analyzer.getQueryCtx());
// If scale is greater than the scale of the decimal, this should be a no-op,
// so we do not need change the scale of the output decimal.
                    digitsAfter = Math.Min(digitsAfter, (int) scaleLiteral.getLongValue());
                    //Preconditions.checkState(digitsAfter <= ScalarType.MAX_SCALE);
                    // Round/Truncate to a negative scale means to round to the digit before
                    // the decimal e.g. round(1234.56, -2) would be 1200.
                    // The resulting scale is always 0.
                    digitsAfter = Math.Max(digitsAfter, 0);
                }
                else
                {
                    // Round()/Truncate() with no second argument.
                    digitsAfter = 0;
                }

                if ((fnName_.getFunction().Equals("round", StringComparison.OrdinalIgnoreCase) ||
                     fnName_.getFunction().Equals("dround", StringComparison.OrdinalIgnoreCase)) &&
                    digitsAfter < childType.decimalScale())
                {
                    // If we are rounding to fewer decimal places, it's possible we need another
                    // digit before the decimal if the value gets rounded up.
                    ++digitsBefore;
                }
            }

            //Preconditions.checkState(returnType.isDecimal() && !returnType.isWildcardDecimal());
            if (analyzer.isDecimalV2())
            {
                if (digitsBefore + digitsAfter > 38) return SqlNodeType.INVALID;
                return ScalarType.createDecimalType(digitsBefore + digitsAfter, digitsAfter);
            }

            return ScalarType.createClippedDecimalType(digitsBefore + digitsAfter, digitsAfter);
        }

        //TODO - check how to change FeDb
        protected void analyzeImpl(Analyzer analyzer)
        {
            fnName_.analyze(analyzer);

            if (isMergeAggFn())
            {
                // This is the function call expr after splitting up to a merge aggregation.
                // The function has already been analyzed so just do the minimal sanity
                // check here.
                AggregateFunction aggFn = (AggregateFunction) fn_;
                //Preconditions.checkNotNull(aggFn);
                SqlNodeType intermediateType = aggFn.getIntermediateType();
                if (intermediateType == null) intermediateType = type_;
                //Preconditions.checkState(!type_.isWildcardDecimal());
                return;
            }

            // User needs DB access.
            FeDb db = analyzer.getDb(fnName_.getDb(), Privilege.VIEW_METADATA, true);
            if (!db.containsFunction(fnName_.getFunction()))
            {
                analyzer.addWarning(fnName_ + "() unknown");
            }

            if (fnName_.getFunction().Equals("count") && params_.isDistinct())
            {
                // Treat COUNT(DISTINCT ...) special because of how we do the rewrite.
                // There is no version of COUNT() that takes more than 1 argument but after
                // the rewrite, we only need count(*).
                // TODO: fix how we rewrite count distinct.
                Function searchDesc = new Function(fnName_, new SqlNodeType[0], SqlNodeType.INVALID, false);
                fn_ = db.getFunction(searchDesc, Function.CompareMode.IS_NONSTRICT_SUPERTYPE_OF);
                type_ = fn_.getReturnType();
                // Make sure BE doesn't see any TYPE_NULL exprs
                for (int i = 0; i < children_.Count; ++i)
                {
                    if (getChild(i).getType().isNull())
                    {
                        uncheckedCastChild(ScalarType.BOOLEAN, i);
                    }
                }

                return;
            }

            // TODO: We allow implicit cast from string->timestamp but only
            // support avg(timestamp). This means avg(string_col) would work
            // from our casting rules. This is not right.
            // We need to revisit where implicit casts are allowed for string
            // to timestamp
            if (fnName_.getFunction().Equals("avg", StringComparison.OrdinalIgnoreCase) &&
                children_.Count == 1 && children_[0].getType().isStringType())
            {
                analyzer.addWarning(
                    "AVG requires a numeric or timestamp parameter: " + toSql());
            }

            // SAMPLED_NDV() is only valid with two children. Invocations with an invalid number
            // of children are gracefully handled when resolving the function signature.
            if (fnName_.getFunction().Equals("sampled_ndv", StringComparison.OrdinalIgnoreCase)
                && children_.Count == 2)
            {
                if (!(children_[1] is NumericLiteral))
                {
                    analyzer.addWarning(
                        "Second parameter of SAMPLED_NDV() must be a numeric literal in [0,1]: " +
                        children_[1].toSql());
                }

                NumericLiteral samplePerc = (NumericLiteral) children_[1];
                if (samplePerc.getDoubleValue() < 0 || samplePerc.getDoubleValue() > 1.0)
                {
                    analyzer.addWarning(
                        "Second parameter of SAMPLED_NDV() must be a numeric literal in [0,1]: " +
                        samplePerc.toSql());
                }

                // Numeric literals with a decimal point are analyzed as decimals. Without this
                // cast we might resolve to the wrong function because there is no exactly
                // matching signature with decimal as the second argument.
                children_[1] = samplePerc.uncheckedCastTo(SqlNodeType.DOUBLE);
            }

            SqlNodeType[] argTypes = collectChildReturnTypes();
            Function searchDesc = new Function(fnName_, argTypes, SqlNodeType.INVALID, false);
            fn_ = db.getFunction(searchDesc, Function.CompareMode.IS_NONSTRICT_SUPERTYPE_OF);
            if (fn_ == null || (!isInternalFnCall_ && !fn_.userVisible()))
            {
                analyzer.addWarning(getFunctionNotFoundError(argTypes));
            }

            if (isAggregateFunction())
            {
                // subexprs must not contain aggregates
                if (TreeNode<Expr>.contains(children_, Expr.isAggregatePredicate()))
                {
                    analyzer.addWarning(
                        "aggregate function must not contain aggregate parameters: " + toSql());
                }

                // .. or analytic exprs
                //TODO - adapt this call 
                if (Expr.contains(children_, AnalyticExpr.GetType()))
                {

                    analyzer.addWarning("aggregate function must not contain analytic parameters: " + toSql());
                }

                // The catalog contains count() with no arguments to handle count(*) but don't
                // accept count().
                // TODO: can this be handled more cleanly. It does seem like a special case since
                // no other aggregate functions (currently) can accept '*'.
                if (fnName_.getFunction().Equals("count", StringComparison.OrdinalIgnoreCase) &&
                    !params_.isStar() && children_.Count == 0)
                {
                    analyzer.addWarning("count() is not allowed.");
                }

                // TODO: the distinct rewrite does not handle this but why?
                if (params_.isDistinct())
                {
                    // The second argument in group_concat(distinct) must be a constant expr that
                    // returns a string.
                    if (fnName_.getFunction().Equals("group_concat", StringComparison.OrdinalIgnoreCase)
                        && getChildren().Count == 2
                        && !getChild(1).isConstant())
                    {
                        analyzer.addWarning("Second parameter in GROUP_CONCAT(DISTINCT)" +
                                            " must be a constant expression that returns a string.");
                    }

                    if (fn_.getBinaryType() != TFunctionBinaryType.BUILTIN)
                    {
                        analyzer.addWarning("User defined aggregates do not support DISTINCT.");
                    }
                }

                AggregateFunction aggFn = (AggregateFunction) fn_;
                if (aggFn.ignoresDistinct()) params_.setIsDistinct(false);
            }

            if (params_.isIgnoreNulls() && !isAnalyticFnCall_)
            {
                analyzer.addWarning("Function " + fnName_.getFunction().ToUpperInvariant()
                                                + " does not accept the keyword IGNORE NULLS.");
            }

            if (isScalarFunction()) validateScalarFnParams(params_);
            if (fn_ is AggregateFunction
                && ((AggregateFunction) fn_).isAnalyticFn()
                && !((AggregateFunction) fn_).isAggregateFn()
                && !isAnalyticFnCall_)
            {
                analyzer.addWarning("Analytic function requires an OVER clause: " + toSql());
            }

            castForFunctionCall(false, analyzer.isDecimalV2());
            type_ = fn_.getReturnType();
            if (type_.isDecimal() && type_.isWildcardDecimal())
            {
                type_ = resolveDecimalReturnType(analyzer);
            }

            // We do not allow any function to return a type CHAR or VARCHAR
            // TODO add support for CHAR(N) and VARCHAR(N) return values in post 2.0,
            // support for this was not added to the backend in 2.0
            if (type_.isWildcardChar() || type_.isWildcardVarchar())
            {
                type_ = ScalarType.STRING;
            }
        }

        //@Override
        protected float computeEvalCost()
        {
            // TODO(tmarshall): Differentiate based on the specific function.
            return hasChildCosts() ? getChildCosts() + FUNCTION_CALL_COST : UNKNOWN_COST;
        }

        public FunctionCallExpr getMergeAggInputFn()
        {
            return mergeAggInputFn_;
        }

        public void setMergeAggInputFn(FunctionCallExpr fn)
        {
            mergeAggInputFn_ = fn;
        }

/**
 * Checks that no special aggregate params are included in 'params' that would be
 * invalid for a scalar function. Analysis of the param exprs is not done.
 */
        static void validateScalarFnParams(FunctionParams parameters)

        {
            if (parameters.isStar())
            {
                ("Cannot pass '*' to scalar function.");
            }

            if (parameters.isDistinct())
            {
                ("Cannot pass 'DISTINCT' to scalar function.");
            }
        }

/**
 * Validate that the internal state, specifically types, is consistent between the
 * the Update() and Merge() aggregate functions.
 */
        void validateMergeAggFn(FunctionCallExpr inputAggFn)
        {
            //Preconditions.checkState(isMergeAggFn());
            List<Expr> copiedInputExprs = mergeAggInputFn_.getChildren();
            List<Expr> inputExprs = inputAggFn.isMergeAggFn()
                ? inputAggFn.mergeAggInputFn_.getChildren()
                : inputAggFn.getChildren();
            //Preconditions.checkState(copiedInputExprs.Count == inputExprs.Count);
            for (int i = 0; i < inputExprs.Count; ++i)
            {
                SqlNodeType copiedInputType = copiedInputExprs[i].getType();
                SqlNodeType inputType = inputExprs[i].getType();
                //Preconditions.checkState(copiedInputType.Equals(inputType),
                //    String.Format("Copied expr %s arg type %s differs from input expr type %s " +
                //      "in original expr %s", toSql(), copiedInputType.toSql(),
                //      inputType.toSql(), inputAggFn.toSql()));
            }
        }

//@Override
        public override Expr clone()
        {
            return new FunctionCallExpr(this);
        }

        public override bool IsBoundByTupleIds(List<int> tids)
        {
            throw new NotImplementedException();
        }

        public override bool IsBoundBySlotIds(List<SlotId> slotIds)
        {
            throw new NotImplementedException();
        }

        public override void GetIdsHelper(HashSet<int> tupleIds, HashSet<SlotId> slotIds)
        {
            throw new NotImplementedException();
        }

        //@Override
        protected Expr substituteImpl(ExprSubstitutionMap smap, Analyzer analyzer)
        {
            Expr e = base.substituteImpl(smap, analyzer);
            if (!(e is FunctionCallExpr)) return e;
            FunctionCallExpr fn = (FunctionCallExpr) e;
            FunctionCallExpr mergeFn = fn.getMergeAggInputFn();
            if (mergeFn != null)
            {
                // The merge function needs to be substituted as well.
                Expr substitutedFn = mergeFn.substitute(smap, analyzer, true);
                //Preconditions.checkState(substitutedFn is FunctionCallExpr);
                fn.setMergeAggInputFn((FunctionCallExpr) substitutedFn);
            }

            return e;
        }
    }
}
