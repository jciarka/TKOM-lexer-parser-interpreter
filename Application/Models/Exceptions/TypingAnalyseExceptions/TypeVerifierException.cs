namespace Application.Models.Exceptions.SourseParser
{
    public class TypeVerifierException : ComputingException
    {
        public TypeVerifierException(CharacterPosition position) : base(position)
        {
        }

        public TypeVerifierException(CharacterPosition position, string message) : base(position, message)
        {
        }
    }
}
