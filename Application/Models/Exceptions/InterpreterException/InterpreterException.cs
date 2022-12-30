namespace Application.Models.Exceptions.SourseParser
{
    public class InterpreterException : ComputingException
    {
        public InterpreterException(CharacterPosition position) : base(position)
        {
        }

        public InterpreterException(CharacterPosition position, string message) : base(position, message)
        {
        }
    }
}
