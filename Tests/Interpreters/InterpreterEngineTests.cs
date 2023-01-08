using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Interpreter;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.Presenters;
using Application.Models;
using Application.Models.ConfigurationParser;
using Application.Models.Exceptions.Interpreter;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;
using Application.Models.Values;
using Application.Models.Values.BasicTypeValues;
using Application.Models.Values.NativeLibrary;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Interpreters
{
    public class InterpreterEngineTests
    {
        Mock<IErrorHandler> _errorHandlerMock;
        private readonly InterpreterEngine _sut;

        public InterpreterEngineTests()
        {
            _errorHandlerMock = new Mock<IErrorHandler>();
            _sut = new InterpreterEngine(_errorHandlerMock.Object, new InterpreterEngineOptions
            {
                CurrencyTypesInfo = new CurrencyTypesInfo()
                {
                    currencyTypes = new List<string> { "PLN", "USD" },
                    currencyConvertions = new Dictionary<(string CFrom, string CTo), decimal> {
                        { ("PLN", "PLN"), 1 },
                        { ("PLN", "USD"), 0.25M },
                        { ("USD", "USD"), 1 },
                        { ("USD", "PLN"), 4 },
                    }
                }
            });

        }

        [Theory]
        [MemberData(nameof(getTestBasicLiteralValues))]
        public void ShouldBasicLiteralValueBeEqualToVisitableValue(IValue expected, IVisitable literal)
        {
            var actual = invokeAcceptMethod(literal);
            Assert.True(expected.EqualEqual(actual).Value);
        }

        private static IEnumerable<object[]> getTestBasicLiteralValues() => new List<object[]> {
            new object[] {
                new IntValue(0),
                new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { Type = TokenType.LITERAL, IntValue = 0}, emptyPosition())
            },

            new object[] {
                new IntValue(1),
                new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { Type = TokenType.LITERAL, IntValue = 1}, emptyPosition())
            },

            new object[] {
                new DecimalValue(0M),
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { Type = TokenType.LITERAL, DecimalValue = 0}, emptyPosition())
            },

            new object[] {
                new DecimalValue(0.99M),
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { Type = TokenType.LITERAL, DecimalValue = 0.99M}, emptyPosition())
            },

            new object[] {
                new BoolValue(false),
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { Type = TokenType.LITERAL, BoolValue = false}, emptyPosition())
            },

            new object[] {
                new BoolValue(true),
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { Type = TokenType.LITERAL, BoolValue = true}, emptyPosition())
            },

           new object[] {
                new StringValue("TeSt tEsT\n"),
                new Literal(new BasicType(TypeName.STRING, TypeEnum.STRING), new Token() { Type = TokenType.LITERAL, StringValue = "TeSt tEsT\n"}, emptyPosition())
            },

            new object[] {
                new CurrencyValue("PLN", 0M),
                new Literal(new BasicType("PLN", TypeEnum.CURRENCY), new Token() { Type = TokenType.LITERAL, DecimalValue = 0, ValueType = "PLN"}, emptyPosition())
            },

            new object[] {
                new CurrencyValue("CHF", 0.99M),
                new Literal(new BasicType("CHF", TypeEnum.CURRENCY), new Token() { Type = TokenType.LITERAL, DecimalValue = 0.99M, ValueType = "CHF"}, emptyPosition())
            },
        };

        [Fact]
        public void ShouldAccountConstructorReturnInstance()
        {
            var node = new ConstructiorCallExpr(
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                    new ExpressionArgument[] { },
                    emptyPosition()
                );

            setContext();

            var value = invokeAcceptMethod(node);

            Assert.Equal(TypeName.ACCOUNT, value.Type.Name);
            Assert.Equal(TypeEnum.GENERIC, value.Type.Type);
            Assert.Equal(TypeEnum.CURRENCY, ((GenericType)value.Type).ParametrisingType.Type);
        }


        [Fact]
        public void ShouldCollectionConstructorReturnInstance()
        {
            var node = new ConstructiorCallExpr(
                    new GenericType(TypeName.COLLECTION, new BasicType("PLN", TypeEnum.CURRENCY)),
                    new ExpressionArgument[] { },
                    emptyPosition()
                );

            setContext();

            var value = invokeAcceptMethod(node);

            Assert.Equal(TypeName.COLLECTION, value.Type.Name);
            Assert.Equal(TypeEnum.GENERIC, value.Type.Type);
            Assert.Equal(TypeEnum.CURRENCY, ((GenericType)value.Type).ParametrisingType.Type);
        }

        [Fact]
        public void ShouldFunctionCallReturnWrightReturnValue()
        {
            var name = TypeName.INT;
            var typeEnum = TypeEnum.INT;
            var value = 1;

            var returnType = new BasicType(name, typeEnum);

            var testDecl = new FunctionDecl(
                returnType,
                "test",
                new Parameter[] { },
                new BlockStmt(
                    new IStatement[]
                    {
                        new ReturnStmt(emptyPosition(), new Literal(new BasicType(name, typeEnum), new Token() { IntValue = value }, emptyPosition()))
                    },
                    emptyPosition()),
                emptyPosition());

            setContext(new Dictionary<FunctionSignature, ICallable> { { new FixedArgumentsFunctionSignature(testDecl), new Callable(testDecl) } });

            var node = new FunctionCallExpr(
                        "test",
                        new IArgument[] { },
                        emptyPosition()
                    );

            var returnValue = (IntValue)invokeAcceptMethod(node);

            Assert.Equal(value, returnValue.Value);
        }

        [Fact]
        public void ShouldPropertyExpressionReturnItsValue()
        {
            setContext();

            var constructorExpr = new ConstructiorCallExpr(
                     new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                     new ExpressionArgument[]
                     {
                         new ExpressionArgument(
                             new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = 100M }, emptyPosition()),
                             emptyPosition())
                     },
                     emptyPosition()
                 );

            var node1 = new ObjectPropertyExpr(
                 constructorExpr,
                 "Ballance",
                 emptyPosition()
            );

            var node2 = new ObjectPropertyExpr(
                 constructorExpr,
                 "Currency",
                 emptyPosition()
            );

            var returnValue1 = (DecimalValue)invokeAcceptMethod(node1);
            var returnValue2 = (TypeValue)invokeAcceptMethod(node2);

            Assert.Equal(100M, returnValue1.Value);
            Assert.Equal("PLN", returnValue2.Value.Name);
            Assert.Equal(TypeEnum.CURRENCY, returnValue2.Value.Type);
        }

        [Fact]
        public void ShouldMethodExpressionReturnItsValue()
        {
            setContext();

            var constructorExpr = new ConstructiorCallExpr(
                     new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                     new ExpressionArgument[]
                     {
                         new ExpressionArgument(
                             new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = 100M }, emptyPosition()),
                             emptyPosition())
                     },
                     emptyPosition()
                 );

            var node1 = new ObjectMethodExpr(
                 constructorExpr,
                 "Copy",
                 new IArgument[] { },
                 emptyPosition()
            );


            var returnValue = (Reference)invokeAcceptMethod(node1);

            Assert.Equal(TypeName.ACCOUNT, returnValue.Type.Name);
            Assert.Equal(100M, ((DecimalValue)returnValue.Instance!.GetProperty("Ballance")).Value);
            Assert.Equal(new BasicType("PLN", TypeEnum.CURRENCY), ((TypeValue)returnValue.Instance!.GetProperty("Currency")).Value);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void ShouldAndExprReturnsValue(bool first, bool second, bool expectedResult)
        {
            var node = new AndExpr(
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = first }, emptyPosition()),
                new List<IExpression>()
                {
                    new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = second }, emptyPosition())
                },
                emptyPosition()
            );

            var result = ((BoolValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public void ShouldOrExprReturnsValue(bool first, bool second, bool expectedResult)
        {
            var node = new OrExpr(
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = first }, emptyPosition()),
                new List<IExpression>()
                {
                    new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = second }, emptyPosition())
                },
                emptyPosition()
            );

            var result = ((BoolValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 0, TokenType.PLUS, 0)]
        [InlineData(1, 0, TokenType.PLUS, 1)]
        [InlineData(1, 1, TokenType.PLUS, 2)]
        [InlineData(1, -1, TokenType.PLUS, 0)]
        [InlineData(-1, -1, TokenType.PLUS, -2)]
        [InlineData(0, 0, TokenType.MINUS, 0)]
        [InlineData(1, 0, TokenType.MINUS, 1)]
        [InlineData(1, 1, TokenType.MINUS, 0)]
        [InlineData(1, -1, TokenType.MINUS, 2)]
        [InlineData(-1, -1, TokenType.MINUS, 0)]
        public void ShouldIntAddReturnsValue(int first, int second, TokenType @operator, int expectedResult)
        {
            var node = new AdditiveExpr(
                new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = first }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = second }, emptyPosition()))
                },
                emptyPosition()
            );

            var result = ((IntValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 0, TokenType.PLUS, 0)]
        [InlineData(1, 0, TokenType.PLUS, 1)]
        [InlineData(1, 1, TokenType.PLUS, 2)]
        [InlineData(1, -1, TokenType.PLUS, 0)]
        [InlineData(-1, -1, TokenType.PLUS, -2)]
        [InlineData(0, 0, TokenType.MINUS, 0)]
        [InlineData(1, 0, TokenType.MINUS, 1)]
        [InlineData(1, 1, TokenType.MINUS, 0)]
        [InlineData(1, -1, TokenType.MINUS, 2)]
        [InlineData(-1, -1, TokenType.MINUS, 0)]
        public void ShouldDecimalAddReturnsValue(decimal first, decimal second, TokenType @operator, decimal expectedResult)
        {
            var node = new AdditiveExpr(
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = first }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = second }, emptyPosition()))
                },
                emptyPosition()
            );

            var result = ((DecimalValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(10, 0, TokenType.GREATER, true)]
        [InlineData(10, 10, TokenType.GREATER, false)]
        [InlineData(0, 10, TokenType.GREATER, false)]
        [InlineData(10, 0, TokenType.GREATER_EQUAL, true)]
        [InlineData(10, 10, TokenType.GREATER_EQUAL, true)]
        [InlineData(0, 10, TokenType.GREATER_EQUAL, false)]
        [InlineData(10, 0, TokenType.LESS, false)]
        [InlineData(10, 10, TokenType.LESS, false)]
        [InlineData(0, 10, TokenType.LESS, true)]
        [InlineData(10, 0, TokenType.LESS_EQUAL, false)]
        [InlineData(10, 10, TokenType.LESS_EQUAL, true)]
        [InlineData(0, 10, TokenType.LESS_EQUAL, true)]
        [InlineData(10, 0, TokenType.EQUAL_EQUAL, false)]
        [InlineData(10, 10, TokenType.EQUAL_EQUAL, true)]
        [InlineData(0, 10, TokenType.EQUAL_EQUAL, false)]
        [InlineData(10, 0, TokenType.BANG_EQUAL, true)]
        [InlineData(10, 10, TokenType.BANG_EQUAL, false)]
        [InlineData(0, 10, TokenType.BANG_EQUAL, true)]
        public void ShouldIntComparisonReturnsValue(int first, int second, TokenType @operator, bool expectedResult)
        {
            var node = new ComparativeExpr(
                new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = first }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = second }, emptyPosition()))
                },
                emptyPosition()
            );

            var result = ((BoolValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(10, 0, TokenType.GREATER, true)]
        [InlineData(10, 10, TokenType.GREATER, false)]
        [InlineData(0, 10, TokenType.GREATER, false)]
        [InlineData(10, 0, TokenType.GREATER_EQUAL, true)]
        [InlineData(10, 10, TokenType.GREATER_EQUAL, true)]
        [InlineData(0, 10, TokenType.GREATER_EQUAL, false)]
        [InlineData(10, 0, TokenType.LESS, false)]
        [InlineData(10, 10, TokenType.LESS, false)]
        [InlineData(0, 10, TokenType.LESS, true)]
        [InlineData(10, 0, TokenType.LESS_EQUAL, false)]
        [InlineData(10, 10, TokenType.LESS_EQUAL, true)]
        [InlineData(0, 10, TokenType.LESS_EQUAL, true)]
        [InlineData(10, 0, TokenType.EQUAL_EQUAL, false)]
        [InlineData(10, 10, TokenType.EQUAL_EQUAL, true)]
        [InlineData(0, 10, TokenType.EQUAL_EQUAL, false)]
        [InlineData(10, 0, TokenType.BANG_EQUAL, true)]
        [InlineData(10, 10, TokenType.BANG_EQUAL, false)]
        [InlineData(0, 10, TokenType.BANG_EQUAL, true)]
        public void ShouldDecimalComparisonReturnsValue(decimal first, decimal second, TokenType @operator, bool expectedResult)
        {
            var node = new ComparativeExpr(
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = first }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = second }, emptyPosition()))
                },
                emptyPosition()
            );

            var result = ((BoolValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, false, TokenType.EQUAL_EQUAL, false)]
        [InlineData(false, true, TokenType.EQUAL_EQUAL, false)]
        [InlineData(false, false, TokenType.EQUAL_EQUAL, true)]
        [InlineData(true, true, TokenType.EQUAL_EQUAL, true)]
        [InlineData(true, false, TokenType.BANG_EQUAL, true)]
        [InlineData(false, true, TokenType.BANG_EQUAL, true)]
        [InlineData(false, false, TokenType.BANG_EQUAL, false)]
        [InlineData(true, true, TokenType.BANG_EQUAL, false)]
        public void ShouldBoolComparisonReturnsValue(bool first, bool second, TokenType @operator, bool expectedResult)
        {
            var node = new ComparativeExpr(
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = first }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = second }, emptyPosition()))
                },
                emptyPosition()
            );

            var result = ((BoolValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("TeSt", "test", TokenType.EQUAL_EQUAL, false)]
        [InlineData("test", "test", TokenType.EQUAL_EQUAL, true)]
        [InlineData("TeSt", "test", TokenType.BANG_EQUAL, true)]
        [InlineData("test", "test", TokenType.BANG_EQUAL, false)]
        public void ShouldStringComparisonReturnsValue(string first, string second, TokenType @operator, bool expectedResult)
        {
            var node = new ComparativeExpr(
                new Literal(new BasicType(TypeName.STRING, TypeEnum.STRING), new Token() { StringValue = first }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(TypeName.STRING, TypeEnum.STRING), new Token() { StringValue = second }, emptyPosition()))
                },
                emptyPosition()
            );

            var result = ((BoolValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 10, TokenType.STAR, 0)]
        [InlineData(10, 10, TokenType.STAR, 100)]
        [InlineData(10, -10, TokenType.STAR, -100)]
        [InlineData(-10, -10, TokenType.STAR, 100)]
        [InlineData(0, 10, TokenType.SLASH, 0)]
        [InlineData(10, 10, TokenType.SLASH, 1)]
        [InlineData(10, -10, TokenType.SLASH, -1)]
        [InlineData(-10, -10, TokenType.SLASH, 1)]
        public void ShouldDecimalMultiplicationReturnsValue(decimal first, decimal second, TokenType @operator, decimal expectedResult)
        {
            var node = new MultiplicativeExpr(
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = first }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = second }, emptyPosition()))
                },
                emptyPosition()
            );

            var result = ((DecimalValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 10, TokenType.STAR, 0)]
        [InlineData(10, 10, TokenType.STAR, 100)]
        [InlineData(10, -10, TokenType.STAR, -100)]
        [InlineData(-10, -10, TokenType.STAR, 100)]
        [InlineData(0, 10, TokenType.SLASH, 0)]
        [InlineData(10, 10, TokenType.SLASH, 1)]
        [InlineData(10, -10, TokenType.SLASH, -1)]
        [InlineData(-10, -10, TokenType.SLASH, 1)]
        public void ShouldIntMultiplicationReturnsValue(int first, int second, TokenType @operator, int expectedResult)
        {
            var node = new MultiplicativeExpr(
                new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = first }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = second }, emptyPosition()))
                },
                emptyPosition()
            );

            var result = ((IntValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ShouldIntZeroMultiplicationRaise()
        {
            var node = new MultiplicativeExpr(
                new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = 10 }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(TokenType.SLASH, (IExpression)new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = 0 }, emptyPosition()))
                },
                emptyPosition()
            );

            Assert.Throws<ZeroDivisionException>(() => ((IntValue)invokeAcceptMethod(node)).Value);
        }

        [Fact]
        public void ShouldIDecimalZeroMultiplicationRaise()
        {
            var node = new MultiplicativeExpr(
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = 10M }, emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(TokenType.SLASH, (IExpression)new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = 0 }, emptyPosition()))
                },
                emptyPosition()
            );

            Assert.Throws<ZeroDivisionException>(() => ((IntValue)invokeAcceptMethod(node)).Value);
        }

        [Fact]
        public void ShouldIntToDeciamlReturnDecimal()
        {
            setContext();

            var node = new ConversionExpr(
                new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = 10 }, emptyPosition()),
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), emptyPosition()),
                emptyPosition()
            );

            var result = ((DecimalValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(10, result);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(10.1, 10)]
        [InlineData(10.9, 10)]
        public void ShouldDeciamlToIntReturnInt(decimal from, int expectedTo)
        {
            setContext();

            var node = new ConversionExpr(
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = from }, emptyPosition()),
                new Literal(new BasicType(TypeName.INT, TypeEnum.INT), emptyPosition()),
                emptyPosition()
            );

            var result = ((IntValue)invokeAcceptMethod(node)).Value;

            Assert.Equal(expectedTo, result);
        }

        [Theory]
        [InlineData("USD", 10, "PLN", 40)]
        [InlineData("USD", 10, "USD", 10)]
        [InlineData("PLN", 10, "USD", 2.5)]
        [InlineData("PLN", 10, "PLN", 10)]
        public void ShouldCurrencyToCurrencyReturnCurrencyWithExchangedValue(
            string fromCurrency, decimal fromValue, string toCurrency, decimal expectedToValue)
        {
            setContext();

            var node = new ConversionExpr(
                new Literal(new BasicType(fromCurrency, TypeEnum.CURRENCY), new Token() { DecimalValue = fromValue }, emptyPosition()),
                new Literal(new BasicType(toCurrency, TypeEnum.CURRENCY), emptyPosition()),
                emptyPosition()
            );

            var result = ((CurrencyValue)invokeAcceptMethod(node));

            Assert.Equal(expectedToValue, result.Value);
            Assert.Equal(new BasicType(toCurrency, TypeEnum.CURRENCY), result.Type);
        }

        [Fact]
        public void ShouldCurrencyToDeciamlReturnDecimal()
        {
            setContext();

            var node = new ConversionExpr(
                new Literal(new BasicType("PLN", TypeEnum.CURRENCY), new Token() { DecimalValue = 10 }, emptyPosition()),
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), emptyPosition()),
                emptyPosition()
            );

            var result = (DecimalValue)invokeAcceptMethod(node);

            Assert.Equal(10, result.Value);
            Assert.Equal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), result.Type);
        }

        [Fact]
        public void IdentifierReturnsItsValueWhenDeclared()
        {
            setContext();

            var declaration = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token() { IntValue = 10 }, emptyPosition()),
                    new BasicType(TypeName.INT, TypeEnum.INT)
                );

            invokeAcceptMethod(declaration);

            var reference = new Identifier("test", emptyPosition());

            var value = (IntValue)invokeAcceptMethod(reference);

            Assert.Equal(TypeEnum.INT, value.Type.Type);
            Assert.Equal(TypeName.INT, value.Type.Name);
            Assert.Equal(10, value.Value);
        }

        private void setContext(Dictionary<FunctionSignature, ICallable>? parsedFunctions = null)
        {
            parsedFunctions = parsedFunctions ?? new();

            var functions = NativeLibraryProvider.GetFunctions()
                .ToDictionary(x => x.Signature, x => (ICallable)x)
                .Union(parsedFunctions)
                .ToDictionary(x => x.Key, x => x.Value);

            var prototypes = NativeLibraryProvider.GetClassPrototypes()
                .ToDictionary(x => x.Create().Name, x => (IClassPrototype)x);

            var contextField = _sut.GetType().GetField("_programContext", BindingFlags.NonPublic | BindingFlags.Instance);

            contextField!.SetValue(_sut,
                new ProgramCallContext(new CallableSet(functions), new ClassSet(prototypes)));


            var callContextField = _sut.GetType().GetField("_functionContext", BindingFlags.NonPublic | BindingFlags.Instance);

            callContextField!.SetValue(_sut,
                new FunctionCallContext(new Parameter[] { }, new IValue[] { }));
        }

        [Fact]
        public void LambdaExpressionReturnsLambdaInstance()
        {
            setContext();

            var node = new Lambda(
                new Parameter(new BasicType(TypeName.INT, TypeEnum.INT), "test", emptyPosition()),
                new ExpressionStmt(new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token(), emptyPosition()), emptyPosition()),
                emptyPosition());

            var value = (Reference)invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.GENERIC, value.Type.Type);
            Assert.Equal(TypeName.LAMBDA, value.Type.Name);
            Assert.NotNull(value.Instance);
            Assert.IsType<DelegateInstance>(value.Instance);
        }

        [Fact]
        public void FinancialFormStatementRaisesWhenNullReference()
        {
            setContext();

            var declaration = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    null,
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY))
                );

            invokeAcceptMethod(declaration);


            var node = new FinancialFromStmt(
                new Identifier("test", emptyPosition()),
                TokenType.TRANSFER_FROM,
                new Literal(new BasicType("USD", TypeEnum.CURRENCY), new Token() { DecimalValue = 10 }, emptyPosition()),
                emptyPosition());

            Assert.Throws<RuntimeNullReferenceException>(() => invokeAcceptMethod(node));
        }

        [Fact]
        public void FinancialToStatementRaisesWhenNullReference()
        {
            setContext();

            var declaration = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    null,
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY))
                );

            invokeAcceptMethod(declaration);


            var node = new FinancialToStmt(
                new Identifier("test", emptyPosition()),
                TokenType.TRANSFER_TO,
                new Literal(new BasicType("USD", TypeEnum.CURRENCY), new Token() { DecimalValue = 10 }, emptyPosition()),
                emptyPosition());

            Assert.Throws<RuntimeNullReferenceException>(() => invokeAcceptMethod(node));
        }

        [Fact]
        public void FinancialPrctFormStatementRaisesWhenNullReference()
        {
            setContext();

            var declaration = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    null,
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY))
                );

            invokeAcceptMethod(declaration);


            var node = new FinancialFromStmt(
                new Identifier("test", emptyPosition()),
                TokenType.TRANSFER_PRCT_FROM,
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = 10 }, emptyPosition()),
                emptyPosition());

            Assert.Throws<RuntimeNullReferenceException>(() => invokeAcceptMethod(node));
        }

        [Fact]
        public void FinancialPrctToStatementRaisesWhenNullReference()
        {
            setContext();

            var declaration = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    null,
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY))
                );

            invokeAcceptMethod(declaration);

            var node = new FinancialFromStmt(
                new Identifier("test", emptyPosition()),
                TokenType.TRANSFER_FROM,
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token() { DecimalValue = 10 }, emptyPosition()),
                emptyPosition());

            Assert.Throws<RuntimeNullReferenceException>(() => invokeAcceptMethod(node));
        }

        [Fact]
        public void ForeachStatementRaiseNullCollectionExprssion()
        {
            setContext();

            var declaration = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    null,
                    new GenericType(TypeName.COLLECTION, new BasicType("PLN", TypeEnum.CURRENCY))
                );

            invokeAcceptMethod(declaration);

            var node = new ForeachStmt(
                new Parameter(new BasicType(TypeName.INT, TypeEnum.INT), "test", emptyPosition()),
                new Identifier("test", emptyPosition()),
                new ExpressionStmt(new Literal(new BasicType("CHF", TypeEnum.CURRENCY), emptyPosition()), emptyPosition()),
                emptyPosition());

            Assert.Throws<RuntimeNullReferenceException>(() => invokeAcceptMethod(node));
        }

        private IValue invokeAcceptMethod(IVisitable node)
        {
            var args = new object[] { node };
            var method = _sut.GetType().GetMethod("accept", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                return (IValue)method!.Invoke(_sut, args)!;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        }

        private static RulePosition emptyPosition()
        {
            return new RulePosition(new CharacterPosition());
        }
    }
}
