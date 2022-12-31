using Application.Infrastructure.Interpreter;
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
    public class AccountClassPrototype : INativeClassPrototype
    {
        public IClass Create(TypeBase? parametrisingType = null)
        {
            return new AccountClass(parametrisingType!);
        }
    }

    public class AccountClass : INativeClass
    {
        public string Name => TypeName.ACCOUNT;
        public TypeBase _currency;

        public TypeBase Type => new GenericType(TypeName.ACCOUNT, _currency);

        public AccountClass(TypeBase currency)
        {
            _currency = currency;
        }

        public ReadOnlyDictionary<FunctionSignature, IConstructor> Constructors =>
            new ReadOnlyDictionary<FunctionSignature, IConstructor>(
                new Dictionary<FunctionSignature, IConstructor>()
                {
                    {
                        new FixedArgumentsFunctionSignature(
                            new GenericType(TypeName.ACCOUNT, _currency),
                            TypeName.ACCOUNT,
                            new List<TypeBase>() { }),
                        new AccountClassDefaultConstructor(this)
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            new GenericType(TypeName.ACCOUNT, _currency),
                            TypeName.ACCOUNT,
                            new List<TypeBase>() { new BasicType(TypeName.INT, TypeEnum.INT) }),
                        new AccountClassIntValueConstructor(this)
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            new GenericType(TypeName.ACCOUNT, _currency),
                            TypeName.ACCOUNT,
                            new List<TypeBase>() { new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL) }),
                        new AccountClassDeciamalValueConstructor(this)
                    }
                });

        public ReadOnlyDictionary<string, TypeBase> Properties =>
            new ReadOnlyDictionary<string, TypeBase>(
                new Dictionary<string, TypeBase>()
                {
                    { "Currency", new TypeType(_currency) },
                    { "Ballance", new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL) },
                });

        public ReadOnlyDictionary<FunctionSignature, Tuple<TypeBase, IMethod>> Methods =>
            new ReadOnlyDictionary<FunctionSignature, Tuple<TypeBase, IMethod>>(
                new Dictionary<FunctionSignature, Tuple<TypeBase, IMethod>>()
                {
                    {
                        new FixedArgumentsFunctionSignature(new GenericType(TypeName.ACCOUNT, _currency), "Copy", new List<TypeBase>()),
                        new(new GenericType(TypeName.ACCOUNT, _currency), new AccountCopyMethod())
                    }
                });

    }

    public class AccountClassDefaultConstructor : IConstructor
    {
        private readonly IClass _class;

        public AccountClassDefaultConstructor(AccountClass @class)
        {
            _class = @class;
        }

        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var instance = new AccountInstace(_class);

            instance.SetProperty("Currency", new TypeValue(((GenericType)_class.Type).ParametrisingType));

            return new Reference(instance);
        }
    }

    public class AccountClassIntValueConstructor : IConstructor
    {
        private readonly IClass _class;

        public AccountClassIntValueConstructor(AccountClass @class)
        {
            _class = @class;
        }

        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var instance = new AccountInstace(_class);

            instance.SetProperty("Currency", new TypeValue(((GenericType)_class.Type).ParametrisingType));

            instance.SetProperty("Ballance", arguments.First().To(
                new TypeValue(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL)), new()));

            return new Reference(instance);
        }
    }

    public class AccountClassDeciamalValueConstructor : IConstructor
    {
        private readonly IClass _class;

        public AccountClassDeciamalValueConstructor(AccountClass @class)
        {
            _class = @class;
        }

        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var instance = new AccountInstace(_class);

            instance.SetProperty("Ballance", arguments.First());

            return new Reference(instance);
        }
    }

    public class AccountCopyMethod : IMethod
    {
        public IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments)
        {
            var old = ((Reference)arguments.First()).Instance!;

            var newInstance = new AccountInstace(old.Class);

            foreach (var property in old.Class.Properties.Keys)
            {
                newInstance.SetProperty(property, old.GetProperty(property));
            }

            return new Reference(newInstance);
        }
    }
}
