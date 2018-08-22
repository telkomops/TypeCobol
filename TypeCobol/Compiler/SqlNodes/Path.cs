using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Catalog;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     Represents a resolved or unresolved dot-separated path that is rooted at a registered
     tuple descriptor, catalog table/view, or an existing resolved path.
     
     This class implements the resolution logic for mapping an implicit or explicit
     raw path to the corresponding physical types/positions in the schema tree.
     
     Implicit vs. Explicit Paths
     The item of an array and the key/value of maps are accessed via their implicit field
     names. However, if the type of an array item or a map value is a struct, then we allow
     omitting the explicit reference to the struct type in paths for accessing fields
     within that struct as a shorthand for user convenience. An explicit reference to the
     struct type is always legal. Paths that explicitly reference such a struct are
     "physical" because they typically map exactly to the schema representation in the
     underlying storage format (e.g. Parquet/Avro). Paths that omit the struct reference
     are called "implicit". During resolution, explicit paths are always preferred over
     implicit paths for resolving ambiguities.
     
     Example
     create table d.t (
       c array<struct<f:int,item:int,pos:int>>
     );
     
     select ... from d.t.c
       d.t.c   <-- resolves to type array<struct<f:int,item:int,pos:int>>
       c alias <-- type struct<item:struct<f:int,item:int,pos:int>,pos:bigint>>
     
     select c.item.f, c.f from d.t.c
       c.item.f   <-- explicit path to "f"
       c.f        <-- implicit path to "f", skips "item" reference
       (same for the unqualified versions item.f and f)
     
     select c.item, c.item.item from d.t.c
       c.item      <-- explicit path to "item" struct of type struct<f:int,item:string>
        c.item.item <-- explicit path to string "item"; there is no logical path to the
                        string "item" due to the "item" name conflict
        c.pos       <-- explicit path to "pos" of type bigint
        c.item.pos  <-- explicit path to "pos" of type int; there is no logical path to the
                        int "pos" due to the "pos" name conflict
        (same for unqualified versions item, item.item, pos, item.pos)
     
      Please refer to TestImplicitAndExplicitPaths() for analogous examples for maps.
     
      Illegal Implicit Paths
      The intention of implicit paths is to allow users to skip a *single* trivial level of
      indirection in common cases. In particular, it is illegal to implicitly skip multiple
      levels in a path, illustrated as follows.
     
      Example
      create table d.t (
        c array<array<struct<e:int,f:string>>>
      );
     
      select c.f from d.t.c
      select 1 from d.t.c, c.f
        c.f <-- illegal path because it would have to implicitly skip two 'item' fields
     
     
      Uses of Paths and Terminology
     
      Uncorrelated References: Star exprs, SlotRefs and TableRefs that are rooted at a
      catalog Table or a registered TupleDescriptor in the same query block.
     
      Relative References: TableRefs that are rooted at a TupleDescriptor.
     
      Correlated References: SlotRefs and TableRefs that are rooted at a TupleDescriptor
      registered in an ancestor query block are called 'correlated'. All correlated
      references are relative, but not all relative references are correlated.
     
      A Path itself is never said to be un/correlated because it is intentionally unaware
      of the query block that it is used in.
     */
    public class Path
    {
        // Implicit field names of collections.
        public static readonly string ARRAYITEMFIELDNAME = "item";
        public static readonly string ARRAYPOSFIELDNAME = "pos";
        public static readonly string MAPKEYFIELDNAME = "key";
        public static readonly string MAPVALUEFIELDNAME = "value";

        

        // Implicit or explicit raw path to be resolved relative to rootDesc or rootTable.
        // Every raw-path element is mapped to zero, one or two types/positions in resolution.
        private readonly List<string> rawPath;

        // Registered table alias that this path is rooted at, if any.
        // Null if the path is rooted at a catalog table/view.
        private readonly TupleDescriptor rootDesc;

  // Catalog table that this resolved path is rooted at, if any.
  // Null if the path is rooted at a registered tuple that does not
  // belong to a catalog table/view.
  //private readonly FeTable rootTable;

  // Root path that a relative path was created from.
  private readonly Path rootPath;

  // List of matched types and field positions set during resolution. The matched
  // types/positions describe the physical path through the schema tree.
  private readonly List<SqlNodeType> matchedTypes = new List<SqlNodeType>();
        private readonly List<int> matchedPositions = new List<int>();

        // Remembers the indices into rawPath and matchedTypes of the first collection
        // matched during resolution.
        private int firstCollectionPathIdx = -1;
        private int firstCollectionTypeIdx = -1;

        // Indicates whether this path has been resolved. Set in resolve().
        private bool isResolved = false;

        // Caches the result of getAbsolutePath() to avoid re-computing it.
        private List<int> absolutePath = null;

        /**
         * Constructs a Path rooted at the given rootDesc.
         */
        public Path(TupleDescriptor rootDesc, List<string> rawPath)
        {
            if (rootDesc==null || rawPath == null)
            {
                return;
            }
            //rootTable = rootDesc.getTable();
            this.rootDesc = rootDesc;
            rootPath = null;
            this.rawPath = rawPath;
        }

        /**
         * Constructs a Path rooted at the given rootTable.
         */
        //public Path(FeTable rootTable, List<string> rawPath)
        //{
        //    if (rootDesc == null || rawPath == null)
        //    {
        //        return;
        //    }
        //    rootTable = rootTable;
        //    rootDesc = null;
        //    rootPath = null;
        //    this.rawPath = rawPath;
        //}

        /**
         * Constructs a new unresolved path relative to an existing resolved path.
         */
        public Path(Path rootPath, List<string> relRawPath)
        {
            if (rootDesc == null || relRawPath == null ||!rootPath.IsResolved())
            {
                return;
            }
            //rootTable = rootPath.rootTable;
            rootDesc = rootPath.rootDesc;
            this.rootPath = rootPath;
            rawPath = new List<string>(
                rootPath.getRawPath().Count + relRawPath.Count);
            rawPath.AddRange(rootPath.getRawPath());
            rawPath.AddRange(relRawPath);
            matchedTypes.AddRange(rootPath.matchedTypes);
            matchedPositions.AddRange(rootPath.matchedPositions);
            firstCollectionPathIdx = rootPath.firstCollectionPathIdx;
            firstCollectionTypeIdx = rootPath.firstCollectionTypeIdx;
        }

        /**
         * Resolves this path in the context of the root tuple descriptor / root table
         * or continues resolving this relative path from an existing root path.
         * Returns true if the path could be fully resolved, false otherwise.
         * A failed resolution leaves this Path in a partially resolved state.
         */
        public bool resolve()
        {
            if (isResolved) return true;
            if (rootDesc == null /*|| rootTable == null*/)
            {
                return false;
            }
            SqlNodeType currentType = null;
            int rawPathIdx = 0;
            if (rootPath != null)
            {
                // Continue resolving this path relative to the rootPath.
                currentType = rootPath.destType();
                rawPathIdx = rootPath.getRawPath().Count;
            }
            else if (rootDesc != null)
            {
                currentType = rootDesc.GetType();
            }
            else
            {
                // Directly start from the item type because only implicit paths are allowed.
                //currentType = rootTable.getType().getItemType();
            }

            // Map all remaining raw-path elements to field types and positions.
            while (rawPathIdx < rawPath.Count)
            {
                if (!currentType.isComplexType()) return false;
                StructType structType = getTypeAsStruct(currentType);
                // Resolve explicit path.
                StructField field = structType.getField(rawPath[rawPathIdx]);
                if (field == null)
                {
                    // Resolve implicit path.
                    if (structType is CollectionStructType) {
                        field = ((CollectionStructType)structType).getOptionalField();
                        // Collections must be matched explicitly.
                        if (field.getType().isCollectionType()) return false;
                    } else {
                        // Failed to resolve implicit or explicit path.
                        return false;
                    }
                    // Update the physical types/positions.
                    matchedTypes.Add(field.getType());
                    matchedPositions.Add(field.getPosition());
                    currentType = field.getType();
                    // Do not consume a raw-path element.
                    continue;
                }
                matchedTypes.Add(field.getType());
                matchedPositions.Add(field.getPosition());
                if (field.getType().isCollectionType() && firstCollectionPathIdx == -1)
                {
                    if (firstCollectionTypeIdx != -1)
                    {
                        return false;
                    }
                    firstCollectionPathIdx = rawPathIdx;
                    firstCollectionTypeIdx = matchedTypes.Count - 1;
                }
                currentType = field.getType();
                ++rawPathIdx;
            }
           if (matchedTypes.Count != matchedPositions.Count || matchedTypes.Count <= rawPath.Count)
            {
                return false;
            }
            isResolved = true;
            return true;
        }

        /**
         * If the given type is a collection, returns a collection struct type representing
         * named fields of its explicit path. Returns the given type itself if it is already
         * a struct. Requires that the given type is a complex type.
         */
        public static StructType getTypeAsStruct(SqlNodeType t)
        {
            if(!t.isComplexType())
            { return null; }
            if (t.isStructType()) return (StructType)t;
            if (t.isArrayType())
            {
                return CollectionStructType.createArrayStructType((ArrayType)t);
            }
            else
            {
                if (!t.isMapType())
                {
                    return null;
                }
                return CollectionStructType.createMapStructType((MapType)t);
            }
        }

        /**
         * Returns a list of table names that might be referenced by the given path.
         * The path must be non-empty.
         *
         * Examples: path -> result
         * a -> [<sessionDb>.a]
         * a.b -> [<sessionDb>.a, a.b]
         * a.b.c -> [<sessionDb>.a, a.b]
         * a.b.c... -> [<sessionDb>.a, a.b]
         */
        public static List<TableName> getCandidateTables(List<string> path, string sessionDb)
        {
            if(path == null && path.Any())
            {
                return new List<TableName>();
            }
            List<TableName> result = new List<TableName>();
            int end = Math.Min(2, path.Count);
            for (int tblNameIdx = 0; tblNameIdx < end; ++tblNameIdx)
            {
                string dbName = (tblNameIdx == 0) ? sessionDb : path[0];
                string tblName = path[tblNameIdx];
                result.Add(new TableName(dbName, tblName));
            }
            return result;
        }

        //public FeTable getRootTable() { return rootTable; }
        public TupleDescriptor getRootDesc() { return rootDesc; }

        public bool isRootedAtTable()
        {
            //TODO - evalute impact of not using method as rootTable depends on Hive
            /*return rootTable != null;*/
            return false;

        }
        public bool isRootedAtTuple() { return rootDesc != null; }
        public List<string> getRawPath() { return rawPath; }
        public bool IsResolved() { return this.isResolved; }

        public List<SqlNodeType> getMatchedTypes()
        {
            if (!isResolved)
            { return null; }
            return matchedTypes;
        }

        public bool hasNonDestCollection()
        {
            if (!isResolved)
            { return false; }
            return firstCollectionPathIdx != -1 &&
                firstCollectionPathIdx != rawPath.Count - 1;
        }

        public string getFirstCollectionName()
        {
            if (!isResolved)
            { return null; }
            if (firstCollectionPathIdx == -1) return null;
            return rawPath[firstCollectionPathIdx];
        }

        public SqlNodeType getFirstCollectionType()
        {
            if (!isResolved)
            { return null; }
            if (firstCollectionTypeIdx == -1) return null;
            return matchedTypes[firstCollectionTypeIdx];
        }

        public int getFirstCollectionIndex()
        {
            if (!isResolved)
            { return -1; }
            return firstCollectionTypeIdx;
        }

        public SqlNodeType destType()
        {
            if (!isResolved)
            { return null; }
            if (!matchedTypes.Any()) return matchedTypes[matchedTypes.Count - 1];
            if (rootDesc != null) return rootDesc.GetType();
            //if (rootTable != null) return rootTable.GetType();
            return null;
        }

        //public FeTable destTable()
        //{
        //    if (!isResolved)
        //    { return null; }
        //    if (rootTable != null && rootDesc == null && !matchedTypes.Any())
        //    {
        //        return rootTable;
        //    }
        //    return null;
        //}

        /**
         * Returns the destination Column of this path, or null if the destination of this
         * path is not a Column. This path must be rooted at a table or a tuple descriptor
         * corresponding to a table for the destination to be a Column.
         */
        public Column destColumn()
        {
            if (!isResolved)
            { return null; }
            //TODO = analize how to get Column if rootTable doesn't come from hive
            //if (rootTable == null || rawPath.Count != 1) return null;
            //return rootTable.getColumn(rawPath[rawPath.Count - 1]);
        }

        /**
         * Returns the destination tuple descriptor of this path, or null
         * if the destination of this path is not a registered alias.
         */
        public TupleDescriptor destTupleDesc()
        {
           if (!isResolved)
            { return null; }
            if (rootDesc != null && !matchedTypes.Any()) return rootDesc;
            return null;
        }

        //TODO = analize how to get getFullyQualifiedRawPath if rootTable doesn't come from hive
        //public List<string> getFullyQualifiedRawPath()
        //{
        //    if(rootTable == null || rootDesc == null)
        //    {
        //        return new List<string>();
        //    }
        //    List<string> result =new List<string>(rawPath.Count + 2);
        //    if (rootDesc != null)
        //    {
        //        result.AddRange(new List<string>(rootDesc.getAlias().Split(new string[] { "\\." }, StringSplitOptions.RemoveEmptyEntries)));
        //    }
        //    else
        //    {
        //        result.Add(rootTable.getDb().getName());
        //        result.Add(rootTable.getName());
        //    }
        //    result.AddRange(rawPath);
        //    return result;
        //}

        /**
         * Returns the absolute explicit path starting from the fully-qualified table name.
         * The goal is produce a canonical non-ambiguous path that can be used as an
         * identifier for table and slot references.
         *
         * Example:
         * create table mydb.test (a array<struct<f1:int,f2:string>>);
         * use mydb;
         * select f1 from test t, t.a;
         *
         * This function should return the following for the path of the 'f1' SlotRef:
         * mydb.test.a.item.f1
         */
        public List<string> getCanonicalPath()
        {
            List<string> result = new List<string>();
            getCanonicalPath(result);
            return result;
        }

        /**
         * Recursive helper for getCanonicalPath().
         */
        private void getCanonicalPath(List<string> result)
        {
            SqlNodeType currentType = null;
            if (isRootedAtTuple())
            {
                rootDesc.getPath().getCanonicalPath(result);
                currentType = rootDesc.GetType();
            }
            else
            {
                if (!isRootedAtTable())
                {
                    return;
                }
                //TODO = analize how to get Column if rootTable doesn't come from hive
                //result.Add(rootTable.getTableName().getDb());
                //result.Add(rootTable.getTableName().getTbl());
                //currentType = rootTable.getType().getItemType();
            }
            // Compute the explicit path from the matched positions. Note that rawPath is
            // not sufficient because it could contain implicit matches.
            for (int i = 0; i < matchedPositions.Count; ++i)
            {
                StructType structType = getTypeAsStruct(currentType);
                int matchPos = matchedPositions[i];
                if (matchPos > structType.getFields().Count)
                {
                    continue;
                }
                StructField match = structType.getFields()[matchPos];
                result.Add(match.getName());
                currentType = match.getType();
            }
        }

        /**
         * Returns the absolute physical path in positions starting from the schema root to the
         * destination of this path.
         */
        public List<int> getAbsolutePath()
        {
            if (absolutePath != null) return absolutePath;
            if (!isResolved)
            {
                return new List<int>();
            }
            absolutePath = new List<int>();
            if (rootDesc != null) absolutePath.AddRange(rootDesc.getPath().getAbsolutePath());
            absolutePath.AddRange(matchedPositions);
            return absolutePath;
        }
        
        public override string ToString()
        {
            //if (rootTable == null || rootDesc == null)
            //{
            //    return string.Empty;
            //}
            string pathRoot = null;
            if (rootDesc != null)
            {
                pathRoot = rootDesc.getAlias();
            }
            //TODO = analize how to get Column if rootTable doesn't come from hive
            //else
            //{
            //    pathRoot = rootTable.getFullName();
            //}
            if (!rawPath.Any()) return pathRoot;
            return pathRoot + "." + string.Join(".",rawPath);
        }

        /**
         * Returns a raw path from a known root alias and field name.
         */
        public static List<string> createRawPath(string rootAlias, string fieldName)
        {
            List<string> result = new List<string>(rootAlias.Split(new string[] { "\\." },StringSplitOptions.RemoveEmptyEntries));
            result.Add(fieldName);
            return result;
        }

        public static Path createRelPath(Path rootPath, string[]fieldNames)
        {
            if (!rootPath.IsResolved())
            {
                return null;
            }
            Path result = new Path(rootPath, new List<string>(fieldNames));
            return result;
        }
    }

    public enum PathType
    {
        SLOTREF,
        TABLEREF,
        STAR,
        ANY, // Reference to any field or table in schema.
    }
}
