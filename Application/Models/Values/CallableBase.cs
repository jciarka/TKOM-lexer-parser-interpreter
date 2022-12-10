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

        public ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
