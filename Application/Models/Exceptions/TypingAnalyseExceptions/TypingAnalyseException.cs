namespace Application.Models.Exceptions.SourseParser
{
    public class TypingAnalyseException : ComputingException
    {
        public TypingAnalyseException(CharacterPosition position) : base(position)
        {
        }

        public TypingAnalyseException(CharacterPosition position, string message) : base(position, message)
        {
        }
    }
}
