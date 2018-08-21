using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Class to represent a function name. Function names are specified as
     * db.function_name.
     */
    public class FunctionName
    {
        // Only set for parsed function names.
        private readonly List<string> fnNamePath_;

        // Set/validated during analysis.
        private string db_;
        private string fn_;
        private bool isBuiltin_ = false;
        private bool isAnalyzed_ = false;

        /**
         * C'tor for parsed function names. The function names could be invalid. The validity
         * is checked during analysis.
         */
        public FunctionName(List<string> fnNamePath)
        {
            fnNamePath_ = fnNamePath;
        }

        public FunctionName(string dbName, string fn)
        {
            db_ = (dbName != null) ? dbName.ToLowerInvariant() : null;
            fn_ = fn.ToLowerInvariant();
            fnNamePath_ = null;
        }

        public FunctionName(string fn) :
            this(null, fn)
        {
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is FunctionName)) return false;
            FunctionName o = (FunctionName) obj;
            if ((db_ == null || o.db_ == null) && (db_ != o.db_))
            {
                if (db_ == null && o.db_ != null) return false;
                if (db_ != null && o.db_ == null) return false;
                if (!db_.Equals(o.db_)) return false;
            }

            return fn_.Equals(o.fn_);
        }

        public string getDb()
        {
            return db_;
        }

        public string getFunction()
        {
            return fn_;
        }

        public bool isFullyQualified()
        {
            return db_ != null;
        }

        public bool isBuiltin()
        {
            return isBuiltin_;
        }

        public List<string> getFnNamePath()
        {
            return fnNamePath_;
        }

        public override string ToString()
        {
            // The fnNamePath_ is not always set.
            if (!isAnalyzed_ && fnNamePath_ != null) return string.Join(".", fnNamePath_);
            if (db_ == null || isBuiltin_) return fn_;
            return db_ + "." + fn_;
        }

        public void analyze(Analyzer analyzer)
        {
            analyze(analyzer, true);
        }

        /**
         * Path resolution happens as follows.
         *
         * Fully-qualified function name:
         * - Set the database name to the database name specified.
         *
         * Non-fully-qualified function name:
         * - When preferBuiltinsDb is true:
         *   - If the function name specified has the same name as a built-in function,
         *     set the database name to _impala_builtins.
         *   - Else, set the database name to the current session DB name.
         * - When preferBuiltinsDb is false: set the database name to current session DB name.
         */
        public void analyze(Analyzer analyzer, bool preferBuiltinsDb)
        {
            if (isAnalyzed_) return;
            analyzeFnNamePath();
            if (fn_.IsNullOrEmpty()) throw new AnalysisException("Function name cannot be empty.");
            if (fn_.Where((t, i) => !(char.IsLetterOrDigit(fn_,i) || t == '_')).Any())
            {
                throw new AnalysisException(
                    "Function names must be all alphanumeric or underscore. " +
                    "Invalid name: " + fn_);
            }

            int digit;
            if (int.TryParse(fn_[0].ToString(), out digit))
            {
                throw new AnalysisException("Function cannot start with a digit: " + fn_);
            }

            // Resolve the database for this function.
            Db builtinDb = BuiltinsDb.getInstance();
            if (!isFullyQualified())
            {
                db_ = analyzer.getDefaultDb();
                if (preferBuiltinsDb && builtinDb.containsFunction(fn_))
                {
                    db_ = BuiltinsDb.NAME;
                }
            }

            // Preconditions.checkNotNull(db_);
            isBuiltin_ = db_.Equals(BuiltinsDb.NAME) &&
                         builtinDb.containsFunction(fn_);
            isAnalyzed_ = true;
        }

        private void analyzeFnNamePath()
        {
            if (fnNamePath_ == null) return;
            if (fnNamePath_.Count > 2 || fnNamePath_.IsNullOrEmpty())
            {
                throw new AnalysisException(
                    string.Format("Invalid function name: '{0}'. Expected [dbname].funcname.",
                        string.Join(".", fnNamePath_)));
            }
            else if (fnNamePath_.Count > 1)
            {
                db_ = fnNamePath_[0].ToLowerInvariant();
                fn_ = fnNamePath_[1].ToLowerInvariant();
            }
            else
            {
                //Preconditions.checkState(fnNamePath_.size() == 1);
                fn_ = fnNamePath_[0].ToLowerInvariant();
            }
        }
    }
}
