using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;
using TypeCobol.Compiler.SqlNodes.Common;
using TupleId = System.Int32;

namespace TypeCobol.Compiler.SqlNodes
{
    public class SlotRef : Expr
    {
        public override string ToSqlImpl()
        {
            if (label_ != null) return label_;
            if (rawPath_ != null) return ToSqlUtils.getPathSql(rawPath_);
            return "<slot " + desc_.getId() + ">";
        }

        public override string DebugString()
        {
            StringBuilder toStrHelper = new StringBuilder();
            if (rawPath_ != null) toStrHelper.Append("path" + string.Join(".", rawPath_.ToList()));
            toStrHelper.Append("type" + type_.toSql());
            String idStr = (desc_ == null ? "null" : desc_.getId().ToString());
            toStrHelper.Append("id" + idStr);
            return toStrHelper.ToString();
        }

        public override bool LocalEquals(Expr that)
        {
            if (!base.localEquals(that)) return false;
            SlotRef other = (SlotRef) that;
            // check slot ids first; if they're both set we only need to compare those
            // (regardless of how the ref was constructed)
            if (desc_ != null && other.desc_ != null)
            {
                return desc_.getId().Equals(other.desc_.getId());
            }

            return label_ == null
                ? other.label_ == null
                : label_.Equals(other.label_, StringComparison.OrdinalIgnoreCase);
        }

        public override void ResetAnalysisState()
        {
            throw new NotImplementedException();
        }

        public override Expr clone()
        {
            return new SlotRef(this);
        }

        protected override bool IsConstantImpl()
        {
            return false;
        }

        public override Expr IgnoreImplicitCast()
        {
            throw new NotImplementedException();
        }

        private readonly List<String> rawPath_;
        private readonly String label_; // printed in toSql()

        // Results of analysis.
        private SlotDescriptor desc_;

        public SlotRef(List<String> rawPath) : base()
        {
            rawPath_ = rawPath;
            label_ = ToSqlUtils.getPathSql(rawPath_);
        }

        /**
         * C'tor for a "dummy" SlotRef used in substitution maps.
         */
        public SlotRef(String alias) : base()
        {
            rawPath_ = null;
            // Relies on the label_ being compared in Equals().
            label_ = ToSqlUtils.getIdentSql(alias.ToLowerInvariant());
        }

        /**
         * C'tor for a "pre-analyzed" ref to a slot.
         */
        public SlotRef(SlotDescriptor desc) : base()
        {
            if (desc.isScanSlot())
            {
                rawPath_ = desc.getPath().getRawPath();
            }
            else
            {
                rawPath_ = null;
            }

            desc_ = desc;
            type_ = desc.getType();
            evalCost_ = SLOT_REF_COST;
            String alias = desc.getParent().getAlias();
            label_ = (alias != null ? alias + "." : "") + desc.getLabel();
            numDistinctValues_ = desc.getStats().getNumDistinctValues();
            analysisDone();
        }

        /**
         * C'tor for cloning.
         */
        private SlotRef(SlotRef other) : base(other)
        {
            rawPath_ = other.rawPath_;
            label_ = other.label_;
            desc_ = other.desc_;
            type_ = other.type_;
        }

        protected void analyzeImpl(Analyzer analyzer)
        {
            // TODO: derived slot refs (e.g., star-expanded) will not have rawPath set.
            // Change construction to properly handle such cases.
            //Preconditions.checkState(rawPath_ != null);
            Path resolvedPath = null;
            try
            {
                resolvedPath = analyzer.resolvePath(rawPath_, Path.PathType.SLOTREF);
            }
            catch (TableLoadingException)
            {
                // Should never happen because we only check registered table aliases.
                //Preconditions.checkState(false);
            }

            //Preconditions.checkNotNull(resolvedPath);
            desc_ = analyzer.registerSlotRef(resolvedPath);
            type_ = desc_.getType();
            if (!type_.isSupported())
            {
                throw new AnalysisException("Unsupported type '"
                                            + type_.toSql() + "' in '" + toSql() + "'.");
            }

            if (type_.isInvalid())
            {
                // In this case, the metastore contained a string we can't parse at all
                // e.g. map. We could report a better error if we stored the original
                // HMS string.
                throw new AnalysisException("Unsupported type in '" + toSql() + "'.");
            }

            numDistinctValues_ = desc_.getStats().getNumDistinctValues();
            FeTable rootTable = resolvedPath.getRootTable();
            if (rootTable != null && rootTable.getNumRows() > 0)
            {
                // The NDV cannot exceed the #rows in the table.
                numDistinctValues_ = Math.Min(numDistinctValues_, rootTable.getNumRows());
            }
        }

