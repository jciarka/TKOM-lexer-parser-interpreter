using Application.Models;
using Application.Models.Tokens;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.ConfigurationParser
{
    public class InvalidConversionValueException : ConfigurationException
    {

        public InvalidConversionValueException(Token token)
            : base(token, ConfigurationParsingSevernity.CRITICAL, prepareMessage(token))
        {

        }

        private static string prepareMessage(Token token)
        {
            return $"(LINE: {token.Position!.Line}, column: {token.Position.Column}) " +
                $"Invalid token value: \"{token.Lexeme}\", expected devimal literal";
        }
    }
}
