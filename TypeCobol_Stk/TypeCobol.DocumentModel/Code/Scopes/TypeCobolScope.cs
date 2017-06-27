using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Symbols;

namespace TypeCobol.DocumentModel.Code.Scopes
{
    /// <summary>
    /// A Cobol Scope which contains Cobol Symbols declared within a Scope
    /// </summary>
    public class TypeCobolScope<T> where T : TypeCobolSymbol
    {
        /// <summary>
        /// 
        /// The Owner of this scope
        /// </summary>
        public TypeCobolSymbol Owner
        {
            get;
            set;
        }
        /// <summary>
        /// All Symbols in this scope
        /// </summary>
        Dictionary<String, T> symbols = null;

        /// <summary>
        /// Enter a Symbol in this scope, if a previous symbol with the same name exits, it is replaced.
        /// </summary>
        /// <param name="sym">The Symbol to enter</param>
        public void Enter(T sym)
        {
            if (symbols == null)
                symbols = new Dictionary<String, T>();
            symbols[sym.Name.ToLower()] = sym;
        }

        /** Enter symbol sym in this scope if not already exists.
         */
        public void EnterIfNotExist(T sym)
        {
            TypeCobolSymbol symbol = Lookup(sym.Name);
            if (symbol != null)
                Enter(sym);
        }

        /// <summary>
        /// Lookup a symbol in this scope
        /// </summary>
        /// <param name="name">The Symbol's name</param>
        /// <returns>The Symbol if it exist, null otherwise</returns>
        public T Lookup(String name)
        {
            T symbol = null;
            if (symbols != null)
                symbols.TryGetValue(name.ToLower(), out symbol);
            return symbol;
        }
    }
}
