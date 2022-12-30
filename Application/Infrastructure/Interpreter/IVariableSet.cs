using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public interface IVariableSet
    {
        IVariableSet? Previous { get; }
        void Set(string name, IValue type);
        bool TryFind(string name, out IValue? type);
    }
}