using Application.Infrastructure.Interpreter;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Types;
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
                            TypeName.ACCOUNT,
                            new List<TypeBase>() { new TypeType(null!) }),

                        new CollectionClassConstructor(ParametrisingType)
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
                            new List<TypeBase>()),
                        new(ParametrisingType, new CollectionClassFirstMethod())
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            ParametrisingType,
                            "Last",
                            new List<TypeBase>()),
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
        private readonly TypeBase _parametrisingType;

        public CollectionClassConstructor(TypeBase parametrisingType)
        {
            _parametrisingType = parametrisingType;
        }

        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionClassAddMethod : IMethod
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionClassDeleteMethod : IMethod
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionClassFirstMethod : IMethod
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionClassLastMethod : IMethod
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionClassWhereMethod : IMethod
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionClassCopyMethod : IMethod
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
