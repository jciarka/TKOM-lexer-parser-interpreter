using Application.Models.Grammar.Expressions.Terms;

namespace Application.Infrastructure.Interpreter
{
    public interface ICallableAnalyseSet
    {
        bool TryFind(FunctionCallExprDescription description, out TypeBase? returnType);
    }
}