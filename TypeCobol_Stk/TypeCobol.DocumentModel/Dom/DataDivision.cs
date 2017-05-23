using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom
{
    /// <summary>
    /// The Cobol DATA DIVISION Code Model.
    /// </summary>
    public class DataDivision : CodeElementGroup
    {
	    public DataDivisionHeader DataDivisionHeader 
        {
            get
            {
                return (DataDivisionHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }
        /// <summary>
        /// The File Section
        /// </summary>
        public FileSection FileSection
        {
            get;
            set;
        }

        /// <summary>
        /// Working Storage Section
        /// </summary>
        public WorkingStorageSection WorkingStorageSection
        {
            get;
            set;
        }

        /// <summary>
        /// The Local Storage Section
        /// </summary>
        public LocalStorageSection LocalStorageSection
        {
            get;
            set;
        }

        /// <summary>
        /// The Linkage Section
        /// </summary>
        public LinkageSection LinkageSection
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public DataDivision()
            : base(CodeDomType.DataDivision)
        {
        }

        /// <summary>
        /// Header constructor.
        /// </summary>
        public DataDivision(DataDivisionHeader dataDivisionHeader)
            : base(CodeDomType.DataDivision)
        {
            this.DataDivisionHeader = dataDivisionHeader;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<Compiler.CodeElements.CodeElement> GetEnumerator()
        {
            if (this.DataDivisionHeader != null)
                yield return this.DataDivisionHeader ;
            if (this.FileSection != null)
                yield return this.FileSection;
            if (this.WorkingStorageSection != null)
                yield return this.WorkingStorageSection;
            if (this.LocalStorageSection != null)
                yield return this.LocalStorageSection;
            if (this.LinkageSection != null)
                yield return this.LinkageSection;
        }
    }

    public class StorageEntry : CodeElementProxy<CodeElement>
    {
        public StorageEntry(CodeElement ce)
            : base(ce)
        {
        }
        public class DataDefinition : StorageEntry
        {
            public DataDefinition(DataDefinitionEntry dde) : base(dde)
            {
            }
        }

        public class ExecSql : StorageEntry
        {
            public ExecSql(ExecSqlStatement ess)
                : base(ess)
            {
            }
        }
    }

    /// <summary>
    /// A iList of StorageEntry
    /// </summary>
    public class StorageEntries : List<TypeCobol.DocumentModel.Dom.StorageEntry>
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public StorageEntries()
        {
        }
    }

    public class ExecSqlStatement : CodeElementGroup
    {
        public ExecStatement ExecStatement
        {
            get;
            set;
        }

        public SentenceEnd SentenceEnd
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ExecSqlStatement() : base(CodeDomType.ExecSqlStatement)
        {
        }

        /// <summary>
        /// Exec Statement constructor
        /// </summary>
        public ExecSqlStatement(ExecStatement execStmt)
            : base(CodeDomType.ExecSqlStatement)
        {
            this.ExecStatement = execStmt;
        }

        /// <summary>
        /// Exec Statement End constructor
        /// </summary>
        public ExecSqlStatement(ExecStatement execStmt, SentenceEnd sentenceEnd)
            : base(CodeDomType.ExecSqlStatement)
        {
            this.ExecStatement = execStmt;
            this.SentenceEnd = sentenceEnd;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.ExecStatement != null)
                yield return this.ExecStatement;
            if (SentenceEnd != null)
                yield return SentenceEnd;
        }
    }

    public class DataDefinitionEntry : CodeElementProxy<CodeElement>
    {
        public DataDefinitionEntry(CodeElement ce)
            : base(ce)
        {
        }

	    public class DataDescription : DataDefinitionEntry
        {
            public DataDescription(DataDescriptionEntry entry) : base(entry)
            {
            }
            public DataDefinitionEntry DataDefinitionEntry
            {
                get
                {
                    return (DataDefinitionEntry)Target;
                }
            }
        }

	    public class DataRedefines : DataDefinitionEntry
        {
            public DataRedefines(DataRedefinesEntry entry)
                : base(entry)
            {
            }
            public DataRedefinesEntry DataRedefinesEntry
            {
                get
                {
                    return (DataRedefinesEntry)Target;
                }
            }
        }

	    public class DataRenames : DataDefinitionEntry
        {
            public DataRenames(DataRenamesEntry entry)
                : base(entry)
            {
            }
            public DataRenamesEntry DataRenamesEntry
            {
                get
                {
                    return (DataRenamesEntry)Target;
                }
            }
        }

        public class DataCondition : DataDefinitionEntry
        {
            public DataCondition(DataConditionEntry entry)
                : base(entry)
            {
            }
            public DataConditionEntry DataConditionEntry
            {
                get
                {
                    return (DataConditionEntry)Target;
                }
            }
        }
    };

    /// <summary>
    /// A List od Data Definition Entry
    /// </summary>
    public class DataDefinitionEntries : List<DataDefinitionEntry>
    {
        public DataDefinitionEntries()
        {
        }
    }

    /// <summary>
    /// A File Description Entry in a FILE SECTION
    /// </summary>
    public class FileDescription : CodeElementGroup
    {
        TypeCobol.Compiler.CodeElements.FileDescriptionEntry FileDescriptionEntry
        {
            get
            {
                return (FileDescriptionEntry)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }
        /// <summary>
        /// Data Definition Entries
        /// </summary>
        public DataDefinitionEntries DataDefinitionEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public FileDescription() : base(CodeDomType.FileDescription)
        {
        }

        /// <summary>
        /// FileDescriptionEntry constructor
        /// </summary>
        public FileDescription(TypeCobol.Compiler.CodeElements.FileDescriptionEntry fileDescriptionEntry)
            : base(CodeDomType.FileDescription)
        {
            this.FileDescriptionEntry = fileDescriptionEntry;
        }

        /// <summary>
        /// FileDescriptionEntry, Data Definition Entries constructor
        /// </summary>
        public FileDescription(TypeCobol.Compiler.CodeElements.FileDescriptionEntry fileDescriptionEntry, DataDefinitionEntries dataDefinitionEntries)
            : base(CodeDomType.FileDescription)
        {
            this.FileDescriptionEntry = fileDescriptionEntry;
            this.DataDefinitionEntries = dataDefinitionEntries;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.FileDescriptionEntry != null)
                yield return this.FileDescriptionEntry;
            if (this.DataDefinitionEntries != null)
            {
                foreach (DataDefinitionEntry dde in this.DataDefinitionEntries)
                    yield return dde;
            }
        }
    }

    /// <summary>
    /// A List of File Description Entry
    /// </summary>
    public class FileDescriptionEntries : List<FileDescription>
    {
        public FileDescriptionEntries()
        {
        }
    }

    /// <summary>
    /// The FILE SECTION
    /// </summary>
    public class FileSection : CodeElementGroup
    {
        public FileSectionHeader FileSectionHeader
        {
            get
            {
                return (FileSectionHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        public FileDescriptionEntries FileDescriptionEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public FileSection()
            : base(CodeDomType.FileSection)
        {
        }

        /// <summary>
        /// File Section Header Constructor
        /// </summary>
        public FileSection(FileSectionHeader fileSectionHeader)
            : base(CodeDomType.FileSection)
        {
            this.FileSectionHeader = fileSectionHeader;
        }

        /// <summary>
        /// File Section Header Constructor
        /// </summary>
        public FileSection(FileSectionHeader fileSectionHeader, FileDescriptionEntries fileDescriptionEntries)
            : base(CodeDomType.FileSection)
        {
            this.FileSectionHeader = fileSectionHeader;
            this.FileDescriptionEntries = fileDescriptionEntries;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.FileSectionHeader != null)
                yield return this.FileSectionHeader;
            if (this.FileDescriptionEntries != null)
            {
                foreach (FileDescription fde in this.FileDescriptionEntries)
                    yield return fde;
            }
        }
    }

    /// <summary>
    /// Abstract class for STORAGE SECTION
    /// </summary>
    public abstract class StorageSection : CodeElementGroup
    {
        /// <summary>
        /// Enties in this Storage
        /// </summary>
        public StorageEntries StorageEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Storage type constructor
        /// </summary>
        /// <param name="type"></param>
        public StorageSection(CodeDomType type) : base(type)
        {
        }

        /// <summary>
        /// Full constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="storageEntries"></param>
        public StorageSection(CodeDomType type, StorageEntries storageEntries) : base(type)
        {
            this.StorageEntries = storageEntries;
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.StorageEntries != null)
            {
                foreach(StorageEntry se in this.StorageEntries)
                    yield return se;
            }
        }
    }

    /// <summary>
    /// The WORKING STORAGE SECTION
    /// </summary>
    public class WorkingStorageSection : StorageSection
    {
        public WorkingStorageSectionHeader WorkingStorageSectionHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public WorkingStorageSection() : base(CodeDomType.WorkingStorageSection)
        {
        }

        /// <summary>
        /// Header constructor
        /// </summary>
        public WorkingStorageSection(WorkingStorageSectionHeader workingStorageSectionHeader)
            : base(CodeDomType.WorkingStorageSection)
        {
            this.WorkingStorageSectionHeader = workingStorageSectionHeader;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public WorkingStorageSection(WorkingStorageSectionHeader workingStorageSectionHeader, StorageEntries storageEntries)
            : base(CodeDomType.WorkingStorageSection, storageEntries)
        {
            this.WorkingStorageSectionHeader = workingStorageSectionHeader;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.WorkingStorageSectionHeader != null)
                yield return this.WorkingStorageSectionHeader;

            IEnumerator<CodeElement> baseEnum = base.GetEnumerator();
            while(baseEnum.MoveNext())
                yield return baseEnum.Current;
        }
    }

    /// <summary>
    /// LOCAL STORAGE SECTION
    /// </summary>
    public class LocalStorageSection : StorageSection
    {
        public LocalStorageSectionHeader LocalStorageSectionHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public LocalStorageSection() : base(CodeDomType.LocalStorageSection)
        {
        }

        /// <summary>
        /// Header constructor
        /// </summary>
        public LocalStorageSection(LocalStorageSectionHeader localStorageSectionHeader)
            : base(CodeDomType.LocalStorageSection)
        {
            this.LocalStorageSectionHeader = localStorageSectionHeader;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public LocalStorageSection(LocalStorageSectionHeader localStorageSectionHeader, StorageEntries storageEntries)
            : base(CodeDomType.LocalStorageSection, storageEntries)
        {
            this.LocalStorageSectionHeader = localStorageSectionHeader;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.LocalStorageSectionHeader != null)
                yield return this.LocalStorageSectionHeader;

            IEnumerator<CodeElement> baseEnum = base.GetEnumerator();
            while(baseEnum.MoveNext())
                yield return baseEnum.Current;
        }
    }

    /// <summary>
    /// LINKAGE SECTION
    /// </summary>
    public class LinkageSection : CodeElementGroup
    {
        public LinkageSectionHeader LinkageSectionHeader
        {
            get
            {
                return (LinkageSectionHeader)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        public DataDefinitionEntries DataDefinitionEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public LinkageSection() : base(CodeDomType.LinkageSection)
        {
        }

        /// <summary>
        /// Header constructor
        /// </summary>
        public LinkageSection(LinkageSectionHeader linkageSectionHeader)
            : base(CodeDomType.LinkageSection)
        {
            this.LinkageSectionHeader = linkageSectionHeader;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public LinkageSection(LinkageSectionHeader linkageSectionHeader, DataDefinitionEntries dataDefinitionEntries)
            : base(CodeDomType.LinkageSection)
        {
            this.LinkageSectionHeader = linkageSectionHeader;
            this.DataDefinitionEntries = dataDefinitionEntries;
        }

        public override void Accept<R, D>(Visitor.CodeDomVisitor<R, D> v, D data)
        {
            v.Visit(this, data);
        }

        public override IEnumerator<CodeElement> GetEnumerator()
        {
            if (this.LinkageSectionHeader != null)
                yield return this.LinkageSectionHeader;
            if (this.DataDefinitionEntries != null)
            {
                foreach (DataDefinitionEntry dde in this.DataDefinitionEntries)
                    yield return dde;
            }
        }
    }

}
