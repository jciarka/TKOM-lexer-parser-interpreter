using Application.Models.Values;
using Application.Models.Values.NativeLibrary;

namespace Application.Infrastructure.Presenters
{
    public class TypingAnalyserOptions
    {
        public IEnumerable<INativeClassPrototype> NativeClasses { get; set; } = NativeLibraryProvider.GetClassPrototypes();
        public IEnumerable<INativeCallable> NativeCallables { get; set; } = NativeLibraryProvider.GetFunctions();
    }
}