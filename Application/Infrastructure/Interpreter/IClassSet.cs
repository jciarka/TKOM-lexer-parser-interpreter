using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public interface IClassSet
    {
        bool TryFindConstructor(TypeBase classType, IEnumerable<TypeBase> argumentTypes, out IConstructor? callable);
        bool TryFindMethod(TypeBase classType, FunctionCallExprDescription description, out IMethod? callable);
    }
}