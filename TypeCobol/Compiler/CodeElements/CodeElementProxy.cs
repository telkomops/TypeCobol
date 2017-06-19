using Antlr4.Runtime;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.Diagnostics;
using TypeCobol.Compiler.Scanner;

namespace TypeCobol.Compiler.CodeElements
{
    /// <summary>
    /// This class is a proxy for another Code Element, it forwards all public method calls
    /// to the underlying target Code Element.
    /// </summary>
    /// <typeparam name="C"></typeparam>
    public class CodeElementProxy<C> : TypeCobol.Compiler.CodeElements.CodeElement where C : TypeCobol.Compiler.CodeElements.CodeElement
    {
        /// <summary>
        /// The Target Code Element.
        /// </summary>
        public TypeCobol.Compiler.CodeElements.CodeElement Target
        {
            get;
            set;
        }

        /// <summary>
        /// Code Element Type constructor
        /// </summary>
        /// <param name="type"></param>
        public CodeElementProxy(CodeElementType type)
            : base(type)
        {
            Target = null;
        }

        /// <summary>
        /// Code Element Type, Target constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target">Target</param>
        public CodeElementProxy(CodeElementType type, TypeCobol.Compiler.CodeElements.CodeElement target)
            : base(type)
        {
            Target = target;
        }

        /// <summary>
        /// Target Constructor, with CodeElementType.CodeElementProxy as Code Element Type.
        /// </summary>
        /// <param name="target"></param>
        public CodeElementProxy(TypeCobol.Compiler.CodeElements.CodeElement target) : base(CodeElementType.CodeElementProxy)
        {
            Target = target;
        }

        public override ISemanticData SemanticData 
        {
            get
            {
                return Target != null ? Target.SemanticData : base.SemanticData;
            }
            set
            {
                if (Target != null)
                    Target.SemanticData = value;
                else
                    base.SemanticData = value;
            }
        }

        public override IList<Token> ConsumedTokens
        {
            get { 
                return Target != null ? Target.ConsumedTokens : base.ConsumedTokens; 
            }
            set
            {
                if (Target != null)
                    Target.ConsumedTokens = value;
                else
                    base.ConsumedTokens = value;
            }
        }

        public override IDictionary<Token, SymbolInformation> SymbolInformationForTokens 
        {
            get
            {
                return Target != null ? Target.SymbolInformationForTokens : base.SymbolInformationForTokens;
            }
            set
            {
                if (Target != null)
                    Target.SymbolInformationForTokens = value;
                else
                    base.SymbolInformationForTokens = value;
            }
        }

        public override IList<StorageArea> StorageAreaReads 
        {
            get
            {
                return Target != null ? Target.StorageAreaReads : base.StorageAreaReads;
            }
            set
            {
                if (Target != null)
                    Target.StorageAreaReads = value;
                else
                    base.StorageAreaReads = value;

            }
        }

        public override IList<ReceivingStorageArea> StorageAreaWrites 
        {
            get
            {
                return Target != null ? Target.StorageAreaWrites : base.StorageAreaWrites;
            }
            set
            {
                if (Target != null)
                    Target.StorageAreaWrites = value;
                else
                    base.StorageAreaWrites = value;
            }
        }

        public override GroupCorrespondingImpact StorageAreaGroupsCorrespondingImpact 
        {
            get
            {
                return Target != null ? Target.StorageAreaGroupsCorrespondingImpact : base.StorageAreaGroupsCorrespondingImpact;
            }
            set
            {
                if (Target != null)
                    Target.StorageAreaGroupsCorrespondingImpact = value;
                else
                    base.StorageAreaGroupsCorrespondingImpact = value;
            }
        }

        public override CallTarget CallTarget 
        {
            get
            {
                return Target != null ? Target.CallTarget : base.CallTarget;
            }
            set
            {
                if (Target != null)
                    Target.CallTarget = value;
                else
                    base.CallTarget = value;
            }
        }

        public override IList<CallSite> CallSites 
        {
            get
            {
                return Target != null ? Target.CallSites : base.CallSites;
            }
            set
            {
                if (Target != null)
                    Target.CallSites = value;
                else
                    base.CallSites = value;
            }
        }

        public override IList<Diagnostic> Diagnostics 
        {
            get
            {
                return Target != null ? Target.Diagnostics : base.Diagnostics;
            }
            set
            {
                if (Target != null)
                    Target.Diagnostics = value;
                else
                    base.Diagnostics = value;
            }
        }

        public override void ApplyPropertiesToCE([NotNull] CodeElement ce)
        {
            if (Target != null)
                Target.ApplyPropertiesToCE(ce);
            else
                base.ApplyPropertiesToCE(ce);
        }

        public override string ToString()
        {
            return Target != null ? Target.ToString() : base.ToString();
        }

        public override bool IsInsideCopy()
        {
            return Target != null ? Target.IsInsideCopy() : base.IsInsideCopy();
        }

        public override bool IsAcrossSourceFile()
        {
            return Target != null ? Target.IsAcrossSourceFile() : base.IsAcrossSourceFile();
        }

        public override string SourceText
        {
            get
            {
                return Target != null ? Target.SourceText : base.SourceText;
            }
        }

        public override string Text
        {
            get
            {
                return Target != null ? Target.Text : base.Text;
            }
        }

        public override int Line
        {
            get
            {
                return Target != null ? Target.Line : base.Line;
            }
        }

        public override int Channel
        {
            get
            {
                return Target != null ? Target.Channel : base.Channel;
            }
        }

        public override int Column
        {
            get
            {
                return Target != null ? Target.Column : base.Column;
            }
        }

        public override int TokenIndex
        {
            get
            {
                return Target != null ? Target.TokenIndex : base.TokenIndex;
            }
        }

        public override int StartIndex
        {
            get
            {
                return Target != null ? Target.StartIndex : base.StartIndex;
            }
        }

        public override int StopIndex
        {
            get
            {
                return Target != null ? Target.StopIndex : base.StopIndex;
            }
        }

        public override ITokenSource TokenSource
        {
            get
            {
                return Target != null ? Target.TokenSource : base.TokenSource;
            }
        }

        public override ICharStream InputStream
        {
            get
            {
                return Target != null ? Target.InputStream : base.InputStream;
            }
        }

        public override R Accept<R, D>(Compiler.CodeElements.ICodeElementVisitor<R, D> v, D data)
        {
            if (Target != null)
                return Target.Accept<R, D>(v, data);
            return default(R);
        }
    }
}
