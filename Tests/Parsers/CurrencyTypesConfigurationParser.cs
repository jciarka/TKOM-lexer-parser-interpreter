using Application.Infrastructure.ConfigurationParser;
using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.Lekser.SourceReaders;
using Application.Models.Exceptions;
using Application.Models.Exceptions.ConfigurationParser;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Parsers
{
    public class CurrencyTypesConfigurationParserTests
    {
        Mock<IErrorHandler> _errorHandlerMock;

        public CurrencyTypesConfigurationParserTests()
        {
            _errorHandlerMock = new Mock<IErrorHandler>();
        }

        [Fact]
        public void ShouldParseAllCurrienciesInHeaderRowWithNormalisation()
        {
            var content = @" USD   cad   EUR 	GBP   HkD  CHF   Jpy   auD  INR  CNY;";
            var lexer = new LexerEngine(new StringSourceReader(content));

            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);

            var config = parser.Parse();

            Assert.Equal("USD", config.currencyTypes[0]);
            Assert.Equal("CAD", config.currencyTypes[1]);
            Assert.Equal("EUR", config.currencyTypes[2]);
            Assert.Equal("GBP", config.currencyTypes[3]);
            Assert.Equal("HKD", config.currencyTypes[4]);
            Assert.Equal("CHF", config.currencyTypes[5]);
            Assert.Equal("JPY", config.currencyTypes[6]);
            Assert.Equal("AUD", config.currencyTypes[7]);
            Assert.Equal("INR", config.currencyTypes[8]);
            Assert.Equal("CNY", config.currencyTypes[9]);
        }

        [Fact]
        public void ShouldInvalidTokenNameRaiseIssueButNotBreakComputing()
        {
            var content = @" 
                        CanDol      GBP;
                CanDol       1      1.1;
                GBP        0.9        1;
            ";
            var lexer = new LexerEngine(new StringSourceReader(content));
            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);

            var config = parser.Parse();

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<ComputingException>()), Times.Once);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidCurrencyNameException>()), Times.Once);

            Assert.Equal("CANDOL", config.currencyTypes[0]);
            Assert.Equal("GBP", config.currencyTypes[1]);
        }

        [Fact]
        public void ShouldDuplicateCurrencyAddsIssue()
        {
            var content = @"USD USD; ";
            var lexer = new LexerEngine(new StringSourceReader(content));
            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);

            var config = parser.Parse();

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<DuplicatedCurrencyException>()), Times.Once);
        }

        [Fact]
        public void ShouldDuplicateCurrencyRowAddsIssue()
        {
            var content = @" 
                        USD      GBP;
                USD       1      1.1;
                USD     0.9        1;
            ";
            var lexer = new LexerEngine(new StringSourceReader(content));
            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);

            var config = parser.Parse();

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<DuplicatedCurrencyException>()), Times.Once);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("1D", 1)]
        [InlineData("1.0", 1)]
        public void ShouldParseDifferendNumericLiteralsFormat(string literal, decimal targetValue)
        {
            var content = $@"
                          USD ;
                USD {literal} ;
            ";

            var lexer = new LexerEngine(new StringSourceReader(content));
            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);
            var config = parser.Parse();

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<DuplicatedCurrencyException>()), Times.Never);
            Assert.Equal(targetValue, config.currencyConvertions[("USD", "USD")]);
        }

        [Fact]
        public void ShouldParseCorrectlyCurrencyArray()
        {
            var content = @"
                     USD   CAD   EUR;
                EUR	0.99  1.35     1;
                USD	   1  1.36  1.01;
                CAD	0.73     1  0.74;
            ";

            var lexer = new LexerEngine(new StringSourceReader(content));
            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);
            var config = parser.Parse();

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<DuplicatedCurrencyException>()), Times.Never);

            Assert.Contains("USD", config.currencyTypes);
            Assert.Contains("CAD", config.currencyTypes);
            Assert.Contains("EUR", config.currencyTypes);

            Assert.Equal(0.99M, config.currencyConvertions[("EUR", "USD")]);
            Assert.Equal(1.35M, config.currencyConvertions[("EUR", "CAD")]);
            Assert.Equal(1M, config.currencyConvertions[("EUR", "EUR")]);

            Assert.Equal(1M, config.currencyConvertions[("USD", "USD")]);
            Assert.Equal(1.36M, config.currencyConvertions[("USD", "CAD")]);
            Assert.Equal(1.01M, config.currencyConvertions[("USD", "EUR")]);

            Assert.Equal(0.73M, config.currencyConvertions[("CAD", "USD")]);
            Assert.Equal(1M, config.currencyConvertions[("CAD", "CAD")]);
            Assert.Equal(0.74M, config.currencyConvertions[("CAD", "EUR")]);
        }

        [Fact]
        public void ShouldInvalidTokenRaiseIssueButNotBreakComputing()
        {
            var content = @" 
                        USD      GBP;
                USD   wrong      1.1;
                GBP     0.9        1;
            ";
            var lexer = new LexerEngine(new StringSourceReader(content));
            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);

            var config = parser.Parse();

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<ComputingException>()), Times.Once);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<UnexpectedTokenException>()), Times.Once);
        }

        [Fact]
        public void ShouldMissingLineRaiseIssueButNotBreakComputing()
        {
            var content = @" 
                        USD      GBP;
                GBP     0.9        1;
            ";
            var lexer = new LexerEngine(new StringSourceReader(content));
            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);

            var config = parser.Parse();

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<ComputingException>()), Times.Once);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<UnexpectedTokenException>()), Times.Once);
        }

        [Fact]
        public void ShouldMissingSemicolonRaiseErrorButNorBrakeComputing()
        {
            var content = @"
                     USD   CAD ;
                USD	   1  1.36 
                CAD	0.73     1 ;
            ";

            var lexer = new LexerEngine(new StringSourceReader(content));
            var parser = new ConfigurationParserEngine(lexer, _errorHandlerMock.Object);
            var config = parser.Parse();

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<ComputingException>()), Times.Once);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<UnexpectedTokenException>()), Times.Once);

            Assert.Contains("USD", config.currencyTypes);
            Assert.Contains("CAD", config.currencyTypes);

            Assert.Equal(1M, config.currencyConvertions[("USD", "USD")]);
            Assert.Equal(1.36M, config.currencyConvertions[("USD", "CAD")]);

            Assert.Equal(0.73M, config.currencyConvertions[("CAD", "USD")]);
            Assert.Equal(1M, config.currencyConvertions[("CAD", "CAD")]);

        }
    }
}
