using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Lexer.Exceptions
{
    public class InvalidLiteralException : LexerException
    {
        public string Lexem { get; set; }

        public InvalidLiteralException(string lexem, CharacterPosition position) : base(position, prepareMessage(lexem, position))
        {
            Position = position;
            Lexem = lexem;
        }

        private static string prepareMessage(string lexem, CharacterPosition position)
        {
            return $"(LINE: {position.Line}, column: {position}) Invalid literal \"{lexem}\"";
        }
    }
}
