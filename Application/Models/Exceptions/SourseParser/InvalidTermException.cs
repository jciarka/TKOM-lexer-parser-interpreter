using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class InvalidTermException : SourceParseException
    {
        public TokenType ExpectedType { get; private set; }

        public InvalidTermException(Token token) : base(token, prepareMessage(token))
        {

        }

        private static string prepareMessage(Token token)
        {
            return $"(LINE: {token.Position!.Line}, column: {token.Position.Column}) " +
                $"Invalid term";
        }
    }
}
