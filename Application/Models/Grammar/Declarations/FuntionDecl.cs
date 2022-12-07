using Application.Infrastructure.Presenters;
using Application.Models.Grammar.Expressions.Terms;

namespace Application.Models.Grammar
{
    public class FunctionDecl : GrammarRuleBase
    {
        public TypeBase? Type { get; }
        public string Name { get; }
        public IEnumerable<Parameter> Parameters { get; }
        public BlockStmt Block { get; }

        public FunctionDecl(string name, IEnumerable<Parameter> parameters, BlockStmt block, TypeBase? type = null)
        {
            Type = type;
            Name = name;
            Parameters = parameters;
            Block = block;
        }

        public void Accept(IPresenterVisitor visitor, int v)
        {
            visitor.Visit(this, v);
        }

        public TypeBase? Accept(ITypingAnalyseVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}