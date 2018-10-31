using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeCobol.Compiler.CodeModel;
using TypeCobol.Compiler.Nodes;
using TypeCobol.LanguageServer.JsonRPC;
using TypeCobol.LanguageServer.VsCodeProtocol;
using String = System.String;

namespace TypeCobol.LanguageServer.TypeCobolCustomLanguageServerProtocol
{
    class TypeCobolCustomLanguageServer : VsCodeProtocol.LanguageServer
    {
        public TypeCobolCustomLanguageServer(IRPCServer rpcServer) : base(rpcServer)
        {
            RemoteConsole = new LanguageServer.TypeCobolCustomLanguageServerProtocol.TypeCobolRemoteConsole(rpcServer);
            rpcServer.RegisterNotificationMethod(MissingCopiesNotification.Type, CallReceiveMissingCopies);
            rpcServer.RegisterNotificationMethod(NodeRefreshNotification.Type, ReceivedRefreshNodeDemand);
            rpcServer.RegisterRequestMethod(NodeRefreshRequest.Type, ReceivedRefreshNodeRequest);
            rpcServer.RegisterNotificationMethod(SignatureHelpContextNotification.Type, ReceivedSignatureHelpContext);
            rpcServer.RegisterRequestMethod(OutlineDataRequest.Type, ReceivedOutlineDataRequest);
        }

        private void CallReceiveMissingCopies(NotificationType notificationType, object parameters)
        {
            try
            {
                OnDidReceiveMissingCopies((MissingCopiesParams)parameters);
            }
            catch (Exception e)
            {
                RemoteConsole.Error(String.Format("Error while handling notification {0} : {1}", notificationType.Method, e.Message));
            }
        }

        private void ReceivedRefreshNodeDemand(NotificationType notificationType, object parameters)
        {
            try
            {
                OnDidReceiveNodeRefresh((NodeRefreshParams) parameters);
            }
            catch (Exception e)
            {
                this.NotifyException(e);
            }
        }

        private ResponseResultOrError ReceivedRefreshNodeRequest(RequestType requestType, object parameters)
        {
            ResponseResultOrError resultOrError = null;
            try
            {
                OnDidReceiveNodeRefresh((NodeRefreshParams)parameters);
                resultOrError = new ResponseResultOrError() { result = true };
            }
            catch (Exception e)
            {
                NotifyException(e);
                resultOrError = new ResponseResultOrError() { code = ErrorCodes.InternalError, message = e.Message};
            }
            return resultOrError;
        }

        private ResponseResultOrError ReceivedOutlineDataRequest(RequestType requestType, object parameters)
        {
            ResponseResultOrError resultOrError = null;
            try
            {
                OutlineData result = OnDidReceiveOutlineData((TextDocumentIdentifier)parameters);
                resultOrError = new ResponseResultOrError() { result = result };
            }
            catch (Exception e)
            {
                NotifyException(e);
                resultOrError = new ResponseResultOrError() { code = ErrorCodes.InternalError, message = e.Message };
            }
            return resultOrError;
        }

        private void ReceivedSignatureHelpContext(NotificationType notificationType, object parameters)
        {
            try
            {
                OnDidReceiveSignatureHelpContext((SignatureHelpContextParams)parameters);
            }
            catch (Exception e)
            {
                this.NotifyException(e);
            }
        }

        /// <summary>
        /// The Missing copies notification is sent from the client to the server
        /// when the client failled to load copies, it send back a list of missing copies to the server.
        /// </summary>
        public virtual void OnDidReceiveMissingCopies(MissingCopiesParams parameter)
        {
            //Nothing to do for now, maybe add some telemetry here...
        }

        /// <summary>
        /// The Node Refresh notification is sent from the client to the server 
        /// It will force the server to do a Node Phase analyze. 
        /// </summary>
        /// <param name="parameter"></param>
        public virtual void OnDidReceiveNodeRefresh(NodeRefreshParams parameter)
        {
            
        }

