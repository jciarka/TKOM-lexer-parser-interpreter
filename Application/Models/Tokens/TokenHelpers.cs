using Application.Infrastructure.Lekser.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Tokens
{
    public class TokenLexeme
    {
        public TokenType Type { get; set; }
        public string? Lexeme { get; set; }
    }

    public class TokenHelpers
    {
        public static IEnumerable<TokenLexeme> TokenLexems =>
            ControlCharactersTokenLexems
                .Union(OneCharOperatorsTokenLexems)
                .Union(TwoCharOperatorsTokenLexems)
                .Union(KeywordsTokenLexems)
                .Union(OtherTokenLexems);

        public static IEnumerable<TokenLexeme> ControlCharactersTokenLexems => new TokenLexeme[]
        {
            new TokenLexeme() { Type = TokenType.LEFT_PAREN, Lexeme = "(" },
            new TokenLexeme() { Type = TokenType.RIGHT_PAREN, Lexeme = ")" },
            new TokenLexeme() { Type = TokenType.LEFT_BRACKET, Lexeme = "[" },
            new TokenLexeme() { Type = TokenType.RIGHT_BRACKET, Lexeme = "]" },
            new TokenLexeme() { Type = TokenType.LEFT_BRACE, Lexeme = "{" },
            new TokenLexeme() { Type = TokenType.RIGHT_BRACE, Lexeme = "}" },
            new TokenLexeme() { Type = TokenType.COMMA, Lexeme = "," },
            new TokenLexeme() { Type = TokenType.DOT, Lexeme = "." },
            new TokenLexeme() { Type = TokenType.SEMICOLON, Lexeme = ";" },
        };

        public static IEnumerable<TokenLexeme> OneCharOperatorsTokenLexems => new TokenLexeme[]
        {
            new TokenLexeme() { Type = TokenType.MINUS, Lexeme =  "-" },
            new TokenLexeme() { Type = TokenType.PLUS, Lexeme =  "+" },
            new TokenLexeme() { Type = TokenType.SLASH, Lexeme =  "/" },
            new TokenLexeme() { Type = TokenType.STAR, Lexeme =  "*" },
            new TokenLexeme() { Type = TokenType.BANG, Lexeme = "!" },
            new TokenLexeme() { Type = TokenType.EQUAL, Lexeme =  "=" },
            new TokenLexeme() { Type = TokenType.GREATER, Lexeme = ">" },
            new TokenLexeme() { Type = TokenType.LESS, Lexeme = "<" },
            new TokenLexeme() { Type = TokenType.PRCT_OF, Lexeme = "%" },
        };

        public static IEnumerable<TokenLexeme> TwoCharOperatorsTokenLexems => new TokenLexeme[]
        {
            new TokenLexeme() { Type = TokenType.BANG_EQUAL, Lexeme = "!=" },
            new TokenLexeme() { Type = TokenType.EQUAL_EQUAL,Lexeme =  "==" },
            new TokenLexeme() { Type = TokenType.GREATER_EQUAL, Lexeme = ">=" },
            new TokenLexeme() { Type = TokenType.LESS_EQUAL, Lexeme = "<=" },
            new TokenLexeme() { Type = TokenType.TRANSFER_FROM, Lexeme = ">>" },
            new TokenLexeme() { Type = TokenType.TRANSFER_TO, Lexeme = "<<" },
            new TokenLexeme() { Type = TokenType.TRANSFER_PRCT_FROM, Lexeme = "%>" },
            new TokenLexeme() { Type = TokenType.TRANSFER_PRCT_TO, Lexeme = "<%" },
            new TokenLexeme() { Type = TokenType.ARROW, Lexeme = "=>" },
        };

        public static IEnumerable<TokenLexeme> OperatorsTokenLexems =>
            OneCharOperatorsTokenLexems.Union(TwoCharOperatorsTokenLexems);


        public static IEnumerable<TokenLexeme> KeywordsTokenLexems => new TokenLexeme[]
        {
            new TokenLexeme() { Type = TokenType.OR, Lexeme = "or" },
            new TokenLexeme() { Type = TokenType.IF, Lexeme = "if" },
            new TokenLexeme() { Type = TokenType.TO, Lexeme = "to" },
            new TokenLexeme() { Type = TokenType.AND, Lexeme = "and" },
            new TokenLexeme() { Type = TokenType.VAR, Lexeme = "var" },
            new TokenLexeme() { Type = TokenType.INT, Lexeme = "int" },
            new TokenLexeme() { Type = TokenType.ELSE, Lexeme = "else" },
            new TokenLexeme() { Type = TokenType.VOID, Lexeme = "void" },
            new TokenLexeme() { Type = TokenType.NULL, Lexeme = "null" },
            new TokenLexeme() { Type = TokenType.TRUE, Lexeme = "true" },
            new TokenLexeme() { Type = TokenType.CHAR, Lexeme = "char" },
            new TokenLexeme() { Type = TokenType.PRINT, Lexeme = "print" },
            new TokenLexeme() { Type = TokenType.WHILE, Lexeme = "while" },
            new TokenLexeme() { Type = TokenType.FALSE, Lexeme = "false" },
            new TokenLexeme() { Type = TokenType.DOUBLE, Lexeme = "double" },
            new TokenLexeme() { Type = TokenType.RETURN, Lexeme = "return" },
        };

        public static IEnumerable<TokenLexeme> TypeTokenLexems => new TokenLexeme[]
        {
            // IMPORTATNT - tokens must be listed in ascending length order!
            new TokenLexeme() { Type = TokenType.INT, Lexeme = "int" },
            new TokenLexeme() { Type = TokenType.VOID, Lexeme = "void" },
            new TokenLexeme() { Type = TokenType.NULL, Lexeme = "null" },
            new TokenLexeme() { Type = TokenType.TRUE, Lexeme = "true" },
            new TokenLexeme() { Type = TokenType.CHAR, Lexeme = "char" },
            new TokenLexeme() { Type = TokenType.PRINT, Lexeme = "print" },
            new TokenLexeme() { Type = TokenType.WHILE, Lexeme = "while" },
            new TokenLexeme() { Type = TokenType.FALSE, Lexeme = "false" },
            new TokenLexeme() { Type = TokenType.DOUBLE, Lexeme = "double" },
            new TokenLexeme() { Type = TokenType.RETURN, Lexeme = "return" },
        };

        public static IEnumerable<TokenLexeme> OtherTokenLexems => new TokenLexeme[]
        {
            new TokenLexeme() { Type = TokenType.EOF, Lexeme = CharactersHelpers.EOF.ToString() }
        };
    }
}
