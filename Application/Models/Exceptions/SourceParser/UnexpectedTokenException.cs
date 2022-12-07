using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class UnexpectedTokenException : SourceParseException
    {
        public TokenType ExpectedType { get; private set; }

        public UnexpectedTokenException(Token token, TokenType expected)
            : base(token, prepareMessage(token, expected))
        {

        }

        private static string prepareMessage(Token token, TokenType expected)
        {
            return $"(LINE: {token.Position!.Line}, column: {token.Position.Column}) " +
                $"Unexpected token of type \"{Enum.GetName(token.Type)}\", expected \"{Enum.GetName(expected)}\"";
        }
    }
}
