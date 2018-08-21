using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.SqlNodes.Common;

namespace TypeCobol.Compiler.SqlNodes
{
    /**
     * Represents a TABLESAMPLE clause.
     *
     * Syntax:
     * <tableref> TABLESAMPLE SYSTEM(<number>) [REPEATABLE(<number>)]
     *
     * The first number specifies the percent of table bytes to sample.
     * The second number specifies the random seed to use.
     */
    public class TableSampleClause : IParseNode
    {
        // Required percent of bytes to sample.
        private readonly long percentBytes;
        // Optional random seed. Null if not specified.
        private readonly long? randomSeed;

        public TableSampleClause(long percentBytes, long randomSeed)
        {
            this.percentBytes = percentBytes;
            this.randomSeed = randomSeed;
        }

        public void analyze(Analyzer analyzer)
        {
            if (percentBytes < 0 || percentBytes > 100)
            {
                throw new AnalysisException(string.Format(
                    "Invalid percent of bytes value '{0}'. " +
                    "The percent of bytes to sample must be between 0 and 100.", percentBytes));
            }
        }

        public long GetPercentBytes => percentBytes;
        public bool HasRandomSeed => randomSeed!=null;
        public long GetRandomSeed => HasRandomSeed ? randomSeed.Value : 0;


        public string toSql()
        {
            return randomSeed.ToString();
        }

        /**
   * Prints the SQL of this TABLESAMPLE clause. The optional REPEATABLE clause is
   * included if 'randomSeed' is non-NULL.
   */
        public string toSql(long randomSeed)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("TABLESAMPLE SYSTEM("+percentBytes+")");
            if (HasRandomSeed)
            {
                builder.Append(" REPEATABLE(" + randomSeed + ")");
            }
            return builder.ToString();
        }
    }
}
