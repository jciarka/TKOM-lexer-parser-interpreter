using Application.Infrastructure.Interpreter;
using Application.Models.Grammar.Expressions.Terms;
using System.Collections.ObjectModel;

namespace Application.Models.Values
{
    public interface IClassPrototype
    {
        IClass Create(TypeBase? parametrisingType = null);
    }

    public interface IClass
    {
        string Name { get; }
        TypeBase Type { get; }

        ReadOnlyDictionary<FunctionSignature, IConstructor> Constructors { get; }
        ReadOnlyDictionary<string, TypeBase> Properties { get; }
        ReadOnlyDictionary<FunctionSignature, Tuple<TypeBase, IMethod>> Methods { get; }
    }

    public interface IGenericClass : IClass
    {
        TypeBase ParametrisingType { get; }
    }
}
