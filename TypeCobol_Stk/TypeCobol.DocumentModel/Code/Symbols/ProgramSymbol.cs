using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Scopes;

namespace TypeCobol.DocumentModel.Code.Symbols
{
    /// <summary>
    /// Reprsents a Program Symbol
    /// </summary>
    public class ProgramSymbol : TypeCobolSymbol
    {
        /// <summary>
        /// Named constructor.
        /// </summary>
        /// <param name="name"></param>
        public ProgramSymbol(String name) : base(name, Kinds.Program)
        {
            Types = new TypeCobolScope<TypedefSymbol>();
            FileData = new TypeCobolScope<VariableSymbol>();
            WorkingStorageData = new TypeCobolScope<VariableSymbol>();
            LocalStorageData = new TypeCobolScope<VariableSymbol>();
            LinkageStorageData = new TypeCobolScope<VariableSymbol>();
            Sections = new TypeCobolScope<SectionSymbol>();
            Paragraphs = new TypeCobolScope<ParagraphSymbol>();
            Functions = new TypeCobolScope<FunctionSymbol>();
            Programs = new TypeCobolScope<ProgramSymbol>();
        }

        /// <summary>
        /// All types of this program.
        /// </summary>
        public TypeCobolScope<TypedefSymbol> Types
        {
            get;
            protected set;
        }

        /// <summary>
        /// File data scope of the program.
        /// </summary>
        public TypeCobolScope<VariableSymbol> FileData
        {
            get;
            protected set;
        }

        /// <summary>
        /// Working Storage data scope of the program.
        /// </summary>
        public TypeCobolScope<VariableSymbol> WorkingStorageData
        {
            get;
            protected set;
        }

        /// <summary>
        /// Working Storage data scope of the program.
        /// </summary>
        public TypeCobolScope<VariableSymbol> LocalStorageData
        {
            get;
            protected set;
        }

        /// <summary>
        /// Linkage Storage data scope of the program.
        /// </summary>
        public TypeCobolScope<VariableSymbol> LinkageStorageData
        {
            get;
            protected set;
        }

        /// <summary>
        /// Section scope of the program.
        /// </summary>
        public TypeCobolScope<SectionSymbol> Sections
        {
            get;
            protected set;
        }

        /// <summary>
        /// Paragraph scope of the program.
        /// </summary>
        public TypeCobolScope<ParagraphSymbol> Paragraphs
        {
            get;
            protected set;
        }

        /// <summary>
        /// Functions scope of the program.
        /// </summary>
        public TypeCobolScope<FunctionSymbol> Functions
        {
            get;
            protected set;
        }

        /// <summary>
        /// Programs scope of the program.
        /// </summary>
        public TypeCobolScope<ProgramSymbol> Programs
        {
            get;
            protected set;
        }
    }
}
