using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using TypeCobol.Compiler.SqlNodes.Catalog;
//TupleId is defined as int in impala
using TupleId = System.Int32;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * A collection of slots that are organized in a CPU-friendly memory layout. A slot is
     * a typed placeholder for a single value operated on at runtime. A slot can be named or
     * anonymous. A named slot corresponds directly to a column or field that can be directly
     * referenced in a query by its name. An anonymous slot represents an intermediate value
     * produced during query execution, e.g., aggregation output.
     * A tuple descriptor has an associated type and a list of slots. Its type is a struct
     * that contains as its fields the list of all named slots covered by this tuple.
     * The list of slots tracks the named slots that are actually referenced in a query, as
     * well as all anonymous slots. Although not required, a tuple descriptor typically
     * only has named or anonymous slots and not a mix of both.
     * Each tuple and slot descriptor has an associated unique id (within the scope of a
     * query). A given slot descriptor is owned by exactly one tuple descriptor.
     *
     * For example, every table reference has a corresponding tuple descriptor. The columns
     * of the table are represented by the tuple descriptor's type (struct type with one
     * field per column). The list of slots tracks which of the table's columns are actually
     * referenced. A similar explanation applies for collection references.
     *
     * A tuple descriptor may be materialized or non-materialized. A non-materialized tuple
     * descriptor acts as a placeholder for 'virtual' table references such as inline views,
     * and must not be materialized at runtime.
     *
     * Memory Layout
     * Slots are placed in descending order by size with trailing bytes to store null flags.
     * Null flags are omitted for non-nullable slots, with the following exceptions for Kudu
     * scan tuples to match Kudu's client row format: If there is at least one nullable Kudu
     * scan slot, then all slots (even non-nullable ones) get a null flag. If there are no
     * nullable Kudu scan slots, then there are also no null flags.
     * There is no padding between tuples when stored back-to-back in a row batch.
     *
     * Example: select bool_col, int_col, string_col, smallint_col from functional.alltypes
     * Slots:   string_col|int_col|smallint_col|bool_col|null_byte
     * Offsets: 0          16      20           22       23
     */
    public class TupleDescriptor
    {
        //TODO implement class
        private readonly TupleId id;
        private readonly string debugName;
        private readonly List<SlotDescriptor> slots = new List<SlotDescriptor>();

        // Resolved path to the collection corresponding to this tuple descriptor, if any,
        // Only set for materialized tuples.
        private Path path_;

        // Type of this tuple descriptor. Used for slot/table resolution in analysis.
        private StructType type_;

        // All legal aliases of this tuple.
        private string[] aliases_;

        // If true, requires that aliases_.length() == 1. However, aliases_.length() == 1
        // does not imply an explicit alias because nested collection refs have only a
        // single implicit alias.
        private bool hasExplicitAlias_;

        // If false, this tuple doesn't need to be materialized.
        private bool isMaterialized_ = true;

        // If true, computeMemLayout() has been called and we can't add any additional slots.
        private bool hasMemLayout_ = false;

        private int byteSize_;  // of all slots plus null indicators
        private int numNullBytes_;
        private float avgSerializedSize_;  // in bytes; includes serialization overhead

        public TupleDescriptor(TupleId id, String debugName)
        {
            this.id = id;
            path_ = null;
            this.debugName = debugName;
        }

        public void AddSlot(SlotDescriptor desc)
        {
            if (!hasMemLayout_)
            {
                slots.Add(desc);
            }
        }

        public TupleId getId() { return id; }
        public List<SlotDescriptor> GetSlots => slots;
        public List<SlotDescriptor> GetMaterializedSlots()
        {
            List<SlotDescriptor> result = new List<SlotDescriptor>();
            foreach (var slot in slots.Where(x => x.IsMaterialized()))
            {
                result.Add(slot);
            }
            return result;
        }

        /**
   * Returns all materialized slots ordered by their offset. Valid to call after the
   * mem layout has been computed.
   */
        public List<SlotDescriptor> GetSlotsOrderedByOffset()
        {
            if (!hasMemLayout_)
                return new List<SlotDescriptor>();
            List<SlotDescriptor> result = GetMaterializedSlots();

            return GetMaterializedSlots().Sort(x => x.getByteOffset());
        }
        //TODO = analize how to get Column if rootTable doesn't come from hive
        //public FeTable getTable()
        //{
        //    if (path_ == null) return null;
        //    return path_.getRootTable();
        //}

        //public TableName getTableName()
        //{
        //    FeTable t = getTable();
        //    return (t == null) ? null : t.getTableName();
        //}

        public void setPath(Path p)
        {
            if (p == null || !p.IsResolved() || !p.destType().isCollectionType())
            {
                return;
            }
            path_ = p;
            if (p.destTable() != null)
            {
                // Do not use Path.getTypeAsStruct() to only allow implicit path resolutions,
                // because this tuple desc belongs to a base table ref.
                type_ = (StructType)p.destTable().GetType().getItemType();
            }
            else
            {
                // Also allow explicit path resolutions.
                type_ = Path.getTypeAsStruct(p.destType());
            }
        }

        public Path getPath() { return path_; }
        public void setType(StructType type) { type_ = type; }
        public StructType GetType() { return type_; }
        public int getByteSize() { return byteSize_; }
        public float getAvgSerializedSize() { return avgSerializedSize_; }
        public bool isMaterialized() { return isMaterialized_; }
        public void setIsMaterialized(bool value) { isMaterialized_ = value; }
        public bool hasMemLayout() { return hasMemLayout_; }
        public void setAliases(string[] aliases, bool hasExplicitAlias)
        {
            aliases_ = aliases;
            hasExplicitAlias_ = hasExplicitAlias;
        }
        public bool hasExplicitAlias() { return hasExplicitAlias_; }
        public string getAlias() { return (aliases_ != null) ? aliases_[0] : null; }
        public TableName getAliasAsName()
        {
            return (aliases_ != null) ? new TableName(null, aliases_[0]) : null;
        }

        public TupleDescriptor getRootDesc()
        {
            return path_?.getRootDesc();
        }

        public string debugString()
        {
            string tblStr = (getTable() == null ? "null" : getTable().getFullName());
            List<string> slotStrings = new List<string>();
            foreach (var slot in slots)
            {
                slotStrings.Add(slot.debugString());
            }

            return "id"+id
                +"name"+ debugName
                +"tbl"+tblStr
                +"byte_size"+ byteSize_
                +"is_materialized"+ isMaterialized_
                +"slots [" + string.Join(", ",slotStrings) + "]";
        }

        /**
         * Checks that this tuple is materialized and has a mem layout. Throws if this tuple
         * is not executable, i.e., if one of those conditions is not met.
         */
        public void checkIsExecutable()
        {
            if(!isMaterialized_)
                throw new ArgumentException(String.Format(
                "Illegal reference to non-materialized tuple: debugname={0} alias={1} tid={2}",
                debugName, StringUtils.defaultIfEmpty(getAlias(), "n/a"), id));
            if (!hasMemLayout_)
                throw new InsufficientMemoryException(String.Format(
                "Missing memory layout for tuple: debugname={0} alias={1} tid={2}",
                debugName, StringUtils.defaultIfEmpty(getAlias(), "n/a"), id));
        }

        /**
         * Materialize all slots.
         */
        public void materializeSlots()
        {
            foreach (var slot in slots)
                slot.setIsMaterialized(true);
        }

        //public TTupleDescriptor toThrift(Integer tableId)
        //{
        //    TTupleDescriptor ttupleDesc =
        //        new TTupleDescriptor(id.asInt(), byteSize_, numNullBytes_);
        //    if (tableId == null) return ttupleDesc;
        //    ttupleDesc.setTableId(tableId);
        //    Preconditions.checkNotNull(path_);
        //    ttupleDesc.setTuplePath(path_.getAbsolutePath());
        //    return ttupleDesc;
        //}

        /**
         * In some cases changes are made to a tuple after the memory layout has been computed.
         * This function allows us to recompute the memory layout, if necessary. No-op if this
         * tuple does not have an existing mem layout.
         */
        public void recomputeMemLayout()
        {
            if (!hasMemLayout_) return;
            hasMemLayout_ = false;
            computeMemLayout();
        }

        public void computeMemLayout()
        {
            if (hasMemLayout_) return;
            hasMemLayout_ = true;

            bool alwaysAddNullBit = hasNullableKuduScanSlots();
            avgSerializedSize_ = 0;

            // maps from slot size to slot descriptors with that size
            Dictionary<int, List<SlotDescriptor>> slotsBySize =
                new Dictionary<int, List<SlotDescriptor>>();

            // populate slotsBySize
            int numNullBits = 0;
            int totalSlotSize = 0;
            foreach (var d in slots)
            {
                if (!d.IsMaterialized()) continue;
                ColumnStats stats = d.getStats();
                if (stats.hasAvgSerializedSize())
                {
                    avgSerializedSize_ += d.getStats().getAvgSerializedSize();
                }
                else
                {
                    // TODO: for computed slots, try to come up with stats estimates
                    avgSerializedSize_ += d.getType().getSlotSize();
                }
                if (!slotsBySize.ContainsKey(d.getType().getSlotSize()))
                {
                    slotsBySize.Add(d.getType().getSlotSize(), new List<SlotDescriptor>());
                }
                totalSlotSize += d.getType().getSlotSize();
                slotsBySize[d.getType().getSlotSize()].Add(d);
                if (d.getIsNullable() || alwaysAddNullBit) ++numNullBits;
            }
            // we shouldn't have anything of size <= 0
            if (slotsBySize.ContainsKey(0) || slotsBySize.ContainsKey(-1))
            {
                return;
            }

            // assign offsets to slots in order of descending size
            numNullBytes_ = (numNullBits + 7) / 8;
            int slotOffset = 0;
            int nullIndicatorByte = totalSlotSize;
            int nullIndicatorBit = 0;
            // slotIdx is the index into the resulting tuple struct.  The first (largest) field
            // is 0, next is 1, etc.
            int slotIdx = 0;
            // sort slots in descending order of size
            List<int> sortedSizes = new List<int>(slotsBySize.keySet());
            sortedSizes.Reverse();
            for (int slotSize =0; slotSize< sortedSizes.Count; slotSize++)
            {
                if (slotsBySize[slotSize]==null) continue;
                foreach (SlotDescriptor d in slotsBySize[slotSize])
                {
                    if (!d.IsMaterialized())
                    {
                        continue;
                    }
                    d.setByteSize(slotSize);
                    d.setByteOffset(slotOffset);
                    d.setSlotIdx(slotIdx++);
                    slotOffset += slotSize;

                    // assign null indicator
                    if (d.getIsNullable() || alwaysAddNullBit)
                    {
                        d.setNullIndicatorByte(nullIndicatorByte);
                        d.setNullIndicatorBit(nullIndicatorBit);
                        nullIndicatorBit = (nullIndicatorBit + 1) % 8;
                        if (nullIndicatorBit == 0) ++nullIndicatorByte;
                    }
                    // non-nullable slots have 0 for the byte offset and -1 for the bit mask
                    // to make sure IS NULL always evaluates to false in the BE without having
                    // to check nullability explicitly
                    if (!d.getIsNullable())
                    {
                        d.setNullIndicatorBit(-1);
                        d.setNullIndicatorByte(0);
                    }
                }
            }
            if(slotOffset != totalSlotSize)
            {
                return;
            }

            byteSize_ = totalSlotSize + numNullBytes_;
        }

        /**
         * Returns true if this tuple has at least one materialized nullable Kudu scan slot.
         */
        private bool hasNullableKuduScanSlots()
        {
            if (!(getTable() is FeKuduTable))
            {
                return false;
            }
            foreach (var d in slots)
            {
                if (d.IsMaterialized() && d.getIsNullable()) return true;
            }
            return false;
        }

        /**
         * Return true if the slots being materialized are all partition columns.
         */
        public bool hasClusteringColsOnly()
        {
            FeTable table = getTable();
            if (!(table is FeFsTable) || table.getNumClusteringCols() == 0) return false;

            FeFsTable hdfsTable = (FeFsTable)table;
            foreach (var slotDesc in GetSlots)
            {
                if (!slotDesc.IsMaterialized()) continue;
                if (slotDesc.getColumn() == null ||
                    !hdfsTable.isClusteringColumn(slotDesc.getColumn()))
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Returns a list of slot ids that correspond to partition columns.
         */
        public List<SlotId> getPartitionSlots()
        {
            List<SlotId> partitionSlots = new List<SlotId>();
            foreach (var slotDesc in GetSlots)
            {
                if (slotDesc.getColumn() == null) continue;
                if (slotDesc.getColumn().getPosition() < getTable().getNumClusteringCols())
                {
                    partitionSlots.Add(slotDesc.getId());
                }
            }
            return partitionSlots;
        }

        /**
         * Returns true if the tuple has any variable-length slots.
         */
        public bool hasVarLenSlots()
        {
            foreach (var slot in slots)
            {
                if (!slot.getType().isFixedLengthType()) return true;
            }
            return false;
        }
    }
}
