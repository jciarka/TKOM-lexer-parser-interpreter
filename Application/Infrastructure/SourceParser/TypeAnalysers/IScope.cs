using Application.Models.Grammar.Expressions.Terms;

namespace Application.Infrastructure.Interpreter
{
    public interface IScope
    {
        bool TryAdd(string name, TypeBase type);
        bool TryFind(string name, out TypeBase? type);
    }
}