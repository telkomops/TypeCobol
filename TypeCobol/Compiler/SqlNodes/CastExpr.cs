using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class CastExpr : Expr
    {
        public override string ToSqlImpl()
        {
            if (isImplicit_) return getChild(0).toSql();
            return "CAST(" + getChild(0).toSql() + " AS " + targetTypeDef_.ToString() + ")";
        }

        public override bool LocalEquals(Expr that)
        {
            if (!base.localEquals(that)) return false;
            CastExpr other = (CastExpr) that;
            return isImplicit_ == other.isImplicit_
                   && type_.Equals(other.type_);
            throw new NotImplementedException();
        }

        public override void ResetAnalysisState()
        {
            base.resetAnalysisState();
        }

        public override Expr clone()
        {
            return new CastExpr(this);
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

        protected override bool IsConstantImpl()
        {
            return base.isConstantImpl();
        }

        // Only set for explicit casts. Null for implicit casts.
        private readonly TypeDef targetTypeDef_;

        // True if this is a "pre-analyzed" implicit cast.
        private readonly bool isImplicit_;

        // True if this cast does not change the type.
        private bool noOp_ = false;

        /**
         * C'tor for "pre-analyzed" implicit casts.
         */
        public CastExpr(SqlNodeType targetType, Expr e) : base()
        {
            //Preconditions.checkState(targetType.isValid());
            //Preconditions.checkNotNull(e);
            type_ = targetType;
            targetTypeDef_ = null;
            isImplicit_ = true;
            // replace existing implicit casts
            if (e is CastExpr)
            {
                CastExpr castExpr = (CastExpr) e;
                if (castExpr.isImplicit()) e = castExpr.getChild(0);
            }

            children_.Add(e);

            // Implicit casts don't call analyze()
            // TODO: this doesn't seem like the cleanest approach but there are places
            // we generate these (e.g. table loading) where there is no analyzer object.
            try
            {
                analyze();
                computeNumDistinctValues();
                evalCost_ = computeEvalCost();
            }
            catch (AnalysisException)
            {
                //Preconditions.checkState(false,
                //    "Implicit casts should never throw analysis exception.");
            }

            analysisDone();
        }

        /**
         * C'tor for explicit casts.
         */
        public CastExpr(TypeDef targetTypeDef, Expr e)
        {
            //Preconditions.checkNotNull(targetTypeDef);
            //Preconditions.checkNotNull(e);
            isImplicit_ = false;
            targetTypeDef_ = targetTypeDef;
            children_.Add(e);
        }

        /**
         * Copy c'tor used in clone().
         */
        protected CastExpr(CastExpr other) : base(other)
        {
            targetTypeDef_ = other.targetTypeDef_;
            isImplicit_ = other.isImplicit_;
            noOp_ = other.noOp_;
        }

        private static string getFnName(SqlNodeType targetType)
        {
            return "castTo" + targetType.getPrimitiveType().ToString();
        }

        public static void initBuiltins(Db db)
        {
            foreach (SqlNodeType fromType in SqlNodeType.getSupportedTypes())
            {
                if (fromType.isNull()) continue;
                foreach (SqlNodeType toType in SqlNodeType.getSupportedTypes())
                {
                    string beSymbol;
                    if (toType.isNull()) continue;
                    // Disable casting from string to bool
                    if (fromType.isStringType() && toType.isBoolean()) continue;
                    // Disable casting from bool/timestamp to decimal
                    if ((fromType.isBoolean() || fromType.isDateType()) && toType.isDecimal())
                    {
                        continue;
                    }

                    if (fromType.getPrimitiveType() == PrimitiveType.STRING
                        && toType.getPrimitiveType() == PrimitiveType.CHAR)
                    {
                        // Allow casting from string to Char(N)
                        beSymbol = "impala::CastFunctions::CastToChar";
                        db.addBuiltin(ScalarFunction.createBuiltin(getFnName(ScalarType.CHAR),
                            Lists.newArrayList((SqlNodeType) ScalarType.STRING), false, ScalarType.CHAR,
                            beSymbol, null, null, true));
                        continue;
                    }

                    if (fromType.getPrimitiveType() == PrimitiveType.CHAR
                        && toType.getPrimitiveType() == PrimitiveType.CHAR)
                    {
                        // Allow casting from CHAR(N) to Char(N)
                        beSymbol = "impala::CastFunctions::CastToChar";
                        db.addBuiltin(ScalarFunction.createBuiltin(getFnName(ScalarType.CHAR),
                            Lists.newArrayList((SqlNodeType) ScalarType.createCharType(-1)), false,
                            ScalarType.CHAR, beSymbol, null, null, true));
                        continue;
                    }

                    if (fromType.getPrimitiveType() == PrimitiveType.VARCHAR
                        && toType.getPrimitiveType() == PrimitiveType.VARCHAR)
                    {
                        // Allow casting from VARCHAR(N) to VARCHAR(M)
                        beSymbol = "impala::CastFunctions::CastToStringVal";
                        db.addBuiltin(ScalarFunction.createBuiltin(getFnName(ScalarType.VARCHAR),
                            Lists.newArrayList((SqlNodeType) ScalarType.VARCHAR), false, ScalarType.VARCHAR,
                            beSymbol, null, null, true));
                        continue;
                    }

                    if (fromType.getPrimitiveType() == PrimitiveType.VARCHAR
                        && toType.getPrimitiveType() == PrimitiveType.CHAR)
                    {
                        // Allow casting from VARCHAR(N) to CHAR(M)
                        beSymbol = "impala::CastFunctions::CastToChar";
                        db.addBuiltin(ScalarFunction.createBuiltin(getFnName(ScalarType.CHAR),
                            Lists.newArrayList((SqlNodeType) ScalarType.VARCHAR), false, ScalarType.CHAR,
                            beSymbol, null, null, true));
                        continue;
                    }

                    if (fromType.getPrimitiveType() == PrimitiveType.CHAR
                        && toType.getPrimitiveType() == PrimitiveType.VARCHAR)
                    {
                        // Allow casting from CHAR(N) to VARCHAR(M)
                        beSymbol = "impala::CastFunctions::CastToStringVal";
                        db.addBuiltin(ScalarFunction.createBuiltin(getFnName(ScalarType.VARCHAR),
                            Lists.newArrayList((SqlNodeType) ScalarType.CHAR), false, ScalarType.VARCHAR,
                            beSymbol, null, null, true));
                        continue;
                    }

                    // Disable no-op casts
                    if (fromType.Equals(toType) && !fromType.isDecimal()) continue;
                    string beClass = toType.isDecimal() || fromType.isDecimal() ? "DecimalOperators" : "CastFunctions";
                    beSymbol = "impala::" + beClass + "::CastTo" + Function.getUdfType(toType);
                    db.addBuiltin(ScalarFunction.createBuiltin(getFnName(toType),
                        Lists.newArrayList(fromType), false, toType, beSymbol,
                        null, null, true));
                }
            }
        }


        //      @Override
        //protected void treeToThriftHelper(TExpr container)
        //      {
        //          if (noOp_)
        //          {
        //              getChild(0).treeToThriftHelper(container);
        //              return;
        //          }
        //          super.treeToThriftHelper(container);
        //      }

        //      @Override
        //protected void toThrift(TExprNode msg)
        //      {
        //          msg.node_type = TExprNodeType.FUNCTION_CALL;
        //      }

        public override string DebugString()
        {
            return "isImplicit" + isImplicit_ +
                   "target" + type_ +
                   base.debugString();
        }

        public bool isImplicit()
        {
            return isImplicit_;
        }

        protected void analyzeImpl(Analyzer analyzer)
        {
            //Preconditions.checkState(!isImplicit_);
            targetTypeDef_.analyze(analyzer);
            type_ = targetTypeDef_.getType();
            analyze();
        }

        //@Override
        protected float computeEvalCost()
        {
            return getChild(0).hasCost() ? getChild(0).getCost() + CAST_COST : UNKNOWN_COST;
        }

        private void analyze()
        {
            //Preconditions.checkNotNull(type_);
            if (type_.isComplexType())
            {
                throw new AnalysisException(
                    "Unsupported cast to complex type: " + type_.toSql());
            }

            bool readyForCharCast =
                children_[0].getType().getPrimitiveType() == PrimitiveType.STRING ||
                children_[0].getType().getPrimitiveType() == PrimitiveType.CHAR;
            if (type_.getPrimitiveType() == PrimitiveType.CHAR && !readyForCharCast)
            {
                // Back end functions only exist to cast string types to CHAR, there is not a cast
                // for every type since it is redundant with STRING. Casts to go through 2 casts:
                // (1) cast to string, to stringify the value
                // (2) cast to CHAR, to truncate or pad with spaces
                CastExpr tostring = new CastExpr(ScalarType.STRING, children_[0]);
                tostring.analyze();
                children_[0] = tostring;
            }

            if (children_[0] is NumericLiteral && type_.isFloatingPointType())
            {
                // Special case casting a decimal literal to a floating point number. The
                // decimal literal can be interpreted as either and we want to avoid casts
                // since that can result in loss of accuracy.
                ((NumericLiteral) children_[0]).explicitlyCastToFloat(type_);
            }

            if (children_[0].getType().isNull())
            {
                // Make sure BE never sees TYPE_NULL
                uncheckedCastChild(type_, 0);
            }

            // Ensure child has non-null type (even if it's a null literal). This is required
            // for the UDF interface.
            if (children_[0] is NullLiteral)
            {
                NullLiteral nullChild = (NullLiteral) (children_[0]);
                nullChild.uncheckedCastTo(type_);
            }

            SqlNodeType childType = children_.get(0).type_;
            //Preconditions.checkState(!childType.isNull());
            // IMPALA-4550: We always need to set noOp_ to the correct value, since we could
            // be performing a subsequent analysis run and its correct value might have changed.
            // This can happen if the child node gets substituted and its type changes.
            noOp_ = childType.Equals(type_);
            if (noOp_) return;

            FunctionName fnName = new FunctionName(BuiltinsDb.NAME, getFnName(type_));
            SqlNodeType[] args = {childType};
            Function searchDesc = new Function(fnName, args, SqlNodeType.INVALID, false);
            if (isImplicit_)
            {
                fn_ = BuiltinsDb.getInstance().getFunction(searchDesc,
                    Function.CompareMode.IS_NONSTRICT_SUPERTYPE_OF);
                Preconditions.checkState(fn_ != null);
            }
            else
            {
                fn_ = BuiltinsDb.getInstance().getFunction(searchDesc,
                    Function.CompareMode.IS_IDENTICAL);
                if (fn_ == null)
                {
                    // allow for promotion from CHAR to STRING; only if no exact match is found
                    fn_ = BuiltinsDb.getInstance().getFunction(
                        searchDesc.promoteCharsToStrings(), Function.CompareMode.IS_IDENTICAL);
                }
            }

            if (fn_ == null)
            {
                throw new AnalysisException("Invalid type cast of " + getChild(0).toSql() +
                                            " from " + childType + " to " + type_);
            }

            //Preconditions.checkState(type_.matchesType(fn_.getReturnType()),
            //    type_ + " != " + fn_.getReturnType());
        }

        /**
         * Returns child expr if this expr is an implicit cast, otherwise returns 'this'.
         */
        public override Expr IgnoreImplicitCast()
        {
            if (isImplicit_)
            {
                // we don't expect to see to consecutive implicit casts
                //Preconditions.checkState(
                //    !(getChild(0) is CastExpr) || !((CastExpr)getChild(0)).isImplicit());
                return getChild(0);
            }
            else
            {
                return this;
            }
        }
    }
}
