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
            get;
            set;
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

        public override void Accept<R,D>(Visitor.CodeDomVisitor<R,D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<Compiler.CodeElements.CodeElement> GetEnumerator()
        {
            yield return EnvironmentDivisionHeader;
            if (ConfigurationSection != null)
                yield return ConfigurationSection;
            if (InputOutputSection != null)
                yield return InputOutputSection;

        }
    }

    /// <summary>
    /// Configuration Section
    /// </summary>
    public class ConfigurationSection : CodeElementGroup
    {
        public TypeCobol.Compiler.CodeElements.ConfigurationSectionHeader ConfigurationSectionHeader
        {
            get;
            set;
        }

        public List<ConfigurationParagraph> ConfigurationParagraphs
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
        public ConfigurationSection(TypeCobol.Compiler.CodeElements.ConfigurationSectionHeader confSectHeader, List<ConfigurationParagraph> confParagraphs)
            : base(CodeDomType.ConfigurationSection)
        {
            ConfigurationSectionHeader = confSectHeader;
            ConfigurationParagraphs = confParagraphs;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<Compiler.CodeElements.CodeElement> GetEnumerator()
        {
            yield return ConfigurationSectionHeader;
            if (ConfigurationParagraphs != null)
            {
                foreach (var cp in ConfigurationParagraphs)
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

    public class InputOutputSection : CodeElementGroup
    {
        public InputOutputSectionHeader InputOutputSectionHeader
        {
            get;
            set;
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

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            yield return InputOutputSectionHeader;
            if (FileControlParagraph != null)
                yield return FileControlParagraph;
            if (IoControlParagraph != null)
                yield return IoControlParagraph;
        }
    }

    public class FileControlParagraph : CodeElementGroup
    {
        public FileControlParagraphHeader FileControlParagraphHeader
        {
            get;
            set;
        }

        public List<FileControlEntry> FileControlEntries
        {
            get;
            set;
        }

        public FileControlParagraph() : base(CodeDomType.FileControlParagraph)
        {
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            yield return FileControlParagraphHeader;
            if (FileControlEntries != null)
            {
                foreach(var fce in FileControlEntries)
                    yield return fce;
            }
        }
    }

    public class IoControlParagraph : CodeElementGroup
    {
        public IOControlParagraphHeader IOControlParagraphHeader
        {
            get;
            set;
        }

        public List<IOControlEntry> IOControlEntries
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

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            yield return IOControlParagraphHeader;
            if (IOControlEntries != null)
            {
                foreach (var ioce in IOControlEntries)
                    yield return ioce;
            }
            if (SentenceEnd != null)
                yield return SentenceEnd;
        }
    }
}
