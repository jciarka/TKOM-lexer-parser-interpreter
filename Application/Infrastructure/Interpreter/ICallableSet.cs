using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public interface ICallableSet
    {
        bool TryFind(FunctionCallExprDescription description, out ICallable? callable);
    }
}