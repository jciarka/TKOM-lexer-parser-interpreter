using Application.Infrastructure.Lekser.Helpers;

namespace Application.Models.Tokens
{
    public class TokenHelpers
    {
        public static IDictionary<string, TokenType> AllMap =>
            ControlCharactersMap.ToDictionary(x => x.Key.ToString(), x => x.Value)
                .Union(OneCharacterOperatorsMap.ToDictionary(x => x.Key.ToString(), x => x.Value))
                .Union(TwoCharactersOperatorsMap.ToDictionary(x => new string(new char[] { x.Key.Item1, x.Key.Item2 }), x => x.Value))
                .Union(KeywordsMap)
                .Union(BooleanLiteralMap)
                .Union(OtherMap)
                .ToDictionary(x => x.Key, x => x.Value);

        public static IDictionary<char, TokenType> ControlCharactersMap => new Dictionary<char, TokenType>()
        {
            { '(', TokenType.LEFT_PAREN },
            { ')', TokenType.RIGHT_PAREN },
            { '[', TokenType.LEFT_BRACKET },
            { ']', TokenType.RIGHT_BRACKET },
            { '{', TokenType.LEFT_BRACE },
            { '}', TokenType.RIGHT_BRACE },
            { ',', TokenType.COMMA },
            { '.', TokenType.DOT },
            { ';', TokenType.SEMICOLON },
        };

        public static IDictionary<char, TokenType> OneCharacterOperatorsMap => new Dictionary<char, TokenType>()
        {
            { '-', TokenType.MINUS},
            { '+', TokenType.PLUS},
            { '/', TokenType.SLASH},
            { '*', TokenType.STAR},
            { '!',  TokenType.BANG},
            { '=', TokenType.EQUAL},
            { '>', TokenType.GREATER},
            { '<', TokenType.LESS},
            { '%', TokenType.PRCT_OF},
        };

        public static IDictionary<(char, char), TokenType> TwoCharactersOperatorsMap => new Dictionary<(char, char), TokenType>()
        {
            { ('!', '='), TokenType.BANG_EQUAL },
            { ('=', '='), TokenType.EQUAL_EQUAL },
            { ('>', '='), TokenType.GREATER_EQUAL },
            { ('<', '='), TokenType.LESS_EQUAL },
            { ('>', '>'), TokenType.TRANSFER_FROM },
            { ('<', '<'), TokenType.TRANSFER_TO },
            { ('%', '>'), TokenType.TRANSFER_PRCT_FROM },
            { ('<', '%'), TokenType.TRANSFER_PRCT_TO },
            { ('=', '>'), TokenType.ARROW },
        };

        public static IDictionary<string, TokenType> KeywordsMap => new Dictionary<string, TokenType>()
        {
            { "or", TokenType.OR },
            { "if", TokenType.IF },
            { "to",TokenType.TO },
            {"in" ,TokenType.IN },
            { "and", TokenType.AND },
            { "var", TokenType.VAR },
            { "int", TokenType.INT },
            { "else", TokenType.ELSE },
            { "void", TokenType.VOID },
            { "null", TokenType.NULL },
            { "char",TokenType.CHAR },
            { "print", TokenType.PRINT },
            { "while", TokenType.WHILE },
            { "double", TokenType.DOUBLE },
            { "return", TokenType.RETURN },
            { "lambda", TokenType.LAMBDA },
            { "foreach", TokenType.FOREACH },
        };

        public static IDictionary<string, TokenType> BooleanLiteralMap => new Dictionary<string, TokenType>()
        {
            { "true", TokenType.LITERAL },
            { "false", TokenType.LITERAL },
        };

        public static IDictionary<string, TokenType> TypeMap => new Dictionary<string, TokenType>()
        {
            { "int", TokenType.INT },
            { "void" , TokenType.VOID },
            { "null" , TokenType.NULL },
            { "char", TokenType.CHAR  },
            { "while", TokenType.WHILE  },
            { "double", TokenType.DOUBLE  },
        };

        public static IDictionary<string, TokenType> OtherMap => new Dictionary<string, TokenType>()
        {
            { CharactersHelpers.EOF.ToString(), TokenType.EOF }
        };
    }
}
