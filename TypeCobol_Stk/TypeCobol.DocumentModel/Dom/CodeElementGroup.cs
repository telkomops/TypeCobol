using System;
using System.Collections.Generic;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom
{
    /// <summary>
    /// A class that represents a group of CodeElement.
    /// </summary>
    public abstract class CodeElementGroup : CodeElement, IEnumerable<CodeElement>
    {
        /// <summary>
        /// Cosntructor
        /// </summary>
        /// <param name="type"></param>
        public CodeElementGroup(CodeDomType type) : base((CodeElementType)type)
        {
        }

        /// <summary>
        /// All significant tokens consumed in the source document to build this code element
        /// For a CodeElementGroup this is a Computed value.
        /// </summary>
        public override IList<TypeCobol.Compiler.Scanner.Token> ConsumedTokens
        {
            get 
            {
                List<TypeCobol.Compiler.Scanner.Token> tokens = null;
                IEnumerator<CodeElement> enumGroup = GetEnumerator();
                while (enumGroup.MoveNext())
                {
                    CodeElement ce = enumGroup.Current;
                    var ce_tokens = ce.ConsumedTokens;
                    if (ce_tokens != null)
                    {
                        if (tokens == null)
                            tokens = new List<TypeCobol.Compiler.Scanner.Token>();
                        tokens.AddRange(ce_tokens);
                    }
                }
                return tokens; 
            }
        }

        /// <summary>
        /// Acceptation method on a visitor
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="v"></param>
        public abstract void Accept<R, D>(TypeCobol.DocumentModel.Dom.Visitor.CodeDomVisitor<R, D> v, D data);

        public override void Accept<R, D>(TypeCobol.Compiler.CodeElements.ICodeElementVisitor<R, D> v, D data)
        {
            if (v is TypeCobol.DocumentModel.Dom.Visitor.CodeDomVisitor<R, D>)
                Accept(v as TypeCobol.DocumentModel.Dom.Visitor.CodeDomVisitor<R, D>, data);
        }

        /// <summary>
        /// Enumerator on all Code Element in the group.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<CodeElement> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
