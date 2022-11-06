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
    public class TooLongIdentifierException : LexerException
    {
        public string Lexeme { get; private set; }

        public TooLongIdentifierException(string lexeme, CharacterPosition position) : base(position, prepareMessage(lexeme, position))
        {
            Lexeme = lexeme;
        }

        private static string prepareMessage(string lexeme, CharacterPosition position)
        {
            return $"(LINE: {position.Line}, column: {position}) " +
                $"Too long idenfifier \"{SymbolDisplay.FormatLiteral(lexeme, false)}\"";
        }
    }
}
