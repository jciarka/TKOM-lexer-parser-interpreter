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
    public class UnexpectedTokenException : ConfigurationException
    {
        public TokenType ExpectedType { get; private set; }

        public UnexpectedTokenException(Token token, TokenType expected)
            : base(token, ConfigurationParsingSevernity.CRITICAL, prepareMessage(token, expected))
        {

        }

        private static string prepareMessage(Token token, TokenType expected)
        {
            return $"(LINE: {token.Position!.Line}, column: {token.Position.Column}) " +
                $"Unexpected token of type \"{Enum.GetName(token.Type)}\", expected \"{Enum.GetName(expected)}\"";
        }
    }
}
