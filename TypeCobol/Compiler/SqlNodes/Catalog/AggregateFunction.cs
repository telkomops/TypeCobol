using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Catalog
{
    /**
     * Internal representation of an aggregate function.
     * TODO: Create separate AnalyticFunction class
     */
    public class AggregateFunction : Function
    {

        // Set if different from retType_, null otherwise.
        private SqlNodeType intermediateType_;

        // The symbol inside the binary at location_ that contains this particular.
        // They can be null if it is not required.
        private String updateFnSymbol_;
        private String initFnSymbol_;
        private String serializeFnSymbol_;
        private String mergeFnSymbol_;
        private String getValueFnSymbol_;
        private String removeFnSymbol_;
        private String finalizeFnSymbol_;

        // If true, this aggregate function should ignore distinct.
        // e.g. min(distinct col) == min(col).
        // TODO: currently it is not possible for user functions to specify this. We should
        // extend the create aggregate function stmt to allow additional metadata like this.
        private bool ignoresDistinct_;

        // True if this function can appear within an analytic expr (fn() OVER(...)).
        // TODO: Instead of manually setting this flag for all builtin aggregate functions
        // we should identify this property from the function itself (e.g., based on which
        // functions of the UDA API are implemented).
        // Currently, there is no reliable way of doing that.
        private bool isAnalyticFn_;

        // True if this function can be used for aggregation (without an OVER() clause).
        private bool isAggregateFn_;

        public AggregateFunction(FunctionName name, SqlNodeType[] argTypes, SqlNodeType retType, bool varArgs) : base(
            name, argTypes, retType, varArgs)
        {
        }

        public AggregateFunction(FunctionName name, List<SqlNodeType> args, SqlNodeType retType, bool varArgs) : base(
            name, args, retType, varArgs)
        {
        }

        public AggregateFunction(FunctionName fnName, List<SqlNodeType> argTypes,
            SqlNodeType retType, SqlNodeType intermediateType,
            HdfsUri location, String updateFnSymbol, String initFnSymbol,
            String serializeFnSymbol, String mergeFnSymbol, String getValueFnSymbol,
            String removeFnSymbol, String finalizeFnSymbol)
            : base(fnName, argTypes, retType, false)
        {
            setLocation(location);
            intermediateType_ = (intermediateType.Equals(retType)) ? null : intermediateType;
            updateFnSymbol_ = updateFnSymbol;
            initFnSymbol_ = initFnSymbol;
            serializeFnSymbol_ = serializeFnSymbol;
            mergeFnSymbol_ = mergeFnSymbol;
            getValueFnSymbol_ = getValueFnSymbol;
            removeFnSymbol_ = removeFnSymbol;
            finalizeFnSymbol_ = finalizeFnSymbol;
            ignoresDistinct_ = false;
            isAnalyticFn_ = false;
            isAggregateFn_ = true;
            returnsNonNullOnEmpty_ = false;
        }

        public static AggregateFunction createForTesting(FunctionName fnName,
            List<SqlNodeType> argTypes, SqlNodeType retType, SqlNodeType intermediateType,
            HdfsUri location, String updateFnSymbol, String initFnSymbol,
            String serializeFnSymbol, String mergeFnSymbol, String getValueFnSymbol,
            String removeFnSymbol, String finalizeFnSymbol,
            TFunctionBinaryType fnType)
        {
            AggregateFunction fn = new AggregateFunction(fnName, argTypes, retType,
                intermediateType, location, updateFnSymbol, initFnSymbol,
                serializeFnSymbol, mergeFnSymbol, getValueFnSymbol, removeFnSymbol,
                finalizeFnSymbol);
            fn.setBinaryType(fnType);
            return fn;
        }

        public static AggregateFunction createBuiltin(Db db, String name,
            List<SqlNodeType> argTypes, SqlNodeType retType, SqlNodeType intermediateType,
            String initFnSymbol, String updateFnSymbol, String mergeFnSymbol,
            String serializeFnSymbol, String finalizeFnSymbol, bool ignoresDistinct,
            bool isAnalyticFn, bool returnsNonNullOnEmpty)
        {
            return createBuiltin(db, name, argTypes, retType, intermediateType, initFnSymbol,
                updateFnSymbol, mergeFnSymbol, serializeFnSymbol, null, null, finalizeFnSymbol,
                ignoresDistinct, isAnalyticFn, returnsNonNullOnEmpty);
        }

        public static AggregateFunction createBuiltin(Db db, String name,
            List<SqlNodeType> argTypes, SqlNodeType retType, SqlNodeType intermediateType,
            String initFnSymbol, String updateFnSymbol, String mergeFnSymbol,
            String serializeFnSymbol, String getValueFnSymbol, String removeFnSymbol,
            String finalizeFnSymbol, bool ignoresDistinct, bool isAnalyticFn,
            bool returnsNonNullOnEmpty)
        {
            //Preconditions.checkState(initFnSymbol != null);
            //Preconditions.checkState(updateFnSymbol != null);
            //Preconditions.checkState(mergeFnSymbol != null);
            AggregateFunction fn = new AggregateFunction(new FunctionName(db.getName(), name),
                argTypes, retType, intermediateType, null, updateFnSymbol, initFnSymbol,
                serializeFnSymbol, mergeFnSymbol, getValueFnSymbol, removeFnSymbol,
                finalizeFnSymbol);
            fn.setBinaryType(TFunctionBinaryType.BUILTIN);
            fn.ignoresDistinct_ = ignoresDistinct;
            fn.isAnalyticFn_ = isAnalyticFn;
            fn.isAggregateFn_ = true;
            fn.returnsNonNullOnEmpty_ = returnsNonNullOnEmpty;
            fn.setIsPersistent(true);
            return fn;
        }

        public static AggregateFunction createAnalyticBuiltin(Db db, String name,
            List<SqlNodeType> argTypes, SqlNodeType retType, SqlNodeType intermediateType)
        {
            return createAnalyticBuiltin(db, name, argTypes, retType, intermediateType, null,
                null, null, null, null, true);
        }

        public static AggregateFunction createAnalyticBuiltin(Db db, String name,
            List<SqlNodeType> argTypes, SqlNodeType retType, SqlNodeType intermediateType,
            String initFnSymbol, String updateFnSymbol, String removeFnSymbol,
            String getValueFnSymbol, String finalizeFnSymbol)
        {
            return createAnalyticBuiltin(db, name, argTypes, retType, intermediateType,
                initFnSymbol, updateFnSymbol, removeFnSymbol, getValueFnSymbol, finalizeFnSymbol,
                true);
        }

        public static AggregateFunction createAnalyticBuiltin(Db db, String name,
            List<SqlNodeType> argTypes, SqlNodeType retType, SqlNodeType intermediateType,
            String initFnSymbol, String updateFnSymbol, String removeFnSymbol,
            String getValueFnSymbol, String finalizeFnSymbol, bool isUserVisible)
        {
            AggregateFunction fn = new AggregateFunction(new FunctionName(db.getName(), name),
                argTypes, retType, intermediateType, null, updateFnSymbol, initFnSymbol,
                null, null, getValueFnSymbol, removeFnSymbol, finalizeFnSymbol);
            fn.setBinaryType(TFunctionBinaryType.BUILTIN);
            fn.ignoresDistinct_ = false;
            fn.isAnalyticFn_ = true;
            fn.isAggregateFn_ = false;
            fn.returnsNonNullOnEmpty_ = false;
            fn.setUserVisible(isUserVisible);
            fn.setIsPersistent(true);
            return fn;
        }

        public String getUpdateFnSymbol()
        {
            return updateFnSymbol_;
        }

        public String getInitFnSymbol()
        {
            return initFnSymbol_;
        }

        public String getSerializeFnSymbol()
        {
            return serializeFnSymbol_;
        }

        public String getMergeFnSymbol()
        {
            return mergeFnSymbol_;
        }

        public String getFinalizeFnSymbol()
        {
            return finalizeFnSymbol_;
        }

        public bool ignoresDistinct()
        {
            return ignoresDistinct_;
        }

        public bool isAnalyticFn()
        {
            return isAnalyticFn_;
        }

        public bool isAggregateFn()
        {
            return isAggregateFn_;
        }

        public bool returnsNonNullOnEmpty()
        {
            return returnsNonNullOnEmpty_;
        }

        /**
         * Returns the intermediate type of this aggregate function or null
         * if it is identical to the return type.
         */
        public SqlNodeType getIntermediateType()
        {
            return intermediateType_;
        }

        public void setUpdateFnSymbol(String fn)
        {
            updateFnSymbol_ = fn;
        }

        public void setInitFnSymbol(String fn)
        {
            initFnSymbol_ = fn;
        }

        public void setSerializeFnSymbol(String fn)
        {
            serializeFnSymbol_ = fn;
        }

        public void setMergeFnSymbol(String fn)
        {
            mergeFnSymbol_ = fn;
        }

        public void setFinalizeFnSymbol(String fn)
        {
            finalizeFnSymbol_ = fn;
        }

        public void setIntermediateType(SqlNodeType t)
        {
            intermediateType_ = t;
        }

        protected TSymbolLookupParams getLookupParams()
        {
            return buildLookupParams(getUpdateFnSymbol(), TSymbolType.UDF_EVALUATE,
                intermediateType_, hasVarArgs(), false, getArgs());
        }

        public String toSql(bool ifNotExists)
        {
            StringBuilder sb = new StringBuilder("CREATE AGGREGATE FUNCTION ");
            if (ifNotExists) sb.Append("IF NOT EXISTS ");
            sb.Append(dbName() + "." + signatureString() + "\n")
                .Append(" RETURNS " + getReturnType() + "\n");
            if (getIntermediateType() != null)
            {
                sb.Append(" INTERMEDIATE " + getIntermediateType() + "\n");
            }

            sb.Append(" LOCATION '" + getLocation() + "'\n")
                .Append(" UPDATE_FN='" + getUpdateFnSymbol() + "'\n")
                .Append(" INIT_FN='" + getInitFnSymbol() + "'\n")
                .Append(" MERGE_FN='" + getMergeFnSymbol() + "'\n");
            if (getSerializeFnSymbol() != null)
            {
                sb.Append(" SERIALIZE_FN='" + getSerializeFnSymbol() + "'\n");
            }

            if (getFinalizeFnSymbol() != null)
            {
                sb.Append(" FINALIZE_FN='" + getFinalizeFnSymbol() + "'\n");
            }

            return sb.ToString();
        }


    }
}
