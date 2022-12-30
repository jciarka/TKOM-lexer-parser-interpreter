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

namespace Application.Models.Exceptions.SourseParser
{
    public class ZeroDivisionException : TypeVerifierException
    {
        public ZeroDivisionException()
            : base(new CharacterPosition(), prepareMessage(new RulePosition(new CharacterPosition())))
        {
        }

        public ZeroDivisionException(RulePosition position)
            : base(new CharacterPosition(position), prepareMessage(position))
        {
        }

        private static string prepareMessage(RulePosition position)
        {
            return $"(LINE: {position.Line}, column: {position.Column}) " +
                $"Zero division exception";
        }
    }
}
