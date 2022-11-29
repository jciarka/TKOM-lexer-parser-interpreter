using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Tokens
{
    public enum TokenType
    {
        // One character control tokens
        LEFT_PAREN,
        RIGHT_PAREN,
        LEFT_BRACKET,
        RIGHT_BRACKET,
        LEFT_BRACE,
        RIGHT_BRACE,
        COMMA,
        DOT,
        SEMICOLON,

        // One character tokens
        MINUS,
        PLUS,
        SLASH,
        STAR,
        BANG,
        EQUAL,
        GREATER,
        LESS,
        PRCT_OF,

        // Two character tokens.
        BANG_EQUAL,
        EQUAL_EQUAL,
        GREATER_EQUAL,
        LESS_EQUAL,
        TRANSFER_FROM,
        TRANSFER_TO,
        TRANSFER_PRCT_FROM,
        TRANSFER_PRCT_TO,
        ARROW,

        // Literals.
        LITERAL,

        // Keywords.
        IN,
        OR,
        IF,
        ELSE,
        WHILE,
        VAR,
        INT,
        DOUBLE,
        FOREACH,
        CHAR,
        PRINT,
        RETURN,
        TO,
        VOID,
        NULL,

        // OTHERS
        TYPE,
        COMMENT,
        IDENTIFIER,
        EOF,
        LAMBDA,
        AND,
        BOOL
    }
}
