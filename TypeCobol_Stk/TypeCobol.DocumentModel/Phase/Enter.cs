using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.Diagnostics;
using TypeCobol.DocumentModel.Code.Scopes;
using TypeCobol.DocumentModel.Code.Symbols;
using TypeCobol.DocumentModel.Code.Types;
using TypeCobol.DocumentModel.Dom;
using TypeCobol.DocumentModel.Dom.Visitor;

namespace TypeCobol.DocumentModel.Phase
{
    /// <summary>
    /// This is the Enter Phase, it consist of registering symbols for all definitions
    /// that were found into thei enclosing scope.
    /// 1) All Programs and functions are registered with the corresponding scope.
    /// 2) Programs and function storage areas are visited for creating data symbols.
    /// </summary>
    public class Enter : CodeDomVisitor<Context<TypeCobolSymbol>, Context<TypeCobolSymbol>>
    {
        /// <summary>
        /// Main method: enter all programs in a list of tolevel program elements.
        /// </summary>
        /// <param name="programs">The list of program to pbe processed</param>
        /// <param name="global">The global Symbol Table</param>
        public void Main(List<CobolProgram> programs, GlobalSymbolTable global)
        {
            Complete(programs, global);
        }

        /// <summary>
        /// Visdit a Function declaration.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public virtual Context<TypeCobolSymbol> Visit(FunctionDeclaration that, Context<TypeCobolSymbol> ctx) 
        { 
            //return visitCodeElement(that, data); 
            return null;
        }

        /// <summary>
        /// Visita Procedure division
        /// </summary>
        /// <param name="that"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public virtual Context<TypeCobolSymbol> Visit(ProcedureDivision that, Context<TypeCobolSymbol> ctx) 
        {
            if (that.Elements != null)
            {
                foreach (var element in that.Elements)
                {
                    if (element is ProcedureDivisionElement.Function)
                    {
                        ProcedureDivisionElement.Function fun = element as ProcedureDivisionElement.Function;
                        fun.Accept(this, ctx);
                    }
                }
            }
            return null; 
        }

        /// <summary>
        /// Visit a Cobol Program.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public virtual Context<TypeCobolSymbol> Visit(CobolProgram that, Context<TypeCobolSymbol> ctx) 
        {
            TypeCobolSymbol owner = ctx.Info;
            ProgramSymbol p;
            if (owner.Kind == TypeCobolSymbol.Kinds.Global || owner.Kind == TypeCobolSymbol.Kinds.Namespace)
            {//We are seeing a toplevel program.
                NamespaceSymbol ns = owner as NamespaceSymbol;
                if (ns.Programs.Lookup(that.Name) != null)
                {
                    string message = string.Format("Duplicate Program name{0}.", that.Name);
                    TypeCobol.Compiler.Parser.DiagnosticUtils.AddError(that, message, MessageCode.SemanticTCErrorInParser);
                }
                else
                {
                    p = ns.EnterProgram(that.Name);
                }
            }
            //Visit NestedPrograms.
            if (that.NestedPrograms != null)
            {
                foreach (CobolProgram nested in that.NestedPrograms)
                {
                    nested.Accept(this, ctx);
                }
            }
            else
            {
            }
            //Visit Procedure Division for functions.
            that.ProcedureDivision.Accept(this, ctx);
            return visitCodeElement(that, ctx); 
        }

        /// <summary>
        /// Visitor  method: enter all Programs and Functions in the given tree
        /// </summary>
        /// <param name="program">The program to visit</param>
        /// <param name="ctx">The context visitor argument</param>
        /// <returns></returns>
        TypeCobolType ProgramEnter(CobolProgram program, Context<TypeCobolSymbol> ctx)
        {
            program.Accept(this, ctx);
            if (program.SemanticData != null)
            {
                switch (program.SemanticData.SemanticKind)
                {
                    case Compiler.CodeElements.SemanticKinds.Symbol:
                        return ((ProgramSymbol)program.SemanticData).Type;
                    case Compiler.CodeElements.SemanticKinds.Type:
                        return (TypeCobolType)program.SemanticData;
                }
            }
            return null;
        }

        /// <summary>
        /// Visitor method: entre programs of al ist of programs, returning a list of types.
        /// </summary>
        /// <param name="programs"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        List<TypeCobolType> ProgramEnter(List<CobolProgram> programs, Context<TypeCobolSymbol> ctx)
        {
            List<TypeCobolType> types = new List<TypeCobolType>();
            foreach (CobolProgram program in programs)
            {
                TypeCobolType type = ProgramEnter(program, ctx);
                if (type != null)
                    types.Add(type);
            }
            return types;
        }

        /// <summary>
        /// Main method: enter
        /// </summary>
        /// <param name="programs"></param>
        /// <param name="global">The global Symbol Table</param>
        public void Complete(List<CobolProgram> programs, GlobalSymbolTable global)
        {
            Context<TypeCobolSymbol> ctx = new Context<TypeCobolSymbol>(null, global);
            List<TypeCobolType> types = ProgramEnter(programs, ctx);
        }
    }
}
