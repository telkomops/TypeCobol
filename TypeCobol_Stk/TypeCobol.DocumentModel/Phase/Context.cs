using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.DocumentModel.Dom;

namespace TypeCobol.DocumentModel.Phase
{
    /// <summary>
    /// This class represents a Checking or Attribution context, parameterized with Context information.
    /// </summary>
    public class Context<I> : IEnumerable<Context<I>>
    {
        /// <summary>
        /// The Next enclosing context;
        /// </summary>
        public Context<I> Next;
        /// <summary>
        /// The context enclosing the current context
        /// </summary>
        public Context<I> Outer;
        /// <summary>
        /// The Code element with which this context is associated.
        /// </summary>
        public CodeElement Element;
        /// <summary>
        /// The Top Level Program.
        /// </summary>
        public CobolProgram ToplevelProgram;
        /// <summary>
        /// The next enclosing Program.
        /// </summary>
        public CobolProgram EnclosingProgram;
        /// <summary>
        /// The Next enclosing function
        /// </summary>
        public FunctionDeclaration EnclosingFunction;
        /// <summary>
        /// Generic futher information.
        /// </summary>
        public I Info;
        /// <summary>
        /// Constructor of a context of a given CodeElement with a given info field.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="info"></param>
        public Context(CodeElement element, I info)
        {
            this.Element = element;
            this.Info = info;
        }

        /// <summary>
        /// Duplicate this context, updating the given element and info, 
        /// and copying all other fields.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public Context<I> Dup(CodeElement element, I info)
        {
            return DupTo(new Context<I>(element, info));
        }

        /// <summary>
        /// Duplicate this context into a given context, using its element and info, and
        /// copying all fields.
        /// Linking to this next context.
        /// </summary>
        /// <param name="to">The target context</param>
        /// <returns>The target context</returns>
        public Context<I> DupTo(Context<I> to)
        {
            to.Next = this;
            to.Outer = this.Outer;
            to.ToplevelProgram = this.ToplevelProgram;
            to.EnclosingProgram = this.EnclosingProgram;
            to.EnclosingFunction = this.EnclosingFunction;
            return to;
        }

        /// <summary>
        /// Duplicate this context, updating with the given element
        /// and copying all other fields.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public Context<I> Dup(CodeElement element)
        {
            return Dup(element, this.Info);
        }

        /// <summary>
        /// Iterator on enclosing Context
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Context<I>> GetEnumerator()
        {
            Context<I> next = this;
            if (next.Outer != null)
            {
                Context<I> current = next;
                next = current.Outer;
                yield return current;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
