using System;

namespace TypeCobol.Compiler.CodeElements
{
    /// <summary>
    /// The object IDENTIFICATION DIVISION introduces the object definition, which is
    /// the portion of a class definition that defines the instance objects of the class.
    /// </summary>
    public class ObjectEnd : CodeElementEnd
    {
        public ObjectEnd() : base(CodeElementType.ObjectEnd)
        { }

        public override R Accept<R, D>(ICodeElementVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }
    }
}
