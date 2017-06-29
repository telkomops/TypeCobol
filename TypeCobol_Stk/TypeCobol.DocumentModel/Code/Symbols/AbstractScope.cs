using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Scopes;

namespace TypeCobol.DocumentModel.Code.Symbols
{
    public class AbstractScope : TypeCobolSymbol, IScope
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        protected AbstractScope()
        {
        }

        /// <summary>
        /// Named constructor
        /// </summary>
        protected AbstractScope(String name, Kinds kind)
            : base(name, kind)
        {
        }

        public TypeCobolScope<TypedefSymbol> Types
        {
            get { return null; }
        }

        public TypeCobolScope<VariableSymbol> FileData
        {
            get { return null; }
        }

        public TypeCobolScope<VariableSymbol> WorkingStorageData
        {
            get { return null; }
        }

        public TypeCobolScope<VariableSymbol> LocalStorageData
        {
            get { return null; }
        }

        public TypeCobolScope<VariableSymbol> LinkageStorageData
        {
            get { return null; }
        }

        public TypeCobolScope<SectionSymbol> Sections
        {
            get { return null; }
        }

        public TypeCobolScope<ParagraphSymbol> Paragraphs
        {
            get { return null; }
        }

        public TypeCobolScope<FunctionSymbol> Functions
        {
            get { return null; }
        }

        public TypeCobolScope<ProgramSymbol> Programs
        {
            get { return null; }
        }
    }
}
