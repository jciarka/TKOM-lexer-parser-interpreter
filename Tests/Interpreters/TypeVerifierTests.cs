using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Interpreter;
using Application.Infrastructure.Lekser;
using Application.Infrastructure.Presenters;
using Application.Models;
using Application.Models.Exceptions;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;
using Application.Models.Values;
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
    public class TypeVerifierTests
    {
        Mock<IErrorHandler> _errorHandlerMock;
        private readonly TypeVerifier _sut;

        public TypeVerifierTests()
        {
            _errorHandlerMock = new Mock<IErrorHandler>();
            _sut = new TypeVerifier(_errorHandlerMock.Object);
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL)]
        [InlineData(TypeName.STRING, TypeEnum.STRING)]
        public void ShouldBasicTypeLiteralReturnsItsType(string typeName, TypeEnum typeEnum)
        {
            var literal = new Literal(new BasicType(typeName, typeEnum), new Token(), emptyPosition());

            var type = invokeAcceptMethod(literal);

            Assert.Equal(type.Name, typeName);
            Assert.Equal(type.Type, typeEnum);
        }

        [Theory]
        [InlineData("PLN", TypeEnum.CURRENCY)]
        [InlineData("CHF", TypeEnum.CURRENCY)]
        public void ShouldCurrencyTypeLiteralReturnsItsType(string typeName, TypeEnum typeEnum)
        {
            var literal = new Literal(new BasicType(typeName, typeEnum), new Token(), emptyPosition());

            var type = invokeAcceptMethod(literal);

            Assert.Equal(type.Name, typeName);
            Assert.Equal(type.Type, typeEnum);
        }

        [Fact]
        public void ShouldAccountConstructorReturnInstance()
        {
            var node = new ConstructiorCallExpr(
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                    new ExpressionArgument[] { },
                    emptyPosition()
                );

            setContext();

            var type = (GenericType)invokeAcceptMethod(node);

            Assert.Equal(TypeName.ACCOUNT, type.Name);
            Assert.Equal(TypeEnum.GENERIC, type.Type);
            Assert.Equal(TypeEnum.CURRENCY, type.ParametrisingType.Type);
        }


        [Fact]
        public void ShouldCollectionConstructorReturnInstance()
        {
            var node = new ConstructiorCallExpr(
                    new GenericType(TypeName.COLLECTION, new BasicType(TypeName.INT, TypeEnum.INT)),
                    new ExpressionArgument[] { },
                    emptyPosition()
                );

            setContext();

            var type = (GenericType)invokeAcceptMethod(node);

            Assert.Equal(TypeName.COLLECTION, type.Name);
            Assert.Equal(TypeEnum.GENERIC, type.Type);
            Assert.Equal(TypeEnum.INT, type.ParametrisingType.Type);
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL)]
        [InlineData(TypeName.STRING, TypeEnum.STRING)]
        public void FunctionCallReturnItsReturnType(string name, TypeEnum typeEnum)
        {
            var returnType = new BasicType(name, typeEnum);

            var testDecl = new FunctionDecl(
                returnType,
                "test",
                new Parameter[] { },
                new BlockStmt(new BlockStmt[] { }, emptyPosition()),
                emptyPosition());

            var testSignature = new FixedArgumentsFunctionSignature(testDecl);

            setContext(new Dictionary<FunctionSignature, TypeBase>() { { testSignature, returnType } });

            var node = new FunctionCallExpr(
                        "test",
                        new IArgument[] { },
                        emptyPosition()
                    );


            var type = invokeAcceptMethod(node);

            Assert.Equal(typeEnum, type.Type);
            Assert.Equal(name, type.Name);
        }

        [Fact]
        public void FunctionCallOfNotdeclaredFunctionRaises()
        {
            setContext();

            var node = new FunctionCallExpr(
                        "test",
                        new IArgument[] { },
                        emptyPosition()
                    );

            TypeBase returnType = invokeAcceptMethod(node);

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<FunctionNotDeclaredException>()));
        }

        [Theory]
        [InlineData(TypeName.ACCOUNT, "Ballance", TypeEnum.DECIMAL)]
        [InlineData(TypeName.ACCOUNT, "Currency", TypeEnum.TYPE)]
        public void ShouldPropertyExpressionReturnPropertyType(string calssName, string propName, TypeEnum returnType)
        {
            setContext();

            var node = new ObjectPropertyExpr(
                 new ConstructiorCallExpr(
                     new GenericType(calssName, new BasicType("PLN", TypeEnum.CURRENCY)),
                     new ExpressionArgument[] { },
                     emptyPosition()
                 ),
                 propName,
                 emptyPosition()
            );

            var type = invokeAcceptMethod(node);

            Assert.Equal(returnType, type.Type);
        }

        [Fact]
        public void ShouldPropertyExpressionraisesWhenPropertyNotExists()
        {
            setContext();

            var node = new ObjectPropertyExpr(
                 new ConstructiorCallExpr(
                     new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                     new ExpressionArgument[] { },
                     emptyPosition()
                 ),
                 "test",
                 emptyPosition()
            );

            var type = invokeAcceptMethod(node);

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<PropertyNotDeclaredException>()));
        }

        [Fact]
        public void ShouldMethodExpressionReturnPropertyType()
        {
            setContext();

            var parametrisingType = new BasicType(TypeName.INT, TypeEnum.INT);

            var node = new ObjectMethodExpr(
                 new ConstructiorCallExpr(
                     new GenericType(TypeName.COLLECTION, parametrisingType),
                     new ExpressionArgument[] { },
                     emptyPosition()
                 ),
                 "First",
                 new IArgument[]
                 {
                     new Lambda(
                        new Parameter(parametrisingType, "x", emptyPosition()),
                        new ExpressionStmt(new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token(), emptyPosition()), emptyPosition()),
                        emptyPosition())
                 },
                 emptyPosition()
            );

            var type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.INT, type.Type);
            Assert.Equal(TypeName.INT, type.Name);
        }

        [Fact]
        public void ShouldMethodExpressionRaisesWhenPropertyNotExists()
        {
            setContext();

            var parametrisingType = new BasicType(TypeName.INT, TypeEnum.INT);

            var node = new ObjectMethodExpr(
                 new ConstructiorCallExpr(
                     new GenericType(TypeName.COLLECTION, parametrisingType),
                     new ExpressionArgument[] { },
                     emptyPosition()
                 ),
                 "Test",
                 new IArgument[]
                 {
                     new Lambda(
                        new Parameter(parametrisingType, "x", emptyPosition()),
                        new ExpressionStmt(new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token(), emptyPosition()), emptyPosition()),
                        emptyPosition())
                 },
                 emptyPosition()
            );


            var type = invokeAcceptMethod(node);

            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<MethodNotDeclaredException>()));
        }

        [Fact]
        public void AndExprReturnsBoolTypeOnBoolOperands()
        {
            var node = new AndExpr(
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = true }, emptyPosition()),
                new List<IExpression>()
                {
                    new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = true }, emptyPosition())
                },
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.BOOL, type.Type);
            Assert.Equal(TypeName.BOOL, type.Name);
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT, TypeName.BOOL, TypeEnum.BOOL)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL, TypeName.INT, TypeEnum.INT)]
        [InlineData(TypeName.INT, TypeEnum.INT, TypeName.INT, TypeEnum.INT)]
        public void AndExprRaisesOnNotBoolOperands(string firstName, TypeEnum firstType, string secondName, TypeEnum secondType)
        {
            var node = new AndExpr(
                new Literal(new BasicType(firstName, firstType), new Token() { BoolValue = true }, emptyPosition()),
                new List<IExpression>()
                {
                    new Literal(new BasicType(secondName, secondType), new Token() { BoolValue = true }, emptyPosition())
                },
                emptyPosition()
            );

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void OrExprReturnsBoolTypeOnBoolOperands()
        {
            var node = new OrExpr(
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = true }, emptyPosition()),
                new List<IExpression>()
                {
                    new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = true }, emptyPosition())
                },
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.BOOL, type.Type);
            Assert.Equal(TypeName.BOOL, type.Name);
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT, TypeName.BOOL, TypeEnum.BOOL)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL, TypeName.INT, TypeEnum.INT)]
        [InlineData(TypeName.INT, TypeEnum.INT, TypeName.INT, TypeEnum.INT)]
        public void OrExprRaisesOnNotBoolOperands(string firstName, TypeEnum firstType, string secondName, TypeEnum secondType)
        {
            var node = new OrExpr(
                new Literal(new BasicType(firstName, firstType), new Token() { BoolValue = true }, emptyPosition()),
                new List<IExpression>()
                {
                    new Literal(new BasicType(secondName, secondType), new Token() { BoolValue = true }, emptyPosition())
                },
                emptyPosition()
            );

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL)]
        [InlineData(TypeName.STRING, TypeEnum.STRING)]
        public void ConversionExpressionReturnToType(string name, TypeEnum typeEnum)
        {
            var node = new ConversionExpr(
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token() { BoolValue = true }, emptyPosition()),
                new Literal(new BasicType(name, typeEnum), emptyPosition()),
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            Assert.Equal(typeEnum, type.Type);
            Assert.Equal(name, type.Name);
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT, TokenType.MINUS)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.MINUS)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL, TokenType.BANG)]
        public void NagationExpressionReturnSameTypeForNegatableTypes(string name, TypeEnum typeEnum, TokenType @operator)
        {
            var node = new NegativeExpr(
                @operator,
                new Literal(new BasicType(name, typeEnum), new Token(), emptyPosition()),
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            Assert.Equal(typeEnum, type.Type);
            Assert.Equal(name, type.Name);
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT, TokenType.BANG)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.BANG)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL, TokenType.MINUS)]
        [InlineData(TypeName.STRING, TypeEnum.STRING, TokenType.MINUS)]
        public void NagationExpressionOtherOperandAndOperatorCombinationRaises(string name, TypeEnum typeEnum, TokenType @operator)
        {
            var node = new NegativeExpr(
                @operator,
                new Literal(new BasicType(name, typeEnum), new Token(), emptyPosition()),
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT, TokenType.PLUS)]
        [InlineData(TypeName.INT, TypeEnum.INT, TokenType.MINUS)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.PLUS)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.MINUS)]
        public void AdditiveExpressionReturnsIntOrDecimalType(string name, TypeEnum typeEnum, TokenType @operator)
        {
            var node = new AdditiveExpr(
             new Literal(
                new BasicType(name, typeEnum), new Token(), emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(name, typeEnum), new Token(), emptyPosition())),
                },
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            Assert.Equal(typeEnum, type.Type);
            Assert.Equal(name, type.Name);
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT, TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.PLUS)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TypeName.INT, TypeEnum.INT, TokenType.MINUS)]
        [InlineData(TypeName.INT, TypeEnum.INT, TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.MINUS)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TypeName.INT, TypeEnum.INT, TokenType.PLUS)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL, TypeName.INT, TypeEnum.INT, TokenType.PLUS)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL, TypeName.INT, TypeEnum.INT, TokenType.MINUS)]
        [InlineData(TypeName.STRING, TypeEnum.STRING, TypeName.INT, TypeEnum.INT, TokenType.PLUS)]
        [InlineData(TypeName.STRING, TypeEnum.STRING, TypeName.INT, TypeEnum.INT, TokenType.MINUS)]
        public void AdditiveExpressionOtherOperandAndOperatorCombinationRaises(string firstName, TypeEnum firstType, string lastName, TypeEnum lastType, TokenType @operator)
        {
            var node = new AdditiveExpr(
            new Literal(
                new BasicType(firstName, firstType), new Token(), emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(lastName, lastType), new Token(), emptyPosition())),
                },
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT, TokenType.STAR)]
        [InlineData(TypeName.INT, TypeEnum.INT, TokenType.SLASH)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.STAR)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.SLASH)]
        public void MultiplicativeExpressionReturnsBoolType(string name, TypeEnum typeEnum, TokenType @operator)
        {
            var node = new MultiplicativeExpr(
             new Literal(
                new BasicType(name, typeEnum), new Token(), emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(name, typeEnum), new Token(), emptyPosition())),
                },
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            Assert.Equal(typeEnum, type.Type);
            Assert.Equal(name, type.Name);
        }


        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT, TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.STAR)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TypeName.INT, TypeEnum.INT, TokenType.SLASH)]
        [InlineData(TypeName.INT, TypeEnum.INT, TypeName.DECIMAL, TypeEnum.DECIMAL, TokenType.SLASH)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL, TypeName.INT, TypeEnum.INT, TokenType.STAR)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL, TypeName.INT, TypeEnum.INT, TokenType.STAR)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL, TypeName.INT, TypeEnum.INT, TokenType.SLASH)]
        [InlineData(TypeName.STRING, TypeEnum.STRING, TypeName.INT, TypeEnum.INT, TokenType.STAR)]
        [InlineData(TypeName.STRING, TypeEnum.STRING, TypeName.INT, TypeEnum.INT, TokenType.SLASH)]
        public void MultiplicativeExpressionOtherOperandAndOperatorCombinationRaises(string firstName, TypeEnum firstType, string lastName, TypeEnum lastType, TokenType @operator)
        {
            var node = new MultiplicativeExpr(
            new Literal(
                new BasicType(firstName, firstType), new Token(), emptyPosition()),
                new List<Tuple<TokenType, IExpression>>()
                {
                    Tuple.Create(@operator, (IExpression)new Literal(new BasicType(lastName, lastType), new Token(), emptyPosition())),
                },
                emptyPosition()
            );

            TypeBase type = invokeAcceptMethod(node);

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Theory]
        [InlineData(TypeName.INT, TypeEnum.INT)]
        [InlineData(TypeName.DECIMAL, TypeEnum.DECIMAL)]
        [InlineData(TypeName.BOOL, TypeEnum.BOOL)]
        [InlineData(TypeName.STRING, TypeEnum.STRING)]
        public void IdentifierHasTypeWhenDeclared(string name, TypeEnum typeEnum)
        {
            setContext();

            var declaration = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    new Literal(new BasicType(name, typeEnum), new Token(), emptyPosition()),
                    new BasicType(name, typeEnum)
                );

            invokeAcceptMethod(declaration);

            var reference = new Identifier("test", emptyPosition());

            var type = invokeAcceptMethod(reference);

            Assert.Equal(typeEnum, type.Type);
            Assert.Equal(name, type.Name);
        }

        [Fact]
        public void IdentifierRaisesWhenNotDeclared()
        {
            setContext();

            var node = new Identifier("test", emptyPosition());

            Assert.Throws<NotDefinedVariableException>(() => invokeAcceptMethod(node));
        }

        [Fact]
        public void LambdaExpressionReturnsLambdaType()
        {
            setContext();

            var node = new Lambda(
                new Parameter(new BasicType(TypeName.INT, TypeEnum.INT), "test", emptyPosition()),
                new ExpressionStmt(new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token(), emptyPosition()), emptyPosition()),
                emptyPosition());

            var type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.GENERIC, type.Type);
            Assert.Equal(TypeName.LAMBDA, type.Name);
        }

        [Fact]
        public void FinancialFormStatementRaisesWhenNoCurrencyValue()
        {
            setContext();

            var node = new FinancialFromStmt(
                new ConstructiorCallExpr(
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                    new IArgument[] { },
                    emptyPosition()),
                TokenType.TRANSFER_FROM,
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token(), emptyPosition()),
                emptyPosition());

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void FinancialFormStatementRaisesWhenNoAccount()
        {
            setContext();

            var node = new FinancialFromStmt(
                new Literal(new BasicType("CHF", TypeEnum.ACCOUNT), new Token(), emptyPosition()),
                TokenType.TRANSFER_FROM,
                new Literal(new BasicType("CHF", TypeEnum.ACCOUNT), new Token(), emptyPosition()),
                emptyPosition());

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void FinancialFormReturnNoneType()
        {
            setContext();

            var node = new FinancialFromStmt(
                new ConstructiorCallExpr(
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                    new IArgument[] { },
                    emptyPosition()),
                TokenType.TRANSFER_FROM,
                new Literal(new BasicType("CHF", TypeEnum.CURRENCY), new Token(), emptyPosition()),
                emptyPosition());

            var type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.NULL, type.Type);
        }

        [Fact]
        public void FinancialToStatementRaisesWhenNoCurrencyValue()
        {
            setContext();

            var node = new FinancialToStmt(
                new ConstructiorCallExpr(
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                    new IArgument[] { },
                    emptyPosition()),
                TokenType.TRANSFER_TO,
                new Literal(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL), new Token(), emptyPosition()),
                emptyPosition());

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void FinancialToStatementRaisesWhenNoAccount()
        {
            setContext();

            var node = new FinancialToStmt(
                new Literal(new BasicType("CHF", TypeEnum.ACCOUNT), new Token(), emptyPosition()),
                TokenType.TRANSFER_TO,
                new Literal(new BasicType("CHF", TypeEnum.ACCOUNT), new Token(), emptyPosition()),
                emptyPosition());

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void FinancialToReturnNoneType()
        {
            setContext();

            var node = new FinancialToStmt(
                new ConstructiorCallExpr(
                    new GenericType(TypeName.ACCOUNT, new BasicType("PLN", TypeEnum.CURRENCY)),
                    new IArgument[] { },
                    emptyPosition()),
                TokenType.TRANSFER_TO,
                new Literal(new BasicType("CHF", TypeEnum.CURRENCY), new Token(), emptyPosition()),
                emptyPosition());

            var type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.NULL, type.Type);
        }

        [Fact]
        public void ForeachStatementReturnsNoneType()
        {
            setContext();

            var node = new ForeachStmt(
                new Parameter(new BasicType(TypeName.INT, TypeEnum.INT), "test", emptyPosition()),
                new ConstructiorCallExpr(
                    new GenericType(TypeName.COLLECTION, new BasicType(TypeName.INT, TypeEnum.INT)),
                    new IArgument[] { },
                    emptyPosition()),
                new ExpressionStmt(new Literal(new BasicType("CHF", TypeEnum.CURRENCY), emptyPosition()), emptyPosition()),
                emptyPosition());

            var type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.NULL, type.Type);
        }

        [Fact]
        public void ForeachStatementRaiiseOnNotCollectionExprssion()
        {
            setContext();

            var node = new ForeachStmt(
                new Parameter(new BasicType(TypeName.INT, TypeEnum.INT), "test", emptyPosition()),
                new Literal(new BasicType("CHF", TypeEnum.CURRENCY), emptyPosition()),
                new ExpressionStmt(new Literal(new BasicType("CHF", TypeEnum.CURRENCY), emptyPosition()), emptyPosition()),
                emptyPosition());

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void IsStatementReturnsNoneType()
        {
            var node = new IfStmt(
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token(), emptyPosition()),
                new ExpressionStmt(new Literal(new BasicType("CHF", TypeEnum.CURRENCY), emptyPosition()), emptyPosition()),
                emptyPosition());

            var type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.NULL, type.Type);
        }

        [Fact]
        public void IfStatementRaiseOnNotBoolCondition()
        {
            var node = new IfStmt(
                 new Literal(new BasicType("CHF", TypeEnum.CURRENCY), new Token(), emptyPosition()),
                 new ExpressionStmt(new Literal(new BasicType("CHF", TypeEnum.CURRENCY), emptyPosition()), emptyPosition()),
                 emptyPosition());

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void WhileStatementReturnsNoneType()
        {
            var node = new WhileStmt(
                new Literal(new BasicType(TypeName.BOOL, TypeEnum.BOOL), new Token(), emptyPosition()),
                new ExpressionStmt(new Literal(new BasicType("CHF", TypeEnum.CURRENCY), emptyPosition()), emptyPosition()),
                emptyPosition());

            var type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.NULL, type.Type);
        }

        [Fact]
        public void WhileStatementRaiseOnNotBoolCondition()
        {
            var node = new WhileStmt(
                 new Literal(new BasicType("CHF", TypeEnum.CURRENCY), new Token(), emptyPosition()),
                 new ExpressionStmt(new Literal(new BasicType("CHF", TypeEnum.CURRENCY), emptyPosition()), emptyPosition()),
                 emptyPosition());

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void DeclarationStatementReturnsNoneType()
        {
            setContext();

            var node = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token(), emptyPosition()),
                    new BasicType(TypeName.INT, TypeEnum.INT)
                );

            var type = invokeAcceptMethod(node);

            Assert.Equal(TypeEnum.NULL, type.Type);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<ComputingException>()), Times.Never);
        }

        [Fact]
        public void DeclarationStatementRaisesWhenTypeNotMatchExpression()
        {
            setContext();

            var node = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition(),
                    new Literal(new BasicType(TypeName.INT, TypeEnum.INT), new Token(), emptyPosition()),
                    new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL)
                );

            var type = invokeAcceptMethod(node);

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<InvalidTypeException>()));
        }

        [Fact]
        public void DeclarationStatementRaisesWhenVarAndNoExpression()
        {
            setContext();

            var node = new DeclarationStmt(
                    new Identifier("test", emptyPosition()),
                    emptyPosition()
                );

            var type = invokeAcceptMethod(node);

            TypeBase returnType = invokeAcceptMethod(node);
            _errorHandlerMock.Verify(x => x.HandleError(It.IsAny<ComputingException>()));
        }

        private void setContext(Dictionary<FunctionSignature, TypeBase>? parsedFunctions = null)
        {
            parsedFunctions = parsedFunctions ?? new();

            var functions = NativeLibraryProvider.GetFunctions()
                .ToDictionary(x => x.Signature, x => x.Signature.ReturnType)
                .Union(parsedFunctions)
                .ToDictionary(x => x.Key, x => x.Value);

            var prototypes = NativeLibraryProvider.GetClassPrototypes()
                .ToDictionary(x => x.Create().Name, x => (IClassPrototype)x);

            var contextField = _sut.GetType().GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);

            contextField!.SetValue(_sut,
                new FunctionCallTypeAnalyseContext(
                    new FunctionDecl(null!, null!, new Parameter[] { }, null!, emptyPosition()),
                    new CallableAnalyseSet(functions),
                    new ClassAnalyseSet(prototypes)));
        }

        private TypeBase invokeAcceptMethod(IVisitable node)
        {
            var args = new object[] { node };
            var method = _sut.GetType().GetMethod("accept", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                return (TypeBase)method!.Invoke(_sut, args)!;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        }

        private RulePosition emptyPosition()
        {
            return new RulePosition(new CharacterPosition());
        }
    }
}