        protected float computeEvalCost()
        {
            return SLOT_REF_COST;
        }


        public SlotDescriptor getDesc()
        {
            //Preconditions.checkState(isAnalyzed());
            //Preconditions.checkNotNull(desc_);
            return desc_;
        }

        public SlotId getSlotId()
        {
            //Preconditions.checkState(isAnalyzed());
            //Preconditions.checkNotNull(desc_);
            return desc_.getId();
        }

        public Path getResolvedPath()
        {
            //Preconditions.checkState(isAnalyzed());
            return desc_.getPath();
        }


        //      @Override
        //protected void toThrift(TExprNode msg)
        //      {
        //          msg.node_type = TExprNodeType.SLOT_REF;
        //          msg.slot_ref = new TSlotRef(desc_.getId().asInt());
        //          // we shouldn't be sending exprs over non-materialized slots
        //          Preconditions.checkState(desc_.isMaterialized(), String.format(
        //              "Illegal reference to non-materialized slot: tid=%s sid=%s",
        //              desc_.getParent().getId(), desc_.getId()));
        //          // check that the tuples associated with this slot are executable
        //          desc_.getParent().checkIsExecutable();
        //          if (desc_.getItemTupleDesc() != null) desc_.getItemTupleDesc().checkIsExecutable();
        //      }



        public override int GetHashCode()
        {
            if (desc_ != null) return desc_.getId().GetHashCode();
            return string.Join(".", rawPath_).ToLowerInvariant().GetHashCode();
        }



        //    /** Used for {@link Expr#matches(Expr, Comparator)} */
        //    interface Comparator
        //    {
        //        bool matches(SlotRef a, SlotRef b);
        //    }

        //    /**
        //     * A wrapper around localEquals() used for {@link #Expr#matches(Expr, Comparator)}.
        //     */
        //    static readonly Comparator SLOTREF_EQ_CMP = new Comparator
        //    {
        //public bool matches(SlotRef a, SlotRef b) { return a.localEquals(b); }
        //};

        public override bool IsBoundByTupleIds(List<TupleId> tids)
        {
            //Preconditions.checkState(desc_ != null);
            foreach (TupleId tid in tids)
            {
                if (tid.Equals(desc_.getParent().getId())) return true;
            }

            return false;
        }

        public override bool IsBoundBySlotIds(List<SlotId> slotIds)
        {
            //Preconditions.checkState(isAnalyzed());
            return slotIds.Contains(desc_.getId());
        }


        public override void GetIdsHelper(HashSet<TupleId> tupleIds, HashSet<SlotId> slotIds)
        {
            //Preconditions.checkState(type_.isValid());
            //Preconditions.checkState(desc_ != null);
            if (slotIds != null) slotIds.Add(desc_.getId());
            if (tupleIds != null) tupleIds.Add(desc_.getParent().getId());
        }


        public override String ToString()
        {
            if (desc_ != null)
            {
                return "tid=" + desc_.getParent().getId() + " sid=" + desc_.getId();
            }

            return "no desc set";
        }

        protected Expr uncheckedCastTo(SqlNodeType targetType)
        {
            if (type_.isNull())
            {
                // Hack to prevent null SlotRefs in the BE
                return NullLiteral.create(targetType);
            }
            else
            {
                return base.uncheckedCastTo(targetType);
            }
        }
    }
}
