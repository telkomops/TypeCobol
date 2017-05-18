using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.Compiler.CodeElements;

namespace TypeCobol.DocumentModel.Dom.Visitor
{
    /// <summary>
    /// An abstract CodeDom Visitor
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="D"></typeparam>
    public abstract class CodeDomVisitor<R, D> : TypeCobol.Compiler.CodeElements.ICodeElementVisitor<R, D>
    {
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DataDescriptionEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DataRedefinesEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DataRenamesEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DataConditionEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.FileControlEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.FileDescriptionEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.RerunIOControlEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SameAreaIOControlEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MultipleFileTapeIOControlEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ApplyWriteOnlyIOControlEntry that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ConfigurationSectionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DataDivisionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.FileSectionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.WorkingStorageSectionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.LocalStorageSectionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.LinkageSectionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DeclarativesEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DeclarativesHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.EnvironmentDivisionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.FileControlParagraphHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.FunctionDeclarationHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.FunctionDeclarationEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.InputOutputSectionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.IOControlParagraphHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ParagraphHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ProcedureDivisionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SectionHeader that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SentenceEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.UseAfterIOExceptionStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.UseForDebuggingProcedureStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ClassEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ClassIdentification that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.FactoryEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.FactoryIdentification that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MethodEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MethodIdentification that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ObjectEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ObjectIdentification that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ProgramEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ProgramIdentification that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.LibraryCopyCodeElement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ObjectComputerParagraph that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.RepositoryParagraph that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SourceComputerParagraph that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SpecialNamesParagraph that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AtEndCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AtEndOfPageCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.InvalidKeyCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.NotAtEndCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.NotAtEndOfPageCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.NotInvalidKeyCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.NotOnExceptionCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.NotOnOverflowCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.NotOnSizeErrorCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.OnExceptionCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.OnOverflowCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.OnSizeErrorCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AddStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.CallStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ComputeStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DeleteStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DivideStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.EvaluateStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.InvokeStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MultiplyStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.PerformStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ReadStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ReturnStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.RewriteStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SearchStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.StartStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.StringStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SubtractStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.UnstringStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.WriteStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.XmlStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AcceptFromInputDeviceStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AcceptFromSystemDateStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AddSimpleStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AddGivingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AddCorrespondingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.AlterStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.CallStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ProcedureStyleCallStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.CancelStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.CloseStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ComputeStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ContinueStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DeleteStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DisplayStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DivideSimpleStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DivideGivingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.DivideRemainderStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.EntryStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.EvaluateStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.WhenCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.WhenOtherCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ExecStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ExitMethodStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ExitProgramStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ExitStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.GobackStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.GotoSimpleStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.GotoConditionalStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.IfStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ElseCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.NextSentenceStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.IfStatementEnd that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.InitializeStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.InspectTallyingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.InspectConvertingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.InvokeStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MergeStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MoveSimpleStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MoveCorrespondingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MultiplySimpleStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.MultiplyGivingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.OpenStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.PerformStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ReadStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ReleaseStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.ReturnStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.RewriteStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SearchSerialStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SearchBinaryStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.WhenSearchCondition that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SetStatementForAssignment that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SetStatementForIndexes that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SetStatementForSwitches that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SetStatementForConditions that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SortStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.StartStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.StopStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.StringStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SubtractSimpleStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SubtractGivingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.SubtractCorrespondingStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.UnstringStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.WriteStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.XmlGenerateStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.Compiler.CodeElements.XmlParseStatement that, D data) { return visitCodeElement(that, data); }


        //CodeElementGroup
        public virtual R Visit(CobolProgram that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(ProgramAttributes that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(EnvironmentDivision that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(ConfigurationSection that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(InputOutputSection that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(FileControlParagraph that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(IoControlParagraph that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(DataDivision that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(ExecSqlStatement that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(TypeCobol.DocumentModel.Dom.FileDescription that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(FileSection that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(WorkingStorageSection that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(LocalStorageSection that, D data) { return visitCodeElement(that, data); }
        public virtual R Visit(LinkageSection that, D data) { return visitCodeElement(that, data); }

        //Expressions
        //public virtual R Visit(ReceivingStorageArea that, D data) { return visitCodeElement(that, data); }

        //public virtual R Visit(NumericVariable that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(NumericVariable[] that, D data) { return visitCodeElement(that, data); }

        //public virtual R Visit(NumericValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(GeneratedNumericValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(StorageDataType that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(StorageArea that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(FunctionCallResult that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(FilePropertySpecialRegister that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(IndexStorageArea that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(IntrinsicStorageArea that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(StorageAreaPropertySpecialRegister that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(DataOrConditionStorageArea that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SymbolReference that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(AmbiguousSymbolReference that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ReferenceModifier that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ExternalNameOrSymbolReference that, D data) { return visitCodeElement(that, data); }

        //public virtual R Visit(FunctionCall that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(IntrinsicFunctionCall that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(UserDefinedFunctionCall that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ProcedureCall that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(BooleanValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(GeneratedBooleanValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(Variable that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(VariableOrExpression that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(CallSiteParameter that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ExternalName that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(QualifiedTextName that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SyntaxProperty<bool> that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SymbolInformation that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SymbolDefinitionOrReference that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SymbolDefinition that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(QualifiedSymbolReference that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(TypeCobolQualifiedSymbolReference typeCobolQualifiedSymbolReference);
        //public virtual R Visit(SyntaxValue<string> that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(AlphanumericValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(GeneratedAlphanumericValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(GeneratedSymbolName that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(EnumeratedValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(CharacterValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(RepeatedCharacterValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SymbolType that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(Expression that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ArithmeticExpression that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(NumericVariableOperand that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ArithmeticOperation that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ConditionalExpression that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SignCondition that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(RelationCondition that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(LogicalOperation that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ClassCondition that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ConditionNameConditionOrSwitchStatusCondition that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(VariableBase that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SymbolReferenceVariable that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(AlphanumericVariable that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(CharacterVariable that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(IntegerVariable that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit<T>(SyntaxValue<T> that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(NullPointerValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(IntegerValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(GeneratedIntegerValue that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit<T>(SyntaxProperty<T> that, D data) { return visitCodeElement(that, data); }

        //public virtual R Visit(CallSite that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(CallTarget that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(CallTargetParameter that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SubscriptExpression that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(Value that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(GroupCorrespondingImpact that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(BooleanValueOrExpression that, D data) { return visitCodeElement(that, data); }

        //public virtual R Visit(DataType that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(TableSortingKey that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ValuesRange that, D data) { return visitCodeElement(that, data); }

        //public virtual R VisitVariableWriter(VariableWriter that, D data) { return visitCodeElement(that, data); }
        //public virtual R VisitFunctionCaller(FunctionCaller that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SetSendingVariable that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(SetUPSISwitchInstruction that, D data) { return visitCodeElement(that, data); }
        //public virtual R Visit(ParametersProfile that, D data) { return visitCodeElement(that, data); }

        public virtual R visitCodeElement(CodeElement that, D data) { System.Diagnostics.Debug.Assert(false); return default(R); }
    }
}
