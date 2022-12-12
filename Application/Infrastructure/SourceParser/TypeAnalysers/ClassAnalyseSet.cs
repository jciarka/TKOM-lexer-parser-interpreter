
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public class ClassAnalyseSet : IClassAnalyseSet
    {
        public Dictionary<string, IClassPrototype> _classesBase;

        public ClassAnalyseSet(Dictionary<string, IClassPrototype> classesBase)
        {
            _classesBase = classesBase;
        }

        public bool TryFindConstructor(TypeBase classType, IEnumerable<TypeBase> argumentTypes)
        {
            if (!_classesBase.TryGetValue(classType.Name, out var classPrototype))
            {
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
                    out var _))
            {
                return true;
            }

            return false;
        }

        public bool TryFindMethod(TypeBase classType, FunctionCallExprDescription description, out TypeBase? returnType)
        {
            if (!_classesBase.TryGetValue(classType.Name, out var classPrototype))
            {
                returnType = null;
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
                    out var method))
            {
                returnType = method.Item1;
                return true;
            }

            returnType = null;
            return false;
        }

        public bool TryFindProperty(TypeBase classType, string propIdentifier, out TypeBase? propertyType)
        {
            if (!_classesBase.TryGetValue(classType.Name, out var classPrototype))
            {
                propertyType = null;
                return false;
            }

            TypeBase? parametrisingType = null;

            if (classType.GetType().Equals(typeof(GenericType)))
            {
                parametrisingType = ((GenericType)classType).ParametrisingType;
            }

            var @class = classPrototype.Create(parametrisingType);

            if (@class.Properties.TryGetValue(propIdentifier, out propertyType))
            {
                return true;
            }

            return false;
        }
    }
}