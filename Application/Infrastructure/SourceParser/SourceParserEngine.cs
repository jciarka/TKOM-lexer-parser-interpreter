using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Lekser;
using Application.Models.Exceptions;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.SourceParser
{
    public class SourceParserEngine
    {
        private readonly ILexer _lexer;
        private readonly ParserOptions _options;
        private readonly IErrorHandler _errorHandler;

        public SourceParserEngine(ILexer lexer, ParserOptions options, IErrorHandler errorHandler)
        {
            _lexer = lexer;
            _options = options;
            _errorHandler = errorHandler;
        }

        public ProgramRoot Parse()
        {
            var functions = parseFunctionDeclarations();

            return new ProgramRoot(functions);
        }

        private IEnumerable<FunctionDecl> parseFunctionDeclarations()
        {
            List<FunctionDecl> functions = new List<FunctionDecl>();

            try
            {
                while (tryParseFunctionDeclaration(out FunctionDecl? function))
                {
                    functions.Add(function!);
                }

                if (!checkType(TokenType.EOF))
                {
                    _errorHandler.HandleError(new ExpectedEofException(current));
                }
            }
            catch (ComputingException ex)
            {
                _errorHandler.HandleError(ex);
            }

            return functions;
        }

        private bool tryParseFunctionDeclaration(out FunctionDecl? function)
        {
            // function return type
            if (!checkType(TokenType.TYPE, TokenType.VOID))
            {
                function = null;
                return false;
            }

            TypeBase? type = checkTypeAndAdvance(TokenType.VOID) ? new NoneType() : parseType();

            // function name
            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var name = getCurrentAndAdvance().Lexeme!;

            // left parent 
            if (!checkType(TokenType.LEFT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.LEFT_PAREN));
            }
            else
            {
                advance();
            }

            // parameters
            var parameters = parseParameters();

            // right parent
            if (!checkType(TokenType.RIGHT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
            }
            else
            {
                advance();
            }

            // block
            if (!tryParseBlock(out StatementBase? block) || block!.GetType() != typeof(BlockStmt))
            {
                throw new InvalidStatementException(current);
            }

            function = new FunctionDecl(type, name, parameters, (BlockStmt)block!);

            return true;
        }

        private IEnumerable<Parameter> parseParameters()
        {
            var parameters = new List<Parameter>();

            while (checkType(TokenType.TYPE))
            {
                parameters.Add(parseParameter());

                if (checkType(TokenType.COMMA))
                {
                    advance();
                }
                else if (!checkType(TokenType.RIGHT_PAREN))
                {
                    _errorHandler.HandleError(new MissingTokenException(current, TokenType.COMMA));
                }
            }

            return parameters;
        }

        private Parameter parseParameter()
        {
            if (!checkType(TokenType.TYPE))
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var type = parseType();

            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var identifier = getCurrentAndAdvance().Lexeme!;

            return new Parameter(type, identifier);
        }

        private bool tryParseBlock(out StatementBase? block)
        {
            var statements = new List<StatementBase>();

            // left brace
            if (!checkType(TokenType.LEFT_BRACE))
            {
                block = null;
                return false;
            }

            advance();

            // statement
            while (!checkType(TokenType.RIGHT_BRACE, TokenType.EOF))
            {
                if (tryParseStatement(out var statement))
                {
                    statements.Add(statement!);
                }
            }

            // right brace
            if (!checkType(TokenType.RIGHT_BRACE))
            {
                throw new UnexpectedTokenException(current, TokenType.RIGHT_BRACE);
            }

            advance();

            block = new BlockStmt(statements);
            return true;
        }

        private bool tryParseStatement(out StatementBase? statement)
        {
            try
            {
                if (tryParseIfStmt(out statement) ||
                    tryParseReturnStmt(out statement) ||
                    tryParseForeachStmt(out statement) ||
                    tryParseDeclarationStmt(out statement) ||
                    tryParseReturnStmt(out statement) ||
                    tryParseBlock(out statement) || // expression, assignment, financial operations
                    tryParseExpressionStmt(out statement))
                    return true;
                else
                    _errorHandler.HandleError(new InvalidStatementException(current));
            }
            catch (ComputingException issue)
            {
                _errorHandler.HandleError(issue);
            }

            // synchtonize to next statement
            gotoNextStmt();

            statement = null;
            return false;
        }

        private void gotoNextStmt()
        {
            while (!checkTypeAndAdvance(TokenType.SEMICOLON, TokenType.RIGHT_BRACE, TokenType.EOF))
            {
                // pass
            }

            checkTypeAndAdvance(TokenType.SEMICOLON);
        }

        private bool tryParseIfStmt(out StatementBase? statement)
        {
            if (!checkType(TokenType.IF))
            {
                statement = null;
                return false;
            }

            advance(); // IF

            // condition
            if (!checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.LEFT_PAREN));
            }

            var expression = parseExpression();

            if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
            }

            // then statement 
            if (!tryParseStatement(out var thenStatement))
            {
                throw new InvalidStatementException(current);
            };

            // else statement 
            StatementBase? elseStatement = null;

            if (checkTypeAndAdvance(TokenType.ELSE))
            {
                if (!tryParseStatement(out elseStatement))
                {
                    throw new InvalidStatementException(current);
                };
            }

            statement = new IfStmt(expression, thenStatement!, elseStatement);
            return true;
        }

        private bool tryParseForeachStmt(out StatementBase? statement)
        {
            if (!checkTypeAndAdvance(TokenType.FOREACH))
            {
                statement = null;
                return false;
            }

            // declaration
            if (!checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.LEFT_PAREN));
            }

            var parameter = parseParameter();

            if (!checkTypeAndAdvance(TokenType.IN))
            {
                _errorHandler.HandleError(new UnexpectedTokenException(current, TokenType.IN));
            }

            var expression = parseExpression();

            if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
            }

            // body
            if (!tryParseStatement(out var bodyStatement))
            {
                throw new InvalidStatementException(current);
            };

            statement = new ForeachStmt(parameter, expression, bodyStatement!);
            return true;
        }

        private bool tryParseReturnStmt(out StatementBase? statement)
        {
            if (!checkTypeAndAdvance(TokenType.RETURN))
            {
                statement = null;
                return false;
            }

            if (checkTypeAndAdvance(TokenType.SEMICOLON))
            {
                statement = new ReturnStmt();
            }
            else
            {
                statement = new ReturnStmt(parseExpression());
                skipSemicolon();
            }

            return true;
        }

        private bool tryParseExpressionStmt(out StatementBase? statement)
        {
            var expression = parseExpression();

            if (tryParseAssignmentExpression(expression, out statement))
            {
                skipSemicolon();
                return true;
            }

            if (tryParseFinancialFrom(expression, out statement))
            {
                skipSemicolon();
                return true;
            }

            if (tryParseFinancialTo(expression, out statement))
            {
                skipSemicolon();
                return true;
            }

            skipSemicolon();

            statement = new ExpressionStmt(expression);
            return true;
        }

        private bool tryParseFinancialFrom(ExpressionBase expression, out StatementBase? statement)
        {
            if (!checkType(TokenType.TRANSFER_FROM, TokenType.TRANSFER_PRCT_FROM))
            {
                statement = null;
                return false;
            }

            var operatorToken = getCurrentAndAdvance();

            var valueExpression = parseExpression();

            ExpressionBase? accountToExpression = null;
            if (checkTypeAndAdvance(TokenType.TRANSFER_FROM))
            {
                accountToExpression = parseExpression();
            }

            statement = new FinancialFromStmt(expression, operatorToken.Type, valueExpression, accountToExpression);

            return true;
        }

        private bool tryParseFinancialTo(ExpressionBase expression, out StatementBase? statement)
        {
            if (!checkType(TokenType.TRANSFER_TO, TokenType.TRANSFER_PRCT_TO))
            {
                statement = null;
                return false;
            }

            var operatorToken = getCurrentAndAdvance();

            var valueExpression = parseExpression();

            statement = new FinancialToStmt(expression, operatorToken.Type, valueExpression);

            return true;
        }

        private bool tryParseAssignmentExpression(ExpressionBase lValueExpression, out StatementBase? assignmentStatement)
        {
            if (!checkTypeAndAdvance(TokenType.EQUAL))
            {
                assignmentStatement = null;
                return false;
            }

            if (tryParseIdentifierAssignmentExpression(lValueExpression, out assignmentStatement))
            {
                return true;
            }

            if (tryParsePropertyAssignmentExpression(lValueExpression, out assignmentStatement))
            {
                return true;
            }

            if (tryParseIndexAssignmentExpression(lValueExpression, out assignmentStatement))
            {
                return true;
            }

            assignmentStatement = null;
            return false;
        }

        private bool tryParseIdentifierAssignmentExpression(ExpressionBase lValueExpression, out StatementBase? statement)
        {
            if (lValueExpression.GetType() != typeof(Identifier))
            {
                statement = null;
                return false;
            }

            var rValueExpression = parseExpression();

            statement = new IdentifierAssignmentStatement((Identifier)lValueExpression, rValueExpression);
            return true;
        }

        private bool tryParsePropertyAssignmentExpression(ExpressionBase lValueExpression, out StatementBase? statement)
        {
            if (lValueExpression.GetType() != typeof(ObjectPropertyExpr))
            {
                statement = null;
                return false;
            }

            var rValueExpression = parseExpression();

            statement = new PropertyAssignmentStatement((ObjectPropertyExpr)lValueExpression, rValueExpression);
            return true;
        }

        private bool tryParseIndexAssignmentExpression(ExpressionBase lValueExpression, out StatementBase? statement)
        {
            if (lValueExpression.GetType() != typeof(ObjectIndexExpr))
            {
                statement = null;
                return false;
            }

            var rValueExpression = parseExpression();

            statement = new IndexAssignmentStatement((ObjectIndexExpr)lValueExpression, rValueExpression);
            return true;
        }

        private bool tryParseDeclarationStmt(out StatementBase? statement)
        {
            if (!checkType(TokenType.VAR, TokenType.TYPE))
            {
                statement = null;
                return false;
            }

            var typeToken = current;
            var type = checkTypeAndAdvance(TokenType.VAR) ? null : parseType();

            if (current.Type != TokenType.IDENTIFIER)
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var name = getCurrentAndAdvance().Lexeme!;

            if (type == null && !checkType(TokenType.EQUAL)) // var must be assgned
            {
                throw new UnknownTypeOnVariableDeclaration(typeToken);
            }

            ExpressionBase? valueExpression = null;
            if (checkTypeAndAdvance(TokenType.EQUAL))
            {
                valueExpression = parseExpression();
            }

            if (!checkTypeAndAdvance(TokenType.SEMICOLON))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.SEMICOLON));
            }

            statement = new DeclarationStmt(new Identifier(name), valueExpression, type);
            return true;
        }

        private ExpressionBase parseExpression()
        {
            return parseOrExpression();
        }

        private ExpressionBase parseOrExpression()
        {
            var lExpression = parseAndExpression();
            var rExpressions = new List<ExpressionBase>();

            while (checkTypeAndAdvance(TokenType.OR))
            {
                rExpressions.Add(parseAndExpression());
            }

            if (rExpressions.Count() > 0)
            {
                return new OrExpr(lExpression, rExpressions);
            }

            return lExpression;
        }

        ExpressionBase parseAndExpression()
        {
            var lExpression = parseComparativeExpr();
            var rExpressions = new List<ExpressionBase>();

            while (checkTypeAndAdvance(TokenType.AND))
            {
                rExpressions.Add(parseComparativeExpr());
            }

            if (rExpressions.Count() > 0)
            {
                return new AndExpr(lExpression, rExpressions);
            }

            return lExpression;
        }

        private ExpressionBase parseComparativeExpr()
        {
            var lExpression = parseAdditiveExpr();
            var rExpressions = new List<Tuple<TokenType, ExpressionBase>>();

            while (checkType(
                TokenType.BANG_EQUAL,
                TokenType.EQUAL_EQUAL,
                TokenType.GREATER,
                TokenType.GREATER_EQUAL,
                TokenType.LESS,
                TokenType.LESS_EQUAL))
            {
                var operand = Tuple.Create(getCurrentAndAdvance().Type, parseAdditiveExpr());
                rExpressions.Add(operand);
            }

            if (rExpressions.Count() > 0)
            {
                return new ComparativeExpr(lExpression, rExpressions);
            }

            return lExpression;
        }

        private ExpressionBase parseAdditiveExpr()
        {
            var lExpression = parseMultiplicativeExpr();
            var rExpressions = new List<Tuple<TokenType, ExpressionBase>>();

            while (checkType(TokenType.PLUS, TokenType.MINUS))
            {
                var operand = Tuple.Create(getCurrentAndAdvance().Type, parseMultiplicativeExpr());
                rExpressions.Add(operand);
            }

            if (rExpressions.Count() > 0)
            {
                return new AdditiveExpr(lExpression, rExpressions);
            }

            return lExpression;
        }

        private ExpressionBase parseMultiplicativeExpr()
        {
            var lExpression = parseNegatonExpr();
            var rExpressions = new List<Tuple<TokenType, ExpressionBase>>();

            while (checkType(TokenType.STAR, TokenType.SLASH))
            {
                var operand = Tuple.Create(getCurrentAndAdvance().Type, parseNegatonExpr());
                rExpressions.Add(operand);
            }

            if (rExpressions.Count() > 0)
            {
                return new MultiplicativeExpr(lExpression, rExpressions);
            }

            return lExpression;
        }

        private ExpressionBase parseNegatonExpr()
        {
            if (checkType(TokenType.BANG, TokenType.MINUS))
            {
                return new NegativeExpr(getCurrentAndAdvance().Type, parseConversionExpr());
            }

            return parseConversionExpr();
        }

        private ExpressionBase parseConversionExpr()
        {
            var lExpression = parsePrctOfExpr();

            if (checkTypeAndAdvance(TokenType.TO))
            {
                return new ConversionExpr(lExpression, parseObjectExpr());
            }

            return lExpression;
        }

        private ExpressionBase parsePrctOfExpr()
        {
            var lExpression = parseObjectExpr();

            if (checkTypeAndAdvance(TokenType.PRCT_OF))
            {
                return new PrctOfExpr(lExpression, parseExpression());
            }

            return lExpression;
        }

        private ExpressionBase parseObjectExpr()
        {
            ExpressionBase expression = parseTerm();

            while (checkType(TokenType.DOT, TokenType.LEFT_BRACKET))
            {
                ObjectExprBase? newExpression;

                if (tryParseObjectPrtopertyOrMethodExpression(expression, out newExpression))
                {
                    expression = newExpression!;
                }
                else if (tryParseObjectIndexExpression(expression, out newExpression))
                {
                    expression = newExpression!;
                }
            }

            return expression;
        }

        private bool tryParseObjectPrtopertyOrMethodExpression(ExpressionBase subExpression, out ObjectExprBase? newExpression)
        {
            if (!checkTypeAndAdvance(TokenType.DOT))
            {
                newExpression = null;
                return false;
            }

            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var name = getCurrentAndAdvance().Lexeme!;

            if (checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                var arguments = parseArguments();

                if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
                {
                    _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
                }

                newExpression = new ObjectMethodExpr(subExpression, name, arguments);
            }
            else
            {
                newExpression = new ObjectPropertyExpr(subExpression, name);
            }

            return true;
        }

        private bool tryParseObjectIndexExpression(ExpressionBase subExpression, out ObjectExprBase? newExpression)
        {
            if (!checkTypeAndAdvance(TokenType.LEFT_BRACKET))
            {
                newExpression = null;
                return false;
            }

            var indexExpression = parseExpression();

            if (!checkTypeAndAdvance(TokenType.RIGHT_BRACKET))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_BRACKET));
            }

            newExpression = new ObjectIndexExpr(subExpression, indexExpression);
            return true;
        }

        private IEnumerable<ArgumentBase> parseArguments()
        {
            var arguments = new List<ArgumentBase>();

            while (!checkType(TokenType.RIGHT_PAREN, TokenType.EOF))
            {
                arguments.Add(parseArgument());

                if (checkType(TokenType.COMMA))
                {
                    advance();
                }
                else if (!checkType(TokenType.RIGHT_PAREN))
                {
                    _errorHandler.HandleError(new MissingTokenException(current, TokenType.COMMA));
                }
            }

            return arguments;
        }

        private ArgumentBase parseArgument()
        {
            if (tryParseLambdaArgument(out Lambda? lambda))
            {
                return lambda!;
            }

            return parseExpressionArgument();
        }

        private ArgumentBase parseExpressionArgument()
        {
            return new ExpressionArgument(parseExpression());
        }

        private bool tryParseLambdaArgument(out Lambda? lambda)
        {
            if (!checkTypeAndAdvance(TokenType.LAMBDA))
            {
                lambda = null;
                return false;
            }

            if (!checkType(TokenType.TYPE))
            {
                throw new MissingTokenException(current, TokenType.TYPE);
            }

            var type = parseType();

            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new MissingTokenException(current, TokenType.IDENTIFIER);
            }

            var name = getCurrentAndAdvance().Lexeme!;

            if (!checkType(TokenType.ARROW))
            {
                throw new MissingTokenException(current, TokenType.ARROW);
            }

            var parameter = new Parameter(type, name);

            if (!checkTypeAndAdvance(TokenType.ARROW))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.ARROW));
            }

            if (tryParseBlock(out StatementBase? block))
            {
                lambda = new Lambda(parameter, block!);
            }
            else
            {
                lambda = new Lambda(parameter, new ExpressionStmt(parseExpression()));
            }

            return true;
        }

        private ExpressionBase parseTerm()
        {
            ExpressionBase? term = null;

            if (tryParseParentisedExpression(out term))
            {
                return term!;
            }
            if (tryParseTypeOrConstructor(out term))
            {
                return term!;
            }
            else if (tryParseLiteral(out term))
            {
                return term!;
            }
            else if (tryParseIdentifierOrFunctionCall(out term))
            {
                return term!;
            }

            throw new InvalidTermException(current);
        }

        private bool tryParseParentisedExpression(out ExpressionBase? term)
        {
            if (!checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                term = null;
                return false;
            }

            term = parseExpression();

            if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
            }

            return true;
        }

        private bool tryParseIdentifierOrFunctionCall(out ExpressionBase? term)
        {
            if (!checkType(TokenType.IDENTIFIER))
            {
                term = null;
                return false;
            }

            var name = getCurrentAndAdvance().Lexeme!;

            if (checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                var arguments = parseArguments();

                if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
                {
                    _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
                }

                term = new FunctionCallExpr(name, arguments);
                return true;
            }

            term = new Identifier(name);
            return true;
        }

        private bool tryParseLiteral(out ExpressionBase? term)
        {
            if (!checkType(TokenType.LITERAL))
            {
                term = null;
                return false;
            }

            var literal = getCurrentAndAdvance();

            if (checkType(TokenType.TYPE) && _options.TypesInfo!.CurrencyTypes.ContainsKey(current.ValueType!))
            {
                term = new Literal(getCurrentAndAdvance().ValueType!, literal);
            }
            else
            {
                term = new Literal(parseBasicType(literal), literal);
            }

            return true;
        }

        private bool tryParseTypeOrConstructor(out ExpressionBase? term)
        {
            if (!checkType(TokenType.TYPE))
            {
                term = null;
                return false;
            }

            var type = parseType();

            if (checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                var arguments = parseArguments();

                if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
                {
                    _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
                }

                term = new ConstructiorCallExpr(type, arguments);
                return true;
            }

            term = new Literal(type);
            return true;
        }

        private bool checkType(params TokenType[] types)
        {
            if (types.Contains(_lexer.Current.Type))
            {
                return true;
            }

            return false;
        }

        private bool checkTypeAndAdvance(params TokenType[] types)
        {
            if (types.Contains(_lexer.Current.Type))
            {
                advance();
                return true;
            }

            return false;
        }

        private Token current => _lexer.Current;
        private bool advance() => _lexer.Advance();

        private TypeBase parseType()
        {
            if (!checkType(TokenType.TYPE))
            {
                throw new UnexpectedTokenException(current, TokenType.TYPE);
            }

            var basicTypeToken = getCurrentAndAdvance();

            if (checkTypeAndAdvance(TokenType.LESS))
            {
                if (!checkType(TokenType.TYPE))
                {
                    throw new UnexpectedTokenException(current, TokenType.TYPE);
                }

                var parametrisingType = parseType();

                if (!checkTypeAndAdvance(TokenType.GREATER))
                {
                    _errorHandler.HandleError(new MissingTokenException(current, TokenType.GREATER));
                }

                return new GenericType(basicTypeToken.ValueType!, parametrisingType);
            }

            return new BasicType(basicTypeToken.ValueType!, _options.TypesInfo.Types[basicTypeToken.ValueType!]);
        }

        private BasicType parseBasicType(Token typeToken)
        {
            return new BasicType(typeToken.ValueType!, _options.TypesInfo.Types[typeToken.ValueType!]);
        }

        private Token getCurrentAndAdvance()
        {
            Token token = current;
            advance();
            return token;
        }

        private void skipSemicolon()
        {
            skipToken(TokenType.SEMICOLON);
        }

        private void skipToken(TokenType tokenType)
        {
            if (!checkTypeAndAdvance(tokenType))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.SEMICOLON));
            }
        }
    }
}
