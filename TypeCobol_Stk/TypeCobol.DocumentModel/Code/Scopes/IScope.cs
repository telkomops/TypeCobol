using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Symbols;

namespace TypeCobol.DocumentModel.Code.Scopes
{
    /// <summary>
    /// A Collection of symbol members
    /// </summary>
    public interface IScope
    {
        /// <summary>
        /// Type symbols
        /// </summary>
        TypeCobolScope<TypedefSymbol> Types
        {
            get;
        }
        /// <summary>
        /// File data symbols
        /// </summary>
        TypeCobolScope<VariableSymbol> FileData
        {
            get;
        }
        /// <summary>
        /// WorkingStorageData symbols
        /// </summary>
        TypeCobolScope<VariableSymbol> WorkingStorageData
        {
            get;
        }
        /// <summary>
        /// LocalStorageData symbols
        /// </summary>
        TypeCobolScope<VariableSymbol> LocalStorageData
        {
            get;
        }
        /// <summary>
        /// LinkageStorageData symbols
        /// </summary>
        TypeCobolScope<VariableSymbol> LinkageStorageData
        {
            get;
        }
        /// <summary>
        /// Sections symbols
        /// </summary>
        TypeCobolScope<SectionSymbol> Sections
        {
            get;
        }
        /// <summary>
        /// Paragraphs symbols
        /// </summary>
        TypeCobolScope<ParagraphSymbol> Paragraphs
        {
            get;
        }
        /// <summary>
        /// Functions symbols
        /// </summary>
        TypeCobolScope<FunctionSymbol> Functions
        {
            get;
        }
        /// <summary>
        /// Functions symbols
        /// </summary>
        TypeCobolScope<ProgramSymbol> Programs
        {
            get;
        }
    }
}
