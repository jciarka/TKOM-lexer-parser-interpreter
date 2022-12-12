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
                            new BasicType(TypeName.ACCOUNT, TypeEnum.ACCOUNT),
                            TypeName.ACCOUNT,
                            new List<TypeBase>() { new TypeType(null!) }),
                        new AccountClassTypeConstructor()
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            new BasicType(TypeName.ACCOUNT, TypeEnum.ACCOUNT),
                            TypeName.ACCOUNT,
                            new List<TypeBase>() { new TypeType(null!), new BasicType(TypeName.INT, TypeEnum.INT) }),
                        new AccountClassTypeAndIntValueConstructor()
                    },
                    {
                        new FixedArgumentsFunctionSignature(
                            new BasicType(TypeName.ACCOUNT, TypeEnum.ACCOUNT),
                            TypeName.ACCOUNT,
                            new List<TypeBase>() { new TypeType(null!), new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL) }),
                        new AccountClassTypeConstructor()
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
                        new FixedArgumentsFunctionSignature(new BasicType(TypeName.ACCOUNT, TypeEnum.ACCOUNT), "Copy", new List<TypeBase>()),
                        new(new BasicType(TypeName.ACCOUNT, TypeEnum.ACCOUNT), new AccountCopyMethod())
                    }
                });
    }

    public class AccountClassTypeConstructor : IConstructor
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class AccountClassTypeAndIntValueConstructor : IConstructor
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class AccountClassTypeAndDeciamlValueConstructor : IConstructor
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public class AccountCopyMethod : IMethod
    {
        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
