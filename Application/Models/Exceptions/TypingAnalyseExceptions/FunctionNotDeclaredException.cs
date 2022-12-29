using Application.Infrastructure.Interpreter;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Exceptions.SourseParser
{
    public class FunctionNotDeclaredException : TypeVerifierException
    {
        public FunctionNotDeclaredException(FunctionCallExprDescription signature, RulePosition position)
            : base(new CharacterPosition(position), prepareMessage(signature, position))
        {
        }

        private static string prepareMessage(FunctionCallExprDescription signature, RulePosition position)
        {
            var args = signature.ArgumentTypes ?? new TypeBase[] { };
            var argsString = string.Join(",", args.Select(x => x.Name));
            return $"(Line {position.Line}) Function {signature.Identifier}({argsString}) with specified signature does not exists.";
        }
    }
}
