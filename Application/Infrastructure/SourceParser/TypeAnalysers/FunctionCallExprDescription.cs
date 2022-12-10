using Application.Models.Grammar.Expressions.Terms;

namespace Application.Infrastructure.Interpreter
{
    public class FunctionCallExprDescription
    {
        public string Identifier { get; }
        public IEnumerable<TypeBase> ArgumentTypes { get; set; }

        public FunctionCallExprDescription(string identifier, IEnumerable<TypeBase> argumentTypes)
        {
            Identifier = identifier;
            ArgumentTypes = argumentTypes;
        }
    }
}