using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.LanguageServer.JsonRPC;
using TypeCobol.LanguageServer.VsCodeProtocol;

namespace TypeCobol.LanguageServer.TypeCobolCustomLanguageServerProtocol
{
    public static class OutlineDataRequest
    {
        public static readonly RequestType Type = new RequestType("typecobol/outlineDataRequest", typeof(TextDocumentIdentifier), typeof(OutlineData), null);
    }
}
