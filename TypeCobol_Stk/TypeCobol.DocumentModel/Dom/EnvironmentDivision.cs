using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom
{   
    /// <summary>
    /// EnvironmentDivision
    /// </summary>
    public class EnvironmentDivision : CodeElementGroup
    {
        /// <summary>
        /// Environment Division Header
        /// </summary>
        public TypeCobol.Compiler.CodeElements.EnvironmentDivisionHeader EnvironmentDivisionHeader
        {
            get
            {
                return (EnvironmentDivisionHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        public ConfigurationSection ConfigurationSection
        {
            get;
            set;
        }

        public InputOutputSection InputOutputSection
        {
            get;
            set;
        }

        /// <summary>
        /// Empty Constructor.
        /// </summary>
        public EnvironmentDivision()
            : base(CodeDomType.EnvironmentDivision)
        {
        }

        /// <summary>
        /// Environnement hedear constructor
        /// </summary>
        /// <param name="environmentDivisionHeader"></param>
        public EnvironmentDivision(TypeCobol.Compiler.CodeElements.EnvironmentDivisionHeader environmentDivisionHeader)
            : base(CodeDomType.EnvironmentDivision)
        {
            EnvironmentDivisionHeader = environmentDivisionHeader;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="environmentDivisionHeader"></param>
        /// <param name="configurationSection"></param>
        /// <param name="inputOutputSection"></param>
        public EnvironmentDivision(TypeCobol.Compiler.CodeElements.EnvironmentDivisionHeader environmentDivisionHeader,
            ConfigurationSection configurationSection,
            InputOutputSection inputOutputSection)
            : base(CodeDomType.EnvironmentDivision)
        {
            EnvironmentDivisionHeader = environmentDivisionHeader;
            ConfigurationSection = configurationSection;
            InputOutputSection = inputOutputSection;
        }

        public override R Accept<R,D>(Visitor.CodeDomVisitor<R,D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<Compiler.CodeElements.CodeElement> GetEnumerator()
        {
            if (this.EnvironmentDivisionHeader != null)
                yield return this.EnvironmentDivisionHeader;
            if (this.ConfigurationSection != null)
                yield return this.ConfigurationSection;
            if (this.InputOutputSection != null)
                yield return this.InputOutputSection;

        }
    }

    /// <summary>
    /// Configuration Section
    /// </summary>
    public class ConfigurationSection : CodeElementGroup
    {
        public TypeCobol.Compiler.CodeElements.ConfigurationSectionHeader ConfigurationSectionHeader
        {
            get
            {
                return (TypeCobol.Compiler.CodeElements.ConfigurationSectionHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        public ConfigurationParagraphs ConfigurationParagraphs
        {
            get;
            set;
        }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public ConfigurationSection()
            : base(CodeDomType.ConfigurationSection)
        {
        }

        /// <summary>
        /// Configuration Section Header Constructor
        /// </summary>
        public ConfigurationSection(TypeCobol.Compiler.CodeElements.ConfigurationSectionHeader confSectHeader)
            : base(CodeDomType.ConfigurationSection)
        {
            ConfigurationSectionHeader = confSectHeader;
        }

        /// <summary>
        /// Configuration Section Header Constructor
        /// </summary>
        public ConfigurationSection(TypeCobol.Compiler.CodeElements.ConfigurationSectionHeader confSectHeader, ConfigurationParagraphs confParagraphs)
            : base(CodeDomType.ConfigurationSection)
        {
            ConfigurationSectionHeader = confSectHeader;
            ConfigurationParagraphs = confParagraphs;
        }

        public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<Compiler.CodeElements.CodeElement> GetEnumerator()
        {
            if (this.ConfigurationSectionHeader != null)
                yield return this.ConfigurationSectionHeader;
            if (this.ConfigurationParagraphs != null)
            {
                foreach (var cp in this.ConfigurationParagraphs)
                {
                    yield return cp;
                }
            }
        }
    }

    public class ConfigurationParagraph : CodeElementProxy<CodeElement>
    {
        public ConfigurationParagraph(CodeElement ce) : base(ce)
        {
        }
        public class SourceComputer : ConfigurationParagraph
        {
            public SourceComputer(SourceComputerParagraph scp) : base(scp)
            {
            }
            public SourceComputerParagraph SourceComputerParagraph
            {
                get
                {
                    return (SourceComputerParagraph)Target;
                }
            }
        }
        public class ObjectComputer : ConfigurationParagraph
        {
            public ObjectComputer(ObjectComputerParagraph scp)
                : base(scp)
            {
            }
            public ObjectComputerParagraph ObjectComputerParagraph
            {
                get
                {
                    return (ObjectComputerParagraph)Target;
                }
            }
        }
        public class SpecialNames : ConfigurationParagraph
        {
            public SpecialNames(SpecialNamesParagraph snp)
                : base(snp)
            {
            }
            public SpecialNamesParagraph SpecialNamesParagraph
            {
                get
                {
                    return (SpecialNamesParagraph)Target;
                }
            }
        }
        public class Repository : ConfigurationParagraph
        {
            public Repository(RepositoryParagraph snp)
                : base(snp)
            {
            }
            public RepositoryParagraph RepositoryParagraph
            {
                get
                {
                    return (RepositoryParagraph)Target;
                }
            }
        }
    }

    /// <summary>
    /// A List of Configuration Paragarph
    /// </summary>
    public class ConfigurationParagraphs : List<ConfigurationParagraph>
    {
    }

    public class InputOutputSection : CodeElementGroup
    {
        public InputOutputSectionHeader InputOutputSectionHeader
        {
            get
            {
                return (InputOutputSectionHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        public FileControlParagraph FileControlParagraph
        {
            get;
            set;
        }

        public IoControlParagraph IoControlParagraph
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public InputOutputSection() : base(CodeDomType.InputOutputSection)
        {
        }

        public InputOutputSection(InputOutputSectionHeader header, FileControlParagraph fileControl, IoControlParagraph ioCtrlParagraph)
            : base(CodeDomType.InputOutputSection)
        {
            InputOutputSectionHeader = header;
            FileControlParagraph = fileControl;
            IoControlParagraph = ioCtrlParagraph;
        }

        public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.InputOutputSectionHeader != null)
                yield return this.InputOutputSectionHeader;
            if (this.FileControlParagraph != null)
                yield return this.FileControlParagraph;
            if (this.IoControlParagraph != null)
                yield return this.IoControlParagraph;
        }
    }

    /// <summary>
    /// A List of FileControlEntry
    /// </summary>
    public class FileControlEntries : List<FileControlEntry>
    {
        public FileControlEntries()
        {
        }
    }

    public class FileControlParagraph : CodeElementGroup
    {
        public FileControlParagraphHeader FileControlParagraphHeader
        {
            get
            {
                return (FileControlParagraphHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        public FileControlEntries FileControlEntries
        {
            get;
            set;
        }

        public FileControlParagraph() : base(CodeDomType.FileControlParagraph)
        {
        }

        public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.FileControlParagraphHeader != null)
                yield return this.FileControlParagraphHeader;
            if (this.FileControlEntries != null)
            {
                foreach (var fce in this.FileControlEntries)
                    yield return fce;
            }
        }
    }

    /// <summary>
    /// A List of IOControlEntry
    /// </summary>
    public class IOControlEntries : List<IOControlEntry>
    {
        public IOControlEntries()
        {
        }
    }

    public class IoControlParagraph : CodeElementGroup
    {
        public IOControlParagraphHeader IOControlParagraphHeader
        {
            get
            {
                return (IOControlParagraphHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        public IOControlEntries IOControlEntries
        {
            get;
            set;
        }

        public SentenceEnd SentenceEnd
        {
            get;
            set;
        }

        public IoControlParagraph() : base(CodeDomType.IoControlParagraph)
        {
        }

        public override R Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            return v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.IOControlParagraphHeader != null)
                yield return this.IOControlParagraphHeader;
            if (this.IOControlEntries != null)
            {
                foreach (var ioce in this.IOControlEntries)
                    yield return ioce;
            }
            if (this.SentenceEnd != null)
                yield return this.SentenceEnd;
        }
    }
}
