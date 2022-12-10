using Application.Infrastructure.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values
{
    public interface ICallable
    {
        ValueBase Call(IInterpreterEngine interpreter, params ValueBase[] arguments);
    }

    public interface IMethod : ICallable
    {
    }

    public interface IConstructor : ICallable
    {
    }
}
