using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUVienna.CS_CUP.Runtime;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.Compiler.Concurrency;
using TypeCobol.Compiler.Scanner;
using TypeCobol.Compiler.SqlScanner;

namespace TypeCobol.Compiler.SqlParser
{
    /// <summary>
    /// SQL Tokenizer used in conjuction with CUP parser
    /// </summary>
    public class SqlTokenizer : TUVienna.CS_CUP.Runtime.Scanner, IEnumerable<TUVienna.CS_CUP.Runtime.Symbol>, IEnumerator<TUVienna.CS_CUP.Runtime.Symbol>
    {
        /// <summary>
        /// Internal Symbol Yielder
        /// </summary>
        private IEnumerator<TUVienna.CS_CUP.Runtime.Symbol> _symbolYielder;
        /// <summary>
        /// The EOF symbol
        /// </summary>
        public TUVienna.CS_CUP.Runtime.Symbol EndSymbol => new TUVienna.CS_CUP.Runtime.Symbol((int)TokenType.EOF,null);
        /// <summary>
        /// The first token read
        /// </summary>
        public SqlToken FirstToken { get; private set; }
        /// <summary>
        /// The Last Token read.
        /// </summary>
        public SqlToken LastToken { get; private set; }
        
        private List<SqlToken> symbolArray { get; set; }

        /// <summary>
        /// The Last stop symbol encountered.
        /// </summary>
        public Symbol LastStopSymbol { get; private set; }
        
        public SqlTokenizer(SqlToken[] sqlSymbols, SqlToken startToken)
        {
            _symbolYielder = GetEnumerator();
            symbolArray = sqlSymbols.ToList();
            FirstToken = startToken;
        }

        public Symbol next_token()
        {
            return _symbolYielder.MoveNext() ? _symbolYielder.Current : EndSymbol;
        }

        public IEnumerator<Symbol> GetEnumerator()
        {
            SqlToken token = FirstToken;
            if (symbolArray.Contains(token))
            {
                int index = symbolArray.IndexOf(token);


                while (index<=symbolArray.Count-1&&token.TokenType!=TokenType.EOF)
                {
                    System.Diagnostics.Debug.WriteLine(token.Text);
                    Symbol symbol = new Symbol((int)token.TokenType+550,token);
                    index++;
                    if (index > symbolArray.Count - 1) continue;
                    token = symbolArray[index];
                    LastToken = token;
                    yield return symbol;
                }
            }

            yield return EndSymbol;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            _symbolYielder = null;
        }

        public bool MoveNext()
        {
            return _symbolYielder != null && _symbolYielder.MoveNext();
        }

        public void Reset()
        {
            _symbolYielder = GetEnumerator();
        }

        public Symbol Current => _symbolYielder.Current;

        object IEnumerator.Current => Current;
    }
}
