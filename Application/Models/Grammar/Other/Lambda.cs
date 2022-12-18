using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class Lambda : GrammarRuleBase, IArgument, IVisitable
    {
        public Parameter Parameter { get; }
        public IStatement Stmt { get; }

        public Lambda(Parameter parameter, IStatement stmt, RulePosition position) : base(position)
        {
            Parameter = parameter;
            Stmt = stmt;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}