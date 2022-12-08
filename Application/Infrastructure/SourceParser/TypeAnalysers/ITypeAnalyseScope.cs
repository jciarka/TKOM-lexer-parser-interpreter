using Application.Models.Grammar.Expressions.Terms;

namespace Application.Infrastructure.Interpreter
{
    public interface ITypeAnalyseScope
    {
        ITypeAnalyseScope? Previous { get; }
        bool TryAdd(string name, TypeBase type);
        bool TryFind(string name, out TypeBase? type);
    }

    public enum ParametersCountType
    {
        FIXED,
        VARIABLE
    }
}