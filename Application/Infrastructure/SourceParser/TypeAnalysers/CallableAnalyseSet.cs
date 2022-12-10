using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Infrastructure.Interpreter
{
    public class CallableAnalyseSet : ICallableAnalyseSet
    {
        public readonly Dictionary<FunctionSignature, TypeBase> _callableBase;

        public CallableAnalyseSet(Dictionary<FunctionSignature, TypeBase> callableBase)
        {
            _callableBase = callableBase;
        }

        public bool TryFind(FunctionCallExprDescription description, out TypeBase? returnType)
        {
            var fixedSignature = new FixedArgumentsFunctionSignature(null!, description.Identifier, description.ArgumentTypes);

            if (_callableBase.TryGetValue(fixedSignature, out returnType))
            {
                return true;
            }

            var variableSignature = new VariableArgumentsFunctionSignature(null!, description.Identifier);

            if (_callableBase.TryGetValue(variableSignature, out returnType))
            {
                return true;
            }

            return false;
        }
    }
}