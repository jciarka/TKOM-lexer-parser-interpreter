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

            if (_lexer.Current.Type == TokenType.COMMENT)
            {
                Advance();
            }
        }

        public Token Current => _lexer.Current;

        public bool Advance()
        {
            do
            {
                if (!_lexer.Advance()) return false;
            }
            while (_lexer.Current.Type == TokenType.COMMENT);

            return true;
        }

        public void Dispose()
        {
            _lexer.Dispose();
        }
    }
}
