using Application.Infrastructure.Helpers;
using Application.Infrastructure.Lekser.Helpers;
using Application.Infrastructure.Lekser.SourceReaders;

using Application.Models;
using Application.Models.Lexer.Exceptions;
using Application.Models.Tokens;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Lekser
{
    public class LexerEngine : ILexer, IDisposable
    {
        private readonly ISourceReader _reader;
        private readonly LinkedList<Token> _waiting;
        private readonly LinkedList<Token> _consumed;
        private readonly LexerOptions _options;

        public LexerEngine(ISourceReader reader, LexerOptions? options = null)
        {
            _options = options ?? new LexerOptions();
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
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

            if (tryBuildEof(out token))
            {
                return token!;
            }

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

            var position = getCharacterPositionDetails();
            var letter = _reader.Current;
            _reader.Advance();

            throw new UnexpectedCharacterException(letter, position);
        }

        private void skipWhiteSpaces()
        {
            char next = _reader.Current;

            while (char.IsWhiteSpace(next) || next.Equals(CharactersHelpers.NL))
            {
                _reader.Advance();
                next = _reader.Current;
            }
        }

        /// <summary>
        /// Builds one character tokens
        /// </summary>
        private bool tryBuildControlCharacterToken(out Token? token)
        {
            token = null;

            var first = _reader.Current;
            var match = TokenHelpers.ControlCharactersTokenLexems
                .Where(x => x.Lexeme!.Equals(first.ToString()));

            if (!match.Any())
            {
                return false;
            }

            _reader.Advance();
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

            var first = _reader.Current;
            var match = TokenHelpers.OperatorsTokenLexems
                .Where(x => x.Lexeme!.First().Equals(first));

            if (!match.Any())
            {
                return false;
            }

            CharacterPosition tokenPosition = getCharacterPositionDetails();
            _reader.Advance();
            var second = _reader.Current;

            var twoCharMatch = TokenHelpers.TwoCharOperatorsTokenLexems
                .Where(x => x.Lexeme!.Length == 2 && x.Lexeme[0].Equals(first) && x.Lexeme![1].Equals(second));

            TokenLexeme lexemeInfo;

            if (twoCharMatch.Any())
            {
                lexemeInfo = twoCharMatch.First();
                _reader.Advance();
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
            var letter = _reader.Current;

            if (!letter.Equals('#'))
            {
                return false;
            }

            CharacterPosition tokenPosition = getCharacterPositionDetails();
            var commentBuilder = new StringBuilder();

            while (!letter.Equals(CharactersHelpers.NL) && !letter.Equals(CharactersHelpers.EOF))
            {
                _reader.Advance();
                commentBuilder.Append(letter);
                letter = _reader.Current;
            }

            token = new Token()
            {
                Type = TokenType.COMMENT,
                Position = tokenPosition,
                Lexeme = commentBuilder.ToString(),
            };

            return true;
        }

        /// <summary>
        /// Builds numeric character tokens (must start with digits)
        /// </summary>
        private bool tryBuildNumericLiteralToken(out Token? token)
        {
            token = null;

            var letter = _reader.Current;

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

            letter = _reader.Current;

            if (char.IsLetterOrDigit(letter))
            {
                throw new InvalidLiteralException(stringBuilder.ToString(), getCharacterPositionDetails());
            }

            var finalRepresentation = declaredRepresentation ?? (numberBuilder.IsInteger ? TypeEnum.INT : TypeEnum.DECIMAL);
            var finalType = declaredTypeLexeme ?? (finalRepresentation == TypeEnum.INT ? "int" : "decimal");

            if (finalRepresentation == TypeEnum.INT)
            {
                token = new Token()
                {
                    Lexeme = stringBuilder.ToString(),
                    Type = TokenType.LITERAL,
                    Position = tokenPosition,
                    IntValue = numberBuilder.ToInteger(),
                    ValueType = finalType
                };
            }
            else
            {
                token = new Token()
                {
                    Lexeme = stringBuilder.ToString(),
                    Type = TokenType.LITERAL,
                    Position = tokenPosition,
                    DecimalValue = numberBuilder.ToDecimal(),
                    ValueType = finalType
                };
            }

            return true;
        }

        private void buildNumericPartOfNumber(StringBuilder stringBuilder, NumberBuilder numberBuilder)
        {
            while (char.IsDigit(_reader.Current) || _reader.Current.Equals('.'))
            {
                if (!numberBuilder.tryAppend(_reader.Current))
                {
                    if (numberBuilder.State == NumberBuilderState.OVERFLOWED)
                    {
                        throw new TypeOverflowException(stringBuilder.ToString(), getCharacterPositionDetails());
                    }
                    else
                    {
                        throw new InvalidLiteralException(stringBuilder.ToString(), getCharacterPositionDetails());
                    }
                }

                stringBuilder.Append(_reader.Current);
                _reader.Advance();
            }
        }

        private TypeEnum? buildTypeDeclarationPartOfNumber(StringBuilder builder, out string? typeLexem)
        {
            // type EMPTY
            if (!char.IsLetter(_reader.Current))
            {
                typeLexem = null;
                return null;
            }

            // type DECIMAL
            if (_reader.Current.Equals('D') || _reader.Current.Equals('d'))
            {

                builder.Append(_reader.Current);
                typeLexem = "decimal";

                _reader.Advance();

                return TypeEnum.DECIMAL;
            }

            // type EXTERNAL
            var typeBuilder = new StringBuilder();

            while (char.IsLetterOrDigit(_reader.Current))
            {

                builder.Append(_reader.Current);
                typeBuilder.Append(_reader.Current);

                _reader.Advance();
            }

            var match = _options.TypesInfo!.ExternalTypes.Where(x => x.Lexeme!.Equals(typeBuilder.ToString()));

            if (!match.Any())
            {
                throw new InvalidLiteralException(typeBuilder.ToString(), getCharacterPositionDetails());
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

            if (_reader.Current != '\"')
            {
                return false;
            }

            StringBuilder lexemeBuilder = new StringBuilder();
            StringLiteralBuilder literalBuilder = new StringLiteralBuilder();

            while (literalBuilder.State != LiteralBuilderState.VALID)
            {
                var letter = _reader.Current;
                _reader.Advance();

                lexemeBuilder.Append(letter);
                var result = literalBuilder.tryAppend(letter);

                if (result == false)
                {
                    throw new InvalidLiteralException(lexemeBuilder.ToString(), getCharacterPositionDetails());
                }
            }

            token = new Token()
            {
                Lexeme = lexemeBuilder.ToString(),
                Type = TokenType.LITERAL,
                StringValue = literalBuilder.ToString(),
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

            if (!char.IsLetter(_reader.Current))
            {
                return false;
            }

            var tokenPosition = getCharacterPositionDetails();
            var builder = new StringBuilder();

            // grab next charaters while are letters
            while ((char.IsLetterOrDigit(_reader.Current) || _reader.Current.Equals('_')) && builder.Length < _options.LiteralMaxLength)
            {
                builder.Append(_reader.Current);
                _reader.Advance();
            }

            if (char.IsLetterOrDigit(_reader.Current))
            {
                throw new TooLongIdentifierException(builder.ToString(), getCharacterPositionDetails());
            }

            var lexeme = builder.ToString();

            if (tryMatchKeywordsToken(lexeme, tokenPosition, out token))
            {
                return true;
            }

            if (tryMatchBooleanLiteralToken(lexeme, tokenPosition, out token))
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

        private bool tryMatchBooleanLiteralToken(string lexeme, CharacterPosition tokenPosition, out Token? token)
        {
            token = null;

            bool? boolValue = null;
            if (lexeme.Equals("true")) boolValue = true;
            if (lexeme.Equals("false")) boolValue = false;

            if (boolValue == null)
            {
                return false;
            }

            token = new Token()
            {
                Type = TokenType.LITERAL,
                ValueType = "bool",
                BoolValue = boolValue,
                Lexeme = lexeme,
                Position = tokenPosition,
            };

            return true;
        }

        private bool tryBuildEof(out Token? token)
        {
            token = null;

            if (!_reader.Current.Equals(CharactersHelpers.EOF))
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

        private bool tryMatchKeywordsToken(string lexeme, CharacterPosition tokenPosition, out Token? token)
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

        private bool tryMatchTypeToken(string lexeme, CharacterPosition tokenPosition, out Token? token)
        {
            token = null;

            var match = _options.TypesInfo!.Types.Where(x => x.Lexeme!.Equals(lexeme));

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
                StringValue = typeInfo.Lexeme,
            };

            return true;
        }

        private Token? buildIdentifier(string lexeme, CharacterPosition tokenPosition)
        {
            return new Token()
            {
                Lexeme = lexeme,
                Type = TokenType.IDENTIFIER,
                Position = tokenPosition
            };
        }

        private CharacterPosition getCharacterPositionDetails()
        {
            return new CharacterPosition()
            {
                Column = _reader.Column,
                Line = _reader.Line,
                LinePosition = _reader.LinePosition,
                Position = _reader.Position,
            };
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
