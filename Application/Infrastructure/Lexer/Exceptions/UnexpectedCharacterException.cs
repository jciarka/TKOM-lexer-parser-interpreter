using Application.Models;
using Application.Models.Tokens;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lexer.Exceptions
{
    public class UnexpectedCharacterException : LexerException
    {
        public char Character { get; set; }

        public UnexpectedCharacterException(char character, CharacterPosition position) : base(position, prepareMessage(character, position))
        {
            Position = position;
            Character = character;
        }

        private static string prepareMessage(char character, CharacterPosition position)
        {
            return $"(LINE: {position.Line}, column: {position}) " +
                $"Unexpected character \"{SymbolDisplay.FormatLiteral(character, false)}\"";
        }
    }
}
