using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.SourceParser;
using Application.Models;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Parsers
{
    public class SourceParserTests
    {
        int tokenIndex = 0;
        Mock<ILexer> _lexerMock;
        Mock<IErrorHandler> _errorHandlerMock;

        public SourceParserTests()
        {
            _errorHandlerMock = new Mock<IErrorHandler>();
            _lexerMock = new Mock<ILexer>();
        }

        [Fact]
        public void ShouldIntLiteralBeParsedAsLiteral()
        {
            prepareLexerMoq(new List<Token> { new Token() { Type = TokenType.LITERAL, IntValue = 1, ValueType = TypeName.INT, Position = new CharacterPosition() } });

            var parserEngine = new SourceParserEngine(_lexerMock.Object, new ParserOptions(), _errorHandlerMock.Object);

            bool success = invokePrivateMethod(parserEngine, "tryParseLiteral", out var result);

            Assert.True(success);
            Assert.IsType<Literal>(result);

            Literal actual = (Literal)result!;

            Assert.Equal(1, actual.IntValue);
            Assert.Equal(TypeEnum.INT, actual.Type.Type);
            Assert.Equal(TypeName.INT, actual.Type.Name);
        }

        [Fact]
        public void ShouldDecimalLiteralBeParsedAsLiteral()
        {
            prepareLexerMoq(new List<Token> { new Token() { Type = TokenType.LITERAL, DecimalValue = 1M, ValueType = TypeName.DECIMAL, Position = new CharacterPosition() } });

            var parserEngine = new SourceParserEngine(_lexerMock.Object, new ParserOptions(), _errorHandlerMock.Object);

            bool success = invokePrivateMethod(parserEngine, "tryParseLiteral", out var result);

            Assert.True(success);
            Assert.IsType<Literal>(result);

            Literal actual = (Literal)result!;

            Assert.Equal(1M, actual.DecimalValue);
            Assert.Equal(TypeEnum.DECIMAL, actual.Type.Type);
            Assert.Equal(TypeName.DECIMAL, actual.Type.Name);
        }

        [Fact]
        public void ShouldStringLiteralBeParsedAsLiteral()
        {
            prepareLexerMoq(new List<Token> { new Token() { Type = TokenType.LITERAL, StringValue = "TEST", ValueType = TypeName.STRING, Position = new CharacterPosition() } });

            var parserEngine = new SourceParserEngine(_lexerMock.Object, new ParserOptions(), _errorHandlerMock.Object);

            bool success = invokePrivateMethod(parserEngine, "tryParseLiteral", out var result);

            Assert.True(success);
            Assert.IsType<Literal>(result);

            Literal actual = (Literal)result!;

            Assert.Equal("TEST", actual.StringValue);
            Assert.Equal(TypeEnum.STRING, actual.Type.Type);
            Assert.Equal(TypeName.STRING, actual.Type.Name);
        }

        [Fact]
        public void ShouldCurrencyLiteralBeParsedAsLiteral()
        {
            prepareLexerMoq(new List<Token> { new Token() { Type = TokenType.LITERAL, DecimalValue = 1M, ValueType = "PLN", Position = new CharacterPosition() } });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions() { TypesInfo = new TypesInfoProvider(new List<string> { "PLN" }) },
                _errorHandlerMock.Object);

            bool success = invokePrivateMethod(parserEngine, "tryParseLiteral", out var result);

            Assert.True(success);
            Assert.IsType<Literal>(result);

            Literal actual = (Literal)result!;

            Assert.Equal(1M, actual.DecimalValue);
            Assert.Equal(TypeEnum.CURRENCY, actual.Type.Type);
            Assert.Equal("PLN", actual.Type.Name);
        }

        [Fact]
        public void ShouldTypeLiteralBeParsedAsLiteral()
        {
            prepareLexerMoq(new List<Token> { new Token() { Type = TokenType.TYPE, ValueType = TypeName.INT, Position = new CharacterPosition() } });

            var parserEngine = new SourceParserEngine(_lexerMock.Object, new ParserOptions(), _errorHandlerMock.Object);

            bool success = invokePrivateMethod(parserEngine, "tryParseTypeOrConstructor", out var result);

            Assert.True(success);
            Assert.IsType<Literal>(result);

            Literal actual = (Literal)result!;

            Assert.Equal(TypeEnum.TYPE, actual.Type.Type);
            Assert.Equal(TypeName.INT, actual.ValueType!.Name);
            Assert.Equal(TypeEnum.INT, actual.ValueType!.Type);
        }

        [Fact]
        public void ShouldConstructorBeParsedAsConstructor()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.TYPE, ValueType = TypeName.ACCOUNT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LESS, Position = new CharacterPosition() },
                new Token() { Type = TokenType.TYPE, ValueType = "PLN", Position = new CharacterPosition() },
                new Token() { Type = TokenType.GREATER, DecimalValue = 1M, ValueType = "PLN", Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions() { TypesInfo = new TypesInfoProvider(new List<string> { "PLN" }) },
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseTypeOrConstructor", out var result);

            Assert.True(success);
            Assert.IsType<ConstructiorCallExpr>(result);

            var actual = (ConstructiorCallExpr)result!;

            Assert.Equal(TypeEnum.GENERIC, actual.Type.Type);
            Assert.Equal(TypeName.ACCOUNT, actual.Type.Name);
            Assert.Empty(actual.Arguments!);
        }

        [Fact]
        public void ShouldFunctionCallBeParsedAsFunctionCall()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "print", Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.COMMA, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseIdentifierOrFunctionCall", out var result);

            Assert.True(success);
            Assert.IsType<FunctionCallExpr>(result);

            var actual = (FunctionCallExpr)result!;

            Assert.Equal("print", actual.Name);
            Assert.Equal(2, actual.Arguments.Count());
            Assert.Equal("a", ((actual.Arguments.First() as ExpressionArgument)!.Expression as Identifier)!.Name);
            Assert.Equal("b", ((actual.Arguments.Last() as ExpressionArgument)!.Expression as Identifier)!.Name);
        }

        [Fact]
        public void ShouldIdentifierBeParsedAsIdentifier()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "identifier_name", Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseIdentifierOrFunctionCall", out var result);

            Assert.True(success);
            Assert.IsType<Identifier>(result);

            var actual = (Identifier)result!;

            Assert.Equal("identifier_name", actual.Name);
        }

        [Fact]
        public void ShouldParetisedExpressionBeParsedAsExpression()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "identifier_expression", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseParentisedExpression", out var result);

            Assert.True(success);
            Assert.IsType<Identifier>(result);

            var actual = (Identifier)result!;

            Assert.Equal("identifier_expression", actual.Name);
        }

        [Fact]
        public void ShouldExpressionLambdaBeParsedAsLambda()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.LAMBDA, Position = new CharacterPosition() },
                new Token() { Type = TokenType.TYPE, ValueType = TypeName.INT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.ARROW, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "identifier_expression", Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseLambdaArgument", out var result);

            Assert.True(success);
            Assert.IsType<Lambda>(result);

            var actual = (Lambda)result!;

            Assert.Equal("x", actual.Parameter.Identifier);
            Assert.Equal(TypeEnum.INT, actual.Parameter.Type.Type);
            Assert.Equal(TypeName.INT, actual.Parameter.Type.Name);
            Assert.IsType<ExpressionStmt>(actual.Stmt);
            Assert.IsType<Identifier>((actual.Stmt as ExpressionStmt)!.RightExpression);
        }

        [Fact]
        public void ShouldBlockLambdaBeParsedAsLambda()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.LAMBDA, Position = new CharacterPosition() },
                new Token() { Type = TokenType.TYPE, ValueType = TypeName.INT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.ARROW, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_BRACE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RETURN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACE, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseLambdaArgument", out var result);

            Assert.True(success);
            Assert.IsType<Lambda>(result);

            var actual = (Lambda)result!;

            Assert.Equal("x", actual.Parameter.Identifier);
            Assert.Equal(TypeEnum.INT, actual.Parameter.Type.Type);
            Assert.Equal(TypeName.INT, actual.Parameter.Type.Name);
            Assert.IsType<BlockStmt>(actual.Stmt);
        }

        [Theory]
        [InlineData(TokenType.STAR)]
        [InlineData(TokenType.SLASH)]
        public void ShouldMultiplicativeExprBeParsedAsMultiplicativeExpression(TokenType @operator)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.STAR, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = @operator, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseMultiplicativeExpr", out var result);

            Assert.True(success);
            Assert.IsType<MultiplicativeExpr>(result);

            var actual = (MultiplicativeExpr)result!;

            Assert.IsType<Identifier>(actual.FirstOperand);

            Assert.Equal(TokenType.STAR, actual.Operands.First().Item1);
            Assert.IsType<Identifier>(actual.Operands.First().Item2);

            Assert.Equal(@operator, actual.Operands.Last().Item1);
            Assert.IsType<Literal>(actual.Operands.Last().Item2);
        }

        [Theory]
        [InlineData(TokenType.PLUS)]
        [InlineData(TokenType.MINUS)]
        public void ShouldAdditiveExprBeParsedAsAdditiveExpression(TokenType @operator)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.PLUS, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = @operator, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseAdditiveExpr", out var result);

            Assert.True(success);
            Assert.IsType<AdditiveExpr>(result);

            var actual = (AdditiveExpr)result!;

            Assert.IsType<Identifier>(actual.FirstOperand);

            Assert.Equal(TokenType.PLUS, actual.Operands.First().Item1);
            Assert.IsType<Identifier>(actual.Operands.First().Item2);

            Assert.Equal(@operator, actual.Operands.Last().Item1);
            Assert.IsType<Literal>(actual.Operands.Last().Item2);
        }

        [Theory]
        [InlineData(TokenType.EQUAL_EQUAL)]
        [InlineData(TokenType.BANG_EQUAL)]
        [InlineData(TokenType.GREATER)]
        [InlineData(TokenType.GREATER_EQUAL)]
        [InlineData(TokenType.LESS)]
        [InlineData(TokenType.LESS_EQUAL)]
        public void ShouldComparativeExprBeParsedAsAdditiveExpression(TokenType @operator)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL_EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = @operator, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseComparativeExpr", out var result);

            Assert.True(success);
            Assert.IsType<ComparativeExpr>(result);

            var actual = (ComparativeExpr)result!;

            Assert.IsType<Identifier>(actual.FirstOperand);

            Assert.Equal(TokenType.EQUAL_EQUAL, actual.Operands.First().Item1);
            Assert.IsType<Identifier>(actual.Operands.First().Item2);

            Assert.Equal(@operator, actual.Operands.Last().Item1);
            Assert.IsType<Literal>(actual.Operands.Last().Item2);
        }

        [Fact]
        public void ShouldAndExprBeParsedAsAndExpression()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.AND, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.AND, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseAndExpression", out var result);

            Assert.True(success);
            Assert.IsType<AndExpr>(result);

            var actual = (AndExpr)result!;

            Assert.IsType<Identifier>(actual.FirstOperand);
            Assert.IsType<Identifier>(actual.Operands.First());
            Assert.IsType<Literal>(actual.Operands.Last());
        }

        [Fact]
        public void ShouldOrExprBeParsedAsOrExpression()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.OR, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.OR, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseExpression", out var result);

            Assert.True(success);
            Assert.IsType<OrExpr>(result);

            var actual = (OrExpr)result!;

            Assert.IsType<Identifier>(actual.FirstOperand);
            Assert.IsType<Identifier>(actual.Operands.First());
            Assert.IsType<Literal>(actual.Operands.Last());
        }

        [Fact]
        public void ShouldObjectPropertyExprBeParsedAsObjectPropertyExpression()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.DOT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseObjectExpr", out var result);

            Assert.True(success);
            Assert.IsType<ObjectPropertyExpr>(result);

            var actual = (ObjectPropertyExpr)result!;

            Assert.IsType<Identifier>(actual.Object);
            Assert.Equal("x", ((Identifier)actual.Object).Name);

            Assert.Equal("y", actual.Property);
        }

        [Fact]
        public void ShouldObjectMethodExprBeParsedAsObjectMethodExpression()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.DOT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.COMMA, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseObjectExpr", out var result);

            Assert.True(success);
            Assert.IsType<ObjectMethodExpr>(result);

            var actual = (ObjectMethodExpr)result!;

            Assert.IsType<Identifier>(actual.Object);
            Assert.Equal("x", ((Identifier)actual.Object).Name);
            Assert.Equal(2, actual.Arguments.Count());
            Assert.Equal("a", ((actual.Arguments.First() as ExpressionArgument)!.Expression as Identifier)!.Name);
            Assert.Equal("b", ((actual.Arguments.Last() as ExpressionArgument)!.Expression as Identifier)!.Name);
        }

        [Fact]
        public void ShouldObjectIndexExprBeParsedAsObjectIndexExpression()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_BRACKET, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "i", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACKET, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseObjectExpr", out var result);

            Assert.True(success);
            Assert.IsType<ObjectIndexExpr>(result);

            var actual = (ObjectIndexExpr)result!;

            Assert.IsType<Identifier>(actual.Object);
            Assert.Equal("x", ((Identifier)actual.Object).Name);
            Assert.IsType<Identifier>(actual.IndexExpression);
            Assert.Equal("i", ((Identifier)actual.IndexExpression).Name);
        }

        [Theory]
        [InlineData(TokenType.VAR, "var", null)]
        [InlineData(TokenType.TYPE, "int", TypeEnum.INT)]
        public void ShouldDeclarationStmtBeParsedAsDeclaractionStatement(TokenType tokenType, string lexeme, TypeEnum? typeEnum)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = tokenType, ValueType = lexeme, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseDeclarationStmt", out var result);

            Assert.True(success);
            Assert.IsType<DeclarationStmt>(result);

            var actual = (DeclarationStmt)result!;

            Assert.Equal(typeEnum, actual.Type?.Type);
            Assert.Equal("x", actual.Identifier.Name);
        }

        [Theory]
        [InlineData(TokenType.TRANSFER_TO)]
        [InlineData(TokenType.TRANSFER_PRCT_TO)]
        public void ShouldFinancialToExprBeParsedAsFinancialToExpression(TokenType type)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = type, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "v", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<FinancialToStmt>(result);

            var actual = (FinancialToStmt)result!;

            Assert.IsType<Identifier>(actual.AccountExpression);
            Assert.Equal("x", ((Identifier)actual.AccountExpression).Name);
            Assert.IsType<Identifier>(actual.ValueExpression);
            Assert.Equal("v", ((Identifier)actual.ValueExpression).Name);
        }

        [Theory]
        [InlineData(TokenType.TRANSFER_FROM)]
        [InlineData(TokenType.TRANSFER_PRCT_FROM)]
        public void ShouldWithdrawFinancialFromExprBeParsedAsFinancialToExpression(TokenType type)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = type, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "v", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<FinancialFromStmt>(result);

            var actual = (FinancialFromStmt)result!;

            Assert.IsType<Identifier>(actual.AccountFromExpression);
            Assert.Equal("x", ((Identifier)actual.AccountFromExpression).Name);
            Assert.IsType<Identifier>(actual.ValueExpression);
            Assert.Equal("v", ((Identifier)actual.ValueExpression).Name);
            Assert.Null(actual.AccountToExpression);
        }


        [Theory]
        [InlineData(TokenType.TRANSFER_FROM)]
        [InlineData(TokenType.TRANSFER_PRCT_FROM)]
        public void ShouldMoveFinancialFromExprBeParsedAsFinancialToExpression(TokenType type)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = type, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "v", Position = new CharacterPosition() },
                new Token() { Type = TokenType.TRANSFER_FROM, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<FinancialFromStmt>(result);

            var actual = (FinancialFromStmt)result!;

            Assert.IsType<Identifier>(actual.AccountFromExpression);
            Assert.Equal("x", ((Identifier)actual.AccountFromExpression).Name);
            Assert.IsType<Identifier>(actual.ValueExpression);
            Assert.Equal("v", ((Identifier)actual.ValueExpression).Name);
            Assert.IsType<Identifier>(actual.AccountToExpression);
            Assert.Equal("y", ((Identifier)actual.AccountToExpression!).Name);
        }

        private static bool invokePrivateMethod(SourceParserEngine parserEngine, string methodName, out GrammarRuleBase? result)
        {
            var args = new object[] { null! };
            var method = parserEngine.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            var success = (bool)method!.Invoke(parserEngine, args)!;
            result = args[0] as GrammarRuleBase;
            return success;
        }

        [Fact]
        public void ShouldIdentifierAssignmentStmtBeParsedAsAssignmentStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<IdentifierAssignmentStatement>(result);

            var actual = (IdentifierAssignmentStatement)result!;

            Assert.Equal("x", actual.Identifier.Name);
            Assert.IsType<Literal>(actual.Expression);
        }

        [Fact]
        public void ShouldPropertyAssignmentStmtBeParsedAsAssignmentStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.DOT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<PropertyAssignmentStatement>(result);

            var actual = (PropertyAssignmentStatement)result!;

            Assert.IsType<ObjectPropertyExpr>(actual.Property);
            Assert.IsType<Literal>(actual.Expression);
        }

        [Fact]
        public void ShouldIndexAssignmentStmtBeParsedAsAssignmentStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.DOT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_BRACKET, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "i", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACKET, Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LITERAL, ValueType = TypeName.INT, IntValue = 1, Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<IndexAssignmentStatement>(result);

            var actual = (IndexAssignmentStatement)result!;

            Assert.IsType<ObjectIndexExpr>(actual.IndexExpr);
            Assert.IsType<Literal>(actual.Expression);
        }

        [Fact]
        public void ShouldEmptyReturnStmtBeParsedAsReturnStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.RETURN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<ReturnStmt>(result);

            var actual = (ReturnStmt)result!;
            Assert.Null(actual.ReturnExpression);
        }

        [Fact]
        public void ShouldReturnStmtBeParsedAsReturnStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.RETURN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() }
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<ReturnStmt>(result);

            var actual = (ReturnStmt)result!;

            Assert.IsType<Identifier>(actual.ReturnExpression);
            Assert.Equal("x", ((Identifier)actual.ReturnExpression!).Name);
        }

        [Fact]
        public void ShouldExpressionIfStatementBeParsedAsIfStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IF, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL_EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
                new Token() { Type = TokenType.ELSE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "c", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "d", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<IfStmt>(result);

            var actual = (IfStmt)result!;

            Assert.IsType<ComparativeExpr>(actual.Condition);
            Assert.IsType<IdentifierAssignmentStatement>(actual.ThenStatement);
            Assert.IsType<IdentifierAssignmentStatement>(actual.ElseStatement);
        }

        [Fact]
        public void ShouldBlockIfStatementBeParsedAsIfStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.IF, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL_EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_BRACE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.ELSE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_BRACE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "c", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "d", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACE, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<IfStmt>(result);

            var actual = (IfStmt)result!;

            Assert.IsType<ComparativeExpr>(actual.Condition);
            Assert.IsType<BlockStmt>(actual.ThenStatement);
            Assert.IsType<BlockStmt>(actual.ElseStatement);
        }

        [Theory]
        [InlineData(TokenType.TYPE, "int", TypeEnum.INT)]
        public void ShouldExpressionForeachStatementBeParsedAsForeachStatement(TokenType tokenType, string lexeme, TypeEnum? typeEnum)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.FOREACH, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = tokenType, ValueType = lexeme, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.IN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "col", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<ForeachStmt>(result);

            var actual = (ForeachStmt)result!;

            Assert.Equal("x", actual.Parameter.Identifier);
            Assert.Equal(typeEnum, actual.Parameter.Type?.Type);
            Assert.IsType<IdentifierAssignmentStatement>(actual.Statement);
        }

        [Theory]
        [InlineData(TokenType.TYPE, "int", TypeEnum.INT)]
        public void ShouldBlockForeachStatementBeParsedAsForeachStatement(TokenType tokenType, string lexeme, TypeEnum? typeEnum)
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.FOREACH, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = tokenType, ValueType = lexeme, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.IN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "col", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_BRACE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACE, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<ForeachStmt>(result);

            var actual = (ForeachStmt)result!;

            Assert.Equal("x", actual.Parameter.Identifier);
            Assert.Equal(typeEnum, actual.Parameter.Type?.Type);
            Assert.IsType<BlockStmt>(actual.Statement);
        }

        [Fact]
        public void ShouldBlockStatementBeParsedAsBlockStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.LEFT_BRACE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACE, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<BlockStmt>(result);

            var actual = (BlockStmt)result!;

            Assert.Single(actual!.Statements);
            Assert.IsType<IdentifierAssignmentStatement>(actual.Statements.First());
        }

        [Fact]
        public void ShouldFunctionDeclarationBeParsedAsFunctionDeclaration()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.TYPE, ValueType = TypeName.INT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "function_name", Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.TYPE, ValueType = TypeName.INT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.COMMA, Position = new CharacterPosition() },
                new Token() { Type = TokenType.TYPE, ValueType = TypeName.INT, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_BRACE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACE, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseFunctionDeclaration", out var result);

            Assert.True(success);
            Assert.IsType<FunctionDecl>(result);
            var actual = (FunctionDecl)result!;
        }

        [Fact]
        public void ShouldExpressionWhileStatementBeParsedAsIfStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.WHILE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL_EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<WhileStmt>(result);

            var actual = (WhileStmt)result!;

            Assert.IsType<ComparativeExpr>(actual.Condition);
            Assert.IsType<IdentifierAssignmentStatement>(actual.Statement);
        }

        [Fact]
        public void ShouldBlockWhileStatementBeParsedAsIfStatement()
        {
            prepareLexerMoq(new List<Token> {
                new Token() { Type = TokenType.WHILE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "x", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL_EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "y", Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_PAREN, Position = new CharacterPosition() },
                new Token() { Type = TokenType.LEFT_BRACE, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "a", Position = new CharacterPosition() },
                new Token() { Type = TokenType.EQUAL, Position = new CharacterPosition() },
                new Token() { Type = TokenType.IDENTIFIER, Lexeme = "b", Position = new CharacterPosition() },
                new Token() { Type = TokenType.SEMICOLON, Position = new CharacterPosition() },
                new Token() { Type = TokenType.RIGHT_BRACE, Position = new CharacterPosition() },
            });

            var parserEngine = new SourceParserEngine(
                _lexerMock.Object,
                new ParserOptions(),
                _errorHandlerMock.Object
            );

            bool success = invokePrivateMethod(parserEngine, "tryParseStatement", out var result);

            Assert.True(success);
            Assert.IsType<WhileStmt>(result);

            var actual = (WhileStmt)result!;

            Assert.IsType<ComparativeExpr>(actual.Condition);
            Assert.IsType<BlockStmt>(actual.Statement);
        }

        private void prepareLexerMoq(IList<Token> tokens)
        {
            _lexerMock.Setup(x => x.Current).Returns(() => tokenIndex < tokens.Count ? tokens[tokenIndex] : new Token { Type = TokenType.EOF, Position = new CharacterPosition() });
            _lexerMock.Setup(x => x.Advance()).Callback(() => tokenIndex++);
        }
    }
}
