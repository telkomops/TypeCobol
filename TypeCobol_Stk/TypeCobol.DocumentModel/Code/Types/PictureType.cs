using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.DocumentModel.Code.Types
{
    /// <summary>
    /// A Type that Comes from a COBOL PICTURE clause.
    /// </summary>
    public class PictureType : TypeCobolType
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public PictureType()
            : base(Tags.Picture)
        {
        }

        /// <summary>
        /// Tokens Constructor
        /// </summary>
        /// <param name="tokens"></param>
        public PictureType(IList<TypeCobol.Compiler.Scanner.Token> tokens)
            : base(Tags.Picture)
        {
            ConsumedTokens = tokens;
        }

        IList<TypeCobol.Compiler.Scanner.Token> m_ConsumedTokens;
        /// <summary>
        /// The Consumet Tokens that represents this Picture Type.
        /// </summary>
        IList<TypeCobol.Compiler.Scanner.Token> ConsumedTokens
        {
            get
            {
                return m_ConsumedTokens;
            }
            set
            {
                m_ConsumedTokens = value;
                ParsePicture();
            }
        }

        /// <summary>
        /// a Normalized Textual String representation of the Picture clause.
        /// </summary>
        public String Picture
        {
            get;
            internal set;
        }

        /// <summary>
        /// Is this type an alphabetic type
        /// </summary>
        public bool IsAlpha
        {
            get;
            internal set;
        }

        /// <summary>
        /// Is this type a numeric type.
        /// </summary>
        public bool IsNumeric
        {
            get
            {
                return !IsAlpha;
            }
        }

        /// <summary>
        /// The decimal character used if this type is a Decimal,
        /// otherwise the Decimal character is '\0'
        /// </summary>
        public char DecimalSeparator
        {
            get;
            internal set;
        }

        /// <summary>
        /// Is this type a decimal numeric type, that has 'V' or '.' as decimal separator character.
        /// </summary>
        public bool IsDecimal
        {
            get
            {
                return DecimalSeparator == 'V' || DecimalSeparator == '.';
            }
        }

        /// <summary>
        /// If this type is decimal, this the number of digits after the decimal
        /// separator character.
        /// </summary>
        public int Precision
        {
            get;
            internal set;
        }

        /// <summary>
        /// Is this type marked this S, that is to say signed.
        /// </summary>
        public bool IsSigned
        {
            get;
            internal set;
        }

        /// <summary>
        /// Parse the the Consumed Tokens of the PICTURE Clause in order to get all its attributes
        /// : (numeric?, alphabetic, length, precision, signed, decimal)
        /// </summary>
        private void ParsePicture()
        {
        }
    }
}
