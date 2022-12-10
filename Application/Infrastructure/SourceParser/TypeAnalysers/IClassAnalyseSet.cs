using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public interface IClassAnalyseSet
    {
        bool TryFindConstructor(TypeBase classType, IEnumerable<TypeBase> argumentTypes);
        bool TryFindProperty(TypeBase classType, string propIdentifier, out TypeBase? propertyType);
        bool TryFindMethod(TypeBase classType, FunctionCallExprDescription description, out TypeBase? returnType);
    }
}