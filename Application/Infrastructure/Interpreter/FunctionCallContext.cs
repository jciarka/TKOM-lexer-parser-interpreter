using Application.Models.Grammar;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public class FunctionCallContext
    {
        public FunctionType FunctionType { get; }
        public IEnumerable<Tuple<string, IValue>> Locals { get; }
        public IVariableSet Scope { get; private set; }

        public FunctionCallContext(
                IEnumerable<Parameter> parameters,
                IEnumerable<IValue> arguments
            )
        {
            FunctionType = FunctionType.BASIC;
            Locals = parameters.Select(x => x.Identifier).Zip(arguments).Select(x => x.ToTuple());
            Scope = new VariableSet(Locals);
        }

        public void PushScope()
        {
            Scope = new VariableSet(Scope);
        }

        public void PopScope()
        {
            if (Scope.Previous == null)
            {
                throw new NotImplementedException();
            }

            Scope = Scope.Previous;
        }
    }
}