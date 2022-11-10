using Application.Infrastructure.Lekser;
using Application.Infrastructure.Lekser.SourceReaders;
using Application.Models.Lexer.Exceptions;
using Application.Models.Tokens;
using Application.Models.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace Tests.Lexer
{
    public class LexereTests
    {
        [Fact]
        public void ShouldEmptyTextEndWithEof()
        {
            var reader = new StringSourceReader("");
            var lexer = new LexerEngine(reader);

            var token = lexer.Read();

            Assert.Equal(TokenType.EOF, token.Type);
        }

        [Theory]
        [MemberData(nameof(getFixedTextTokenLexems))]
        public void ShouldFixedTextGiveAppropriateTokenTypeAndEndWithEof(string tokenLexeme, TokenType targetType)
        {
            var reader = new StringSourceReader(tokenLexeme);
            var lexer = new LexerEngine(reader);

            var token = lexer.Read();

            Assert.Equal(targetType, token.Type);

            if (token.Type != TokenType.EOF)
            {
                token = lexer.Read();
                Assert.Equal(TokenType.EOF, token.Type);
            }
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("10")]
        [InlineData("100")]
        [InlineData("9999")]
        public void ShouldIntLiteraGiveNumberTokenWithIntType(string integer)
        {
            var reader = new StringSourceReader(integer);
            var lexer = new LexerEngine(reader);

            var token = lexer.Read();

            Assert.Equal(TokenType.LITERAL, token.Type);
            Assert.Equal(integer, token.Lexeme);
            Assert.Equal(int.Parse(integer), token.IntValue);
            Assert.Equal("int", token.ValueType);
        }

        [Theory]
        [InlineData("1.0")]
        [InlineData("100.99")]
        [InlineData("9999.9999")]
        [InlineData("1D")]
        public void ShouldDecimalLiteraGiveNumberTokenWitDecimalType(string number)
        {
            var reader = new StringSourceReader(number);
            var lexer = new LexerEngine(reader);

            var token = lexer.Read();

            var bareDecimalString = String.Concat(number.Where(x => !char.IsLetter(x)));
            var bareDecimal = decimal.Parse(bareDecimalString, new NumberFormatInfo { NumberDecimalSeparator = "." });

            Assert.Equal(TokenType.LITERAL, token.Type);
            Assert.Equal(bareDecimal, token.DecimalValue);
            Assert.Equal(number, token.Lexeme);
            Assert.Equal("decimal", token.ValueType);
        }

        [Theory]
        [InlineData("1PLN", "PLN")]
        [InlineData("1.99PLN", "PLN")]
        [InlineData("100.99PLN", "PLN")]
        [InlineData("9999.99PLN", "PLN")]
        public void ShouldCurrencyLiteralGiveNumberTokenWithIntType(string number, string currency)
        {
            var reader = new StringSourceReader(number);
            var lexer = new LexerEngine(reader, new LexerOptions { TypesInfo = new TypesInfoProvider(new string[] { "PLN", "USD" }) });

            var token = lexer.Read();

            var bareDecimalString = String.Concat(number.Where(x => !char.IsLetter(x)));
            var bareDecimal = decimal.Parse(bareDecimalString, new NumberFormatInfo { NumberDecimalSeparator = "." });

            Assert.Equal(TokenType.LITERAL, token.Type);
            Assert.Equal(bareDecimal, token.DecimalValue);
            Assert.Equal(number, token.Lexeme);
            Assert.Equal(currency, token.ValueType);
        }

        [Theory]
        [InlineData("1.120.1")]
        [InlineData("1D1")]
        public void ShouldInvlaidNumberIdentifierThrowError(string literal)
        {
            var reader = new StringSourceReader(literal);
            var lexer = new LexerEngine(reader);

            Assert.Throws<InvalidLiteralException>(() => lexer.Read());
        }

        [Theory]
        [InlineData("\"test\"", "test")]
        [InlineData("\"test\\\\test\"", "test\\test")]
        [InlineData("\"test\\ntest\"", "test\ntest")]
        [InlineData("\"test\\\"test\"", "test\"test")]
        [InlineData("\"test\\ttest\"", "test\ttest")]
        public void ShouldStringLiteralGiveStringTokenWithWrightValue(string lexeme, string targetValue)
        {
            var reader = new StringSourceReader(lexeme);
            var lexer = new LexerEngine(reader);

            var token = lexer.Read();

            Assert.Equal(TokenType.LITERAL, token.Type);
            Assert.Equal(targetValue, token.StringValue);
            Assert.Equal(lexeme, token.Lexeme);
            Assert.Equal("string", token.ValueType);
        }

        [Theory]
        [InlineData("\"test")]
        [InlineData("\"test\n\"")]
        public void ShouldInvlaidStringIdentifierThrowError(string literal)
        {
            var reader = new StringSourceReader(literal);
            var lexer = new LexerEngine(reader);

            Assert.Throws<InvalidLiteralException>(() => lexer.Read());
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public void ShouldBoolLiteralGiveLiteralTokenWithCorrectValue(string content)
        {
            var reader = new StringSourceReader(content);
            var lexer = new LexerEngine(reader);

            var token = lexer.Read();

            Assert.Equal(TokenType.LITERAL, token.Type);
            Assert.Equal(content.Equals("true") ? true : false, token.BoolValue);
            Assert.Equal("bool", token.ValueType);
            Assert.Equal(content, token.Lexeme);
        }

        [Theory]
        [InlineData("myVariable")]
        [InlineData("my_variable")]
        public void ShouldIdentifierGiveTokenWithWrightValue(string identifier)
        {
            var reader = new StringSourceReader(identifier);
            var lexer = new LexerEngine(reader);

            var token = lexer.Read();

            Assert.Equal(TokenType.IDENTIFIER, token.Type);
            Assert.Equal(identifier, token.Lexeme);
        }

        [Fact]
        public void ShouldTooLongIdentifierRaiseError()
        {
            var content = "myVariabl myVariable";
            var reader = new StringSourceReader(content);
            var lexer = new LexerEngine(reader, new LexerOptions { LiteralMaxLength = 9 });

            var token = lexer.Read();

            Assert.Throws<TooLongIdentifierException>(() => lexer.Read());
        }

        [Fact]
        public void ShouldCommentGiveTokenOfTypeCommentAndFinishATheEndOfLine()
        {
            var builder = new StringBuilder();
            var content = "# my comment is placed here";

            builder.AppendLine(content);
            builder.AppendLine("test");

            var reader = new StringSourceReader(builder.ToString());
            var lexer = new LexerEngine(reader);

            var token = lexer.Read();

            Assert.Equal(TokenType.COMMENT, token.Type);
            Assert.Equal(content, token.Lexeme);

            token = lexer.Read();

            Assert.Equal(TokenType.IDENTIFIER, token.Type);
            Assert.Equal("test", token.Lexeme);

            token = lexer.Read();

            Assert.Equal(TokenType.EOF, token.Type);
        }


        [Theory]
        [InlineData("@")]
        [InlineData("$")]
        [InlineData("^")]
        [InlineData("&")]
        [InlineData("_")]
        [InlineData("?")]
        [InlineData("|")]
        [InlineData("`")]
        public void ShouldUnknownTokenRaiseError(string character)
        {
            var reader = new StringSourceReader(character);
            var lexer = new LexerEngine(reader);

            Assert.Throws<UnexpectedCharacterException>(() => lexer.Read());
        }

        [Fact]
        public void ShouldAnalyseCurrencyConfigurationCorrectly()
        {
            var content = @"
                     USD   CAD   EUR 	GBP   HKD 	 CHF    JPY   AUD 	  INR  CNY;
                USD	   1  1.36  1.01  0.87   7.85     1  148.74  1.56  82.73   7.3;
                CAD	0.73     1  0.74  0.64   5.77  0.74  109.27  1.15  60.78  5.36;
                EUR	0.99  1.35     1  0.86   7.76  0.99  147.04  1.54  81.78  7.22;
                GBP	1.15  1.56  1.16     1	   9   1.15  170.57	 1.79  94.87  8.37;
                HKD	0.13  0.17  0.13  0.11     1   0.13   18.95   0.2  10.54  0.93;
                CHF	   1  1.36  1.01  0.87  7.84      1   148.5  1.56   82.6  7.29;
                JPY	0.01  0.01  0.01  0.01  0.05   0.01       1	 0.01   0.56  0.05;
                AUD	0.64  0.87  0.65  0.56  5.03   0.64   95.34     1  53.03  4.68;
                INR	0.01  0.02  0.01  0.01  0.09   0.01     1.8	 0.02      1  0.09;
                CNY	0.14  0.19  0.14  0.12  1.07   0.14   20.37  0.21  11.33     1;
            ";


            var reader = new StringSourceReader(content);
            var lexer = new LexerEngine(reader);

            Token token;

            for (int i = 0; i < 10; i++)
            {
                token = lexer.Read();

                Assert.Equal(TokenType.IDENTIFIER, token.Type);
                Assert.Equal(3, token.Lexeme!.Length);
            }

            token = lexer.Read();
            Assert.Equal(TokenType.SEMICOLON, token.Type);

            for (int i = 0; i < 10; i++)
            {
                token = lexer.Read();

                Assert.Equal(TokenType.IDENTIFIER, token.Type);
                Assert.Equal(3, token.Lexeme!.Length);

                for (int j = 0; j < 10; j++)
                {
                    token = lexer.Read();

                    Assert.Equal(TokenType.LITERAL, token.Type);
                }

                token = lexer.Read();
                Assert.Equal(TokenType.SEMICOLON, token.Type);
            }

            token = lexer.Read();
            Assert.Equal(TokenType.EOF, token.Type);
        }

        private static IEnumerable<object[]> getFixedTextTokenLexems() =>
            TokenHelpers.TokenLexems.Select(x => new object[] { x.Lexeme!, x.Type! });
    }
}
