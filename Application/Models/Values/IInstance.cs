using System.Collections.ObjectModel;

namespace Application.Models.Values
{
    public interface IInstance
    {
        public IClass Class { get; }

        public void SetProperty(string name, IValue value);
        public IValue GetProperty(string name);

        public IValue InvokeMethod(string name, IEnumerable<IValue> arguments);
    }
}
