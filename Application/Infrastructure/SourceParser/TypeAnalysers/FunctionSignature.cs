using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System.Text;

namespace Application.Infrastructure.Interpreter
{
    public class FunctionSignature
    {
        public string Identifier { get; }
        public IEnumerable<TypeBase> Parameters { get; }

        public FunctionSignature(
            string identifier,
            IEnumerable<TypeBase> parameters)
        {
            Identifier = identifier;
            Parameters = parameters;
        }

        public FunctionSignature(FunctionDecl declaration)
        {
            Identifier = declaration.Name;
            Parameters = declaration.Parameters.Select(x => x.Type);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!obj.GetType().Equals(typeof(FunctionSignature)))
            {
                return false;
            }

            FunctionSignature other = (FunctionSignature)obj;

            return getSignature().Equals(other.getSignature());
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(getSignature());
        }

        public override string ToString()
        {
            return getSignature();
        }

        private string getSignature()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Identifier);
            sb.Append("(");
            sb.Append(string.Join(",", Parameters.Select(x => x.Name)));
            sb.Append(")");

            return sb.ToString();
        }
    }
}