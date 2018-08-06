using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.Compiler.Scanner;
using TypeCobol.Compiler.SqlParser;
using TypeCobol.Compiler.SqlScanner;

namespace TypeCobol.Compiler.Parser
{
    public static class SqlExpressionsBuilder
    {
        public static IList<SqlToken> SqlExpressionParser(ExecStatement statement)
        {
            statement.RecognizedSqlTokens =
                ScanSqlTokens(statement.CodeLines.Select(elem => elem.Value)).ToArray();

            //instanciate the tokenizer over the recognized symbols
            var sqlTokenizer = new SqlTokenizer(statement.RecognizedSqlTokens,
                statement.RecognizedSqlTokens.FirstOrDefault());
            //Init a CUP compiler directive parser
            SqlErrorStrategy cupCobolErrorStrategy = new SqlErrorStrategy();
            
            //pass tokenizer to built parser;
            SqlParser.SqlParser parser =
                new SqlParser.SqlParser(sqlTokenizer) { ErrorReporter = cupCobolErrorStrategy };
            try
            {
                var symbolParsed = parser.parse();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return statement.RecognizedSqlTokens;
        }

        public static IEnumerable<SqlToken> ScanSqlTokens(IEnumerable<string> statement)
        {
            string separator = Environment.NewLine;
            TextReader reader =
                new StringReader(string.Join(separator, statement));
            SqlScanner.SqlScanner scanner = new SqlScanner.SqlScanner(reader);
            List<SqlToken> recognizedSqlSymbols = new List<SqlToken>();

            SqlToken symbol;
            do
            {
                symbol = scanner.GetToken();
                recognizedSqlSymbols.Add(symbol);

            } while (symbol.TokenType != TokenType.EOF);
            return recognizedSqlSymbols.Where(elem => elem.TokenType != TokenType.EOF);
        }
    }
}
