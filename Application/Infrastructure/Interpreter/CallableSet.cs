using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public class CallableSet : ICallableSet
    {
        public readonly Dictionary<FunctionSignature, ICallable> _callableBase;

        public CallableSet(Dictionary<FunctionSignature, ICallable> callableBase)
        {
            _callableBase = callableBase;
        }

        public bool TryFind(FunctionCallExprDescription description, out ICallable? callable)
        {
            var fixedSignature = new FixedArgumentsFunctionSignature(null!, description.Identifier, description.ArgumentTypes);

            if (_callableBase.TryGetValue(fixedSignature, out callable))
            {
                return true;
            }

            var variableSignature = new VariableArgumentsFunctionSignature(null!, description.Identifier);

            if (_callableBase.TryGetValue(variableSignature, out callable))
            {
                return true;
            }

            return false;
        }
    }
}