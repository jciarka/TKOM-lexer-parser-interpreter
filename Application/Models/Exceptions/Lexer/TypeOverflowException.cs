using Application.Models;
using Application.Models.Tokens;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Lexer.Exceptions
{
    public class TypeOverflowException : LexerException
    {
        public string Lexeme { get; private set; }

        public TypeOverflowException(string lexeme, CharacterPosition position) : base(position, prepareMessage(lexeme, position))
        {
            Lexeme = lexeme;
        }

        private static string prepareMessage(string lexeme, CharacterPosition position)
        {
            return $"(LINE: {position.Line}, column: {position}) " +
                $"Literal overflows type size \"{SymbolDisplay.FormatLiteral(lexeme, false)}\"";
        }
    }
}
