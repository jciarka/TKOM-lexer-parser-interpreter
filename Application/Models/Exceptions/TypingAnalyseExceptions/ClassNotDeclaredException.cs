
namespace Application.Models.Exceptions.SourseParser
{
    public class ClassNotDeclaredException : TypingAnalyseException
    {
        public ClassNotDeclaredException(string className)
            : base(new CharacterPosition(), prepareMessage(className))
        {
        }

        private static string prepareMessage(string className)
        {
            return $"Class or constructor does not exist {className}";
        }
    }
}
