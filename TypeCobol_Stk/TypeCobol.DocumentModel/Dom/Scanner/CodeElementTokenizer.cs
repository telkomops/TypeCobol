using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.Concurrency;
using TypeCobol.Compiler.Parser;

namespace TypeCobol.DocumentModel.Dom.Scanner
{
    /// <summary>
    /// The Code Element Tokenizer for CS Cup Symbol.
    /// </summary>
    public class CodeElementTokenizer : TUVienna.CS_CUP.Runtime.Scanner, IEnumerable<TUVienna.CS_CUP.Runtime.Symbol>, IEnumerator<TUVienna.CS_CUP.Runtime.Symbol>
    {
        /// <summary>
        /// With CS CUP real toke start at 2, 0 is for EOF and 1 for error.
        /// </summary>
        public const int CS_CUP_START_TOKEN = 2;
        /// <summary>
        /// The EOF symbol
        /// </summary>
        public static TUVienna.CS_CUP.Runtime.Symbol EOF
        {
            get
            {
                return new TUVienna.CS_CUP.Runtime.Symbol(0, null);
            }
        }
        /// <summary>
        /// The list of Code Elements
        /// </summary>
        public ISearchableReadOnlyList<CodeElementsLine> CodeElementsLines
        {
            get;
            internal set;
        }

        /// <summary>
        /// Internal Symbol Yielder
        /// </summary>
        private IEnumerator<TUVienna.CS_CUP.Runtime.Symbol> symbol_yielder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="codeElementsLines">The List of Code Elements</param>
        public CodeElementTokenizer(ISearchableReadOnlyList<CodeElementsLine> codeElementsLines)
        {
            this.CodeElementsLines = codeElementsLines;
            Reset();
        }

        public TUVienna.CS_CUP.Runtime.Symbol next_token()
        {
            if (symbol_yielder.MoveNext())
                return symbol_yielder.Current;
            return EOF;
        }

        /// <summary>
        /// Enumerator all Symbol from the CodeElementLines
        /// </summary>
        /// <returns>An Enumerator on Symbols</returns>
        public IEnumerator<TUVienna.CS_CUP.Runtime.Symbol> GetEnumerator()
        {
            foreach (CodeElementsLine cel in CodeElementsLines)
            {
                if (cel.CodeElements != null)
                {
                    foreach(Compiler.CodeElements.CodeElement ce in cel.CodeElements)
                    {
                        TUVienna.CS_CUP.Runtime.Symbol symbol = new TUVienna.CS_CUP.Runtime.Symbol(((int)ce.Type) + CS_CUP_START_TOKEN - 1, ce);
                        yield return symbol;
                    }
                }                
            }
            yield return EOF;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public TUVienna.CS_CUP.Runtime.Symbol Current
        {
            get { return symbol_yielder.Current; }
        }

        public void Dispose()
        {
            symbol_yielder = null;            
        }

        object System.Collections.IEnumerator.Current
        {
            get { return symbol_yielder.Current; }
        }

        public bool MoveNext()
        {
            return symbol_yielder != null && symbol_yielder.MoveNext();
        }

        public void Reset()
        {
            symbol_yielder = CodeElementsLines != null ? GetEnumerator() : null;
        }
    }
}
