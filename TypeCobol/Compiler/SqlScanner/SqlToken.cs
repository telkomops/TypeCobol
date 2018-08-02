using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.Scanner;
using TypeCobol.Compiler.SqlParser;

namespace TypeCobol.Compiler.SqlScanner
{
    public class SqlToken : Token
    {
        public int LineIndex { get; private set; }

        public SqlToken()
        {
            
        }

        public SqlToken(TokenType tokenType, int startIndex, int endIndex, ITokensLine line, int lineIndex)
        {
            TokenType = tokenType;
            base.startIndex = startIndex;
            base.stopIndex = endIndex;
            tokensLine = line;
            LineIndex = lineIndex;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
