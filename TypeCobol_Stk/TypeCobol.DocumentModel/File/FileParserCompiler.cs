using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler;
using TypeCobol.Compiler.CodeModel;
using TypeCobol.Compiler.Directives;
using TypeCobol.Compiler.File;
using TypeCobol.Compiler.Preprocessor;
using TypeCobol.Compiler.Scanner;
using TypeCobol.Compiler.Text;

namespace TypeCobol.DocumentModel.File
{
    /// <summary>
    /// Batch compilation of one file on disk or continuous incremental compilation of text in an editor
    /// </summary>
    public class FileParserCompiler : FileCompiler
    {
        /// <summary>
        /// Load a Cobol source file in memory
        /// </summary>
        public FileParserCompiler(string libraryName, string fileName, SourceFileProvider sourceFileProvider, IProcessedTokensDocumentProvider documentProvider, ColumnsLayout columnsLayout, TypeCobolOptions compilerOptions, TypeCobol.Compiler.CodeModel.SymbolTable customSymbols, bool isCopyFile, CompilationProject compilationProject) :
            this(libraryName, fileName, null, sourceFileProvider, documentProvider, columnsLayout, null, compilerOptions, customSymbols, isCopyFile, null, compilationProject, null)
        { }

        /// <summary>
        /// Load a Cobol source file in an pre-existing text document
        /// </summary>
        public FileParserCompiler(string libraryName, string fileName, SourceFileProvider sourceFileProvider, IProcessedTokensDocumentProvider documentProvider, ITextDocument textDocument, TypeCobolOptions compilerOptions, bool isCopyFile, CompilationProject compilationProject) :
            this(libraryName, fileName, null, sourceFileProvider, documentProvider, default(ColumnsLayout), textDocument, compilerOptions, null, isCopyFile, null, compilationProject, null)
        { }

        /// <summary>
        /// Use a pre-existing text document, not yet associated with a Cobol file
        /// </summary>
        public FileParserCompiler(ITextDocument textDocument, SourceFileProvider sourceFileProvider, IProcessedTokensDocumentProvider documentProvider, TypeCobolOptions compilerOptions, bool isCopyFile, CompilationProject compilationProject) :
            this(null, null, null, sourceFileProvider, documentProvider, default(ColumnsLayout), textDocument, compilerOptions, null, isCopyFile, null, compilationProject, null)
        { }

        /// <summary>
        /// Use a pre-existing text document, already initialized from a Cobol file
        /// </summary>
        public FileParserCompiler(string libraryName, string fileName, SourceFileProvider sourceFileProvider, IProcessedTokensDocumentProvider documentProvider, ColumnsLayout columnsLayout, TypeCobolOptions compilerOptions, TypeCobol.Compiler.CodeModel.SymbolTable customSymbols, bool isCopyFile, MultilineScanState scanState, CompilationProject compilationProject, List<RemarksDirective.TextNameVariation> copyTextNameVariations) :
            this(libraryName, fileName, null, sourceFileProvider, documentProvider, columnsLayout, null, compilerOptions, customSymbols, isCopyFile, scanState, compilationProject, copyTextNameVariations)
        { }

        /// <summary>
        /// Common internal implementation for all 4 constructors above
        /// </summary>
        protected FileParserCompiler(string libraryName, string fileName, CobolFile loadedCobolFile, SourceFileProvider sourceFileProvider, IProcessedTokensDocumentProvider documentProvider, ColumnsLayout columnsLayout, ITextDocument textDocument, TypeCobolOptions compilerOptions, SymbolTable customSymbols, bool isCopyFile,
            MultilineScanState scanState, CompilationProject compilationProject, List<RemarksDirective.TextNameVariation> copyTextNameVariations)
            : base(libraryName, fileName, loadedCobolFile, sourceFileProvider, documentProvider, columnsLayout, textDocument, compilerOptions, customSymbols, isCopyFile,
                    scanState, compilationProject, copyTextNameVariations)
        {
            if (!isCopyFile)
            {
                base.CompilationResultsForProgram = new FileCompilationUnit(TextDocument.Source, TextDocument.Lines, compilerOptions, documentProvider, copyTextNameVariations);
                base.CompilationResultsForProgram.CustomSymbols = customSymbols;
            }
        }

        /// <summary>
        /// Synchronous one-time compilation of the current file
        /// </summary>
        public override void CompileOnce()
        {
            if (CompilerOptions.ExecToStep == null)
                CompilerOptions.ExecToStep = ExecutionStep.SemanticCheck;

            if (CompilationResultsForCopy != null)
            {
                CompilationResultsForCopy.UpdateTokensLines(); //Scanner

                if (!(CompilerOptions.ExecToStep > ExecutionStep.Scanner)) return;

                CompilationResultsForCopy.RefreshTokensDocumentSnapshot();
                CompilationResultsForCopy.RefreshProcessedTokensDocumentSnapshot(); //Preprocessor
            }
            else
            {
                CompilationResultsForProgram.UpdateTokensLines(); //Scanner

                if (!(CompilerOptions.ExecToStep > ExecutionStep.Scanner)) return;

                CompilationResultsForProgram.RefreshTokensDocumentSnapshot();
                CompilationResultsForProgram.RefreshProcessedTokensDocumentSnapshot(); //Preprocessor

                if (!(CompilerOptions.ExecToStep > ExecutionStep.Preprocessor)) return;
                if (CompilerOptions.HaltOnMissingCopy && CompilationProject.MissingCopys.Count > 0) return; //If the Option is set to true and there is at least one missing copy, we don't have to run the semantic phase

                CompilationResultsForProgram.RefreshCodeElementsDocumentSnapshot(); //SyntaxCheck

                if (!(CompilerOptions.ExecToStep > ExecutionStep.SyntaxCheck)) return;

                CompilationResultsForProgram.RefreshProgramClassDocumentSnapshot(); //SemanticCheck
            }
        }

    }
}
