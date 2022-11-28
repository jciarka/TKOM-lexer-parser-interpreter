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
    }
}