using Application.Infrastructure.Presenters;

namespace Application.Models.Grammar
{
    public class Lambda : ArgumentBase
    {
        public Parameter Parameter { get; }
        public StatementBase Stmt { get; }

        public Lambda(Parameter parameter, StatementBase stmt)
        {
            Parameter = parameter;
            Stmt = stmt;
        }

        public override void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }
    }
}