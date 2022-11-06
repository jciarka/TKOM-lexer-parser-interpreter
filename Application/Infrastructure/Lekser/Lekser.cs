using Application.Infrastructure.Helpers;
using Application.Infrastructure.Lekser.Helpers;
using Application.Infrastructure.Lekser.SourceReaders;
using Application.Models.Tokens;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lekser
{
    public class Lekser
    {
        private readonly ISourceReader _reader;
        private readonly TypesInfoProvider _typesInfo;
        private readonly LinkedList<Token> _waiting;
        private readonly LinkedList<Token> _consumed;

        public Lekser(ISourceReader reader, TypesInfoProvider typesInfo)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _typesInfo = typesInfo ?? new TypesInfoProvider();
            _waiting = new LinkedList<Token>();
            _consumed = new LinkedList<Token>();
        }

        public Token Future(int index)
        {
            if (_waiting.Count() > index)
            {
                for (int requiredIndex = _waiting.Count(); requiredIndex <= index; requiredIndex++)
                {
                    var next = getNext();

                    _waiting.AddLast(next);

                    if (next.Type == TokenType.EOF)
                    {
                        return next;
                    }
                }
            }

            return _waiting.ElementAt(index);
        }

        public Token Peek()
        {
            if (_waiting.Any())
            {
                return _waiting.First();
            }

            var next = getNext();

            _waiting.AddLast(next);

            return next;
        }

        public Token Read()
        {
            if (_waiting.Any())
            {
                var next = _waiting.First();

                _waiting.RemoveFirst();
                _consumed.AddFirst(next);

                return next;
            }

            return getNext();
        }

        public Token? Previous()
        {
            if (_consumed.Any())
            {
                return _consumed.First();
            }

            return null;
        }

        public Token? Previous(int index)
        {
            if (_consumed.Count() > index)
            {
                return _consumed.ElementAt(index);
            }

            return null;
        }

        private Token getNext()
        {
            Token? token = null;

            skipWhiteSpaces();

            if (tryBuildControlCharacterToken(out token))
            {
                return token!;
            }

            if (tryBuildOperatorToken(out token))
            {
                return token!;
            }

            if (tryBuildComment(out token))
            {
                return token!;
            }

            if (tryBuildStringLiteralToken(out token))
            {
                return token!;
            }

            if (tryBuildNumericLiteralToken(out token))
            {
                return token!;
            }

            if (tryBuildAlphaNumericToken(out token))
            {
                return token!;
            }

            if (tryBuildEof(out token))
            {
                return token!;
            }

            throw new Exception();
        }

        private void skipWhiteSpaces()
        {
            char next = _reader.Peek();

            while (char.IsWhiteSpace(next))
            {
                next++;
            }
        }

        /// <summary>
        /// Builds one character tokens
        /// </summary>
        private bool tryBuildControlCharacterToken(out Token? token)
        {
            token = null;

            var first = _reader.Peek();
            var match = TokenHelpers.ControlCharactersTokenLexems
                .Where(x => x.Lexeme!.Equals(first.ToString()));

            if (!match.Any())
            {
                return false;
            }

            _reader.Read();
            var lexemeInfo = match.First();

            token = new Token()
            {
                Lexeme = lexemeInfo.Lexeme,
                Type = lexemeInfo.Type,
                Position = getCharacterPositionDetails(),
            };

            return true;
        }

        /// <summary>
        /// Builds One or two character tokens build from not alphanumeric characters
        /// </summary>
        private bool tryBuildOperatorToken(out Token? token)
        {
            token = null;

            var first = _reader.Peek();
            var match = TokenHelpers.OperatorsTokenLexems
                .Where(x => x.Lexeme!.First().Equals(first));

            if (!match.Any())
            {
                return false;
            }

            TokenPosition tokenPosition = getCharacterPositionDetails();
            _reader.Read();
            var second = _reader.Peek();

            var twoCharMatch = match
                .Where(x => x.Lexeme!.Length == 2 && x.Lexeme![1].Equals(second));

            TokenLexeme lexemeInfo;

            if (!twoCharMatch.Any())
            {
                lexemeInfo = twoCharMatch.First();
            }
            else
            {
                lexemeInfo = match.First();
            }

            token = new Token()
            {
                Lexeme = lexemeInfo.Lexeme,
                Type = lexemeInfo.Type,
                Position = tokenPosition,
            };

            return true;
        }

        /// <summary>
        /// Builds comments (starts with #)
        /// </summary>
        private bool tryBuildComment(out Token? token)
        {
            token = null;
            var letter = _reader.Peek();

            if (!letter.Equals('#'))
            {
                return false;
            }

            TokenPosition tokenPosition = getCharacterPositionDetails();
            var commentBuilder = new StringBuilder();

            while (!letter.Equals(CharactersHelpers.NL) && !letter.Equals(CharactersHelpers.EOF))
            {
                _reader.Read();
                commentBuilder.Append(letter);
                letter = _reader.Peek();
            }

            token = new Token()
            {
                Type = TokenType.COMMENT,
                Position = tokenPosition,
                Value = commentBuilder.ToString(),
            };

            return true;
        }

        /// <summary>
        /// Builds numeric character tokens (must start with digits)
        /// </summary>
        private bool tryBuildNumericLiteralToken(out Token? token)
        {
            token = null;

            var letter = _reader.Peek();

            if (!char.IsDigit(letter))
            {
                return false;
            }

            var stringBuilder = new StringBuilder();
            var numberBuilder = new NumberBuilder();

            var tokenPosition = getCharacterPositionDetails();

            // build number part
            buildNumericPartOfNumber(stringBuilder, numberBuilder);

            // build type declaration part
            var declaredRepresentation = buildTypeDeclarationPartOfNumber(stringBuilder, out var declaredTypeLexeme);

            var finalRepresentation = declaredRepresentation ?? (numberBuilder.IsInteger ? TypeEnum.INT : TypeEnum.DECIMAL);
            var finalTypeLexem = declaredTypeLexeme ?? (numberBuilder.IsInteger ? "int" : "decimal");

            token = new Token()
            {
                Lexeme = stringBuilder.ToString(),
                Type = TokenType.LITERAL,
                Position = tokenPosition,
                Value = finalRepresentation == TypeEnum.INT ? numberBuilder.ToInteger() : numberBuilder.ToDecimal(),
                ValueType = finalTypeLexem
            };

            return true;
        }

        private void buildNumericPartOfNumber(StringBuilder stringBuilder, NumberBuilder numberBuilder)
        {
            var letter = _reader.Peek();

            while (char.IsDigit(letter) || _reader.Peek().Equals('.'))
            {
                _reader.Read();
                stringBuilder.Append(letter);

                if (!numberBuilder.tryAppend(letter))
                {
                    throw new Exception();
                }

                letter = _reader.Peek();
            }
        }

        private TypeEnum? buildTypeDeclarationPartOfNumber(StringBuilder builder, out string? typeLexem)
        {
            char letter = _reader.Peek();

            // type EMPTY
            if (!char.IsLetter(letter))
            {
                typeLexem = null;
                return null;
            }

            // type DECIMAL
            if (letter.Equals('D') || letter.Equals('d'))
            {
                letter = _reader.Read();
                builder.Append(letter);

                typeLexem = "deciaml";

                return TypeEnum.DECIMAL;
            }

            // type EXTERNAL
            var typeBuilder = new StringBuilder();

            while (char.IsLetterOrDigit(letter))
            {
                _reader.Read();
                builder.Append(letter);
                typeBuilder.Append(letter);

                letter = _reader.Peek();
            }

            var match = _typesInfo.ExternalTypes.Where(x => x.Lexeme!.Equals(typeBuilder.ToString()));

            if (match.Any())
            {
                throw new Exception();
            }

            typeLexem = match.First().Lexeme!;

            // external typeshas decimal representation
            return TypeEnum.DECIMAL;
        }

        /// <summary>
        /// Builds string literal tokens (must start with ")
        /// </summary>
        private bool tryBuildStringLiteralToken(out Token? token)
        {
            token = null;

            if (_reader.Peek() != '\"')
            {
                return false;
            }

            StringBuilder lexemeBuilder = new StringBuilder();
            StringLiteralBuilder literalBuilder = new StringLiteralBuilder();

            while (literalBuilder.State != LiteralBuilderState.VALID)
            {
                var letter = _reader.Read();

                lexemeBuilder.Append(letter);
                var result = literalBuilder.tryAppend(letter);

                if (result == false)
                {
                    throw new Exception();
                }
            }

            token = new Token()
            {
                Lexeme = lexemeBuilder.ToString(),
                Type = TokenType.LITERAL,
                Value = literalBuilder.ToString(),
                ValueType = "string",
                Position = getCharacterPositionDetails(),
            };

            return true;
        }

        /// <summary>
        /// Builds tokens that are build only with alphanumeric characters
        /// </summary>
        private bool tryBuildAlphaNumericToken(out Token? token)
        {
            token = null;

            var tokenPosition = getCharacterPositionDetails();
            var builder = new StringBuilder();

            // grab next charaters while are letters
            while (char.IsLetterOrDigit(_reader.Peek()) && builder.Length < 255)
            {
                builder.Append(_reader.Read());
            }

            if (char.IsLetterOrDigit(_reader.Peek()))
            {
                // overflow
                throw new Exception();
            }

            var lexeme = builder.ToString();

            if (tryMatchKeywordsToken(lexeme, tokenPosition, out token))
            {
                return true;
            }

            if (tryMatchTypeToken(lexeme, tokenPosition, out token))
            {
                return true;
            }

            token = buildIdentifier(lexeme, tokenPosition);
            return true;
        }

        private bool tryBuildEof(out Token? token)
        {
            token = null;

            if (!_reader.Peek().Equals(CharactersHelpers.EOF))
            {
                return false;
            }

            token = new Token
            {
                Type = TokenType.EOF,
                Position = getCharacterPositionDetails(),
            };
            return true;
        }

        private bool tryMatchKeywordsToken(string lexeme, TokenPosition tokenPosition, out Token? token)
        {
            token = null;

            var match = TokenHelpers.KeywordsTokenLexems.Where(x => x.Lexeme!.Equals(lexeme));

            if (!match.Any())
            {
                return false;
            }

            var lexemeInfo = match.First();

            token = new Token()
            {
                Lexeme = lexemeInfo.Lexeme,
                Type = lexemeInfo.Type,
                Position = tokenPosition,
            };

            return true;
        }

        private bool tryMatchTypeToken(string lexeme, TokenPosition tokenPosition, out Token? token)
        {
            token = null;

            var match = _typesInfo.Types.Where(x => x.Lexeme!.Equals(lexeme));

            if (!match.Any())
            {
                return false;
            }

            var typeInfo = match.First();

            token = new Token()
            {
                Lexeme = typeInfo.Lexeme,
                Type = TokenType.TYPE,
                Position = tokenPosition,
                Value = typeInfo.Lexeme,
            };

            return true;
        }

        private Token? buildIdentifier(string lexeme, TokenPosition tokenPosition)
        {
            return new Token()
            {
                Lexeme = lexeme,
                Type = TokenType.IDENTIFIER,
                Position = tokenPosition
            };
        }

        private TokenPosition getCharacterPositionDetails()
        {
            return new TokenPosition()
            {
                Column = _reader.Column,
                Line = _reader.Line,
                LinePosition = _reader.LinePosition,
                Position = _reader.Position,
            };
        }
    }
}
