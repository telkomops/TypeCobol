﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.Diagnostics;
using TypeCobol.Compiler.Directives;
using TypeCobol.Compiler.Scanner;
using TypeCobol.Compiler.Text;

namespace TypeCobol.Compiler.Preprocessor
{
    /// <summary>
    /// Line of tokens after preprocessor execution
    /// </summary>
    public class ProcessedTokensLine : IProcessedTokensLine
    {
        public ProcessedTokensLine(ITokensLine tokensLine)
        {
            TokensLine = tokensLine;
            SourceTokens = tokensLine.SourceTokens;
            ScannerDiagnostics = tokensLine.ScannerDiagnostics;

            ProcessingState = PreprocessorState.NeedsCompilerDirectiveParsing;
        }

        internal ProcessedTokensLine(ProcessedTokensLine processedTokensLine)
        {
            TokensLine = processedTokensLine.TokensLine;
            SourceTokens = processedTokensLine.SourceTokens;
            ScannerDiagnostics = processedTokensLine.ScannerDiagnostics;

            ProcessingState = PreprocessorState.NeedsCompilerDirectiveParsing;
        }

        // --- Reference to source line properties ---

        /// <summary>
        /// Underlying TokensLine
        /// </summary>
        public ITokensLine TokensLine { get; private set; }

        /// <summary>
        /// Tokens found while scanning the raw source text line
        /// (before text manipulation phase)
        /// </summary>
        public IList<Token> SourceTokens { get; private set; }

        /// <summary>
        /// Error and warning messages produced while scanning the raw source text line
        /// (before text manipulation phase)
        /// </summary>
        public IList<Diagnostic> ScannerDiagnostics { get; private set; }

        // --- Computed line properties after preprocessor execution ---

        internal enum PreprocessorState
        {
            NeedsCompilerDirectiveParsing,
            NeedsCopyDirectiveProcessing,
            NeedsReplaceDirectiveProcessing,
            Ready
        }

        /// <summary>
        /// True if the preprocessor has not treated this line yet
        /// and all the following properties have not been set
        /// </summary>
        internal PreprocessorState ProcessingState { get; set; }

        /// <summary>
        /// Tokens produced after parsing the compiler directives.
        /// If a compiler directive is found, several tokens of the source text are grouped 
        /// into one single CompilerDirectiveToken (which can be continued on several lines).
        /// </summary>
        public IList<Token> TokensWithCompilerDirectives { 
            get 
            {
                if (ProcessingState <= PreprocessorState.NeedsCompilerDirectiveParsing)
                {
                    throw new InvalidOperationException("Compiler directives on this line have not been parsed yet");
                }
                if (tokensWithCompilerDirectives == null)
                {
                    return SourceTokens;
                }
                else
                {
                    return tokensWithCompilerDirectives;
                }
            } 
        }

        // Only needed if several source tokens must be grouped into a single CompilerDirectiveToken
        private IList<Token> tokensWithCompilerDirectives;

        /// <summary>
        /// True if compiler directives have been recognized on the current line
        /// (true on each line for multiline compiler directives)
        /// </summary>
        public bool HasCompilerDirectives { get { return tokensWithCompilerDirectives != null; } }
        
        /// <summary>
        /// Compiler listing control directive found on the current line
        /// *CBL or *CONTROL, EJECT, SKIP1 or SKIP2 or SKIP3, TITLE
        /// (these compiler directives can't span several lines, and you can only write one of them per line)
        /// </summary>
        public CompilerDirective CompilerListingControlDirective { get; private set; }
        
        /// <summary>
        /// Imported compilation documents for each COPY directive found (starting) on this line
        /// </summary>
        public IDictionary<CopyDirective, ImportedTokensDocument> ImportedDocuments { get; private set; }
        
        /// <summary>
        /// Last REPLACE compiler directive found on this this line
        /// </summary>
        public ReplaceDirective ReplaceDirective { get; private set; }

