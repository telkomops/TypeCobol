using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Scopes;

namespace TypeCobol.DocumentModel.Code.Symbols
{
    /// <summary>
    /// Symbol that represents a Namespace. A namespace can only contains 
    /// types, variables, programs oe namespaces.
    /// </summary>
    public class NamespaceSymbol : TypeCobolSymbol
    {
        /// <summary>
        /// Named constructor.
        /// </summary>
        /// <param name="name"></param>
        public NamespaceSymbol(String name)
            : base(name, Kinds.Namespace)
        {
            Types = new TypeCobolScope<TypedefSymbol>();
            Variables = new TypeCobolScope<VariableSymbol>();
            Programs = new TypeCobolScope < ProgramSymbol >();
            Namespaces = new TypeCobolScope<NamespaceSymbol>();
        }

        /// <summary>
        /// Enter a Program in this namespace
        /// </summary>
        /// <param name="name">Program's name</param>
        /// <returns>The ProgramSymbol</returns>
        public ProgramSymbol EnterProgram(String name)
        {
            ProgramSymbol prgSym = Programs.Lookup(name);
            if (prgSym == null) 
            {
                prgSym = new ProgramSymbol(name);
                Programs.Enter(prgSym);
            }
            return prgSym;
        }

        /// <summary>
        /// All types and variables declared in this namespace.
        /// </summary>
        public TypeCobolScope<TypedefSymbol> Types
        {
            get;
            set;
        }

        /// <summary>
        /// All types and variables declared in this namespace.
        /// </summary>
        public TypeCobolScope<VariableSymbol> Variables
        {
            get;
            set;
        }

        /// <summary>
        /// All programs declared in this namespace.
        /// </summary>
        public TypeCobolScope<ProgramSymbol> Programs
        {
            get;
            set;
        }

        /// <summary>
        /// All namespaces declared in this namespace.
        /// </summary>
        public TypeCobolScope<NamespaceSymbol> Namespaces
        {
            get;
            set;
        }
    }
}
