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
        public ICallableAnalyseSet CallableSet { get; }
        public IClassAnalyseSet ClassSet { get; }

        public FunctionCallTypeAnalyseContext(
            FunctionDecl declaration,
            ICallableAnalyseSet? signaturesSet = null,
            IClassAnalyseSet? classAnalyseSet = null)
        {
            FunctionType = FunctionType.BASIC;
            ReturnType = declaration.Type;
            Locals = declaration.Parameters.Select(x => new Tuple<string, TypeBase>(x.Identifier, x.Type));
            Scope = new TypeAnalyseScope(Locals);
            CallableSet = signaturesSet ?? new CallableAnalyseSet(new());
            ClassSet = classAnalyseSet ?? new ClassAnalyseSet(new());
        }

        public FunctionCallTypeAnalyseContext(
            Lambda lambda,
            FunctionCallTypeAnalyseContext parentContext)
        {
            FunctionType = FunctionType.LAMBDA;
            ReturnType = new NoneType();
            Locals = new Tuple<string, TypeBase>[] { new Tuple<string, TypeBase>(lambda.Parameter.Identifier, lambda.Parameter.Type) };
            Scope = new TypeAnalyseScope(Locals, parentContext.Scope);
            CallableSet = parentContext.CallableSet;
            ClassSet = parentContext.ClassSet;
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
