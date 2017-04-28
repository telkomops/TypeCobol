using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.DocumentModel.Code.Types
{
    /// <summary>
    /// Class that represents an Array Type
    /// </summary>
    public class ArrayType : TypeCobolType
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public ArrayType()
            : base(Tags.Array)
        {
        }

        /// <summary>
        /// The Number of Occurennce in the arry.
        /// </summary>
        public int NOccur
        {
            get;
            set;
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
