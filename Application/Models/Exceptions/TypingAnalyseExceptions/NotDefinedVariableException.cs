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
    public class NotDefinedVariableException : TypeVerifierException
    {
        public string? Variable { get; }

        public NotDefinedVariableException(string variable)
            : base(new CharacterPosition(), prepareMessage(new CharacterPosition(), variable))
        {
            Variable = variable;
        }

        private static string prepareMessage(CharacterPosition position, string variable)
        {
            return $"(LINE: {position.Line}, column: {position.Column}) " +
                $"Not defined variable reference - {variable}";
        }
    }
}
