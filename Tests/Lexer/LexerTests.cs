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

            var token = lexer.Current;

            Assert.Equal(TokenType.EOF, token.Type);
        }

        [Theory]
        [MemberData(nameof(getFixedTextTokenLexems))]
        public void ShouldFixedTextGiveAppropriateTokenTypeAndEndWithEof(string tokenLexeme, TokenType targetType)
        {
            var reader = new StringSourceReader(tokenLexeme);
            var lexer = new LexerEngine(reader);

            Assert.Equal(targetType, lexer.Current.Type);

            if (lexer.Current.Type != TokenType.EOF)
            {
                lexer.Advance();
                Assert.Equal(TokenType.EOF, lexer.Current.Type);
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

            Assert.Equal(TokenType.LITERAL, lexer.Current.Type);
            Assert.Equal(integer, lexer.Current.Lexeme);
            Assert.Equal(int.Parse(integer), lexer.Current.IntValue);
            Assert.Equal("int", lexer.Current.ValueType);
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

            var bareDecimalString = String.Concat(number.Where(x => !char.IsLetter(x)));
            var bareDecimal = decimal.Parse(bareDecimalString, new NumberFormatInfo { NumberDecimalSeparator = "." });

            Assert.Equal(TokenType.LITERAL, lexer.Current.Type);
            Assert.Equal(bareDecimal, lexer.Current.DecimalValue);
            Assert.Equal(number, lexer.Current.Lexeme);
            Assert.Equal("decimal", lexer.Current.ValueType);
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

            var bareDecimalString = String.Concat(number.Where(x => !char.IsLetter(x)));
            var bareDecimal = decimal.Parse(bareDecimalString, new NumberFormatInfo { NumberDecimalSeparator = "." });

            Assert.Equal(TokenType.LITERAL, lexer.Current.Type);
            Assert.Equal(bareDecimal, lexer.Current.DecimalValue);
            Assert.Equal(number, lexer.Current.Lexeme);
            Assert.Equal(currency, lexer.Current.ValueType);
        }

        [Theory]
        [InlineData("1.120.1")]
        [InlineData("1D1")]
        public void ShouldInvlaidNumberIdentifierThrowError(string literal)
        {
            var reader = new StringSourceReader(literal);
            Assert.Throws<InvalidLiteralException>(() => new LexerEngine(reader));
        }

        [Theory]
        [InlineData("10000000000000000000000000000000")]
        [InlineData("10000000000000000000000000000000.11111111111111111111111111111111")]
        public void ShouldToobigNumberIdentifierThrowOverflowError(string literal)
        {
            var reader = new StringSourceReader(literal);
            Assert.Throws<TypeOverflowException>(() => new LexerEngine(reader));
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

            Assert.Equal(TokenType.LITERAL, lexer.Current.Type);
            Assert.Equal(targetValue, lexer.Current.StringValue);
            Assert.Equal(lexeme, lexer.Current.Lexeme);
            Assert.Equal("string", lexer.Current.ValueType);
        }

        [Theory]
        [InlineData("\"test")]
        [InlineData("\"test\n\"")]
        public void ShouldInvlaidStringIdentifierThrowError(string literal)
        {
            var reader = new StringSourceReader(literal);

            Assert.Throws<InvalidLiteralException>(() => new LexerEngine(reader));
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public void ShouldBoolLiteralGiveLiteralTokenWithCorrectValue(string content)
        {
            var reader = new StringSourceReader(content);
            var lexer = new LexerEngine(reader);

            Assert.Equal(TokenType.LITERAL, lexer.Current.Type);
            Assert.Equal(content.Equals("true") ? true : false, lexer.Current.BoolValue);
            Assert.Equal("bool", lexer.Current.ValueType);
            Assert.Equal(content, lexer.Current.Lexeme);
        }

        [Theory]
        [InlineData("myVariable")]
        [InlineData("my_variable")]
        public void ShouldIdentifierGiveTokenWithWrightValue(string identifier)
        {
            var reader = new StringSourceReader(identifier);
            var lexer = new LexerEngine(reader);

            Assert.Equal(TokenType.IDENTIFIER, lexer.Current.Type);
            Assert.Equal(identifier, lexer.Current.Lexeme);
        }

        [Fact]
        public void ShouldTooLongIdentifierRaiseError()
        {
            var content = "myVariabl myVariable";
            var reader = new StringSourceReader(content);
            var lexer = new LexerEngine(reader, new LexerOptions { LiteralMaxLength = 9 });

            Assert.Throws<TooLongIdentifierException>(() => lexer.Advance());
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

            Assert.Equal(TokenType.COMMENT, lexer.Current.Type);
            Assert.Equal(content, lexer.Current.Lexeme);

            lexer.Advance();

            Assert.Equal(TokenType.IDENTIFIER, lexer.Current.Type);
            Assert.Equal("test", lexer.Current.Lexeme);

            lexer.Advance();

            Assert.Equal(TokenType.EOF, lexer.Current.Type);
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

            Assert.Throws<UnexpectedCharacterException>(() => new LexerEngine(reader));
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

            for (int i = 0; i < 10; i++)
            {
                Assert.Equal(TokenType.IDENTIFIER, lexer.Current.Type);
                Assert.Equal(3, lexer.Current.Lexeme!.Length);
                lexer.Advance();
            }

            Assert.Equal(TokenType.SEMICOLON, lexer.Current.Type);
            lexer.Advance();

            for (int i = 0; i < 10; i++)
            {
                Assert.Equal(TokenType.IDENTIFIER, lexer.Current.Type);
                Assert.Equal(3, lexer.Current.Lexeme!.Length);

                lexer.Advance();

                for (int j = 0; j < 10; j++)
                {
                    Assert.Equal(TokenType.LITERAL, lexer.Current.Type);
                    lexer.Advance();
                }

                Assert.Equal(TokenType.SEMICOLON, lexer.Current.Type);
                lexer.Advance();
            }

            Assert.Equal(TokenType.EOF, lexer.Current.Type);
        }

        private static IEnumerable<object[]> getFixedTextTokenLexems() =>
            TokenHelpers.AllMap.Select(x => new object[] { x.Key, x.Value });
    }
}
