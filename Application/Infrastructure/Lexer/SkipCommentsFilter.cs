using Application.Infrastructure.Lekser;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lexer
{
    public class SkipCommentsFilter : ILexer
    {
        private readonly ILexer _lexer;

        public SkipCommentsFilter(ILexer lexer)
        {
            _lexer = lexer;
        }

        public Token Peek()
        {
            Token token = _lexer.Peek();

            while (token.Type == TokenType.COMMENT)
            {
                _lexer.Read();
                token = _lexer.Peek();
            }

            return token;
        }

        public Token Read()
        {
            Peek(); // advances if comment

            return _lexer.Read();
        }

        public void Dispose()
        {
            _lexer.Dispose();
        }
    }
}
