using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.Interpreter
{
    public class RuntimeStackOverflowException : RuntimeException
    {
        public RuntimeStackOverflowException() : base()
        {
        }

        public RuntimeStackOverflowException(RulePosition position) : base(position)
        {
        }

        protected override string getMessage()
        {
            return $"(LINE: {Position.Line}, column: {Position.Column}) " +
                $"Stack overflow exception";
        }
    }
}
