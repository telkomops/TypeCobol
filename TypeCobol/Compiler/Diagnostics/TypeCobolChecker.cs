﻿using System;
using Antlr4.Runtime;
using System.Collections.Generic;
using TypeCobol.Compiler.AntlrUtils;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.Compiler.CodeElements.Expressions;
using TypeCobol.Compiler.CodeModel;
using TypeCobol.Compiler.Parser;
using TypeCobol.Compiler.Parser.Generated;
using TypeCobol.Compiler.CodeElements.Functions;

namespace TypeCobol.Compiler.Diagnostics {


	class ReadOnlyPropertiesChecker: NodeListener {

		private static string[] READONLY_DATATYPES = { "DATE", };

		public IList<Type> GetCodeElements() {
			return new List<Type> { typeof(TypeCobol.Compiler.CodeModel.SymbolWriter), };
		}
		public void OnNode(Node node, ParserRuleContext c, Program program) {
			var element = node.CodeElement as TypeCobol.Compiler.CodeModel.SymbolWriter;
			var table = program.SymbolTable;
			foreach (var pair in element.Symbols) {
				if (pair.Item2 == null) continue; // no receiving item
				var lr = table.Get(pair.Item2);
				if (lr.Count != 1) continue; // ambiguity or not referenced; not my job
				var receiving = lr[0];
				checkReadOnly(node.CodeElement, receiving);
			}
		}

		private static void checkReadOnly(CodeElement e, DataDescriptionEntry receiving) {
			if (receiving.TopLevel == null) return;
		    foreach (var type in READONLY_DATATYPES) {
				if (type.Equals(receiving.TopLevel.DataType.Name.ToUpper())) {
					DiagnosticUtils.AddError(e, type + " properties are read-only");
				}
			}
		}
	}



	class FunctionChecker: NodeListener {
		public IList<Type> GetCodeElements() {
			return new List<Type> { typeof(TypeCobol.Compiler.CodeModel.IdentifierUser), };
		}

		public void OnNode(Node node, ParserRuleContext context, Program program) {
			var element = node.CodeElement as TypeCobol.Compiler.CodeModel.IdentifierUser;
			foreach (var identifier in element.Identifiers) {
				CheckIdentifier(node.CodeElement, program.SymbolTable, identifier);
			}
		}

		private static void CheckIdentifier(CodeElement e, SymbolTable table, Identifier identifier) {
			var fun = identifier as FunctionReference;
			if (fun == null) return;// we only check functions
			var def = table.GetFunction(fun.Name);
			if (def == null) return;// ambiguity is not our job
			if (fun.Parameters.Count > def.InputParameters.Count) {
				var message = String.Format("Function {0} only takes {1} parameters", def.Name, def.InputParameters.Count);
				DiagnosticUtils.AddError(e, message);
			}
			for (int c = 0; c < def.InputParameters.Count; c++) {
				var expected = def.InputParameters[c];
				if (c < fun.Parameters.Count) {
					var actual = fun.Parameters[c].Value;
					if (actual is Identifier) {
						var found = table.Get(((Identifier)actual).Name);
						if (found.Count != 1) continue;// ambiguity is not our job
						var type = found[0].DataType;
						// type check. please note:
						// 1- if only one of [actual|expected] types is null, overriden DataType.!= operator will detect of it
						// 2- if both are null, wee WANT it to break: in TypeCobol EVERYTHING should be typed,
						//    and things we cannot know their type as typed as DataType.Unknown (which is a non-null valid type).
						if (type == null || type != expected.Type) {
							var message = String.Format("Function {0} expected parameter {1} of type {2} (actual: {3})", def.Name, c+1, expected.Type, type);
							DiagnosticUtils.AddError(e, message);
						}
						var length = found[0].MemoryArea.Length;
						if (length > expected.Length) {
							var message = String.Format("Function {0} expected parameter {1} of max length {2} (actual: {3})", def.Name, c+1, expected.Length, length);
							DiagnosticUtils.AddError(e, message);
						}
					}
				} else {
					var message = String.Format("Function {0} is missing parameter {1} of type {2}", def.Name, c+1, expected.Type);
					DiagnosticUtils.AddError(e, message);
				}
			}
		}
	}



	class FunctionDeclarationChecker: NodeListener {

