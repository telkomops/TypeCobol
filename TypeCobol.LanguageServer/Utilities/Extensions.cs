using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeCobol.LanguageServer.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method to get the TypeCobol.ExecutionStep value corresponding to a Workspace.LsrTestingOption value.
        /// </summary>
        /// <param name="lsrOptions">The lsr testing option value</param>
        /// <param name="defaultValue">The default TypeCobol.ExecutionStep instance value</param>
        /// <returns>The corresponding TypeCobol.ExecutionStep if any, null otherwise</returns>
        public static TypeCobol.ExecutionStep? ExecutionStep(this Workspace.LsrTestingOptions lsrOptions, TypeCobol.ExecutionStep defaultValue)
        {
            switch (lsrOptions)
            {
                case Workspace.LsrTestingOptions.NoLsrTesting:
                    return defaultValue;
                case Workspace.LsrTestingOptions.LsrSourceDocumentTesting:
                    return null;
                case Workspace.LsrTestingOptions.LsrScanningPhaseTesting:
                    return TypeCobol.ExecutionStep.Scanner;
                case Workspace.LsrTestingOptions.LsrPreprocessingPhaseTesting:
                    return TypeCobol.ExecutionStep.Preprocessor;
                case Workspace.LsrTestingOptions.LsrParsingPhaseTesting:
                    return TypeCobol.ExecutionStep.SyntaxCheck;
                case Workspace.LsrTestingOptions.LsrSemanticPhaseTesting:
                    return TypeCobol.ExecutionStep.CrossCheck;
            }
            return defaultValue;
        }
    }
}
