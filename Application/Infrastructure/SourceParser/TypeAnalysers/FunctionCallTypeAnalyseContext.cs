using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Interpreter
{
    public class FunctionCallTypeAnalyseContext
    {
        public FunctionType FunctionType { get; }
        public TypeBase ReturnType { get; private set; }
        public IEnumerable<Tuple<string, TypeBase>> Locals { get; }
        public ITypeAnalyseScope Scope { get; private set; }
        public Dictionary<FunctionSignature, TypeBase> Functions { get; }

        public FunctionCallTypeAnalyseContext(FunctionDecl declaration, Dictionary<FunctionSignature, TypeBase>? FunctionsSignatures = null)
        {
            FunctionType = FunctionType.BASIC;
            ReturnType = declaration.Type;
            Locals = declaration.Parameters.Select(x => new Tuple<string, TypeBase>(x.Identifier, x.Type));
            Scope = new TypeAnalyseScope(Locals);
            Functions = FunctionsSignatures ?? new();
        }

        public FunctionCallTypeAnalyseContext(Lambda lambda, ITypeAnalyseScope scope, Dictionary<FunctionSignature, TypeBase>? FunctionsSignatures = null)
        {
            FunctionType = FunctionType.LAMBDA;
            ReturnType = new NoneType();
            Locals = new Tuple<string, TypeBase>[] { new Tuple<string, TypeBase>(lambda.Parameter.Identifier, lambda.Parameter.Type) };
            Scope = new TypeAnalyseScope(Locals, scope);
            Functions = FunctionsSignatures ?? new();
        }

        public void PushScope()
        {
            Scope = new TypeAnalyseScope(Scope);
        }

        public void PopScope()
        {
            if (Scope.Previous == null)
            {
                throw new NotImplementedException();
            }

            Scope = Scope.Previous;
        }

        public bool CheckReturnType(TypeBase type)
        {
            if (FunctionType == FunctionType.LAMBDA && ReturnType.Type == TypeEnum.VOID)
            {
                ReturnType = type;
                return true;
            }
            else
            {
                return ReturnType.Equals(type);
            }
        }
    }

    public enum FunctionType
    {
        BASIC,
        LAMBDA
    }
}