		public IList<Type> GetCodeElements() {
			return new List<Type> { typeof(FunctionDeclarationHeader), };
		}
		public void OnNode(Node node, ParserRuleContext context, Program program) {
			var header = node.CodeElement as FunctionDeclarationHeader;
			var visibility = header.Visibility;
			IList<InputParameter> inputs = new List<InputParameter>();
			IList<DataName> outputs = new List<DataName>();
			IList<DataName> inouts = new List<DataName>();
			DataName returning = null;
			Node profile = null;
			var profiles = node.GetChildren(typeof(FunctionDeclarationProfile));
			if (profiles.Count < 1) // no PROCEDURE DIVISION internal to function
				DiagnosticUtils.AddError(header, "Function \""+header.Name+"\" has no parameters and does nothing.");
			else if (profiles.Count > 1)
				foreach(var p in profiles)
					DiagnosticUtils.AddError(p.CodeElement, "Function \""+header.Name+"\" can have only one parameters profile.");
			else profile = profiles[0];
			if (profile != null) {
				var p = ((FunctionDeclarationProfile)profile.CodeElement);
				inputs  = p.InputParameters;
				outputs = p.OutputParameters;
				inouts  = p.InoutParameters;
				returning = p.ReturningParameter;
			}
			var filesection = node.Get("file");
			if (filesection != null) // TCRFUN_DECLARATION_NO_FILE_SECTION
				DiagnosticUtils.AddError(filesection.CodeElement, "Illegal FILE SECTION in function declaration", context);

			var parametersdeclared = new List<Parameter>();
			var linkage = node.Get("linkage");
			if (linkage == null) {
				if (inputs.Count > 0 || outputs.Count > 0 || inouts.Count > 0)
					DiagnosticUtils.AddError(header, "Missing LINKAGE SECTION for parameters declaration.");
			} else {
			    var data = linkage.GetChildren(typeof(DataDescriptionEntry));
				foreach(var n in data) {
					var d = (DataDescriptionEntry)n.CodeElement;
					bool custom = false;//TODO
					parametersdeclared.Add(new Parameter(d.Name.Name, custom, d.DataType, d.MemoryArea.Length));
				}
			}
			var inparameters = new List<Parameter>();
			foreach(var p in inputs) CheckParameter(p.DataName.Name, parametersdeclared, inparameters, profile.CodeElement);
			var outparameters = new List<Parameter>();
			foreach(var p in outputs) CheckParameter(p.Name, parametersdeclared, outparameters, profile.CodeElement);
			var ioparameters = new List<Parameter>();
			foreach(var p in inouts) CheckParameter(p.Name, parametersdeclared, ioparameters, profile.CodeElement);
			Parameter preturning = null;
			if (returning != null) preturning = CheckParameter(returning.Name, parametersdeclared, profile.CodeElement);
			foreach(var pd in parametersdeclared) {
				var used = Validate(preturning, pd.Name);
				if (used == null) used = GetParameter(inparameters,  pd.Name);
				if (used == null) used = GetParameter(outparameters, pd.Name);
				if (used == null) used = GetParameter(ioparameters,  pd.Name);
				if (used == null) {
					var data = GetParameter(linkage, pd.Name);
					DiagnosticUtils.AddError(data, pd.Name+" is not a parameter.");
				}
			}
			var function = new Function(header.Name, inparameters, outparameters, ioparameters, preturning, visibility);
			if (!function.IsProcedure && !function.IsFunction)
				DiagnosticUtils.AddError(profile.CodeElement, header.Name+" is neither procedure nor function.", context);
			node.SymbolTable.EnclosingScope.Register(function);
		}
		private Parameter CheckParameter(string pname, IList<Parameter> declared, CodeElement ce) {
			var parameter = GetParameter(declared, pname);
			if (parameter == null) DiagnosticUtils.AddError(ce, pname+" undeclared in LINKAGE SECTION.");
			return parameter;
		}
		private void CheckParameter(string pname, IList<Parameter> declared, IList<Parameter> parameters, CodeElement ce) {
			var parameter = GetParameter(declared, pname);
			if (parameter != null) parameters.Add(parameter);
			else DiagnosticUtils.AddError(ce, pname+" undeclared in LINKAGE SECTION.");
		}
		private Parameter GetParameter(IList<Parameter> parameters, string name) {
			foreach(var p in parameters)
				if (Validate(p, name) != null) return p;
			return null;
		}
		private Parameter Validate(Parameter parameter, string name) {
			if (parameter != null && parameter.Name.Equals(name)) return parameter;
			return null;
		}
		private DataDescriptionEntry GetParameter(Node node, string name) {
			var data = node.CodeElement as DataDescriptionEntry;
			if (data != null && data.QualifiedName.Matches(name)) return data;
			foreach(var child in node.Children) {
				var found = GetParameter(child, name);
				if (found != null) return found;
			}
			return null;
		}
	}



	/// <summary>Checks the TypeCobol custom functions rule: TCRFUN_NO_SECTION_OR_PARAGRAPH_IN_LIBRARY.</summary>
	class LibraryChecker: NodeListener {
		public IList<Type> GetCodeElements() {
			return new List<Type> { typeof(ProcedureDivisionHeader), };
		}
		public void OnNode(Node node, ParserRuleContext context, Program program) {
			var pdiv = node.CodeElement as ProcedureDivisionHeader;
			bool isPublicLibrary = false;
			var elementsInError = new List<CodeElement>();
			var errorMessages = new List<string>();
			foreach(var child in node.Children) {
				var ce = child.CodeElement;
				if (child.CodeElement == null) {
					elementsInError.Add(node.CodeElement);
					errorMessages.Add("Illegal default section in library.");
				} else {
					var function = child.CodeElement as FunctionDeclarationHeader;
					if (function != null) {
						isPublicLibrary = isPublicLibrary || function.Visibility == AccessModifier.Public;
					} else {
						elementsInError.Add(child.CodeElement);
						errorMessages.Add("Illegal non-function item in library");
					}
				}
			}
			if (isPublicLibrary) {
				for(int c = 0; c < errorMessages.Count; c++)
					DiagnosticUtils.AddError(elementsInError[c], errorMessages[c], context);
			}
		}
	}


}