using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class FunctionDecl : GrammarRuleBase, IVisitable
    {
        public TypeBase Type { get; }
        public string Name { get; }
        public IEnumerable<Parameter> Parameters { get; }
        public BlockStmt Block { get; }

        public FunctionDecl(TypeBase type, string name, IEnumerable<Parameter> parameters, BlockStmt block, RulePosition position)
            : base(position)
        {
            Type = type;
            Name = name;
            Parameters = parameters;
            Block = block;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}