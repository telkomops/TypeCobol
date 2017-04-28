using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeCobol.DocumentModel.Code.Scopes;
using TypeCobol.DocumentModel.Code.Types;

namespace TypeCobol.DocumentModel.Code.Symbols
{
    /// <summary>
    /// The Symbol of a Function declaration
    /// </summary>
    public class FunctionSymbol : ProgramSymbol
    {
        /// <summary>
        /// Name constructor
        /// </summary>
        /// <param name="name">Function's name</param>
        public FunctionSymbol(String name) : base(name)
        {
            //Override the Kind here.
            base.Kind = Kinds.Function;
        }

        /// <summary>
        /// Full constructor.
        /// </summary>
        /// <param name="name">Function's name</param>
        /// <param name="parameters">Function's paramaetrs</param>
        /// <param name="returnType">Function's return type</param>
        public FunctionSymbol(String name, List<VariableSymbol> parameters, FunctionType returnType)
            : base(name)
        {
            this.FunctionType = new FunctionType(parameters, returnType);
        }

        /// <summary>
        /// Function parameters.
        /// </summary>
        public List<VariableSymbol> Parameters
        {
            get
            {
                return this.FunctionType != null ? this.FunctionType.Parameters : null;
            }
        }

        /// <summary>
        /// The type returnes
        /// </summary>
        public TypeCobolType ReturnType
        {
            get
            {
                return this.FunctionType != null ? this.FunctionType.ReturnType : null;
            }
        }

        /// <summary>
        /// The Function's type.
        /// </summary>
        public FunctionType FunctionType
        {
            get
            {
                return (FunctionType)base.Type;
            }
            set
            {
                //Enter Parameters in the Scope.
                foreach (var param in value.Parameters)
                {
                    LinkageStorageData.Enter(param);
                }
                base.Type = value;                
            }
        }

        /// <summary>
        /// Func
        /// </summary>
        public override TypeCobolType Type
        {
            get
            {
                return base.Type;
            }
            set
            {
                //Ensure that this is the type of a function.
                System.Diagnostics.Debug.Assert(value is FunctionType);
                this.FunctionType = (FunctionType)value;
            }
        }
    }
}
