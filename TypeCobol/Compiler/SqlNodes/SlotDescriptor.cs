using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;

namespace TypeCobol.Compiler.SqlNodes
{
    public class SlotDescriptor
    {
        private readonly SlotId id;
        private readonly TupleDescriptor parent;

      // Resolved path to the column/field corresponding to this slot descriptor, if any,
      // Only set for slots that represent a column/field materialized in a scan.
        private Path path;
        private SqlNodeType type;

        // Tuple descriptor for collection items. Only set if type is an array or map.
        private TupleDescriptor itemTupleDesc;

        // for SlotRef.toSql() in the absence of a path
        private string label;

        // Expr(s) materialized into this slot; multiple exprs for unions. Should be empty if
        // path is set.
        private List<Expr> sourceExprs = new List<Expr>();

        // if false, this slot doesn't need to be materialized in parent tuple
        // (and physical layout parameters are invalid)
        private bool isMaterialized = false;

        // if false, this slot cannot be NULL
        // Note: it is still possible that a SlotRef pointing to this descriptor could have a
        // NULL value if the entire tuple is NULL, for example as the result of an outer join.
        private bool isNullable = true;

        // physical layout parameters
        private int byteSize;
        private int byteOffset;  // within tuple
        private int nullIndicatorByte;  // index into byte array
        private int nullIndicatorBit; // index within byte
        private int slotIdx;          // index within tuple struct

        private ColumnStats stats;  // only set if 'column' isn't set

        public SlotDescriptor(SlotId id, TupleDescriptor parent)
        {
            if (id == null || parent == null)
            {
                return;
            }
            this.id = id;
            this.parent = parent;
            byteOffset = -1;  // invalid
        }


        public SlotDescriptor(SlotId id, TupleDescriptor parent, SlotDescriptor src)
        {
            if (id==null || parent == null)
            {
                return;
            }
            this.id = id;
            this.parent = parent;
            type = src.type;
            itemTupleDesc = src.itemTupleDesc;
            path = src.path;
            label = src.label;
            sourceExprs = src.sourceExprs;
            isMaterialized = src.isMaterialized;
            isNullable = src.isNullable;
            byteSize = src.byteSize;
            byteOffset = src.byteOffset;
            nullIndicatorByte = src.nullIndicatorByte;
            nullIndicatorBit = src.nullIndicatorBit;
            slotIdx = src.slotIdx;
            stats = src.stats;
        }

        public int getNullIndicatorByte() { return nullIndicatorByte; }
        public void setNullIndicatorByte(int nullIndicatorByte)
        {
            this.nullIndicatorByte = nullIndicatorByte;
        }
        public int getNullIndicatorBit() { return nullIndicatorBit; }
        public void setNullIndicatorBit(int nullIndicatorBit)
        {
            this.nullIndicatorBit = nullIndicatorBit;
        }
        public SlotId getId() { return id; }
        public TupleDescriptor getParent() { return parent; }
        public SqlNodeType getType() { return type; }
        public void setType(SqlNodeType type) { this.type = type; }
        public TupleDescriptor getItemTupleDesc() { return itemTupleDesc; }
        public void setItemTupleDesc(TupleDescriptor t)
        {
            if (itemTupleDesc!=null)
            {
                throw new ArgumentException("Item tuple descriptor already set.");
            }
            itemTupleDesc = t;
        }
        public bool IsMaterialized() { return isMaterialized; }
        public void setIsMaterialized(bool value) { isMaterialized = value; }
        public bool getIsNullable() { return isNullable; }
        public void setIsNullable(bool value) { isNullable = value; }
        public int getByteSize() { return byteSize; }
        public void setByteSize(int byteSize) { this.byteSize = byteSize; }
        public int getByteOffset() { return byteOffset; }
        public void setByteOffset(int byteOffset) { this.byteOffset = byteOffset; }
        public void setSlotIdx(int slotIdx) { this.slotIdx = slotIdx; }
        public string getLabel() { return label; }
        public void setLabel(string label) { this.label = label; }
        public void setSourceExprs(List<Expr> exprs) { sourceExprs = exprs; }
        public void setSourceExpr(Expr expr) { sourceExprs = Collections.singletonList(expr); }
        public void addSourceExpr(Expr expr) { sourceExprs.Add(expr); }
        public List<Expr> getSourceExprs() { return sourceExprs; }
        public void setStats(ColumnStats stats) { this.stats = stats; }

        public void setPath(Path path)
        {
            if (path==null || !path.isRootedAtTuple() || path.getRootDesc() != parent)
            {
                return;
            }
            this.path = path;
            type = path.destType();
            label = string.Join(".",path.getRawPath());

            // Set nullability, if this refers to a KuduColumn.
            if (path.destColumn() is KuduColumn) {
                KuduColumn kuduColumn = (KuduColumn)path.destColumn();
                isNullable = kuduColumn.isNullable();
            }
        }

        public Path getPath() { return path; }
        public bool isScanSlot() { return path != null && path.isRootedAtTable(); }
        public Column getColumn() { return !isScanSlot() ? null : path.destColumn(); }

        public ColumnStats getStats()
        {
            if (stats == null)
            {
                Column c = getColumn();
                if (c != null)
                {
                    stats = c.getStats();
                }
                else
                {
                    stats = new ColumnStats(type);
                }
            }
            return stats;
        }

