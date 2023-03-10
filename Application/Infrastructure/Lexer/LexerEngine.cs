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
    public class LexerEngine : ILexer
    {
        private readonly ISourceReader _reader;
        private readonly LexerOptions _options;
        private CharacterPosition _currentPosition;

        public Token Current { get; private set; }

#pragma warning disable CS8618 // Current assigned inside advance generates null assignement warning

        public LexerEngine(ISourceReader reader, LexerOptions? options = null)
        {
            _options = options ?? new LexerOptions();
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _currentPosition = getCharacterPositionDetails();

            Advance(); // load first token to current
        }

#pragma warning restore CS8618 // Current assigned inside advance


        public bool Advance()
        {
            skipWhiteSpaces();

            _currentPosition = getCharacterPositionDetails();

            if (tryBuildEof())
            {
                return false;
            }

            if (tryBuildControlCharacterToken() ||
                tryBuildOperatorToken() ||
                tryBuildComment() ||
                tryBuildStringLiteralToken() ||
                tryBuildNumericLiteralToken() ||
                tryBuildAlphaNumericToken())
            {
                return true;
            }

            var letter = _reader.Current;
            _reader.Advance();

            throw new UnexpectedCharacterException(letter, _currentPosition);
        }

        private void skipWhiteSpaces()
        {
            while (char.IsWhiteSpace(_reader.Current) || _reader.Current.Equals(CharactersHelpers.NL))
            {
                _reader.Advance();
            }
        }

        /// <summary>
        /// Builds one character tokens
        /// </summary>
        private bool tryBuildControlCharacterToken()
        {
            if (!TokenHelpers.ControlCharactersMap.ContainsKey(_reader.Current))
            {
                return false;
            }

            Current = new Token()
            {
                Lexeme = _reader.Current.ToString(),
                Type = TokenHelpers.ControlCharactersMap[_reader.Current],
                Position = _currentPosition,
            };

            _reader.Advance();

            return true;
        }

        /// <summary>
        /// Builds One or two character tokens build from not alphanumeric characters
        /// </summary>
        private bool tryBuildOperatorToken()
        {
            var first = _reader.Current;

            if (!TokenHelpers.OneCharacterOperatorsMap.ContainsKey(first))
            {
                return false;
            }

            _reader.Advance();

            if (!TokenHelpers.TwoCharactersOperatorsMap.ContainsKey((first, _reader.Current)))
            {
                Current = new Token()
                {
                    Lexeme = first.ToString(),
                    Type = TokenHelpers.OneCharacterOperatorsMap[first],
                    Position = _currentPosition,
                };
            }
            else
            {
                Current = new Token()
                {
                    Lexeme = new string(new char[] { first, _reader.Current }),
                    Type = TokenHelpers.TwoCharactersOperatorsMap[(first, _reader.Current)],
                    Position = _currentPosition,
                };

                _reader.Advance();
            }

            return true;
        }

        /// <summary>
        /// Builds comments (starts with #)
        /// </summary>
        private bool tryBuildComment()
        {
            var letter = _reader.Current;

            if (!letter.Equals('#'))
            {
                return false;
            }

            var commentBuilder = new StringBuilder();

            while (!letter.Equals(CharactersHelpers.NL) && !letter.Equals(CharactersHelpers.EOF))
            {
                _reader.Advance();
                commentBuilder.Append(letter);
                letter = _reader.Current;
            }

            Current = new Token()
            {
                Type = TokenType.COMMENT,
                Position = _currentPosition,
                Lexeme = commentBuilder.ToString(),
            };

            return true;
        }

        /// <summary>
        /// Builds numeric character tokens (must start with digits)
        /// </summary>
        private bool tryBuildNumericLiteralToken()
        {
            var letter = _reader.Current;

            if (!char.IsDigit(letter))
            {
                return false;
            }

            var stringBuilder = new StringBuilder();

            try
            {
                var valueRepresentation = buildNumericPartOfNumber(stringBuilder, out var value);
                var declaredRepresentation = buildTypeDeclarationPartOfNumber(stringBuilder);
                var finalRepresentation = declaredRepresentation ?? valueRepresentation;

                letter = _reader.Current;
                if (char.IsLetterOrDigit(letter))
                {
                    throw new InvalidLiteralException(stringBuilder.ToString(), getCharacterPositionDetails());
                }

                if (finalRepresentation == TypeEnum.INT)
                {
                    checked
                    {
                        Current = new Token()
                        {
                            Lexeme = stringBuilder.ToString(),
                            Type = TokenType.LITERAL,
                            Position = _currentPosition,
                            IntValue = (int)value,
                            ValueType = TypeName.INT
                        };
                    }
                }
                else
                {
                    Current = new Token()
                    {
                        Lexeme = stringBuilder.ToString(),
                        Type = TokenType.LITERAL,
                        Position = _currentPosition,
                        DecimalValue = value,
                        ValueType = TypeName.DECIMAL,
                    };
                }

            }
            catch (OverflowException)
            {
                throw new TypeOverflowException(stringBuilder.ToString(), _currentPosition);
            }

            return true;
        }

        private TypeEnum buildNumericPartOfNumber(StringBuilder stringBuilder, out decimal value)
        {
            // integer part
            value = 0m;
            while (char.IsDigit(_reader.Current))
            {
                checked
                {
                    value = 10 * value + (_reader.Current - '0');
                }

                stringBuilder.Append(_reader.Current);
                _reader.Advance();
            }

            if (!_reader.Current.Equals('.'))
            {
                return TypeEnum.INT;
            }

            stringBuilder.Append(_reader.Current);
            _reader.Advance();

            if (!char.IsDigit(_reader.Current))
            {
                throw new InvalidLiteralException(stringBuilder.ToString(), getCharacterPositionDetails());
            }

            // factorial part
            decimal factorial = 1m;
            while (char.IsDigit(_reader.Current))
            {

                checked
                {
                    factorial *= 0.1m;
                    value += (_reader.Current - '0') * factorial;
                }

                stringBuilder.Append(_reader.Current);
                _reader.Advance();
            }

            if (_reader.Current == '.')
            {
                throw new InvalidLiteralException(stringBuilder.ToString(), getCharacterPositionDetails());
            }

            return TypeEnum.DECIMAL;
        }

        private TypeEnum? buildTypeDeclarationPartOfNumber(StringBuilder builder)
        {
            // type EMPTY
            if (!char.IsLetter(_reader.Current))
            {
                return null;
            }

            // type DECIMAL
            if (_reader.Current.Equals('D') || _reader.Current.Equals('d'))
            {

                builder.Append(_reader.Current);
                _reader.Advance();

                return TypeEnum.DECIMAL;
            }

            throw new InvalidLiteralException(builder.ToString(), _currentPosition);
        }

        /// <summary>
        /// Builds string literal tokens (must start with ")
        /// </summary>
        private bool tryBuildStringLiteralToken()
        {
            if (_reader.Current != '\"')
            {
                return false;
            }

            StringBuilder builder = new StringBuilder();
            StringBuilder literalBuilder = new StringBuilder();

            builder.Append(_reader.Current);
            _reader.Advance();

            while (_reader.Current != '\"')
            {
                if (_reader.Current.Equals(CharactersHelpers.EOF) || _reader.Current.Equals(CharactersHelpers.NL))
                {
                    throw new InvalidLiteralException(builder.ToString(), getCharacterPositionDetails());
                }

                // TO DO: overflow

                if (_reader.Current.Equals('\\'))
                {
                    builder.Append(_reader.Current);
                    _reader.Advance();

                    switch (_reader.Current)
                    {
                        case 'n':
                            literalBuilder.Append('\n');
                            break;
                        case 't':
                            literalBuilder.Append('\t');
                            break;
                        case 'f':
                            literalBuilder.Append('\f');
                            break;
                        case 'b':
                            literalBuilder.Append('\b');
                            break;
                        case 'v':
                            literalBuilder.Append('\v');
                            break;
                        case '\'':
                            literalBuilder.Append('\'');
                            break;
                        case '\"':
                            literalBuilder.Append('\"');
                            break;
                        case '\\':
                            literalBuilder.Append('\\');
                            break;
                        default:
                            throw new InvalidLiteralException(builder.ToString(), getCharacterPositionDetails());
                    }
                }
                else
                {
                    literalBuilder.Append(_reader.Current);
                }

                builder.Append(_reader.Current);
                _reader.Advance();
            }
            builder.Append(_reader.Current);
            _reader.Advance();

            Current = new Token()
            {
                Lexeme = builder.ToString(),
                Type = TokenType.LITERAL,
                StringValue = literalBuilder.ToString(),
                Position = _currentPosition,
                ValueType = TypeName.STRING
            };
            return true;
        }

        /// <summary>
        /// Builds tokens that are build only with alphanumeric characters
        /// </summary>
        private bool tryBuildAlphaNumericToken()
        {
            if (!char.IsLetter(_reader.Current))
            {
                return false;
            }

            var builder = new StringBuilder();

            // grab next charaters while are letters
            while ((char.IsLetterOrDigit(_reader.Current) || _reader.Current.Equals('_'))
                   && builder.Length < _options.LiteralMaxLength)
            {
                builder.Append(_reader.Current);
                _reader.Advance();
            }

            if (char.IsLetterOrDigit(_reader.Current))
            {
                throw new TooLongIdentifierException(builder.ToString(), getCharacterPositionDetails());
            }

            var lexeme = builder.ToString();

            if (tryMatchKeywordsToken(lexeme))
            {
                return true;
            }

            if (tryMatchBooleanLiteralToken(lexeme))
            {
                return true;
            }

            if (tryMatchTypeToken(lexeme))
            {
                return true;
            }

            Current = buildIdentifier(lexeme);
            return true;
        }

        private bool tryMatchBooleanLiteralToken(string lexeme)
        {
            bool? boolValue = null;
            if (lexeme.Equals("true")) boolValue = true;
            if (lexeme.Equals("false")) boolValue = false;

            if (boolValue == null)
            {
                return false;
            }

            Current = new Token()
            {
                Type = TokenType.LITERAL,
                BoolValue = boolValue,
                Lexeme = lexeme,
                Position = _currentPosition,
                ValueType = TypeName.BOOL,
            };

            return true;
        }

        private bool tryBuildEof()
        {
            if (!_reader.Current.Equals(CharactersHelpers.EOF))
            {
                return false;
            }

            Current = new Token
            {
                Type = TokenType.EOF,
                Position = _currentPosition,
            };

            return true;
        }

        private bool tryMatchKeywordsToken(string lexeme)
        {
            if (!TokenHelpers.KeywordsMap.ContainsKey(lexeme))
            {
                return false;
            }

            Current = new Token()
            {
                Lexeme = lexeme,
                Type = TokenHelpers.KeywordsMap[lexeme],
                Position = _currentPosition,
            };

            return true;
        }

        private bool tryMatchTypeToken(string lexeme)
        {
            if (!_options.TypesInfo!.Types.ContainsKey(lexeme))
            {
                return false;
            }

            Current = new Token()
            {
                Type = TokenType.TYPE,
                Position = _currentPosition,
                Lexeme = lexeme,
                ValueType = lexeme
            };

            return true;
        }

        private Token buildIdentifier(string lexeme)
        {
            return new Token()
            {
                Lexeme = lexeme,
                Type = TokenType.IDENTIFIER,
                Position = _currentPosition
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
    }
}
