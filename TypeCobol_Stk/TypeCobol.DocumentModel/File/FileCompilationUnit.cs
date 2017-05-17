using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler;
using TypeCobol.Compiler.AntlrUtils;
using TypeCobol.Compiler.CodeModel;
using TypeCobol.Compiler.Concurrency;
using TypeCobol.Compiler.Directives;
using TypeCobol.Compiler.Parser;
using TypeCobol.Compiler.Preprocessor;
using TypeCobol.Compiler.Text;

namespace TypeCobol.DocumentModel.File
{
    public class FileCompilationUnit : CompilationUnit
    {
        /// <summary>
        /// Initializes a new compilation document from a list of text lines.
        /// This method does not scan the inserted text lines to produce tokens.
        /// You must explicitely call UpdateTokensLines() to start an initial scan of the document.
        /// </summary>
        public FileCompilationUnit(TextSourceInfo textSourceInfo, IEnumerable<ITextLine> initialTextLines, TypeCobolOptions compilerOptions, IProcessedTokensDocumentProvider processedTokensDocumentProvider, List<RemarksDirective.TextNameVariation> copyTextNameVariations) :
            base(textSourceInfo, initialTextLines, compilerOptions, processedTokensDocumentProvider, copyTextNameVariations)
        {
        }

        /// <summary>
        /// Creates a new snapshot of the document viewed as complete Cobol Program or Class.
        /// (if the code elements lines changed since the last time this method was called)
        /// Thread-safe : this method can be called from any thread.
        /// </summary>
        public override void RefreshProgramClassDocumentSnapshot()
        {
            // Make sure two threads don't try to update this snapshot at the same time
            bool snapshotWasUpdated = false;
            lock (lockObjectForProgramClassDocumentSnapshot)
            {
                // Capture previous snapshot at one point in time
                CodeElementsDocument codeElementsDocument = CodeElementsDocumentSnapshot;

                // Check if an update is necessary and compute changes to apply since last version
                if (ProgramClassDocumentSnapshot == null || ProgramClassDocumentSnapshot.PreviousStepSnapshot.CurrentVersion != codeElementsDocument.CurrentVersion)
                {
                    // Start perf measurement
                    PerfStatsForProgramClassParser.OnStartRefresh();

                    // Program and Class parsing is not incremental : the objects are rebuilt each time this method is called
                    if (false)
                    {//OLD FASHION
                        Program newProgram;
                        Class newClass;
                        IList<ParserDiagnostic> newDiagnostics;
                        //TODO cast to ImmutableList<CodeElementsLine> sometimes fails here
                        ProgramClassParserStep.ParseProgramOrClass(TextSourceInfo, ((ImmutableList<CodeElementsLine>)codeElementsDocument.Lines), CompilerOptions, CustomSymbols, out newProgram, out newClass, out newDiagnostics);

                        // Capture the result of the parse in a new snapshot
                        ProgramClassDocumentSnapshot = new ProgramClassDocument(
                            codeElementsDocument, ProgramClassDocumentSnapshot == null ? 0 : ProgramClassDocumentSnapshot.CurrentVersion + 1,
                            newProgram, newClass, newDiagnostics);
                        snapshotWasUpdated = true;
                    }
                    else
                    {//EXPERIMENTAL FASHION
                        TypeCobol.DocumentModel.Dom.CobolProgram cobolProgram = ParseCodeDom(((ImmutableList<CodeElementsLine>)codeElementsDocument.Lines));
                    }

                    // Stop perf measurement
                    PerfStatsForProgramClassParser.OnStopRefresh();
                }
            }
        }

        protected TypeCobol.DocumentModel.Dom.CobolProgram ParseCodeDom(ISearchableReadOnlyList<CodeElementsLine> Lines)
        {
            TypeCobol.DocumentModel.Dom.Scanner.CodeElementTokenizer scanner = new TypeCobol.DocumentModel.Dom.Scanner.CodeElementTokenizer(Lines);
            TypeCobol.DocumentModel.Dom.Parser.ProgramParser pp = new TypeCobol.DocumentModel.Dom.Parser.ProgramParser(scanner);
            TUVienna.CS_CUP.Runtime.Symbol symbol = pp.parse();
            TypeCobol.DocumentModel.Dom.CobolProgram cobolProgram = (TypeCobol.DocumentModel.Dom.CobolProgram)symbol.value;
            return cobolProgram;
        }
    }    

}
