using Application.Infrastructure.Interpreter;
using Application.Models.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values
{
    public class Callable : ICallable
    {
        private FunctionDecl _declaration;

        public Callable(FunctionDecl functionDecl)
        {
            _declaration = functionDecl;
        }

        public IValue Call(IInterpreterEngine interpreter, params IValue[] arguments)
        {
            return interpreter.InterpretFunctionCall(_declaration, _declaration.Parameters, arguments);
        }
    }
}
