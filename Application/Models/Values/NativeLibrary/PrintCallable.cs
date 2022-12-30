using Application.Infrastructure.Interpreter;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.NativeLibrary
{
    public class PrintCallable : INativeCallable
    {
        private readonly FunctionSignature _signature = new VariableArgumentsFunctionSignature(new NoneType(), "print");

        public ArgumentsCountType ArgumentsCountType => ArgumentsCountType.VARIABLE;
        public FunctionSignature Signature => _signature;

        public IValue Call(IInterpreterEngine interpreter, params IValue[] arguments)
        {
            Console.WriteLine(string.Join("", arguments.Select(x => x.ToString())));
            return new NullValue();
        }
    }
}
