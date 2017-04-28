using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler;
using TypeCobol.Compiler.Diagnostics;
using TypeCobol.Compiler.Directives;
using TypeCobol.Compiler.File;
using TypeCobol.Compiler.Preprocessor;
using TypeCobol.Compiler.Scanner;
using TypeCobol.Compiler.Text;

namespace TypeCobol.DocumentModel.Tests
{
    public class ComplertionProto
    {
        public static CompilationProject CopyProject;

        public static TypeCobolOptions CompilerOptions = new TypeCobolOptions();

        /// <summary>
        /// C:\Users\MAYANJE\EID-Project\Tests\BuildTypeCobolProto\CopyTestFiles
        /// </summary>
        static ComplertionProto()
        {
            CopyProject = new CompilationProject("copy",
                //PlatformUtils.GetPathForProjectFile(@"Compiler\Preprocessor\CopyTestFiles"), new string[] { ".cbl", ".cpy" },
                //"C:\\Users\\MAYANJE\\EID-Project\\Tests\\BuildTypeCobolProto\\CopyTestFiles", 
                "U:\\CopyTestFiles",
                new string[] { ".cbl", ".cpy" },
                Encoding.Unicode, EndOfLineDelimiter.CrLfCharacters, 0, ColumnsLayout.CobolReferenceFormat, CompilerOptions);
        }

        private static string ProcessTokensDocument(string testName, ProcessedTokensDocument processedDoc)
        {
            // Tokens
            StringBuilder sbTokens = new StringBuilder();
            ITokensLinesIterator tokens = processedDoc.ProcessedTokens;
            Token token = tokens.NextToken();
            if (token != Token.END_OF_FILE)
            {
                string documentPath = null;
                int lineIndex = -1;
                do
                {
                    if (tokens.DocumentPath != documentPath)
                    {
                        documentPath = tokens.DocumentPath;
                        sbTokens.AppendLine("** Document path " + documentPath + " **");
                    }
                    if (tokens.LineIndex != lineIndex)
                    {
                        lineIndex = tokens.LineIndex;
                        sbTokens.AppendLine("-- Line " + (lineIndex + 1) + " --");
                    }
                    sbTokens.AppendLine(token.ToString());
                }
                while ((token = tokens.NextToken()) != Token.END_OF_FILE);
            }

            // Errors
            StringBuilder sbDiagnostics = new StringBuilder();
            sbDiagnostics.AppendLine();
            sbDiagnostics.AppendLine("++ Preprocessor diagnostics ++");
            bool hasDiagnostic = false;
            int lineNumber = 1;
            foreach (var line in processedDoc.Lines)
            {
                if (line.PreprocessorDiagnostics != null)
                {
                    sbDiagnostics.AppendLine("-- Line " + lineNumber + " --");
                    foreach (Diagnostic diagnostic in line.PreprocessorDiagnostics)
                    {
                        hasDiagnostic = true;
                        sbDiagnostics.AppendLine(diagnostic.ToString());
                    }
                }
                lineNumber++;
            }

            return sbTokens.ToString() + (hasDiagnostic ? sbDiagnostics.ToString() : "");
        }

        public static string ProcessCopyDirectives(string testName)
        {
            ProcessedTokensDocument processedDoc = CopyProject.GetProcessedTokensDocument(null, testName);
            return ProcessTokensDocument(testName, processedDoc);
        }

        public static void CheckWithCopyResultFile(string result, string testName)
        {
            //using (StreamReader reader = new StreamReader(PlatformUtils.GetStreamForProjectFile(@"Compiler\Preprocessor\CopyResultFiles\" + testName + ".txt")))
            using (StreamReader reader = new StreamReader("C:\\Users\\MAYANJE\\EID-Project\\Tests\\BuildTypeCobolProto\\CopyResultFiles\\" + testName + ".txt"))
            {
                string expectedResult = reader.ReadToEnd();
                if (result != expectedResult)
                {
                    throw new Exception("Tokens and diagnostics produced by preprocessor in test \"" + testName + "\" don't match the expected result");
                }
                else
                {
                    System.Console.WriteLine("Tokens and diagnostics produced by preprocessor in test \"" + testName + "\" MATCHES the expected result");
                }
            }
        }

        static void Main(string[] args)
        {
            string testName = "PgmCopyReplacing";
            string result = ProcessCopyDirectives(testName);
            CheckWithCopyResultFile(result, testName);
        }
    }
}
