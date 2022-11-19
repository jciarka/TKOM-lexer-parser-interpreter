using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class MissingTokenException : SourceParseException
    {
        public MissingTokenException(Token token, TokenType expected)
            : base(token, prepareMessage(token, expected))
        {
        }

        private static string prepareMessage(Token token, TokenType expected)
        {
            return $"(LINE: {token.Position!.Line}, COLUMN: {token.Position.Column}) " +
                $"Missing token of type \"{Enum.GetName(expected)}\"";
        }
    }
}
