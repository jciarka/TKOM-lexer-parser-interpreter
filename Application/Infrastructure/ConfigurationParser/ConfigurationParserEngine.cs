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
        private readonly ILexer _lexer;

        public ConfigurationParserEngine(ILexer lexer)
        {
            _lexer = lexer;
        }

        public CurrencyTypesConfiguration Parse(out ICollection<ComputingException> issues)
        {
            issues = new List<ComputingException>();
            var config = new CurrencyTypesConfiguration();

            parseHeader(config.currencyTypes, issues);
            parseBody(config.currencyTypes, config.currencyConvertions, issues);

            return config;
        }

        private void parseHeader(ICollection<string> currencyTypes, ICollection<ComputingException> parseIssues)
        {
            try
            {
                parseHeaderLine(currencyTypes, parseIssues);
            }
            catch (ComputingException issue)
            {
                parseIssues.Add(issue);
            }
        }

        private void parseHeaderLine(ICollection<string> currencyTypes, ICollection<ComputingException> parseIssues)
        {
            while (_lexer.Current.Type == TokenType.IDENTIFIER)
            {
                var normalized = normalize(_lexer.Current.Lexeme!);

                if (currencyTypes.Contains(normalized))
                {
                    parseIssues.Add(new DuplicatedCurrencyException(_lexer.Current));
                }

                if (!validateName(normalized))
                {
                    parseIssues.Add(new InvalidCurrencyNameException(_lexer.Current));
                }

                currencyTypes.Add(normalized);
                _lexer.Advance();
            }

            if (_lexer.Current.Type != TokenType.SEMICOLON)
            {
                parseIssues.Add(new UnexpectedTokenException(_lexer.Current, TokenType.SEMICOLON));
                return;
            }

            _lexer.Advance();
        }

        private void parseBody(
                ICollection<string> currencyTypes,
                Dictionary<(string CFrom, string CTo), decimal> currencyConvertion,
                ICollection<ComputingException> parseIssues
            )
        {
            for (int i = 0; i < currencyTypes.Count(); i++)
            {
                try
                {
                    parseBodyLine(currencyTypes, currencyConvertion, parseIssues);
                }
                catch (ComputingException issue)
                {
                    parseIssues.Add(issue);
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

        private void parseBodyLine(
                ICollection<string> currencyTypes,
                Dictionary<(string CFrom, string CTo), decimal> currencyConvertion,
                ICollection<ComputingException> parseIssues)
        {
            var currencyFrom = readLineCurrencyFrom(currencyConvertion);

            foreach (var currencyTo in currencyTypes)
            {
                readLineCurrencyConversion(currencyConvertion, currencyFrom, currencyTo);
            }

            if (_lexer.Current.Type != TokenType.SEMICOLON)
            {
                parseIssues.Add(new UnexpectedTokenException(_lexer.Current, TokenType.SEMICOLON));
                return;
            }

            _lexer.Advance();
        }

        private string readLineCurrencyFrom(Dictionary<(string CFrom, string CTo), decimal> currencyConvertion)
        {
            if (_lexer.Current.Type != TokenType.IDENTIFIER)
            {
                throw new UnexpectedTokenException(_lexer.Current, TokenType.IDENTIFIER);
            }

            if (currencyConvertion.Keys.Any(x => x.CFrom.Equals(_lexer.Current.Lexeme)))
            {
                throw new DuplicatedCurrencyException(_lexer.Current);
            }

            var lexeme = _lexer.Current.Lexeme!;
            _lexer.Advance();

            return normalize(lexeme);
        }

        private void readLineCurrencyConversion(Dictionary<(string CFrom, string CTo), decimal> currencyConvertion, string currencyFrom, string currencyTo)
        {
            if (_lexer.Current.Type != TokenType.LITERAL)
            {
                throw new UnexpectedTokenException(_lexer.Current, TokenType.LITERAL);
            }

            currencyConvertion.Add(
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
