using Application.Models.Types;

namespace Application.Models.Exceptions.SourseParser
{
    public class NonTypeException : TypingAnalyseException
    {
        public IEnumerable<TypeEnum> ExpectedTypes { get; }

        public NonTypeException(params TypeEnum[] expectedTypes)
            : base(new CharacterPosition(), prepareMessage(new CharacterPosition(), expectedTypes))
        {
            ExpectedTypes = expectedTypes;
        }

        private static string prepareMessage(CharacterPosition position, IEnumerable<TypeEnum> expectedTypes)
        {
            return $"(LINE: {position.Line}, column: {position.Column}) " +
              $"Expression returns no type: expected \"{string.Join(" / ", expectedTypes)}\"";
        }
    }
}
