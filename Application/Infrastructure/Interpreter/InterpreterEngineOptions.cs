using Application.Models.ConfigurationParser;
using Application.Models.Types;
using Application.Models.Values.NativeLibrary;

namespace Application.Infrastructure.Interpreter
{
    public class InterpreterEngineOptions
    {
        public CurrencyTypesInfo CurrencyTypesInfo { get; set; } = new CurrencyTypesInfo();
        public IEnumerable<INativeClassPrototype> NativeClasses { get; set; } = NativeLibraryProvider.GetClassPrototypes();
        public IEnumerable<INativeCallable> NativeCallables { get; set; } = NativeLibraryProvider.GetFunctions();
    }
}