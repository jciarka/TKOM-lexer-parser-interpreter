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
        IValue Call(IInterpreterEngine interpreter, IEnumerable<IValue> arguments);
    }

    public interface IMethod : ICallable
    {
    }

    public interface IConstructor : ICallable
    {
    }
}
