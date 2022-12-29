using Application.Infrastructure.Interpreter;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Exceptions.SourseParser
{
    public class FunctionRedefinitionException : TypeVerifierException
    {
        public FunctionRedefinitionException(FunctionSignature signature, RulePosition position)
            : base(new CharacterPosition(position), prepareMessage(signature, position))
        {
        }

        private static string prepareMessage(FunctionSignature signature, RulePosition position)
        {
            var args = signature.Parameters ?? new TypeBase[] { };
            var argsString = string.Join(",", args.Select(x => x.Name));
            return $"(Line {position.Line}) Function {signature.Identifier}({argsString}) redefinition";
        }
    }
}
