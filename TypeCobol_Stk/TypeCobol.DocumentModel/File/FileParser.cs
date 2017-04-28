using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler;
using TypeCobol.Compiler.Directives;
using TypeCobol.Compiler.File;
using TypeCobol.Compiler.Preprocessor;
using TypeCobol.Compiler.Text;

namespace TypeCobol.DocumentModel.File
{
    /// <summary>
    /// Our File Parser
    /// </summary>
    public class FileParser : TypeCobol.Parser
    {
        public FileParser()
        {
        }

        /// <summary>
        /// Create a File Compiler
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="fileName"></param>
        /// <param name="sourceFileProvider"></param>
        /// <param name="documentProvider"></param>
        /// <param name="columnsLayout"></param>
        /// <param name="compilerOptions"></param>
        /// <param name="customSymbols"></param>
        /// <param name="isCopyFile"></param>
        /// <param name="compilationProject"></param>
        /// <returns></returns>
        protected virtual FileCompiler CreateFileCompiler(string libraryName, string fileName, SourceFileProvider sourceFileProvider, IProcessedTokensDocumentProvider documentProvider, ColumnsLayout columnsLayout, TypeCobolOptions compilerOptions, TypeCobol.Compiler.CodeModel.SymbolTable customSymbols, bool isCopyFile, CompilationProject compilationProject)
        {
            return new FileParserCompiler(libraryName, fileName, sourceFileProvider, documentProvider, columnsLayout, compilerOptions, customSymbols, isCopyFile, compilationProject);
        }

        public void Init(string path, TypeCobolOptions options, DocumentFormat format = null, IList<string> copies = null)
        {
            FileCompiler compiler;
            if (Compilers.TryGetValue(path, out compiler)) return;
            string filename = System.IO.Path.GetFileName(path);
            var root = new System.IO.DirectoryInfo(System.IO.Directory.GetParent(path).FullName);
            if (format == null) format = GetFormat(path);

            CompilationProject project = new CompilationProject(path, root.FullName, Extensions,
                format.Encoding, format.EndOfLineDelimiter, format.FixedLineLength, format.ColumnsLayout, options);
            //Add copy folder into sourceFileProvider
            SourceFileProvider sourceFileProvider = project.SourceFileProvider;
            copies = copies ?? new List<string>();
            foreach (var folder in copies)
            {
                sourceFileProvider.AddLocalDirectoryLibrary(folder, false, CopyExtensions, format.Encoding, format.EndOfLineDelimiter, format.FixedLineLength);
            }
            compiler = new FileParserCompiler(null, filename, project.SourceFileProvider, project, format.ColumnsLayout, options, CustomSymbols, false, project);

            Compilers.Add(path, compiler);
            Inits.Add(path, false);
        }

    }
}
