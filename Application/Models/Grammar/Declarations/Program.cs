using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Grammar
{
    public class Program : GrammarRuleBase
    {
        public IEnumerable<FuntionDecl> FunctionDeclaration { get; private set; }

        public Program(IEnumerable<FuntionDecl> functionDeclaration)
        {
            FunctionDeclaration = functionDeclaration;
        }
    }
}
