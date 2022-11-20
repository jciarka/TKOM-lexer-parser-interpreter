using Application.Infrastructure.Lekser;
using Application.Infrastructure.Lekser.SourceReaders;
using Application.Infrastructure.Lexer;
using Application.Models.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Lexer
{
    public class SkipCommentsFilterTests
    {
        private readonly Mock<ILexer> _lexerMock;

        public SkipCommentsFilterTests()
        {
            _lexerMock = new Mock<ILexer>();
        }

        [Theory]
        [MemberData(nameof(prepareDataForShouldSkipCommentAtSource))]
        public void ShouldSkipCommentAtSource(Token[] tokens, TokenType[] tokenTypes)
        {
            int i = 0;

            // arange
            _lexerMock.Setup(x => x.Read())
                .Returns(() => i < tokens.Length ? tokens[i] : new Token { Type = TokenType.EOF })
                .Callback(() => i++);

            _lexerMock.Setup(x => x.Peek())
                .Returns(() => i < tokens.Length ? tokens[i] : new Token { Type = TokenType.EOF });

            var lexer = new SkipCommentsFilter(_lexerMock.Object);

            foreach (var tokenType in tokenTypes)
            {
                Assert.Equal(tokenType, lexer.Read().Type);
            }
        }

        private static IEnumerable<object[]> prepareDataForShouldSkipCommentAtSource()
        {
            yield return new object[] {
                new Token[]
                {
                    new Token { Type = TokenType.COMMENT, Lexeme = "# To jest komentarz" },
                    new Token { Type = TokenType.EOF }
                },
                new TokenType[]
                {
                    TokenType.EOF,
                }
            };

            yield return new object[] {
                new Token[]
                {
                    new Token { Type = TokenType.IDENTIFIER, Lexeme = "identifier" },
                    new Token { Type = TokenType.COMMENT, Lexeme = "# To jest komentarz" },
                    new Token { Type = TokenType.LITERAL, Lexeme = "10" },
                    new Token { Type = TokenType.EOF }
                },
                new TokenType[]
                {
                    TokenType.IDENTIFIER,
                    TokenType.LITERAL,
                    TokenType.EOF,
                }
            };
        }
    }
}
