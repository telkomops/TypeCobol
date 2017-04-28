using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.DocumentModel.Code.Types
{
    /// <summary>
    /// A TypeDef type.
    /// </summary>
    public class TypedefType : TypeCobolType
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public TypedefType()
            : base(Tags.Typedef)
        {
        }

        /// <summary>
        /// The target type of the Typedef
        /// </summary>
        public TypeCobolType TargetType
        {
            get;
            set;
        }
    }
}
