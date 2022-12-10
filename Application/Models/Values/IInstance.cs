using System.Collections.ObjectModel;

namespace Application.Models.Values
{
    public interface IInstance
    {
        public IClass Class { get; set; }
        public ReadOnlyDictionary<string, ValueBase> Properties { get; }
    }
}
