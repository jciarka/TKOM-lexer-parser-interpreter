using Application.Models.Grammar;
using Application.Models.Types;

namespace Application.Models.Exceptions.SourseParser
{
    public class NonTypeException : TypeVerifierException
    {
        public IEnumerable<TypeEnum> ExpectedTypes { get; }

        public NonTypeException(RulePosition position, params TypeEnum[] expectedTypes)
            : base(new CharacterPosition(position), prepareMessage(position, expectedTypes))
        {
            ExpectedTypes = expectedTypes;
        }

        private static string prepareMessage(RulePosition position, IEnumerable<TypeEnum> expectedTypes)
        {
            return $"(LINE: {position.Line}) " +
              $"Expression returns no type: expected \"{string.Join(" / ", expectedTypes)}\"";
        }
    }
}
