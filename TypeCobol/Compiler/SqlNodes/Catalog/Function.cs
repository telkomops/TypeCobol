using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    /**
     * Base class for all functions.
     * Each function can be of the following 4 types.
     * - Native/IR stored in db params (persisted, visible to Impala)
     * - Hive UDFs stored in the HMS (visible to Hive + Impala)
     * - Java UDFs which are not persisted (visible to Impala but not Hive)
     * - Builtin functions, which are recreated after every restart of the
     *   catalog. (persisted, visible to Impala)
     */
    public class Function : CatalogObjectImpl
    {
        // Enum for how to compare function signatures.
        // For decimal types, the type in the function can be a wildcard, i.e. decimal(*,*).
        // The wildcard can *only* exist as function type, the caller will always be a
        // fully specified decimal.
        // For the purposes of function type resolution, decimal(*,*) will match exactly
        // with any fully specified decimal (i.e. fn(decimal(*,*)) matches identically for
        // the call to fn(decimal(1,0)).
        public enum CompareMode
        {
            // Two signatures are identical if the number of arguments and their types match
            // exactly and either both signatures are varargs or neither.
            IS_IDENTICAL,

            // Two signatures are indistinguishable if there is no way to tell them apart
            // when matching a particular instantiation. That is, their fixed arguments
            // match exactly and the remaining varargs have the same type.
            // e.g. fn(int, int, int) and fn(int...)
            // Argument types that are NULL are ignored when doing this comparison.
            // e.g. fn(NULL, int) is indistinguishable from fn(int, int)
            IS_INDISTINGUISHABLE,

            // X is a supertype of Y if Y.arg[i] can be strictly implicitly cast to X.arg[i]. If
            /// X has vargs, the remaining arguments of Y must be strictly implicitly castable
            // to the var arg type. The key property this provides is that X can be used in place
            // of Y. e.g. fn(int, double, string...) is a supertype of fn(tinyint, float, string,
            // string)
            IS_SUPERTYPE_OF,

            // Nonstrict supertypes broaden the definition of supertype to accept implicit casts
            // of arguments that may result in loss of precision - e.g. decimal to float.
            IS_NONSTRICT_SUPERTYPE_OF,
        }

        // User specified function name e.g. "Add"
        private FunctionName name_;

        private readonly SqlNodeType retType_;
        // Array of parameter types.  empty array if this function does not have parameters.
        private SqlNodeType[] argTypes_;

        // If true, this function has variable arguments.
        // TODO: we don't currently support varargs with no fixed types. i.e. fn(...)
        private bool hasVarArgs_;

        // If true (default), this function is called directly by the user. For operators,
        // this is false. If false, it also means the function is not visible from
        // 'show functions'.
        private bool userVisible_;

        // Absolute path in HDFS for the binary that contains this function.
        // e.g. /udfs/udfs.jar
        //private HdfsUri location_;
        //private TFunctionBinaryType binaryType_;

        // Set to true for functions that survive service restarts, including all builtins,
        // native and IR functions, but only Java functions created without a signature.
        private bool isPersistent_;

        public Function(FunctionName name, SqlNodeType[] argTypes,
      SqlNodeType retType, bool varArgs)
        {
            this.name_ = name;
            this.hasVarArgs_ = varArgs;
            if (argTypes == null)
            {
                argTypes_ = new SqlNodeType[0];
            }
            else
            {
                this.argTypes_ = argTypes;
            }
            if (retType == null)
            {
                this.retType_ = ScalarType.INVALID;
            }
            else
            {
                this.retType_ = retType;
            }
            this.userVisible_ = true;
        }

        public Function(FunctionName name, List<SqlNodeType> args,
            SqlNodeType retType, bool varArgs)
        : this(name, (SqlNodeType[])null, retType, varArgs)
        { 
            if (args != null && args.Count > 0)
            {
                argTypes_ = args.ToArray(new SqlNodeType[args.Count]);
            }
            else
            {
                argTypes_ = new SqlNodeType[0];
            }
        }

        /**
         * Static helper method to create a function with a given TFunctionBinaryType.
         */
        //public static Function createFunction(String db, String fnName, List<SqlNodeType> args,
        //    SqlNodeType retType, bool varArgs, TFunctionBinaryType fnType)
        //{
        //    Function fn =
        //        new Function(new FunctionName(db, fnName), args, retType, varArgs);
        //    fn.setBinaryType(fnType);
        //    return fn;
        //}

        public FunctionName getFunctionName() { return name_; }
        public String functionName() { return name_.getFunction(); }
        public String dbName() { return name_.getDb(); }
        public SqlNodeType getReturnType() { return retType_; }
        public SqlNodeType[] getArgs() { return argTypes_; }
        // Returns the number of arguments to this function.
        public int getNumArgs() { return argTypes_.Length; }
        //public HdfsUri getLocation() { return location_; }
        //public TFunctionBinaryType getBinaryType() { return binaryType_; }
        public bool hasVarArgs() { return hasVarArgs_; }
        public bool isPersistent() { return isPersistent_; }
        public bool userVisible() { return userVisible_; }
        public SqlNodeType getVarArgsType()
        {
            if (!hasVarArgs_) return SqlNodeType.INVALID;
            //Preconditions.checkState(argTypes_.Length > 0);
            return argTypes_[argTypes_.Length - 1];
        }

        //public void setLocation(HdfsUri loc) { location_ = loc; }
        //public void setBinaryType(TFunctionBinaryType type) { binaryType_ = type; }
        public void setHasVarArgs(bool v) { hasVarArgs_ = v; }
        public void setIsPersistent(bool v) { isPersistent_ = v; }
        public void setUserVisible(bool b) { userVisible_ = b; }

        // Returns a string with the signature in human readable format:
        // FnName(argtype1, argtyp2).  e.g. Add(int, int)
        public String signatureString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name_.getFunction())
              .Append("(")
              .Append(string.Join(", ",argTypes_.ToList()));
            if (hasVarArgs_) sb.Append("...");
            sb.Append(")");
            return sb.ToString();
        }
        
  public override bool Equals(Object o)
        {
            if (!(o is Function)) return false;
            return compare((Function)o, CompareMode.IS_IDENTICAL);
        }

        // Compares this to 'other' for mode.
        public bool compare(Function other, CompareMode mode)
        {
            switch (mode)
            {
                case IS_IDENTICAL: return isIdentical(other);
                case IS_INDISTINGUISHABLE: return isIndistinguishable(other);
                case IS_SUPERTYPE_OF: return isSuperTypeOf(other, true);
                case IS_NONSTRICT_SUPERTYPE_OF: return isSuperTypeOf(other, false);
                default:
                    Preconditions.checkState(false);
                    return false;
            }
        }
        /**
         * Returns true if 'this' is a supertype of 'other'. Each argument in other must
         * be implicitly castable to the matching argument in this. If strict is true,
         * only consider conversions where there is no loss of precision.
         */
        private bool isSuperTypeOf(Function other, bool strict)
        {
            if (!other.name_.Equals(name_)) return false;
            if (!this.hasVarArgs_ && other.argTypes_.Length != this.argTypes_.Length)
            {
                return false;
            }
            if (this.hasVarArgs_ && other.argTypes_.Length < this.argTypes_.Length) return false;
            for (int i = 0; i < this.argTypes_.Length; ++i)
            {
                if (!SqlNodeType.isImplicitlyCastable(
                    other.argTypes_[i], this.argTypes_[i], strict, strict))
                {
                    return false;
                }
            }
            // Check trailing varargs.
            if (this.hasVarArgs_)
            {
                for (int i = this.argTypes_.Length; i < other.argTypes_.Length; ++i)
                {
                    if (other.argTypes_[i].matchesType(this.getVarArgsType())) continue;
                    if (!SqlNodeType.isImplicitlyCastable(other.argTypes_[i], this.getVarArgsType(),
                          strict, strict))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /**
         * Converts any CHAR arguments to be STRING arguments
         */
        public Function promoteCharsToStrings()
        {
            SqlNodeType[] promoted = argTypes_.clone();
            for (int i = 0; i < promoted.Length; ++i)
            {
                if (promoted[i].isScalarType(PrimitiveType.CHAR)) promoted[i] = ScalarType.STRING;
            }
            return new Function(name_, promoted, retType_, hasVarArgs_);
        }

        private bool isIdentical(Function o)
        {
            if (!o.name_.Equals(name_)) return false;
            if (o.argTypes_.Length != this.argTypes_.Length) return false;
            if (o.hasVarArgs_ != this.hasVarArgs_) return false;
            for (int i = 0; i < this.argTypes_.Length; ++i)
            {
                if (!o.argTypes_[i].matchesType(this.argTypes_[i])) return false;
            }
            return true;
        }

        private bool isIndistinguishable(Function o)
        {
            if (!o.name_.Equals(name_)) return false;
            int minArgs = Math.Min(o.argTypes_.Length, this.argTypes_.Length);
            // The first fully specified args must be identical.
            for (int i = 0; i < minArgs; ++i)
            {
                if (o.argTypes_[i].isNull() || this.argTypes_[i].isNull()) continue;
                if (!o.argTypes_[i].matchesType(this.argTypes_[i])) return false;
            }
            if (o.argTypes_.Length == this.argTypes_.Length) return true;

            if (o.hasVarArgs_ && this.hasVarArgs_)
            {
                if (!o.getVarArgsType().matchesType(this.getVarArgsType())) return false;
                if (this.getNumArgs() > o.getNumArgs())
                {
                    for (int i = minArgs; i < this.getNumArgs(); ++i)
                    {
                        if (this.argTypes_[i].isNull()) continue;
                        if (!this.argTypes_[i].matchesType(o.getVarArgsType())) return false;
                    }
                }
                else
                {
                    for (int i = minArgs; i < o.getNumArgs(); ++i)
                    {
                        if (o.argTypes_[i].isNull()) continue;
                        if (!o.argTypes_[i].matchesType(this.getVarArgsType())) return false;
                    }
                }
                return true;
            }
            else if (o.hasVarArgs_)
            {
                // o has var args so check the remaining arguments from this
                if (o.getNumArgs() > minArgs) return false;
                for (int i = minArgs; i < this.getNumArgs(); ++i)
                {
                    if (this.argTypes_[i].isNull()) continue;
                    if (!this.argTypes_[i].matchesType(o.getVarArgsType())) return false;
                }
                return true;
            }
            else if (this.hasVarArgs_)
            {
                // this has var args so check the remaining arguments from s
                if (this.getNumArgs() > minArgs) return false;
                for (int i = minArgs; i < o.getNumArgs(); ++i)
                {
                    if (o.argTypes_[i].isNull()) continue;
                    if (!o.argTypes_[i].matchesType(this.getVarArgsType())) return false;
                }
                return true;
            }
            else
            {
                // Neither has var args and the lengths don't match
                return false;
            }
        }

  //      @Override
  //public TCatalogObjectType getCatalogObjectType() { return TCatalogObjectType.FUNCTION; }
        //@Override
  public String getName() { return getFunctionName().ToString(); }
        //@Override
  public String getUniqueName()
        {
            return "FUNCTION:" + name_.ToString() + "(" + signatureString() + ")";
        }

        // Child classes must override this function.
        public String toSql(bool ifNotExists) { return ""; }

        //public TCatalogObject toTCatalogObject()
        //{
        //    TCatalogObject result = new TCatalogObject();
        //    result.setType(TCatalogObjectType.FUNCTION);
        //    result.setFn(toThrift());
        //    result.setCatalog_version(getCatalogVersion());
        //    return result;
        //}

        //public TFunction toThrift()
        //{
        //    TFunction fn = new TFunction();
        //    fn.setSignature(signatureString());
        //    fn.setName(name_.toThrift());
        //    fn.setBinary_type(binaryType_);
        //    if (location_ != null) fn.setHdfs_location(location_.ToString());
        //    fn.setArg_types(SqlNodeType.toThrift(argTypes_));
        //    fn.setRet_type(getReturnType().toThrift());
        //    fn.setHas_var_args(hasVarArgs_);
        //    fn.setIs_persistent(isPersistent_);
        //    // TODO: Comment field is missing?
        //    // fn.setComment(comment_)
        //    return fn;
        //}

        //public static Function fromThrift(TFunction fn)
        //{
        //    List<SqlNodeType> argTypes = Lists.newArrayList();
        //    for (TColumnType t: fn.getArg_types())
        //    {
        //        argTypes.add(SqlNodeType.fromThrift(t));
        //    }

        //    Function function = null;
        //    if (fn.isSetScalar_fn())
        //    {
        //        TScalarFunction scalarFn = fn.getScalar_fn();
        //        function = new ScalarFunction(FunctionName.fromThrift(fn.getName()), argTypes,
        //            SqlNodeType.fromThrift(fn.getRet_type()), new HdfsUri(fn.getHdfs_location()),
        //            scalarFn.getSymbol(), scalarFn.getPrepare_fn_symbol(),
        //            scalarFn.getClose_fn_symbol());
        //    }
        //    else if (fn.isSetAggregate_fn())
        //    {
        //        TAggregateFunction aggFn = fn.getAggregate_fn();
        //        function = new AggregateFunction(FunctionName.fromThrift(fn.getName()), argTypes,
        //            SqlNodeType.fromThrift(fn.getRet_type()),
        //            SqlNodeType.fromThrift(aggFn.getIntermediate_type()),
        //            new HdfsUri(fn.getHdfs_location()), aggFn.getUpdate_fn_symbol(),
        //            aggFn.getInit_fn_symbol(), aggFn.getSerialize_fn_symbol(),
        //            aggFn.getMerge_fn_symbol(), aggFn.getGet_value_fn_symbol(),
        //            null, aggFn.getFinalize_fn_symbol());
        //    }
        //    else
        //    {
        //        // In the case where we are trying to look up the object, we only have the
        //        // signature.
        //        function = new Function(FunctionName.fromThrift(fn.getName()),
        //            argTypes, SqlNodeType.fromThrift(fn.getRet_type()), fn.isHas_var_args());
        //    }
        //    function.setBinaryType(fn.getBinary_type());
        //    function.setHasVarArgs(fn.isHas_var_args());
        //    if (fn.isSetIs_persistent())
        //    {
        //        function.setIsPersistent(fn.isIs_persistent());
        //    }
        //    else
        //    {
        //        function.setIsPersistent(false);
        //    }
        //    return function;
        //}

        //protected final TSymbolLookupParams buildLookupParams(String symbol,
        //    TSymbolType symbolType, SqlNodeType retArgType, bool hasVarArgs, bool needsRefresh,
        //    SqlNodeType...argTypes)
        //{
        //    TSymbolLookupParams lookup = new TSymbolLookupParams();
        //    // Builtin functions do not have an external library, they are loaded directly from
        //    // the running process
        //    lookup.location =
        //        binaryType_ != TFunctionBinaryType.BUILTIN ? location_.ToString() : "";
        //    lookup.symbol = symbol;
        //    lookup.symbol_type = symbolType;
        //    lookup.fn_binary_type = binaryType_;
        //    lookup.arg_types = SqlNodeType.toThrift(argTypes);
        //    lookup.has_var_args = hasVarArgs;
        //    lookup.needs_refresh = needsRefresh;
        //    if (retArgType != null) lookup.setRet_arg_type(retArgType.toThrift());
        //    return lookup;
        //}

        //protected TSymbolLookupParams getLookupParams()
        //{
        //    throw new NotImplementedException(
        //        "getLookupParams not implemented for " + getClass().getSimpleName());
        //}

        // Looks up the last time the function's source file was updated as recorded in its
        // backend lib-cache entry. Returns -1 if a modified time is not applicable.
        // If an error occurs and the mtime cannot be retrieved, an IllegalStateException is
        // thrown.

        //TODO - readonly in JAVA - put sealed override wher the base class is implemented
        public long getLastModifiedTime()
        {
            if (getBinaryType() != TFunctionBinaryType.BUILTIN && getLocation() != null)
            {
                //Preconditions.checkState(!getLocation().ToString().isEmpty());
                TSymbolLookupParams lookup = Preconditions.checkNotNull(getLookupParams());
                try
                {
                    TSymbolLookupResult result = FeSupport.LookupSymbol(lookup);
                    return result.last_modified_time;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        "Unable to get last modified time for lib file: " + getLocation().ToString(),
                        e);
                }
            }
            return -1;
        }

        // Returns the resolved symbol in the binary. The BE will do a lookup of 'symbol'
        // in the binary and try to resolve unmangled names.
        // If this function is expecting a return argument, retArgType is that type. It should
        // be null if this function isn't expecting a return argument.
        public String lookupSymbol(String symbol, TSymbolType symbolType, SqlNodeType retArgType,
            bool hasVarArgs, SqlNodeType argTypes) 
        {
    if (symbol.Length == 0) {
                if (binaryType_ == TFunctionBinaryType.BUILTIN)
                {
                    // We allow empty builtin symbols in order to stage work in the FE before its
                    // implemented in the BE
                    return symbol;
                }
                throw new AnalysisException("Could not find symbol ''");
            }

            TSymbolLookupParams lookup =
        buildLookupParams(symbol, symbolType, retArgType, hasVarArgs, true, argTypes);

    try {
                TSymbolLookupResult result = FeSupport.LookupSymbol(lookup);
                switch (result.result_code)
                {
                    case SYMBOL_FOUND:
                        return result.symbol;
                    case BINARY_NOT_FOUND:
                        Preconditions.checkState(binaryType_ != TFunctionBinaryType.BUILTIN);
                        throw new AnalysisException(
                            "Could not load binary: " + location_.getLocation() + "\n" +
                            result.error_msg);
                    case SYMBOL_NOT_FOUND:
                        throw new AnalysisException(result.error_msg);
                    default:
                        // Should never get here.
                        throw new AnalysisException("Internal Error");
                }
            } catch (Exception e) {
                // Should never get here.
                //e.printStackTrace();
                throw new AnalysisException("Could not find symbol: " + symbol, e);
            }
        }

        public String lookupSymbol(String symbol, TSymbolType symbolType)
        {
        //    Preconditions.checkState(
        //symbolType == TSymbolType.UDF_PREPARE || symbolType == TSymbolType.UDF_CLOSE);
            return lookupSymbol(symbol, symbolType, null, false);
        }

        public static String getUdfType(SqlNodeType t)
        {
            switch (t.getPrimitiveType())
            {
                case bool:
                    return "BooleanVal";
                case TINYINT:
                    return "TinyIntVal";
                case SMALLINT:
                    return "SmallIntVal";
                case INT:
                    return "IntVal";
                case BIGINT:
                    return "BigIntVal";
                case FLOAT:
                    return "FloatVal";
                case DOUBLE:
                    return "DoubleVal";
                case STRING:
                case VARCHAR:
                case CHAR:
                case FIXED_UDA_INTERMEDIATE:
                    // These types are marshaled into a StringVal.
                    return "StringVal";
                case TIMESTAMP:
                    return "TimestampVal";
                case DECIMAL:
                    return "DecimalVal";
                default:
                    Preconditions.checkState(false, t.ToString());
                    return "";
            }
        }

        /**
         * Returns true if the given function matches the specified category.
         */
        public static bool categoryMatch(Function fn, TFunctionCategory category)
        {
            //Preconditions.checkNotNull(category);
            return (category == TFunctionCategory.SCALAR && fn is ScalarFunction)
        || (category == TFunctionCategory.AGGREGATE
            && fn is AggregateFunction
            && ((AggregateFunction)fn).isAggregateFn())
        || (category == TFunctionCategory.ANALYTIC
            && fn is AggregateFunction
            && ((AggregateFunction)fn).isAnalyticFn());
        }
    }
}
