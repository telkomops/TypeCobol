using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.DocumentModel.Code.Types
{
    /// <summary>
    /// The Pointer Type.
    /// </summary>
    public class PointerType : TypeCobolType
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PointerType()
            : base(Tags.Pointer)
        {
        }

        /// <summary>
        /// The Type of an Element
        /// </summary>
        public TypeCobolType ElementType
        {
            get;
            set;
        }

    }
}
