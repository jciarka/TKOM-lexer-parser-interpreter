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

            return new ProgramRoot(functions, functions.First().Position);
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

            var position = new RulePosition(current.Position!);
            TypeBase? type = checkTypeAndAdvance(TokenType.VOID) ? new NoneType() : parseType();

            // function name
            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var name = getCurrentAndAdvance().Lexeme!;

            // left parent 
            assertTypeAndAdvance(new MissingTokenException(current, TokenType.LEFT_PAREN), TokenType.LEFT_PAREN);

            // parameters
            var parameters = parseParameters();

            // right parent
            assertTypeAndAdvance(new MissingTokenException(current, TokenType.RIGHT_PAREN), TokenType.RIGHT_PAREN);

            // block
            if (!tryParseBlock(out IStatement? block) || block!.GetType() != typeof(BlockStmt))
            {
                throw new InvalidStatementException(current);
            }

            function = new FunctionDecl(type, name, parameters, (BlockStmt)block!, position);

            return true;
        }

        private IEnumerable<Parameter> parseParameters()
        {
            var parameters = new List<Parameter>();

            // check first parameter
            if (!tryParseParameter(out var parameter))
            {
                // no parameters
                return parameters;
            }

            parameters.Add(parameter!);

            // if comma - another parameter expected
            while (checkTypeAndAdvance(TokenType.COMMA))
            {
                if (!tryParseParameter(out parameter))
                {
                    _errorHandler.HandleError(new MissingParameterException(current));
                }

                parameters.Add(parameter!);
            }

            return parameters;
        }

        private bool tryParseParameter(out Parameter? parameter)
        {
            if (!checkType(TokenType.TYPE))
            {
                parameter = null;
                return false;
                //throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var type = parseType();

            assertType(new UnexpectedTokenException(current, TokenType.IDENTIFIER), TokenType.IDENTIFIER);

            var identifier = getCurrentAndAdvance();

            parameter = new Parameter(type, identifier.Lexeme!, new RulePosition(identifier.Position!));
            return true;
        }

        private bool tryParseBlock(out IStatement? block)
        {
            var statements = new List<IStatement>();

            // left brace
            if (!checkType(TokenType.LEFT_BRACE))
            {
                block = null;
                return false;
            }

            var position = new RulePosition(getCurrentAndAdvance().Position!);

            // statement
            while (tryParseStatement(out var statement))
            {
                statements.Add(statement!);
            }

            // right brace
            assertTypeAndAdvance(new UnexpectedTokenException(current, TokenType.RIGHT_BRACE), TokenType.RIGHT_BRACE);

            block = new BlockStmt(statements, position);
            return true;
        }

        private bool tryParseStatement(out IStatement? statement)
        {
            try
            {
                if (tryParseIfStmt(out statement) ||
                    tryParseReturnStmt(out statement) ||
                    tryParseForeachStmt(out statement) ||
                    tryParseWhileStmt(out statement) ||
                    tryParseDeclarationStmt(out statement) ||
                    tryParseReturnStmt(out statement) ||
                    tryParseBlock(out statement) || // expression, assignment, financial operations
                    tryParseExpressionStmt(out statement))
                    return true;

                return false;
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

        private bool tryParseIfStmt(out IStatement? statement)
        {
            if (!checkType(TokenType.IF))
            {
                statement = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            advance(); // IF

            // condition
            if (!checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.LEFT_PAREN));
            }

            if (!tryParseExpression(out var expression))
            {
                throw new MissingExpressionException(current);
            }

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
            IStatement? elseStatement = null;

            if (checkTypeAndAdvance(TokenType.ELSE))
            {
                if (!tryParseStatement(out elseStatement))
                {
                    throw new InvalidStatementException(current);
                };
            }

            statement = new IfStmt(expression!, thenStatement!, position, elseStatement);
            return true;
        }

        private bool tryParseForeachStmt(out IStatement? statement)
        {
            var position = new RulePosition(current.Position!);
            if (!checkTypeAndAdvance(TokenType.FOREACH))
            {
                statement = null;
                return false;
            }

            // declaration
            assertTypeAndAdvance(new MissingTokenException(current, TokenType.LEFT_PAREN), TokenType.LEFT_PAREN);

            if (!tryParseParameter(out var parameter))
                throw new MissingParameterException(current);

            assertTypeAndAdvance(new UnexpectedTokenException(current, TokenType.IN), TokenType.IN);

            if (!tryParseExpression(out var expression))
                throw new MissingExpressionException(current);

            assertTypeAndAdvance(new MissingTokenException(current, TokenType.RIGHT_PAREN), TokenType.RIGHT_PAREN);

            // body
            if (!tryParseStatement(out var bodyStatement))
                throw new InvalidStatementException(current);

            statement = new ForeachStmt(parameter!, expression!, bodyStatement!, position);
            return true;
        }

        private bool tryParseWhileStmt(out IStatement? statement)
        {
            var position = new RulePosition(current.Position!);
            if (!checkTypeAndAdvance(TokenType.WHILE))
            {
                statement = null;
                return false;
            }

            // condition
            assertTypeAndAdvance(new MissingTokenException(current, TokenType.LEFT_PAREN), TokenType.LEFT_PAREN);

            if (!tryParseExpression(out var conditionExpression))
            {
                throw new MissingExpressionException(current);
            }

            assertTypeAndAdvance(new MissingTokenException(current, TokenType.RIGHT_PAREN), TokenType.RIGHT_PAREN);

            // body
            if (!tryParseStatement(out var bodyStatement))
                throw new InvalidStatementException(current);

            statement = new WhileStmt(conditionExpression!, bodyStatement!, position);
            return true;
        }

        private bool tryParseReturnStmt(out IStatement? statement)
        {
            var position = new RulePosition(current.Position!);
            if (!checkTypeAndAdvance(TokenType.RETURN))
            {
                statement = null;
                return false;
            }

            if (checkType(TokenType.SEMICOLON))
            {
                statement = new ReturnStmt(position);
            }
            else if (tryParseExpression(out var expression))
            {
                statement = new ReturnStmt(position, expression);
            }
            else
            {
                throw new MissingExpressionException(current);
            }

            skipSemicolon();
            return true;
        }

        private bool tryParseExpressionStmt(out IStatement? statement)
        {
            var position = new RulePosition(current.Position!);

            if (!tryParseExpression(out var expression))
            {
                statement = null;
                return false;
            };

            if (!tryParseAssignmentExpression(expression!, position, out statement)
                && !tryParseFinancialFrom(expression!, position, out statement)
                && !tryParseFinancialTo(expression!, position, out statement))
            {
                statement = new ExpressionStmt(expression!, position);
            }

            skipSemicolon();
            return true;
        }

        private bool tryParseFinancialFrom(IExpression expression, RulePosition position, out IStatement? statement)
        {
            if (!checkType(TokenType.TRANSFER_FROM, TokenType.TRANSFER_PRCT_FROM))
            {
                statement = null;
                return false;
            }

            var operatorToken = getCurrentAndAdvance();

            if (!tryParseExpression(out var valueExpression))
            {
                throw new MissingExpressionException(current);
            }

            IExpression? accountToExpression = null;
            if (checkTypeAndAdvance(TokenType.TRANSFER_FROM) && !tryParseExpression(out accountToExpression))
            {
                throw new MissingExpressionException(current);
            }

            statement = new FinancialFromStmt(expression, operatorToken.Type, valueExpression!, position, accountToExpression);

            return true;
        }

        private bool tryParseFinancialTo(IExpression expression, RulePosition position, out IStatement? statement)
        {
            if (!checkType(TokenType.TRANSFER_TO, TokenType.TRANSFER_PRCT_TO))
            {
                statement = null;
                return false;
            }

            var operatorToken = getCurrentAndAdvance();

            if (!tryParseExpression(out var valueExpression))
            {
                throw new MissingExpressionException(current);
            }

            statement = new FinancialToStmt(expression, operatorToken.Type, valueExpression!, position);

            return true;
        }

        private bool tryParseAssignmentExpression(IExpression lValueExpression, RulePosition position, out IStatement? assignmentStatement)
        {
            if (!checkTypeAndAdvance(TokenType.EQUAL))
            {
                assignmentStatement = null;
                return false;
            }

            if (tryParseIdentifierAssignmentExpression(lValueExpression, position, out assignmentStatement)
                || tryParsePropertyAssignmentExpression(lValueExpression, position, out assignmentStatement)
                || tryParseIndexAssignmentExpression(lValueExpression, position, out assignmentStatement))
            {
                return true;
            }

            throw new MissingExpressionException(current);
        }

        private bool tryParseIdentifierAssignmentExpression(IExpression lValueExpression, RulePosition position, out IStatement? statement)
        {
            if (lValueExpression.GetType() != typeof(Identifier))
            {
                statement = null;
                return false;
            }

            if (!tryParseExpression(out var rValueExpression))
            {
                throw new MissingExpressionException(current);
            }

            statement = new IdentifierAssignmentStatement((Identifier)lValueExpression, rValueExpression!, position);
            return true;
        }

        private bool tryParsePropertyAssignmentExpression(IExpression lValueExpression, RulePosition position, out IStatement? statement)
        {
            if (lValueExpression.GetType() != typeof(ObjectPropertyExpr))
            {
                statement = null;
                return false;
            }

            if (!tryParseExpression(out var rValueExpression))
            {
                throw new MissingExpressionException(current);
            }

            statement = new PropertyAssignmentStatement((ObjectPropertyExpr)lValueExpression, rValueExpression!, position);
            return true;
        }

        private bool tryParseIndexAssignmentExpression(IExpression lValueExpression, RulePosition position, out IStatement? statement)
        {
            if (lValueExpression.GetType() != typeof(ObjectIndexExpr))
            {
                statement = null;
                return false;
            }

            if (!tryParseExpression(out var rValueExpression))
            {
                throw new MissingExpressionException(current);
            }

            statement = new IndexAssignmentStatement((ObjectIndexExpr)lValueExpression, rValueExpression!, position);
            return true;
        }

        private bool tryParseDeclarationStmt(out IStatement? statement)
        {
            if (!checkType(TokenType.VAR, TokenType.TYPE))
            {
                statement = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            var typeToken = current;
            var type = checkTypeAndAdvance(TokenType.VAR) ? null : parseType();

            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var nameToken = getCurrentAndAdvance();

            if (type == null && !checkType(TokenType.EQUAL)) // var must be assgned
            {
                throw new UnknownTypeOnVariableDeclaration(typeToken);
            }

            IExpression? valueExpression = null;
            if (checkTypeAndAdvance(TokenType.EQUAL) && !tryParseExpression(out valueExpression))
            {
                throw new MissingExpressionException(current);
            }

            if (!checkTypeAndAdvance(TokenType.SEMICOLON))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.SEMICOLON));
            }

            statement = new DeclarationStmt(
                new Identifier(nameToken.Lexeme!, new RulePosition(nameToken.Position!)),
                position,
                valueExpression!,
                type);

            return true;
        }

        private bool tryParseExpression(out IExpression? expression)
        {
            if (!tryParseAndExpression(out var lExpression))
            {
                expression = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            var rExpressions = new List<IExpression>();

            while (checkTypeAndAdvance(TokenType.OR))
            {
                if (!tryParseAndExpression(out var nextExpression))
                {
                    throw new MissingExpressionException(current);
                }
                rExpressions.Add(nextExpression!);
            }

            if (rExpressions.Count() > 0)
            {
                expression = new OrExpr(lExpression!, rExpressions, position);
                return true;
            }

            expression = lExpression;
            return true;
        }

        private bool tryParseAndExpression(out IExpression? expression)
        {
            if (!tryParseComparativeExpr(out var lExpression))
            {
                expression = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            var rExpressions = new List<IExpression>();

            while (checkTypeAndAdvance(TokenType.AND))
            {
                if (!tryParseComparativeExpr(out var nextExpression))
                {
                    throw new MissingExpressionException(current);
                }
                rExpressions.Add(nextExpression!);
            }

            if (rExpressions.Count() > 0)
            {
                expression = new AndExpr(lExpression!, rExpressions, position);
                return true;
            }

            expression = lExpression;
            return true;
        }

        private bool tryParseComparativeExpr(out IExpression? expression)
        {
            if (!tryParseAdditiveExpr(out var lExpression))
            {
                expression = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            var rExpressions = new List<Tuple<TokenType, IExpression>>();

            while (checkType(
                TokenType.BANG_EQUAL,
                TokenType.EQUAL_EQUAL,
                TokenType.GREATER,
                TokenType.GREATER_EQUAL,
                TokenType.LESS,
                TokenType.LESS_EQUAL))
            {
                var @operator = getCurrentAndAdvance().Type;

                if (!tryParseAdditiveExpr(out var nextExpression))
                {
                    throw new MissingExpressionException(current);
                }

                var operand = Tuple.Create(@operator, nextExpression);
                rExpressions.Add(operand!);
            }

            if (rExpressions.Count() > 0)
            {
                expression = new ComparativeExpr(lExpression!, rExpressions, position);
                return true;
            }

            expression = lExpression;
            return true;
        }

        private bool tryParseAdditiveExpr(out IExpression? expression)
        {
            if (!tryParseMultiplicativeExpr(out var lExpression))
            {
                expression = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            var rExpressions = new List<Tuple<TokenType, IExpression>>();

            while (checkType(TokenType.PLUS, TokenType.MINUS))
            {
                var @operator = getCurrentAndAdvance().Type;

                if (!tryParseMultiplicativeExpr(out var nextExpression))
                {
                    throw new MissingExpressionException(current);
                }

                var operand = Tuple.Create(@operator, nextExpression);
                rExpressions.Add(operand!);
            }

            if (rExpressions.Count() > 0)
            {
                expression = new AdditiveExpr(lExpression!, rExpressions, position);
                return true;
            }

            expression = lExpression;
            return true;
        }

        private bool tryParseMultiplicativeExpr(out IExpression? expression)
        {
            if (!tryParseNegatonExpr(out var lExpression))
            {
                expression = null;
                return false;
            }

            var position = new RulePosition(current.Position!);

            var rExpressions = new List<Tuple<TokenType, IExpression>>();

            while (checkType(TokenType.STAR, TokenType.SLASH))
            {
                var @operator = getCurrentAndAdvance().Type;

                if (!tryParseNegatonExpr(out var nextExpression))
                {
                    throw new MissingExpressionException(current);
                }

                var operand = Tuple.Create(@operator, nextExpression);
                rExpressions.Add(operand!);
            }

            if (rExpressions.Count() > 0)
            {
                expression = new MultiplicativeExpr(lExpression!, rExpressions, position);
                return true;
            }

            expression = lExpression;
            return true;
        }

        private bool tryParseNegatonExpr(out IExpression? expression)
        {
            var position = new RulePosition(current.Position!);
            if (checkType(TokenType.BANG, TokenType.MINUS))
            {
                var @operator = getCurrentAndAdvance().Type;

                if (!tryParseConversionExpr(out var nextExpression))
                {
                    throw new MissingExpressionException(current);
                }

                expression = new NegativeExpr(@operator, nextExpression!, position);
                return true;
            }

            if (!tryParseConversionExpr(out expression))
            {
                return false;
            }

            return true;
        }

        private bool tryParseConversionExpr(out IExpression? expression)
        {
            if (!tryParsePrctOfExpr(out var lExpression))
            {
                expression = null;
                return false;
            }

            var position = new RulePosition(current.Position!);

            if (checkTypeAndAdvance(TokenType.TO))
            {
                if (!tryParseObjectExpr(out var rExpression))
                {
                    throw new MissingExpressionException(current);
                }

                expression = new ConversionExpr(lExpression!, rExpression!, position);
                return true;
            }

            expression = lExpression;
            return true;
        }

        private bool tryParsePrctOfExpr(out IExpression? expression)
        {
            if (!tryParseObjectExpr(out var lExpression))
            {
                expression = null;
                return false;
            }

            if (checkTypeAndAdvance(TokenType.PRCT_OF))
            {
                if (!tryParseExpression(out var nextExpression))
                {
                    throw new MissingExpressionException(current);
                }

                expression = new PrctOfExpr(lExpression!, nextExpression!, lExpression!.Position);
                return true;
            }

            expression = lExpression;
            return true;
        }

        private bool tryParseObjectExpr(out IExpression? expression)
        {
            if (!tryParseTerm(out expression))
            {
                return false;
            }

            IExpression? newExpression = null;
            while (tryParseObjectPrtopertyOrMethodExpression(expression!, out newExpression)
                    || tryParseObjectIndexExpression(expression!, out newExpression))
            {
                expression = newExpression!;
            }

            return true;
        }

        private bool tryParseObjectPrtopertyOrMethodExpression(IExpression subExpression, out IExpression? newExpression)
        {
            if (!checkTypeAndAdvance(TokenType.DOT))
            {
                newExpression = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var name = getCurrentAndAdvance().Lexeme!;

            if (checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                var arguments = parseArguments();

                assertTypeAndAdvance(new MissingTokenException(current, TokenType.RIGHT_PAREN), TokenType.RIGHT_PAREN);

                newExpression = new ObjectMethodExpr(subExpression, name, arguments, position);
            }
            else
            {
                newExpression = new ObjectPropertyExpr(subExpression, name, position);
            }

            return true;
        }

        private bool tryParseObjectIndexExpression(IExpression subExpression, out IExpression? newExpression)
        {
            if (!checkTypeAndAdvance(TokenType.LEFT_BRACKET))
            {
                newExpression = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            if (!tryParseExpression(out var indexExpression))
            {
                throw new MissingExpressionException(current);
            }

            assertTypeAndAdvance(new MissingTokenException(current, TokenType.RIGHT_BRACKET), TokenType.RIGHT_BRACKET);

            newExpression = new ObjectIndexExpr(subExpression, indexExpression!, position);
            return true;
        }

        private IEnumerable<IArgument> parseArguments()
        {
            var arguments = new List<IArgument>();

            if (!tryParseArgument(out var argument))
            {
                return arguments;
            }

            arguments.Add(argument!);

            while (checkTypeAndAdvance(TokenType.COMMA))
            {
                if (!tryParseArgument(out argument))
                {
                    _errorHandler.HandleError(new MissingExpressionException(current));
                }

                arguments.Add(argument!);
            }

            return arguments;
        }

        private bool tryParseArgument(out IArgument? argument)
        {
            if (tryParseLambdaArgument(out argument)
                || tryParseExpressionArgument(out argument))
            {
                return true;
            }

            return false;
        }

        private bool tryParseExpressionArgument(out IArgument? argument)
        {
            var position = new RulePosition(current.Position!);

            if (!tryParseExpression(out var expression))
            {
                argument = null;
                return false;
            }

            argument = new ExpressionArgument(expression!, position);
            return true;
        }

        private bool tryParseLambdaArgument(out IArgument? lambda)
        {
            if (!checkTypeAndAdvance(TokenType.LAMBDA))
            {
                lambda = null;
                return false;
            }

            var position = new RulePosition(current.Position!);

            if (!checkType(TokenType.TYPE))
            {
                throw new MissingTokenException(current, TokenType.TYPE);
            }

            var type = parseType();

            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new MissingTokenException(current, TokenType.IDENTIFIER);
            }

            var nameToken = getCurrentAndAdvance();

            if (!checkType(TokenType.ARROW))
            {
                throw new MissingTokenException(current, TokenType.ARROW);
            }

            var parameter = new Parameter(type, nameToken.Lexeme!, new RulePosition(nameToken.Position!));

            if (!checkTypeAndAdvance(TokenType.ARROW))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.ARROW));
            }

            if (tryParseBlock(out IStatement? block))
            {
                lambda = new Lambda(parameter, block!, position);
            }
            else
            {
                var expressionPosition = new RulePosition(current.Position!);

                if (!tryParseExpression(out var lambdaExpression))
                {
                    throw new MissingExpressionException(current);
                }

                lambda = new Lambda(parameter, new ExpressionStmt(lambdaExpression!, expressionPosition), position);
            }

            return true;
        }

        private bool tryParseTerm(out IExpression? expression)
        {
            if (tryParseParentisedExpression(out expression))
            {
                return true;
            }
            if (tryParseTypeOrConstructor(out expression))
            {
                return true;
            }
            else if (tryParseLiteral(out expression))
            {
                return true;
            }
            else if (tryParseIdentifierOrFunctionCall(out expression))
            {
                return true;
            }

            expression = null;
            return false;
        }

        private bool tryParseParentisedExpression(out IExpression? term)
        {
            if (!checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                term = null;
                return false;
            }

            if (!tryParseExpression(out var expression))
            {
                _errorHandler.HandleError(new MissingExpressionException(current));
                term = null;
                return false;
            }

            if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
            {
                _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
            }

            term = expression;
            return true;
        }

        private bool tryParseIdentifierOrFunctionCall(out IExpression? term)
        {
            if (!checkType(TokenType.IDENTIFIER))
            {
                term = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            var name = getCurrentAndAdvance().Lexeme!;

            if (checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                var arguments = parseArguments();

                if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
                {
                    _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
                }

                term = new FunctionCallExpr(name, arguments, position);
                return true;
            }

            term = new Identifier(name, position);
            return true;
        }

        private bool tryParseLiteral(out IExpression? term)
        {
            if (!checkType(TokenType.LITERAL))
            {
                term = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            var literal = getCurrentAndAdvance();

            if (checkType(TokenType.TYPE) && _options.TypesInfo!.CurrencyTypes.ContainsKey(current.ValueType!))
            {
                term = new Literal(getCurrentAndAdvance().ValueType!, literal, position);
            }
            else
            {
                term = new Literal(parseBasicType(literal), literal, position);
            }

            return true;
        }

        private bool tryParseTypeOrConstructor(out IExpression? term)
        {
            if (!checkType(TokenType.TYPE))
            {
                term = null;
                return false;
            }

            var position = new RulePosition(current.Position!);
            var type = parseType();

            if (checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                var arguments = parseArguments();

                if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
                {
                    _errorHandler.HandleError(new MissingTokenException(current, TokenType.RIGHT_PAREN));
                }

                term = new ConstructiorCallExpr(type, arguments, position);
                return true;
            }

            term = new Literal(type, position);
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

        private bool assertType(ComputingException issue, params TokenType[] types)
        {
            if (!checkType(types))
            {
                _errorHandler.HandleError(issue);
                return false;
            }

            return true;
        }

        private bool assertTypeAndAdvance(ComputingException issue, params TokenType[] types)
        {
            if (!checkTypeAndAdvance(types))
            {
                _errorHandler.HandleError(issue);
                return false;
            }

            return true;
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
