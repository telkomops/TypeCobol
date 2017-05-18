using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom.Visitor
{
    /// <summary>
    /// A base class for a CodeDomVisitor. It goes thru CodeElementGroup
    /// </summary>
    public class BaseCodeDomVisitor<R, D> : CodeDomVisitor<R, D>
    {
        public virtual R visitCodeElement(CodeElement that, D data) 
        {
            if (that is CodeElementGroup)
            {
                CodeElementGroup group = that as CodeElementGroup;
                var groupEneum = group.GetEnumerator();
                while (groupEneum.MoveNext())
                {
                    var ce = groupEneum.Current;
                    ce.Accept(this, data);
                }
            }
            return default(R);
        }
    }
}
