using Application.Infrastructure.Interpreter;
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
    public class FunctionNotDeclaredException : TypingAnalyseException
    {
        public FunctionNotDeclaredException(FunctionSignature signature)
            : base(new CharacterPosition(), prepareMessage(signature))
        {
        }

        private static string prepareMessage(FunctionSignature signature)
        {
            return $"Odwołanie do niezadeklarowanej funkcji {signature}";
        }
    }
}
