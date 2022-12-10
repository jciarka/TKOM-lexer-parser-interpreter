using Application.Infrastructure.Interpreter;

namespace Application.Models.Exceptions.SourseParser
{
    public class FunctionNotDeclaredException : TypingAnalyseException
    {
        public FunctionNotDeclaredException(FunctionCallExprDescription signature)
            : base(new CharacterPosition(), prepareMessage(signature))
        {
        }

        private static string prepareMessage(FunctionCallExprDescription signature)
        {
            return $"Not declared function call {signature}";
        }
    }
}
