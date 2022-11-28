using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Lekser;
using Application.Models.ConfigurationParser;
using Application.Models.Exceptions;
using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.ConfigurationParser
{
    public class ConfigurationParserEngine
    {
        private readonly CurrencyTypesInfo _result = new();
        private readonly ILexer _lexer;
        private readonly IErrorHandler _errorHandler;

        public ConfigurationParserEngine(ILexer lexer, IErrorHandler errorHandler)
        {
            _lexer = lexer;
            _errorHandler = errorHandler;
        }

        public CurrencyTypesInfo Parse()
        {
            parseHeader();

            parseBody();

            return _result;
        }

        private void parseHeader()
        {
            try
            {
                parseHeaderLine();
            }
            catch (ComputingException issue)
            {
                _errorHandler.HandleError(issue);
            }
        }

        private void parseHeaderLine()
        {
            while (_lexer.Current.Type == TokenType.IDENTIFIER)
            {
                var normalized = normalize(_lexer.Current.Lexeme!);

                if (_result.currencyTypes.Contains(normalized))
                {
                    _errorHandler.HandleError(new DuplicatedCurrencyException(_lexer.Current));
                }

                if (!validateName(normalized))
                {
                    _errorHandler.HandleError(new InvalidCurrencyNameException(_lexer.Current));
                }

                _result.currencyTypes.Add(normalized);
                _lexer.Advance();
            }

            if (_lexer.Current.Type != TokenType.SEMICOLON)
            {
                _errorHandler.HandleError(new UnexpectedTokenException(_lexer.Current, TokenType.SEMICOLON));
                return;
            }

            _lexer.Advance();
        }

        private void parseBody()
        {
            for (int i = 0; i < _result.currencyTypes.Count(); i++)
            {
                try
                {
                    parseBodyLine();
                }
                catch (ComputingException issue)
                {
                    _errorHandler.HandleError(issue);
                    skipToSemicolon();

                    if (_lexer.Current.Type == TokenType.EOF) return;
                }
            }
        }

        private void skipToSemicolon()
        {
            while (_lexer.Current.Type != TokenType.SEMICOLON)
            {
                if (_lexer.Current.Type == TokenType.EOF)
                {
                    break;
                }

                _lexer.Advance();
            }
            _lexer.Advance();
        }

        private void parseBodyLine()
        {
            var currencyFrom = readLineCurrencyFrom();

            foreach (var currencyTo in _result.currencyTypes)
            {
                readLineCurrencyConversion(currencyFrom, currencyTo);
            }

            if (_lexer.Current.Type != TokenType.SEMICOLON)
            {
                _errorHandler.HandleError(new UnexpectedTokenException(_lexer.Current, TokenType.SEMICOLON));
                return;
            }

            _lexer.Advance();
        }

        private string readLineCurrencyFrom()
        {
            if (_lexer.Current.Type != TokenType.IDENTIFIER)
            {
                throw new UnexpectedTokenException(_lexer.Current, TokenType.IDENTIFIER);
            }

            if (_result.currencyConvertions.Keys.Any(x => x.CFrom.Equals(_lexer.Current.Lexeme)))
            {
                throw new DuplicatedCurrencyException(_lexer.Current);
            }

            var lexeme = _lexer.Current.Lexeme!;
            _lexer.Advance();

            return normalize(lexeme);
        }

        private void readLineCurrencyConversion(string currencyFrom, string currencyTo)
        {
            if (_lexer.Current.Type != TokenType.LITERAL)
            {
                throw new UnexpectedTokenException(_lexer.Current, TokenType.LITERAL);
            }

            _result.currencyConvertions.Add(
                (currencyFrom, currencyTo),
                _lexer.Current.DecimalValue ?? _lexer.Current.IntValue ?? throw new InvalidConversionValueException(_lexer.Current));

            _lexer.Advance();
        }

        private bool validateName(string normalized)
        {
            return normalized.Count() == 3 && normalized.Where(x => char.IsLetter(x)).Count() == 3;
        }

        private string normalize(string identifier)
        {
            return identifier!.Trim().ToUpper();
        }
    }
}
