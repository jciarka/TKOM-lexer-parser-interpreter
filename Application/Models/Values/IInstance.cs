using Application.Infrastructure.Interpreter;
using Application.Models.Grammar.Expressions.Terms;
using System.Collections.ObjectModel;

namespace Application.Models.Values
{
    public interface IInstance
    {
        public TypeBase Type { get; }
        public IClass Class { get; }

        public void SetProperty(string name, IValue value);
        public IValue GetProperty(string name);
        public IValue InvokeMethod(IInterpreterEngine interpreter, string name, IEnumerable<IValue> arguments);
    }
}