        internal TokensGroup InsertCompilerDirectiveTokenOnFirstLine(IList<Token> tokensOnFirstLineBeforeCompilerDirective, CompilerDirective compilerDirective, bool hasError, IList<Token> compilerDirectiveTokensOnFirstLine, IList<Token> tokensOnFirstLineAfterCompilerDirective)
        {
            // Register compiler listing control directive
            if( compilerDirective.Type == CompilerDirectiveType.ASTERISK_CBL ||
                compilerDirective.Type == CompilerDirectiveType.ASTERISK_CONTROL ||
                compilerDirective.Type == CompilerDirectiveType.EJECT ||
                compilerDirective.Type == CompilerDirectiveType.SKIP1 ||
                compilerDirective.Type == CompilerDirectiveType.SKIP2 ||
                compilerDirective.Type == CompilerDirectiveType.SKIP3 ||
                compilerDirective.Type == CompilerDirectiveType.TITLE)
            {
                CompilerListingControlDirective = compilerDirective;
            }

            // Register COPY directives
            // Prepare dictionary for COPY imported documents
            if( compilerDirective.Type == CompilerDirectiveType.COPY ||
                compilerDirective.Type == CompilerDirectiveType.EXEC_SQL_INCLUDE)
            {
                if(ImportedDocuments == null)
                {
                    ImportedDocuments = new Dictionary<CopyDirective, ImportedTokensDocument>(1);
                }
                ImportedDocuments.Add((CopyDirective)compilerDirective, null);
            }

            // Register REPLACE compiler directive
            if(compilerDirective.Type == CompilerDirectiveType.REPLACE)
            {
                ReplaceDirective = (ReplaceDirective)compilerDirective;
            }

            // Initialize tokensWithCompilerDirectives
            // (first compiler directive found on this line)
            if (!HasCompilerDirectives)
            {
                // Initialize tokens list
                tokensWithCompilerDirectives = new List<Token>();

                // Keep tokens before compiler directive
                if (tokensOnFirstLineBeforeCompilerDirective != null)
                {
                    foreach (Token token in tokensOnFirstLineBeforeCompilerDirective)
                    {
                        tokensWithCompilerDirectives.Add(token);
                    }
                }  
            }
            // Update tokensWithCompilerDirectives
            // (several compiler directives on the same line)
            else
            {
                // Reset tokens list
                IList<Token> previousTokens = tokensWithCompilerDirectives;
                tokensWithCompilerDirectives = new List<Token>();
                
                // Keep tokens before compiler directive
                Token firstTokenOfCompilerDirective = compilerDirectiveTokensOnFirstLine[0];
                foreach (Token token in previousTokens)
                {
                    if(token == firstTokenOfCompilerDirective)
                    {
                        break;
                    }
                    tokensWithCompilerDirectives.Add(token);
                } 
            }

            // Build a CompilerDirectiveToken wrapping all matched tokens on the first line
            CompilerDirectiveToken directiveToken = new CompilerDirectiveToken(
                compilerDirective,
                compilerDirectiveTokensOnFirstLine,
                hasError);

            // Add the compilerDirectiveToken
            tokensWithCompilerDirectives.Add(directiveToken);

            // Keep tokens after compiler directive
            if (tokensOnFirstLineAfterCompilerDirective != null)
            {
                foreach (Token token in tokensOnFirstLineAfterCompilerDirective)
                {
                    tokensWithCompilerDirectives.Add(token);
                }
            }     

            // Return potentially continued compiler directive token
            if (tokensOnFirstLineAfterCompilerDirective == null)
            {
                return directiveToken;
            }
            else
            {
                return null;
            }
        }

        internal TokensGroup InsertCompilerDirectiveTokenOnNextLine(TokensGroup continuedTokensGroupOnPreviousLine, IList<Token> compilerDirectiveTokensOnNextLine, IList<Token> tokensOnFirstLineAfterCompilerDirective)
        {
            // Initialize tokens list
            IsContinuedFromPreviousLine = true;
            tokensWithCompilerDirectives = new List<Token>();

            // Build a ContinuationTokensGroup wrapping all matched tokens on the next line
            ContinuationTokensGroup continuationToken = null;
            try
            {
                continuationToken = new ContinuationTokensGroup(
                    continuedTokensGroupOnPreviousLine,
                    compilerDirectiveTokensOnNextLine);
            }
            catch(Exception e)
            {
                string error = e.Message;
            }

            // Add the ContinuationTokensGroup
            tokensWithCompilerDirectives.Add(continuationToken);

            // Keep tokens after compiler directive
            if (tokensOnFirstLineAfterCompilerDirective != null)
            {
                foreach (Token token in tokensOnFirstLineAfterCompilerDirective)
                {
                    tokensWithCompilerDirectives.Add(token);
                }
            }

            // Return potentially continued compiler directive token
            if (tokensOnFirstLineAfterCompilerDirective == null)
            {
                return continuationToken;
            }
            else
            {
                return null;
            }
        }
               
        /// <summary>
        /// True if this line contains one processed token continued from the previous line
        /// </summary>
        public bool IsContinuedFromPreviousLine { get; private set; }

        /// <summary>
        /// Error and warning messages produced while scanning the raw source text line
        /// (before text manipulation phase)
        /// </summary>
        public IList<Diagnostic> PreprocessorDiagnostics { get; private set; }

        /// <summary>
        /// Lazy initialization of diagnostics list
        /// </summary>
        internal void AddDiagnostic(Diagnostic diag)
        {
            if(PreprocessorDiagnostics == null)
            {
                PreprocessorDiagnostics = new List<Diagnostic>();
            }
            PreprocessorDiagnostics.Add(diag);
        }

        // --- temp temp ---

        public TextLineType Type
        {
            get
            {
                return TokensLine.Type;
            }

            set
            {
                TokensLine.Type = value;
            }
        }

        public TextArea SequenceNumber
        {
            get
            {
                return TokensLine.SequenceNumber;
            }
        }

        public string SequenceNumberText
        {
            get
            {
                return TokensLine.SequenceNumberText;
            }
        }

        public TextArea Indicator
        {
            get
            {
                return TokensLine.Indicator;
            }
        }

        public char IndicatorChar
        {
            get
            {
                return TokensLine.IndicatorChar;
            }
        }

        public TextArea Source
        {
            get
            {
                return TokensLine.Source;
            }
        }

        public string SourceText
        {
            get
            {
                return TokensLine.SourceText;
            }
        }

        public TextArea Comment
        {
            get
            {
                return TokensLine.Comment;
            }
        }

        public string CommentText
        {
            get
            {
                return TokensLine.CommentText;
            }
        }

        public int InitialLineIndex
        {
            get
            {
                return TokensLine.InitialLineIndex;
            }
        }

        public string Text
        {
            get
            {
                return TokensLine.Text;
            }
        }

        public int Length
        {
            get
            {
                return TokensLine.Length;
            }
        }

        public object LineTrackingReferenceInSourceDocument
        {
            get
            {
                return TokensLine.LineTrackingReferenceInSourceDocument;
            }
        }

        public string TextSegment(int startIndex, int endIndexInclusive)
        {
            return TokensLine.TextSegment(startIndex, endIndexInclusive);
        }

        // --- temp temp ---
    }
}
