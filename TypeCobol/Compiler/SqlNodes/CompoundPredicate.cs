using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Atn;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * &&, ||, ! predicates.
     *
     */
    public class CompoundPredicate : Predicate
    {
        private readonly Operator op_;

        public static void initBuiltins(Db db)
        {
            // AND and OR are implemented as custom exprs, so they do not have a function symbol.
            db.addBuiltin(ScalarFunction.createBuiltinOperator(
                              Operator.AND.name(), "",
                              Lists. < SqlNodeType > newArrayList(SqlNodeType.BOOLEAN, SqlNodeType.BOOLEAN),
                SqlNodeType.BOOLEAN));
            db.addBuiltin(ScalarFunction.createBuiltinOperator(
                              Operator.OR.name(), "",
                              Lists. < SqlNodeType > newArrayList(SqlNodeType.BOOLEAN, SqlNodeType.BOOLEAN),
                SqlNodeType.BOOLEAN));
            db.addBuiltin(ScalarFunction.createBuiltinOperator(
                              Operator.NOT.name(), "impala::CompoundPredicate::Not",
                              Lists. < SqlNodeType > newArrayList(SqlNodeType.BOOLEAN), SqlNodeType.BOOLEAN));
        }

        public CompoundPredicate(Operator op, Expr e1, Expr e2) : base()
        {
            this.op_ = op;
            //Preconditions.checkNotNull(e1);
            children_.Add(e1);
            //Preconditions.checkArgument(op == Operator.NOT && e2 == null
            //|| op != Operator.NOT && e2 != null);
            if (e2 != null) children_.Add(e2);
        }

        /**
         * Copy c'tor used in clone().
         */
        protected CompoundPredicate(CompoundPredicate other) : base(other)
        {
            op_ = other.op_;
        }

        public Operator getOp()
        {
            return op_;
        }

        public override bool LocalEquals(Expr that)
        {
            return base.localEquals(that) && ((CompoundPredicate) that).op_ == op_;
        }

        public override String DebugString()
        {
            return "op" + op_ + base.debugString();
        }


        public override String ToSqlImpl()
        {
            if (children_.Count == 1)
            {
                if (op_ == Operator.NOT)
                    return "NOT " + getChild(0).toSql();
            }

            return getChild(0).toSql() + " " + op_.ToString() + " " + getChild(1).toSql();

        }

        //      @Override
        //protected void toThrift(TExprNode msg)
        //      {
        //          msg.node_type = TExprNodeType.COMPOUND_PRED;
        //      }

        //      @Override
        protected void analyzeImpl(Analyzer analyzer)
        {
            base.analyzeImpl(analyzer);
            // Check that children are predicates.
            foreach (Expr e in children_)
            {
                if (!e.getType().isBoolean() && !e.getType().isNull())
                {
                    throw new AnalysisException(String.Format("Operand '{0}' part of predicate " +
                                                              "'{1}' should return type 'BOOLEAN' but returns type '{2}'.",
                        e.toSql(), toSql(), e.getType().toSql()));
                }
            }

            fn_ = getBuiltinFunction(analyzer, op_.ToString(), collectChildReturnTypes(),
                Function.CompareMode.IS_NONSTRICT_SUPERTYPE_OF);
            //Preconditions.checkState(fn_ != null);
            //Preconditions.checkState(fn_.getReturnType().isBoolean());
            castForFunctionCall(false, analyzer.isDecimalV2());

            if (!getChild(0).hasSelectivity() ||
                (children_.Count == 2 && !getChild(1).hasSelectivity()))
            {
                // Give up if one of our children has an unknown selectivity.
                selectivity_ = -1;
                return;
            }

            switch (op_.value)
            {
                case OperatorValue.AND:
                    selectivity_ = getChild(0).getSelectivity() * getChild(1).getSelectivity();
                    break;
                case OperatorValue.OR:
                    selectivity_ = getChild(0).getSelectivity() + getChild(1).getSelectivity()
                                   - getChild(0).getSelectivity() * getChild(1).getSelectivity();
                    break;
                case OperatorValue.NOT:
                    selectivity_ = 1.0 - getChild(0).getSelectivity();
                    break;
            }

            selectivity_ = Math.Max(0.0, Math.Min(1.0, selectivity_));
        }

        protected float computeEvalCost()
        {
            return hasChildCosts() ? getChildCosts() + COMPOUND_PREDICATE_COST : UNKNOWN_COST;
        }

        /**
         * Negates a CompoundPredicate.
         */
        public Expr negate()
        {
            if (op_ == Operator.NOT) return getChild(0);
            Expr negatedLeft = getChild(0).negate();
            Expr negatedRight = getChild(1).negate();
            Operator newOp = (op_ == Operator.OR) ? Operator.AND : Operator.OR;
            return new CompoundPredicate(newOp, negatedLeft, negatedRight);
        }

        /**
         * Creates a conjunctive predicate from a list of exprs.
         */
        public static Expr createConjunctivePredicate(List<Expr> conjuncts)
        {
            return createCompoundTree(conjuncts, Operator.AND);
        }

        /**
         * Creates a disjunctive predicate from a list of exprs.
         */
        public static Expr createDisjunctivePredicate(List<Expr> disjuncts)
        {
            return createCompoundTree(disjuncts, Operator.OR);
        }

        private static Expr createCompoundTree(List<Expr> exprs, Operator op)
        {
            //Preconditions.checkState(op == Operator.AND || op == Operator.OR);
            Expr result = null;
            foreach (Expr expr in exprs)
            {
                if (result == null)
                {
                    result = expr;
                    continue;
                }

                result = new CompoundPredicate(op, result, expr);
            }

            return result;
        }

        public override Expr clone()
        {
            return new CompoundPredicate(this);
        }

        // Create an AND predicate between two exprs, 'lhs' and 'rhs'. If
        // 'rhs' is null, simply return 'lhs'.
        public static Expr createConjunction(Expr lhs, Expr rhs)
        {
            if (rhs == null) return lhs;
            return new CompoundPredicate(Operator.AND, rhs, lhs);
        }


        public class Operator
        {
            private readonly string description;
            public readonly OperatorValue value;

            public Operator(string description, OperatorValue value)
            {
                this.description = description;
                this.value = value;
            }

            public static readonly Operator AND = new Operator("AND", OperatorValue.AND);
            public static readonly Operator OR = new Operator("OR", OperatorValue.OR);
            public static readonly Operator NOT = new Operator("NOT", OperatorValue.NOT);
        }

        public enum OperatorValue
        {
            AND,
            OR,
            NOT
        }
    }
}
