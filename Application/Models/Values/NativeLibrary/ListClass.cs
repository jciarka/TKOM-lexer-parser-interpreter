using Application.Infrastructure.Interpreter;
using Application.Models.Exceptions.Interpreter;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
using Application.Models.Values.BasicTypeValues;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.NativeLibrary
{
    public class CollectionClassPrototype : INativeClassPrototype
    {
        public IClass Create(TypeBase? parametrisingType = null)
        {
            return new CollectionClass(parametrisingType!);
        }
    }

    public class CollectionClass : INativeGenericClass
    {
        public string Name => TypeName.COLLECTION;

        public TypeBase ParametrisingType { get; }
        public TypeBase Type => new GenericType(TypeName.COLLECTION, ParametrisingType);

        public CollectionClass(TypeBase parametrisingType)
        {
            ParametrisingType = parametrisingType;
        }

        public ReadOnlyDictionary<FunctionSignature, IConstructor> Constructors =>
            new ReadOnlyDictionary<FunctionSignature, IConstructor>(
                new Dictionary<FunctionSignature, IConstructor>()
                {
                    {
                        new FixedArgumentsFunctionSignature(
                            new GenericType(TypeName.COLLECTION, ParametrisingType),
                            TypeName.COLLECTION,
                            new List<TypeBase>() { }),

                        new CollectionClassConstructor(this)
                    }
                });

        public ReadOnlyDictionary<string, TypeBase> Properties => new ReadOnlyDictionary<string, TypeBase>(
            new Dictionary<string, TypeBase>() { });

        public ReadOnlyDictionary<FunctionSignature, Tuple<TypeBase, IMethod>> Methods =>
            new ReadOnlyDictionary<FunctionSignature, Tuple<TypeBase, IMethod>>(
                new Dictionary<FunctionSignature, Tuple<TypeBase, IMethod>>()
                {
                    {
                        new FixedArgumentsFunctionSignature(
                            new NoneType(),
                            "Add",
                            new List<TypeBase>() { ParametrisingType }),
                        new(new NoneType(), new CollectionClassAddMethod())
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            new NoneType(),
                            "Delete",
                            new List<TypeBase>() { new BasicType(TypeName.INT, TypeEnum.INT) }),
                        new(new NoneType(), new CollectionClassDeleteMethod())
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            ParametrisingType,
                            "First",
                            new List<TypeBase>() { new GenericType(TypeName.LAMBDA, ParametrisingType) }),
                        new(ParametrisingType, new CollectionClassFirstMethod())
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            ParametrisingType,
                            "Last",
                            new List<TypeBase>() { new GenericType(TypeName.LAMBDA, ParametrisingType) }),
                        new(ParametrisingType, new CollectionClassLastMethod())
                    },
                    { new FixedArgumentsFunctionSignature(
                            new GenericType(TypeName.COLLECTION, ParametrisingType),
                            "Where",
                            new List<TypeBase>() { new GenericType(TypeName.LAMBDA, ParametrisingType) }),
                        new(new GenericType(TypeName.COLLECTION, ParametrisingType), new CollectionClassWhereMethod())
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            new GenericType(TypeName.COLLECTION, ParametrisingType),
                            "Copy", new List<TypeBase>() { }),
                        new(new GenericType(TypeName.COLLECTION, ParametrisingType), new CollectionClassCopyMethod()) },
                    }
                );
    }

    public class CollectionClassConstructor : IConstructor
    {
        private readonly IClass _class;

        public CollectionClassConstructor(IClass @class)
        {
            _class = @class;
        }

        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var instance = new CollectionInstance(_class);
            return new Reference(instance);
        }
    }

    public class CollectionClassAddMethod : IMethod
    {
        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var collection = (CollectionInstance)((Reference)arguments.First()).Instance!;
            collection.Values.Add(arguments.Last());
            return new EmptyValue();
        }
    }

    public class CollectionClassDeleteMethod : IMethod
    {
        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var collection = (CollectionInstance)((Reference)arguments.First()).Instance!;
            var index = ((IntValue)arguments.Last()).Value;

            if (collection.Values.Count() >= index)
            {
                new ReferenceOutOfRangeException(index);
            }

            collection.Values.RemoveAt(index);
            return new EmptyValue();
        }
    }

    public class CollectionClassFirstMethod : IMethod
    {
        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var collection = (CollectionInstance)((Reference)arguments.First()).Instance!;
            var lambda = (DelegateInstance)((Reference)arguments.Last()).Instance!;

            var hits = collection.Values.Where(x => ((BoolValue)lambda.Call(interpreter, new IValue[] { x })).Value);

            if (hits.Count() == 0)
            {
                return ValuesFactory.GetDefaultValue(((GenericType)collection.Type).ParametrisingType);
            }

            return hits.First();
        }
    }

    public class CollectionClassLastMethod : IMethod
    {
        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var collection = (CollectionInstance)((Reference)arguments.First()).Instance!;
            var lambda = (DelegateInstance)((Reference)arguments.Last()).Instance!;

            var hits = collection.Values.Where(x => ((BoolValue)lambda.Call(interpreter, new IValue[] { x })).Value);

            if (hits.Count() == 0)
            {
                return ValuesFactory.GetDefaultValue(((GenericType)collection.Type).ParametrisingType);
            }

            return hits.Last();
        }
    }

    public class CollectionClassWhereMethod : IMethod
    {
        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var collection = (CollectionInstance)((Reference)arguments.First()).Instance!;
            var lambda = (DelegateInstance)((Reference)arguments.Last()).Instance!;

            var hits = collection.Values.Where(x => ((BoolValue)lambda.Call(interpreter, new IValue[] { x })).Value);

            var newInstance = new CollectionInstance(collection.Class);

            foreach (var hit in hits)
            {
                newInstance.Values.Add(hit);
            }

            return new Reference(newInstance);
        }
    }

    public class CollectionClassCopyMethod : IMethod
    {
        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var collection = (CollectionInstance)((Reference)arguments.First()).Instance!;
            var newInstance = new CollectionInstance(collection.Class);

            newInstance.Values.AddRange(collection.Values);
            return new Reference(newInstance);
        }
    }
}
