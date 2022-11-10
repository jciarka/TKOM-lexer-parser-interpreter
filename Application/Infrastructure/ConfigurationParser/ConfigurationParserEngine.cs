﻿using Application.Infrastructure.Lekser;
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
            var token = _lexer.Peek();

            while (token.Type == TokenType.IDENTIFIER)
            {
                var normalized = normalize(token.Lexeme!);

                if (currencyTypes.Contains(normalized))
                {
                    parseIssues.Add(new DuplicatedCurrencyException(token));
                }

                if (!validateName(normalized))
                {
                    parseIssues.Add(new InvalidCurrencyNameException(token));
                }

                currencyTypes.Add(normalized);

                _lexer.Read();
                token = _lexer.Peek();
            }

            if (token.Type != TokenType.SEMICOLON)
            {
                parseIssues.Add(new UnexpectedTokenException(token, TokenType.SEMICOLON));
                return;
            }
            _lexer.Read();
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

                    if (_lexer.Peek().Type == TokenType.EOF) return;
                }
            }
        }

        private void skipToSemicolon()
        {
            var token = _lexer.Read();
            while (token.Type != TokenType.SEMICOLON)
            {
                token = _lexer.Peek();

                if (token.Type == TokenType.EOF)
                {
                    break;
                }

                token = _lexer.Read();
            }
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

            var token = _lexer.Peek();
            if (token.Type != TokenType.SEMICOLON)
            {
                parseIssues.Add(new UnexpectedTokenException(token, TokenType.SEMICOLON));
                return;
            }
            _lexer.Read();
        }

        private string readLineCurrencyFrom(Dictionary<(string CFrom, string CTo), decimal> currencyConvertion)
        {
            var token = _lexer.Read();

            if (token.Type != TokenType.IDENTIFIER)
            {
                throw new UnexpectedTokenException(token, TokenType.IDENTIFIER);
            }

            if (currencyConvertion.Keys.Any(x => x.CFrom.Equals(token.Lexeme)))
            {
                throw new DuplicatedCurrencyException(token);
            }

            return normalize(token.Lexeme!);
        }

        private void readLineCurrencyConversion(Dictionary<(string CFrom, string CTo), decimal> currencyConvertion, string currencyFrom, string currencyTo)
        {
            var token = _lexer.Read();

            if (token.Type != TokenType.LITERAL)
            {
                throw new UnexpectedTokenException(token, TokenType.LITERAL);
            }

            currencyConvertion.Add(
                (currencyFrom, currencyTo),
                token.DecimalValue ?? token.IntValue ?? throw new InvalidConversionValueException(token));
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
