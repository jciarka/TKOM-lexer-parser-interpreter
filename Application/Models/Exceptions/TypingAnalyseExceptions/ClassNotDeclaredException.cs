
using Application.Models.Grammar;

namespace Application.Models.Exceptions.SourseParser
{
    public class ClassNotDeclaredException : TypingAnalyseException
    {
        public ClassNotDeclaredException(string className, RulePosition position)
            : base(new CharacterPosition(position), prepareMessage(className, position))
        {
        }

        private static string prepareMessage(string className, RulePosition position)
        {
            return $"(Line: {position.Line}) Class or constructor {className} with specified signature does not exists.";
        }
    }

    internal class PropertyNotDeclaredException : ComputingException
    {
        public PropertyNotDeclaredException(string className, string propertyName, RulePosition position)
            : base(new CharacterPosition(position), prepareMessage(className, propertyName, position))
        {
        }

        private static string prepareMessage(string className, string propertyName, RulePosition position)
        {
            return $"(Line: {position.Line}) Property {propertyName} does not exists at class {className}.";
        }
    }
}