        /**
         * Checks if this descriptor describes  an array "pos" pseudo-column.
         *
         * Note: checking whether the column is null distinguishes between top-level columns
         * and nested types. This check more specifically looks just for a reference to the
         * "pos" field of an array type.
         */
        public bool isArrayPosRef()
        {
            SqlNodeType parentType = parent?.GetType();
            if (parentType is CollectionStructType) {
                if (((CollectionStructType)parentType).isArrayStruct() &&
                    label.Equals(Path.ARRAYPOSFIELDNAME))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Assembles the absolute materialized path to this slot starting from the schema
         * root. The materialized path points to the first non-struct schema element along the
         * path starting from the parent's tuple path to this slot's path.
         *
         * The materialized path is used to determine when a new tuple (containing a new
         * instance of this slot) should be created. A tuple is emitted for every data item
         * pointed to by the materialized path. For scalar slots this trivially means that every
         * data item goes into a different tuple. For collection slots, the materialized path
         * specifies how many data items go into a single collection value.
         *
         * For scalar slots, the materialized path is the same as its path. However, for
         * collection slots, the materialized path may be different than path. This happens
         * when the query materializes a "flattened" collection composed of concatenated nested
         * collections.
         *
         * For example, given the table:
         *   CREATE TABLE tbl (id bigint, outerarray array<array<int>>);
         *
         * And the query:
         *   select id, innerarray.item from tbl t, t.outerarray.item innerarray
         *
         * The path 't.outerarray.item' corresponds to the absolute path [1,0]. However, the
         * 'innerarray' slot appears in the table-level tuple, with tuplePath [] (i.e. one
         * tuple materialized per table row). There is a single array materialized per
         * 'outerarray', not per 'innerarray'. Thus the materializedPath for this slot will be
         * [1], not [1,0].
         */
        public List<int> getMaterializedPath()
        {
            if (parent==null)
            {
                return new List<int>();
            }
            // A slot descriptor typically only has a path if the parent also has one.
            // However, we sometimes materialize inline-view tuples when generating plan trees
            // with EmptySetNode portions. In that case, a slot descriptor could have a non-empty
            // path pointing into the inline-view tuple (which has no path).
            if (!isScanSlot() || parent.getPath() == null)
            {
                return new List<int>();
            }
            if (!path.IsResolved())
            {
                return new List<int>();
            }

            List<int> materializedPath = new List<int>(path.getAbsolutePath());
            // For scalar types, the materialized path is the same as path
            if (type.isScalarType()) return materializedPath;
            Preconditions.checkState(type.isCollectionType());
            Preconditions.checkState(path.getFirstCollectionIndex() != -1);
            // Truncate materializedPath after first collection element
            // 'offset' adjusts for the index returned by path.getFirstCollectionIndex() being
            // relative to path.getRootDesc()
            int offset = !path.isRootedAtTuple() ? 0 :
                path.getRootDesc().getPath().getAbsolutePath().size();
            materializedPath.subList(
                offset + path.getFirstCollectionIndex() + 1, materializedPath.size()).clear();
            return materializedPath;
        }

        /**
         * Initializes a slot by setting its source expression information
         */
        public void initFromExpr(Expr expr)
        {
            setLabel(expr.toSql());
            if(!sourceExprs.isEmpty())
            { return; }
            setSourceExpr(expr);
            setStats(ColumnStats.fromExpr(expr));
            if (!expr.getType().isValid()) { return; }
            setType(expr.getType());
        }

        /**
         * Return true if the physical layout of this descriptor matches the physical layout
         * of the other descriptor, but not necessarily ids.
         */
        public bool LayoutEquals(SlotDescriptor other)
        {
            if (!getType().Equals(other.getType())) return false;
            if (isNullable != other.isNullable) return false;
            if (getByteSize() != other.getByteSize()) return false;
            if (getByteOffset() != other.getByteOffset()) return false;
            if (getNullIndicatorByte() != other.getNullIndicatorByte()) return false;
            if (getNullIndicatorBit() != other.getNullIndicatorBit()) return false;
            return true;
        }

        //public TSlotDescriptor toThrift()
        //{
        //    Preconditions.checkState(isMaterialized);
        //    List<int> materializedPath = getMaterializedPath();
        //    TSlotDescriptor result = new TSlotDescriptor(
        //        id.asInt(), parent.getId().asInt(), type.toThrift(),
        //        materializedPath, byteOffset, nullIndicatorByte, nullIndicatorBit,
        //        slotIdx);
        //    if (itemTupleDesc != null)
        //    {
        //        // Check for recursive or otherwise invalid item tuple descriptors. Since we assign
        //        // tuple ids globally in increasing order, the id of an item tuple descriptor must
        //        // always have been generated after the parent tuple id if the tuple/slot belong
        //        // to a base table. For example, tuples/slots introduced during planning do not
        //        // have such a guarantee.
        //        Preconditions.checkState(!isScanSlot() ||
        //            itemTupleDesc.getId().asInt() > parent.getId().asInt());
        //        result.setItemTupleId(itemTupleDesc.getId().asInt());
        //    }
        //    return result;
        //}

        public string debugString()
        {
            string pathStr = (path == null) ? "null" : path.ToString();
            string typeStr = (type == null ? "null" : type.ToString());
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("id {0}", id));
            sb.Append(string.Format("path {0}", pathStr));
            sb.Append(string.Format("type {0}", typeStr));
            sb.Append(string.Format("materialized {0}", isMaterialized));
            sb.Append(string.Format("byteSize {0}", byteSize));
            sb.Append(string.Format("byteOffset {0}", byteOffset));
            sb.Append(string.Format("nullable {0}", isNullable));
            sb.Append(string.Format("nullIndicatorByte {0}", nullIndicatorByte));
            sb.Append(string.Format("nullIndicatorBit {0}", nullIndicatorBit));
            sb.Append(string.Format("slotIdx {0}", slotIdx));
            sb.Append(string.Format("stats {0}", stats));
            return sb.ToString();
        }

    }
}
