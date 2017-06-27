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
        /// Diagnostics encountered during Enter Phases.
        /// </summary>
        public List<Diagnostic> Diagnostics
        {
            get;
            private set;
        }

        /// <summary>
        /// Add an error diagnostic.
        /// </summary>
        /// <param name="diag"></param>
        public void AddDiagnostic(Diagnostic diag)
        {
            if (Diagnostics == null)
                Diagnostics = new List<Diagnostic>();
            Diagnostics.Add(diag);
        }

        /// <summary>
        /// A Dictionary mapping programs and namespaces to the context current at the points of their definitions.        
        /// </summary>
        Dictionary<TypeCobolSymbol, Context<TypeCobolSymbol>> TypeCtxs =
                new Dictionary<TypeCobolSymbol, Context<TypeCobolSymbol>>();

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
            TypeCobolSymbol owner = ctx.Info;
            IScope enclScope = EnterSymbolScope(ctx) as IScope;
            TypeCobolScope<FunctionSymbol> funScope = enclScope.Functions;
            FunctionSymbol f = null;
            if (enclScope == null)
            {//Hum.. the enclosing symbol is not a Scope ==> This is a fatal error.
                throw new System.ApplicationException(string.Format("Fatal Error No Enclosing Scope for Function '{0}'", that.Name));
            }
            if (funScope == null)
            {//Hum.. the enclosing symbol has no Function Scope ==> This is a fatal error.
                throw new System.ApplicationException(string.Format("Fatal Error No Enclosing Function Scope for Function '{0}'", that.Name));
            }
            if (funScope.Lookup(that.Name) != null)
            {
                string message = string.Format("Duplicate Function name '{0}' in symbol '{1}'.", that.Name, owner.Name);
                TypeCobol.Compiler.AntlrUtils.ParserDiagnostic diag = new TypeCobol.Compiler.AntlrUtils.ParserDiagnostic(message, that.StartIndex + 1, that.StopIndex + 1, that.ConsumedTokens[0].Line, null, MessageCode.SemanticTCErrorInParser);
                AddDiagnostic(diag);
            }
            f = new FunctionSymbol(that.Name);
            //Create a type for this function
            f.Type = new FunctionType(f);

            // The Semantic Data of the Program is the Symbol
            that.SemanticData = f;
            if (funScope != null && f != null)
                funScope.Enter(f);
            //Setup a context for Function body in TypeCtxs table,
            //to be retrieved later in StorageEnter and Checking phasses.
            Context<TypeCobolSymbol> funCtx = FunctionContext(that, ctx);
            TypeCtxs[f] = funCtx;
            return funCtx;
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
        /// Create a new context for a Program bodies
        /// 
        /// </summary>
        /// <param name="program">The program to create the new context</param>
        /// <param name="ctx">The context current outside the program definition</param>
        /// <returns>The new program's context</returns>
        public Context<TypeCobolSymbol> ProgramContext(CobolProgram program, Context<TypeCobolSymbol> ctx)
        {
            Context<TypeCobolSymbol> prgCtx = ctx.Dup(program, program.SemanticData as TypeCobolSymbol);
            prgCtx.EnclosingProgram = program;
            prgCtx.Outer = ctx;
            return prgCtx;
        }

        /// <summary>
        /// Create a new context for a Function bodies
        /// 
        /// </summary>
        /// <param name="fun">The function to create the new context</param>
        /// <param name="ctx">The context current outside the function definition</param>
        /// <returns>The new function's context</returns>
        public Context<TypeCobolSymbol> FunctionContext(FunctionDeclaration fun, Context<TypeCobolSymbol> ctx)
        {
            Context<TypeCobolSymbol> prgCtx = ctx.Dup(fun, fun.SemanticData as TypeCobolSymbol);
            prgCtx.EnclosingFunction = fun;
            prgCtx.Outer = ctx;
            return prgCtx;
        }

        /// <summary>
        /// The Symbol scope in which a member definition in the context is to be entered
        /// This is usually the context's symbol scope, except for Program contexts,
        /// where members go in the program member symbol scope.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        TypeCobolSymbol EnterSymbolScope(Context<TypeCobolSymbol> ctx)
        {
            if (ctx.Element.Type == (TypeCobol.Compiler.CodeElements.CodeElementType)CodeDomType.CobolProgram)
            {
                return ((ProgramSymbol)((CobolProgram)ctx.Element).SemanticData);
            }
            else
            {
                return ctx.Info;
            }
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
            IScope enclScope = EnterSymbolScope(ctx) as IScope;
            TypeCobolScope<ProgramSymbol> prgScope = enclScope.Programs;
            ProgramSymbol p = null;
            if (owner.Kind == TypeCobolSymbol.Kinds.Global || owner.Kind == TypeCobolSymbol.Kinds.Namespace)
            {//We are seeing a toplevel program.
                NamespaceSymbol ns = owner as NamespaceSymbol;
                if (ns.Programs.Lookup(that.Name) != null)
                {
                    string message = string.Format("Duplicate Program name{0}.", that.Name);
                    TypeCobol.Compiler.AntlrUtils.ParserDiagnostic diag = new TypeCobol.Compiler.AntlrUtils.ParserDiagnostic(message, that.StartIndex + 1, that.StopIndex + 1, that.ConsumedTokens[0].Line, null, MessageCode.SemanticTCErrorInParser);
                    AddDiagnostic(diag);
                }
                else
                {
                    p = new ProgramSymbol(that.Name);                    
                    //Create a type for this program
                    p.Type = new ProgramType(p);
                }
            }
            else
            {//This is a nested Program.
                if (enclScope == null)
                {//Hum.. the enclosing symbol is not a Scope ==> This is a fatal error.
                    throw new System.ApplicationException(string.Format("Fatal Error No Enclosing Scope for Program '{0}'", that.Name));
                }                
                if (prgScope == null)
                {//Hum.. the enclosing symbol has no Program Scope ==> This is a fatal error.
                    throw new System.ApplicationException(string.Format("Fatal Error No Enclosing Program Scope for Program '{0}'", that.Name));
                }
                if (prgScope.Lookup(that.Name) != null)
                {
                    string message = string.Format("Duplicate Program name '{0}' in symbol '{1}'.", that.Name, owner.Name);
                    TypeCobol.Compiler.AntlrUtils.ParserDiagnostic diag = new TypeCobol.Compiler.AntlrUtils.ParserDiagnostic(message, that.StartIndex + 1, that.StopIndex + 1, that.ConsumedTokens[0].Line, null, MessageCode.SemanticTCErrorInParser);
                    AddDiagnostic(diag);
                }
                p = new ProgramSymbol(that.Name);
                //Create a type for this program
                p.Type = new ProgramType(p);                
            }
            // The Semantic Data of the Program is the Symbol
            that.SemanticData = p;
            if (prgScope != null && p != null)
                prgScope.Enter(p);
            //Setup a context for Program body in TypeCtxs table,
            //to be retrieved later in StorageEnter and Checking phasses.
            Context<TypeCobolSymbol> prgCtx = ProgramContext(that, ctx);
            TypeCtxs[p] = prgCtx;

            //Visit NestedPrograms.
            if (that.NestedPrograms != null)
            {
                foreach (CobolProgram nested in that.NestedPrograms)
                {
                    nested.Accept(this, ctx);
                }
            }
            //Visit Procedure Division for functions.
            if (that.ProcedureDivision != null)
            {
                that.ProcedureDivision.Accept(this, ctx);
            }
            return prgCtx; 
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
