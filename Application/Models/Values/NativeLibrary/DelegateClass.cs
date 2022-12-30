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
    public class DelegateClass : INativeClass
    {
        public string Name => TypeName.LAMBDA;

        public TypeBase Type => new GenericType(TypeName.LAMBDA, _parametrisingType);

        private readonly TypeBase _parametrisingType;

        public DelegateClass(TypeBase parametrisingType)
        {
            _parametrisingType = parametrisingType;
        }

        public ReadOnlyDictionary<FunctionSignature, IConstructor> Constructors =>
            new ReadOnlyDictionary<FunctionSignature, IConstructor>(
                new Dictionary<FunctionSignature, IConstructor>());

        public ReadOnlyDictionary<string, TypeBase> Properties =>
            new ReadOnlyDictionary<string, TypeBase>(
                    new Dictionary<string, TypeBase>());

        public ReadOnlyDictionary<FunctionSignature, Tuple<TypeBase, IMethod>> Methods =>
            new ReadOnlyDictionary<FunctionSignature, Tuple<TypeBase, IMethod>>(
                    new Dictionary<FunctionSignature, Tuple<TypeBase, IMethod>>());
    }
}
