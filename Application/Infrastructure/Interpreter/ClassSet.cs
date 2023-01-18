
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public class ClassSet : IClassSet
    {
        public Dictionary<string, IClassPrototype> _classesBase;

        public ClassSet(Dictionary<string, IClassPrototype> classesBase)
        {
            _classesBase = classesBase;
        }

        public bool TryFindConstructor(TypeBase classType, IEnumerable<TypeBase> argumentTypes, out IConstructor? callable)
        {
            if (!_classesBase.TryGetValue(classType.Name, out var classPrototype))
            {
                callable = null!;
                return false;
            }

            TypeBase? parametrisingType = null;
            if (classType.GetType().Equals(typeof(GenericType)))
            {
                parametrisingType = ((GenericType)classType).ParametrisingType;
            }

            var @class = classPrototype.Create(parametrisingType);

            if (@class.Constructors.TryGetValue(
                    new FixedArgumentsFunctionSignature(null!, classType.Name, argumentTypes),
                    out callable))
            {
                return true;
            }

            return false;
        }

        public bool TryFindMethod(TypeBase classType, FunctionCallExprDescription description, out IMethod? callable)
        {
            if (!_classesBase.TryGetValue(classType.Name, out var classPrototype))
            {
                callable = null;
                return false;
            }

            TypeBase? parametrisingType = null;
            if (classType.GetType().Equals(typeof(GenericType)))
            {
                parametrisingType = ((GenericType)classType).ParametrisingType;
            }

            var @class = classPrototype.Create(parametrisingType);

            if (@class.Methods.TryGetValue(
                    new FixedArgumentsFunctionSignature(null!, description.Identifier, description.ArgumentTypes),
                    out var methodTuple))
            {
                callable = methodTuple.Item2;
                return true;
            }

            callable = null;
            return false;
        }
    }
}