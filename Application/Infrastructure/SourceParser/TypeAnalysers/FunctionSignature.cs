using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System.Text;

namespace Application.Infrastructure.Interpreter
{
    public abstract class FunctionSignature
    {
        public TypeBase ReturnType { get; }
        public string Identifier { get; }
        public IEnumerable<TypeBase> Parameters { get; }

        public FunctionSignature(
            TypeBase returnType,
            string identifier,
            IEnumerable<TypeBase> parameters)
        {
            ReturnType = returnType;
            Identifier = identifier;
            Parameters = parameters;
        }

        public FunctionSignature(FunctionDecl declaration)
        {
            ReturnType = declaration.Type;
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

        protected abstract string getSignature();

    }


    class FixedArgumentsFunctionSignature : FunctionSignature
    {
        public FixedArgumentsFunctionSignature(FunctionDecl declaration) : base(declaration)
        {
        }

        public FixedArgumentsFunctionSignature(TypeBase returnType, string identifier, IEnumerable<TypeBase> parameters)
            : base(returnType, identifier, parameters)
        {
        }

        protected override string getSignature()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Identifier);
            sb.Append("(");
            sb.Append(string.Join(",", Parameters.Select(x => x.Name)));
            sb.Append(")");

            return sb.ToString();
        }
    }

    class VariableArgumentsFunctionSignature : FunctionSignature
    {
        public VariableArgumentsFunctionSignature(TypeBase returnType, string identifier)
               : base(returnType, identifier, new List<TypeBase>())
        {
        }

        protected override string getSignature()
        {
            return $"{Identifier}(<any>)";
        }
    }
}