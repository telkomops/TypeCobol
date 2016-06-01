﻿using System.Collections.Generic;
using TypeCobol.Codegen.Nodes;
using TypeCobol.Compiler.CodeElements;
using TypeCobol.Compiler.Text;

namespace TypeCobol.Codegen {

	public class TreeToCode: NodeVisitor {

		/// <summary>Input source code</summary>
		public readonly IList<ICobolTextLine> Input;
		/// <summary>Columns layout of the generated text</summary>
		private ColumnsLayout Layout;
		/// <summary>Generated code is written here</summary>
		public System.IO.StringWriter Output;
		/// <summary>Index in Input of the next line to write</summary>
		private int offset = 0;

		public TreeToCode(IEnumerable<ICobolTextLine> source = null, ColumnsLayout layout = ColumnsLayout.FreeTextFormat) {
			if (source == null) Input = new List<ICobolTextLine>();
			else Input = new List<ICobolTextLine>(source);
			Output = new System.IO.StringWriter();
			this.Layout = layout;
		}

		public void Visit(Node node) {
			bool doVisitChildren = Process(node);
			if (doVisitChildren) foreach(var child in node.Children) child.Accept(this);
		}

		private bool Process(Node node) {
			string text = "";
			var generated = node as Generated;
			foreach(var line in node.Lines) {
				if (generated != null)
					// if we write generated code, we INSERT one line of code between Input lines;
					// thus, we must decrease offset as it'll be re-increased by Write(line) and
					// we don't want to fuck up next iteration
					offset--;
				else
					// before we copy an original line of code, we must still write non-source
					// lines (eg. comments or empty lines) so they are preserved in Output
					WriteInputLinesUpTo(line);
				Write(line);
			}
			return generated == null || ((Generated)node).IsLeaf;
		}

		/// <summary>
		/// Write all lines between the last written line (ie. Input[offset-1]) and a given line.
		/// If line is not contained in Input, or if line is contained in Input but before offset,
		///	all remaining Input will be written. In other words: don't fall in one of these cases.
		/// </summary>
		/// <param name="line"></param>
		/// <returns>Number of lines written during this method call.</returns>
		private int WriteInputLinesUpTo(ITextLine line) {
			int lines = 0;
			while (offset < Input.Count) {
				if (Input[offset] == line) break;
				Write(Input[offset]);
				lines++;
			}
			return lines;
		}

		private void Write(ITextLine line) {
			var lines = Indent(line);
			//TODO: format what is written to free format or 80 columns
			foreach(var l in lines) Output.WriteLine(l.Text);
			offset++;
		}

		private IEnumerable<ITextLine> Indent(ITextLine line) {
			var results = new List<ITextLine>();
			var cobol = line as CobolTextLine;
			if (cobol != null) {
				if (Layout == ColumnsLayout.CobolReferenceFormat) {
					results.Add(line);
				} else
				if (Layout == ColumnsLayout.FreeTextFormat) {
					results.Add(new TextLineSnapshot(-1, cobol.SourceText ?? "", null));
				} else
					throw new System.NotImplementedException("Unsuported columns layout: "+Layout);
			} else {
				if (Layout == ColumnsLayout.CobolReferenceFormat) {
					bool isComment = line.Text.Trim().StartsWith("*");
					var lines = CobolTextLine.Create(line.Text, isComment, Layout, line.InitialLineIndex);
					foreach(var l in lines) results.Add(l);
				} else
				if (Layout == ColumnsLayout.FreeTextFormat) {
					results.Add(line);
				} else
					throw new System.NotImplementedException("Unsuported columns layout: "+Layout);
			}
			if (results.Count < 1)
				throw new System.NotImplementedException("Unsuported ITextLine type: "+line.GetType());
			return results;
		}
	}

}
