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