        /// <summary>
        /// The Outline Data notification is sent from the client to the server 
        /// </summary>
        /// <param name="parameter"></param>
        public virtual OutlineData OnDidReceiveOutlineData(TextDocumentIdentifier parameter)
        {
            if (parameter == null) return null;
            // --- Node
            // Get the text document as well as the node
            var fileCompiler = TypeCobolServer.GetFileCompilerFromStringUri(parameter.uri);
            if (!fileCompiler.CompilationResultsForProgram.CodeElementsDocumentSnapshot.CodeElements.Any())
                return null;
            var codeElement =
                fileCompiler.CompilationResultsForProgram.CodeElementsDocumentSnapshot.CodeElements.First();
            var node = CompletionFactory.GetMatchingNode(fileCompiler, codeElement);

            if (node == null) return null;

            // --- OutlineData
            // Structure the JSON needed to be send back to the plugin in a OutlineData format
            var data = new List<OutlineData.Node>();

            // PROGRAM 
            var program = new OutlineData.Node
            {
                id = Guid.NewGuid().ToString(),
                name = node.Name
            };
            program.parent = program.id;
            program.line = node.CodeElement.Line.ToString();
            var programChildNodes = new List<OutlineData.Node>();
            var divisions = node.GetChildren<Node>();

            // --- DATA DIVISION
            var dataDivisionNode = divisions.ToList().Find(item => item.ID.Equals("data-division"));
            if (dataDivisionNode != null)
            {
                var dataDivisionNodeChildNodes = new List<OutlineData.Node>();
                var dataDivision = new OutlineData.Node
                {
                    id = Guid.NewGuid().ToString(),
                    name = dataDivisionNode.ID.ToUpper(),
                    parent = program.id,
                    line = dataDivisionNode.CodeElement.Line.ToString()
                };
                
                // ------ SECTION
                var dataDivisionNodeSections = dataDivisionNode.GetChildren<Node>();
                foreach (var dataDivisionNodeSection in dataDivisionNodeSections)
                {
                    if (dataDivisionNodeSection.ID.Equals("working-storage") || dataDivisionNodeSection.ID.Equals("linkage"))
                    {
                        var sectionNode = new OutlineData.Node
                        {
                            id = Guid.NewGuid().ToString(),
                            name = dataDivisionNodeSection.ID.ToUpper(),
                            parent = dataDivision.id,
                            line = dataDivisionNodeSection.CodeElement.Line.ToString()
                        };

                        // --------- WORKING STORAGE
                        if (dataDivisionNodeSection.ID.Equals("working-storage"))
                        {
                            var declarations = dataDivisionNodeSection.GetChildren<DataDefinition>();
                            var workingStorageChildNodes = new List<OutlineData.Node>();
                            foreach (var declaration in declarations)
                            {
                                if (declaration.IsPartOfATypeDef)
                                {
                                    var declarationNode = new OutlineData.Node
                                    {
                                        id = Guid.NewGuid().ToString(),
                                        name = declaration.Name + ": TYPEDEF",
                                        parent = sectionNode.id,
                                        line = declaration.CodeElement.Line.ToString()
                                    };
                                    workingStorageChildNodes.Add(declarationNode);
                                }
                            }

                            sectionNode.childNodes = workingStorageChildNodes.ToArray();
                        }

                        dataDivisionNodeChildNodes.Add(sectionNode);
                    }
                }

                dataDivision.childNodes = dataDivisionNodeChildNodes.ToArray();
                programChildNodes.Add(dataDivision);
            }

            var procedureDivisionNode = divisions.ToList().Find(item => item.ID.Equals("procedure-division"));
            if (procedureDivisionNode != null)
            {
                // --- PROCEDURE DIVISION
                var procedureDivision = new OutlineData.Node
                {
                    id = Guid.NewGuid().ToString(),
                    name = procedureDivisionNode.ID.ToUpper(),
                    parent = program.id,
                    line = procedureDivisionNode.CodeElement.Line.ToString(),
                };

                // Get the functions in the nodeFile
                // Get the sections in each function
                // Add the sections to each function and functions to the PROCEDURE DIVISION
                var proceduresDivisionList = new List<OutlineData.Node>();
                var documentFunctions = node.SymbolTable.EnclosingScope.Functions;
                foreach (var documentFunction in documentFunctions)
                {
                    // foreach function with a different signature
                    foreach (var documentFunctionSignature in documentFunction.Value)
                    {
                        // construct the name of the function showed from his name, input, inout and output parameters
                        var functionName = new StringBuilder();
                        functionName.Append(documentFunctionSignature.Name);

                        if (documentFunctionSignature.Profile.InputParameters.Count > 0 ||
                            documentFunctionSignature.Profile.InoutParameters.Count > 0)
                        {
                            functionName.Append("(");
                            foreach (var inputParameter in documentFunctionSignature.Profile.InputParameters)
                            {
                                var inputType = inputParameter.DataType.Name == "?" ? inputParameter.Usage.ToString() : inputParameter.DataType.Name;

                                if (inputParameter == documentFunctionSignature.Profile.InputParameters.First())
                                {
                                    functionName.Append("INPUT: " + inputType);
                                    if (documentFunctionSignature.Profile.InputParameters.Count > 1)
                                        functionName.Append(", ");
                                    if (documentFunctionSignature.Profile.InoutParameters.Count > 0)
                                        functionName.Append(" ");
                                    continue;
                                }

                                if (inputParameter != documentFunctionSignature.Profile.InputParameters.Last())
                                {
                                    functionName.Append(inputType + ", ");
                                    continue;
                                }

                                functionName.Append(inputType);

                                if (documentFunctionSignature.Profile.InoutParameters.Count > 0)
                                    functionName.Append(" ");
                            }

                            foreach (var inoutParameter in documentFunctionSignature.Profile.InoutParameters)
                            {
                                var inoutType = inoutParameter.DataType.Name == "?" ? inoutParameter.Usage.ToString() : inoutParameter.DataType.Name;

                                if (inoutParameter == documentFunctionSignature.Profile.InoutParameters.First())
                                {
                                    functionName.Append("INOUT: " + inoutType);
                                    if (documentFunctionSignature.Profile.InoutParameters.Count > 1)
                                        functionName.Append(", ");
                                    continue;
                                }

                                if (inoutParameter != documentFunctionSignature.Profile.InoutParameters.Last())
                                {
                                    functionName.Append(inoutType + ", ");
                                    continue;
                                }

                                functionName.Append(inoutType);
                            }
                            functionName.Append(")");
                        }

                        if (documentFunctionSignature.Profile.OutputParameters.Count > 0)
                        {
                            functionName.Append(": ");
                            foreach (var outputParameter in documentFunctionSignature.Profile.OutputParameters)
                            {
                                var outputType = outputParameter.DataType.Name == "?" ? outputParameter.Usage.ToString() : outputParameter.DataType.Name;

                                if (outputParameter == documentFunctionSignature.Profile.OutputParameters.Last())
                                {
                                    functionName.Append(outputType);
                                }
                                else 
                                {
                                    functionName.Append(outputType + ", ");
                                }
                            }
                        }

                        // create a new function node 
                        var function = new OutlineData.Node
                        {
                            id = Guid.NewGuid().ToString(),
                            name = functionName.ToString(),
                            parent = procedureDivision.id,
                            line = documentFunctionSignature.CodeElement.Line.ToString(),
                            lineEnd = documentFunctionSignature.SelfAndChildrenLines.ToList().Find(item => item.Text.TrimStart().Equals("end-declare."))?.LineIndex.ToString()
                        };

                        // foreach section of the function
                        var documentFunctionSignatureSections = documentFunctionSignature.Children.First().SelfAndChildrenLines;
                        var documentFunctionSignatureChildrensNodes = new List<OutlineData.Node>();
                        foreach (var documentFunctionSignatureSection in documentFunctionSignatureSections)
                        {
                            // verify the section is a section then create a OutlineData.Node for each of them
                            if (documentFunctionSignatureSection.Text.EndsWith("section."))
                            {
                                documentFunctionSignatureChildrensNodes.Add(new OutlineData.Node
                                {
                                    id = Guid.NewGuid().ToString(),
                                    name = documentFunctionSignatureSection.Text.TrimStart(),
                                    parent = function.id,
                                    line = (documentFunctionSignatureSection.LineIndex + 1).ToString()
                                });
                            }
                        }
                        // Add the section node list to the function childNodes
                        function.childNodes = documentFunctionSignatureChildrensNodes.ToArray();
                        // Add the function to the list of PROCEDURE DIVISION
                        proceduresDivisionList.Add(function);
                    }
                }
                procedureDivision.childNodes = proceduresDivisionList.ToArray();
                programChildNodes.Add(procedureDivision);
            }
            

            // Add the nodes to the Program and add the program to the data NodeList
            program.childNodes = programChildNodes.ToArray();
            data.Add(program);
            return new OutlineData(data.ToArray());

        }

        public virtual void OnDidReceiveSignatureHelpContext(SignatureHelpContextParams procedureHash)
        {
            
        }

        /// <summary>
        /// Missing copies notifications are sent from the server to the client to signal
        /// that some copies where missing during TypeCobol parsing.
        /// </summary>
        public virtual void SendMissingCopies(MissingCopiesParams parameters)
        {
            this.rpcServer.SendNotification(MissingCopiesNotification.Type, parameters);
        }

        /// <summary>
        /// Loading Issue notification is sent from the server to the client
        /// Usefull to let the client knows that a error occured while trying to load Intrinsic/Dependencies. 
        /// </summary>
        public virtual void SendLoadingIssue(LoadingIssueParams parameters)
        {
            this.rpcServer.SendNotification(LoadingIssueNotification.Type, parameters);
        }
    }
}
