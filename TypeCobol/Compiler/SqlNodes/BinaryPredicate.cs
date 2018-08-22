using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    public class BinaryPredicate : Predicate
    {
        public class Operator
        {
            private readonly string description;
            private readonly string name;
            public OperatorValue value;

            public Operator(string description, string name, OperatorValue value)
            {
                this.description = description;
                this.name = name;
                this.value = value;
            }

            public static readonly Operator EQ = new Operator("=", "eq", OperatorValue.EQ);
            public static readonly Operator NE = new Operator("!=", "ne", OperatorValue.NE);
            public static readonly Operator LE = new Operator("<=", "le", OperatorValue.LE);
            public static readonly Operator GE = new Operator("<=", "le", OperatorValue.GE);
            public static readonly Operator LT = new Operator("<=", "le", OperatorValue.LT);
            public static readonly Operator GT = new Operator("<=", "le", OperatorValue.GT);
            public static readonly Operator DISTINCT_FROM = new Operator("<=", "le", OperatorValue.DISTINCT_FROM);
            public static readonly Operator NOT_DISTINCT = new Operator("<=", "le", OperatorValue.NOT_DISTINCT);
            public static readonly Operator NULL_MATCHING_EQ = new Operator("<=", "le", OperatorValue.NULL_MATCHING_EQ);

            public override String ToString()
            {
                return description;
            }

            public String getName()
            {
                return name;
            }

            public bool isEquivalence()
            {
                return this == EQ || this == NOT_DISTINCT;
            }

            public OperatorValue converse()
            {
                switch (value)
                {
                    case OperatorValue.EQ:
                        return OperatorValue.EQ;
                    case OperatorValue.NE:
                        return OperatorValue.EQ;
                    case OperatorValue.LE:
                        return OperatorValue.GE;
                    case OperatorValue.GE:
                        return OperatorValue.LE;
                    case OperatorValue.LT:
                        return OperatorValue.GT;
                    case OperatorValue.GT:
                        return OperatorValue.LT;
                    case OperatorValue.DISTINCT_FROM:
                        return OperatorValue.DISTINCT_FROM;
                    case OperatorValue.NOT_DISTINCT:
                        return OperatorValue.NOT_DISTINCT;
                    case OperatorValue.NULL_MATCHING_EQ:
                        throw new InvalidOperationException("not implemented");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            public enum OperatorValue
            {
                EQ,
                NE,
                LE,
                GE,
                LT,
                GT,
                DISTINCT_FROM,
                NOT_DISTINCT,
                NULL_MATCHING_EQ
            }
        }

        // true if this BinaryPredicate is inferred from slot equivalences, false otherwise.
        private bool isInferred_ = false;

        public static void initBuiltins(Db db)
        {
            for (SqlNodeType t:
            SqlNodeType.getSupportedTypes())
            {
                if (t.isNull()) continue; // NULL is handled through type promotion.
                db.addBuiltin(ScalarFunction.createBuiltinOperator(
                    Operator.EQ.getName(), Lists.newArrayList(t, t), SqlNodeType.BOOLEAN));
                db.addBuiltin(ScalarFunction.createBuiltinOperator(
                    Operator.NE.getName(), Lists.newArrayList(t, t), SqlNodeType.BOOLEAN));
                db.addBuiltin(ScalarFunction.createBuiltinOperator(
                    Operator.LE.getName(), Lists.newArrayList(t, t), SqlNodeType.BOOLEAN));
                db.addBuiltin(ScalarFunction.createBuiltinOperator(
                    Operator.GE.getName(), Lists.newArrayList(t, t), SqlNodeType.BOOLEAN));
                db.addBuiltin(ScalarFunction.createBuiltinOperator(
                    Operator.LT.getName(), Lists.newArrayList(t, t), SqlNodeType.BOOLEAN));
                db.addBuiltin(ScalarFunction.createBuiltinOperator(
                    Operator.GT.getName(), Lists.newArrayList(t, t), SqlNodeType.BOOLEAN));
            }
        }

        public Operator op_;

        public Operator getOp()
        {
            return op_;
        }

        public void setOp(Operator op)
        {
            op_ = op;
        }

        public BinaryPredicate(Operator op, Expr e1, Expr e2) : base()
        {
            this.op_ = op;
            //Preconditions.checkNotNull(e1);
            children_.Add(e1);
            //Preconditions.checkNotNull(e2);
            children_.Add(e2);
        }

        protected BinaryPredicate(BinaryPredicate other) : base(other)
        {
            op_ = other.op_;
            isInferred_ = other.isInferred_;
        }

        public bool isNullMatchingEq()
        {
            return op_ == Operator.NULL_MATCHING_EQ;
        }

        public bool isInferred()
        {
            return isInferred_;
        }

        public void setIsInferred()
        {
            isInferred_ = true;
        }

        public override String ToSqlImpl()
        {
            return getChild(0).toSql() + " " + op_.ToString() + " " + getChild(1).toSql();
        }

        //      @Override
        //protected void toThrift(TExprNode msg)
        //      {
        //          Preconditions.checkState(children_.size() == 2);
        //          // Cannot serialize a nested predicate.
        //          Preconditions.checkState(!contains(Subquery.class));
        //  // This check is important because we often clone and/or evaluate predicates,
        //  // and it's easy to get the casting logic wrong, e.g., cloned predicates
        //  // with expr substitutions need to be re-analyzed with reanalyze().
        //  Preconditions.checkState(getChild(0).getType().getPrimitiveType() ==
        //                           getChild(1).getType().getPrimitiveType(),
        //      "child 0 type: " + getChild(0).getType() +
        //      " child 1 type: " + getChild(1).getType());
        //  msg.node_type = TExprNodeType.FUNCTION_CALL;
        //}

        public override String DebugString()
        {
            return "op" + op_ + base.debugString();
        }


        protected void analyzeImpl(Analyzer analyzer)
        {
            base.analyzeImpl(analyzer);
            convertNumericLiteralsFromDecimal(analyzer);
            String opName = op_.getName().Equals("null_matching_eq") ? "eq" : op_.getName();
            fn_ = getBuiltinFunction(analyzer, opName, collectChildReturnTypes(),
                Function.CompareMode.IS_NONSTRICT_SUPERTYPE_OF);
            if (fn_ == null)
            {
                throw new AnalysisException("operands of type " + getChild(0).getType().toSql() +
                                            " and " + getChild(1).getType().toSql() + " are not comparable: " +
                                            toSql());
            }
            //Preconditions.checkState(fn_.getReturnType().isBoolean());

            List<Expr> subqueries = new List<Expr>();
            //TODO - rewrite call to collectAll
            //collectAll(Predicates.instanceOf(Subquery.class), subqueries);
            if (subqueries.Count > 1)
            {
                // TODO Remove that restriction when we Add support for independent subquery
                // evaluation.
                throw new AnalysisException("Multiple subqueries are not supported in binary " +
                                            "predicates: " + toSql());

            }
            //TODO - see how to check if class exists in c#
            //if (contains(ExistsPredicate.class)) {
            //  throw new AnalysisException("EXISTS subquery predicates are not " +
            //      "supported in binary predicates: " + toSql());
            //}

            List<InPredicate> inPredicates = new List<InPredicate>();
            //TODO - rewrite call to collect
            //collect(InPredicate.class, inPredicates);
            foreach (InPredicate inPredicate in inPredicates)
            {
                //TODO - see how to check if class exists in c#
                //        if (inPredicate.contains(Subquery.class)) {
                //throw new AnalysisException("IN subquery predicates are not supported in " +
                //    "binary predicates: " + toSql());
                //}
            }

            //if (!contains(Subquery.class)) {
            //  // Don't perform any casting for predicates with subqueries here. Any casting
            //  // required will be performed when the subquery is unnested.
            //  castForFunctionCall(true, analyzer.isDecimalV2());
            //}

            // Determine selectivity
            // TODO: Compute selectivity for nested predicates.
            // TODO: Improve estimation using histograms.
            List<SlotRef> slotRefRef = new List<SlotRef>();
            if ((op_ == Operator.EQ || op_ == Operator.NOT_DISTINCT)
                && isSingleColumnPredicate(slotRefRef, null))
            {
                long distinctValues = slotRefRef.getRef().getNumDistinctValues();
                if (distinctValues > 0)
                {
                    selectivity_ = 1.0 / distinctValues;
                    selectivity_ = Math.Max(0, Math.Min(1, selectivity_));
                }
            }
        }

        protected float computeEvalCost()
        {
            if (!hasChildCosts()) return UNKNOWN_COST;
            if (getChild(0).getType().isFixedLengthType())
            {
                return getChildCosts() + BINARY_PREDICATE_COST;
            }
            else if (getChild(0).getType().isStringType())
            {
                return getChildCosts() +
                       (float) (getAvgStringLength(getChild(0)) + getAvgStringLength(getChild(1))) *
                       BINARY_PREDICATE_COST;
            }
            else
            {
                //TODO(tmarshall): Handle other var length types here.
                return getChildCosts() + VAR_LEN_BINARY_PREDICATE_COST;
            }
        }

        /**
         * If predicate is of the form "<slotref> <op> <expr>", returns expr,
         * otherwise returns null. Slotref may be wrapped in a CastExpr.
         * TODO: revisit CAST handling at the caller
         */
        public Expr getSlotBinding(SlotId id)
        {
            // BinaryPredicates are normalized, so we only need to check the left operand.
            SlotRef slotRef = getChild(0).unwrapSlotRef(false);
            if (slotRef != null && slotRef.getSlotId() == id) return getChild(1);
            return null;
        }

        /**
         * If e is an equality predicate between two slots that only require implicit
         * casts, returns those two slots; otherwise returns null.
         */
        public static Pair<SlotId, SlotId> getEqSlots(Expr e)
        {
            if (!(e is BinaryPredicate)) return null;
            return ((BinaryPredicate) e).getEqSlots();
        }

        /**
         * If this is an equality predicate between two slots that only require implicit
         * casts, returns those two slots; otherwise returns null.
         */
        public Pair<SlotId, SlotId> getEqSlots()
        {
            if (op_ != Operator.EQ) return null;
            SlotRef lhs = getChild(0).unwrapSlotRef(true);
            if (lhs == null) return null;
            SlotRef rhs = getChild(1).unwrapSlotRef(true);
            if (rhs == null) return null;
            return new Pair<SlotId, SlotId>(lhs.getSlotId(), rhs.getSlotId());
        }

        /**
         * If predicate is of the form "<SlotRef> op <Expr>", returns the SlotRef, otherwise
         * returns null.
         */
        public SlotRef getBoundSlot()
        {
            return getChild(0).unwrapSlotRef(true);
        }

        /**
         * Negates a BinaryPredicate.
         */

        public Expr negate()
        {
            Operator newOp = null;
            switch (op_.value)
            {
                case Operator.OperatorValue.EQ:
                    newOp = Operator.NE;
                    break;
                case Operator.OperatorValue.NE:
                    newOp = Operator.EQ;
                    break;
                case Operator.OperatorValue.LT:
                    newOp = Operator.GE;
                    break;
                case Operator.OperatorValue.LE:
                    newOp = Operator.GT;
                    break;
                case Operator.OperatorValue.GE:
                    newOp = Operator.LT;
                    break;
                case Operator.OperatorValue.GT:
                    newOp = Operator.LE;
                    break;
                case Operator.OperatorValue.DISTINCT_FROM:
                    newOp = Operator.NOT_DISTINCT;
                    break;
                case Operator.OperatorValue.NOT_DISTINCT:
                    newOp = Operator.DISTINCT_FROM;
                    break;
                case Operator.OperatorValue.NULL_MATCHING_EQ:
                    throw new InvalidOperationException("Not implemented");
            }

            return new BinaryPredicate(newOp, getChild(0), getChild(1));
        }

        /**
         * Swaps the first and second child in-place and sets the operation to its converse.
         */
        public void reverse()
        {
            var temp = children_[0];
            children_[0] = children_[1];
            children_[1] = temp;
            op_.value = op_.converse();
        }

        public override bool LocalEquals(Expr that)
        {
            return base.localEquals(that) && op_.Equals(((BinaryPredicate) that).op_);
        }

        public override Expr clone()
        {
            return new BinaryPredicate(this);
        }
    }
}
