using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.Compiler.SqlNodes.Common
{
    public class SqlEnumerations
    {
        public enum TJoinOp
        {
            INNER_JOIN,
            LEFT_OUTER_JOIN,
            LEFT_SEMI_JOIN,
            LEFT_ANTI_JOIN,
            // Variant of the LEFT ANTI JOIN that is used for the rewrite of
            // NOT IN subqueries. It can have a single equality join conjunct
            // that returns TRUE when the rhs is NULL.
            NULL_AWARE_LEFT_ANTI_JOIN,
            RIGHT_OUTER_JOIN,
            RIGHT_SEMI_JOIN,
            RIGHT_ANTI_JOIN,
            FULL_OUTER_JOIN,
            CROSS_JOIN,
        }

        // Preference for replica selection
        public enum TReplicaPreference
        {
            CACHE_LOCAL,
            CACHE_RACK,
            DISK_LOCAL,
            DISK_RACK,
            REMOTE
        }

        public enum DistributionMode
        {
            NONE,
            BROADCAST,
            PARTITIONED
        }
    }
}
