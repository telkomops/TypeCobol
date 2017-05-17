using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom
{
    /// <summary>
    /// Enumeration on TyepCobol CodeDom Object
    /// </summary>
    public enum CodeDomType
    {
        CobolProgram = CodeElementType.CodeElementTypeCount,
            ProgramAttributes,
            EnvironmentDivision,
                ConfigurationSection,
                    ConfigurationParagraph,
                InputOutputSection,
                    FileControlParagraph,
                    IoControlParagraph,
    }
}
