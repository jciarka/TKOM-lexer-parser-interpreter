using Application.Infrastructure.Lekser;
using Application.Models.Exceptions;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar;
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

        public SourceParserEngine(ILexer lexer, ParserOptions options)
        {
            _lexer = lexer;
            _options = options;
            issues = new List<ComputingException>();
        }

        private ICollection<ComputingException> issues;

        public ProgramRoot Parse(out IEnumerable<ComputingException> parseIssues)
        {
            var functions = parseFunctionDeclarations();

            parseIssues = issues;
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
                    issues.Add(new ExpectedEofException(current));
                }
            }
            catch (ComputingException ex)
            {
                issues.Add(ex);
            }

            return functions;
        }

        private bool tryParseFunctionDeclaration(out FunctionDecl? function)
        {
            // function return type
            if (checkType(TokenType.TYPE, TokenType.VOID))
            {
                function = null;
                return false;
            }

            var type = advance().Lexeme!;

            // function name
            if (current.Type != TokenType.IDENTIFIER)
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var name = advance().Lexeme!;

            // left parent 
            if (checkType(TokenType.LEFT_PAREN))
            {
                issues.Add(new MissingTokenException(current, TokenType.LEFT_PAREN));
            }
            else
            {
                advance();
            }

            // parameters
            var parameters = parseParameters();

            // right parent
            if (checkType(TokenType.RIGHT_PAREN))
            {
                issues.Add(new MissingTokenException(advance(), TokenType.RIGHT_PAREN));
            }
            else
            {
                advance();
            }

            // block
            if (!tryParseBlock(out BlockStmt? block))
            {
                throw new InvalidStatementException(current);
            }

            function = new FunctionDecl(type, name, parameters, block!);
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
                    issues.Add(new MissingTokenException(current, TokenType.COMMA));
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

            var type = advance().Lexeme!;

            if (!checkType(TokenType.IDENTIFIER))
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var identifier = advance().Lexeme!;

            return new Parameter(type, identifier);
        }

        private bool tryParseBlock(out BlockStmt? block)
        {
            var statements = new List<StatementBase>();

            // left brace
            if (!checkType(TokenType.RIGHT_BRACE))
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
                if (tryParseIfStmt(out statement))
                {
                    return true;
                }
                else if (tryParseReturnStmt(out statement))
                {
                    return true;
                }
                else if (tryParseForeachStmt(out statement))
                {
                    return true;
                }
                else if (tryParseDeclarationStmt(out statement))
                {
                    return true;
                }
                else if (tryParseReturnStmt(out statement))
                {
                    return true;
                }
                else if (tryParseExpressionStmt(out statement)) // expression, assignment, financial operations
                {
                    return true;
                }
                else
                {
                    issues.Add(new InvalidStatementException(current));
                }
            }
            catch (ComputingException issue)
            {
                issues.Add(issue);
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
                issues.Add(new MissingTokenException(current, TokenType.LEFT_PAREN));
            }

            var expression = parseExpression();

            if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
            {
                issues.Add(new MissingTokenException(current, TokenType.RIGHT_PAREN));
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
            if (checkTypeAndAdvance(TokenType.FOREACH))
            {
                statement = null;
                return false;
            }

            // declaration
            if (!checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                issues.Add(new MissingTokenException(current, TokenType.LEFT_PAREN));
            }

            var parameter = parseParameter();

            if (!checkTypeAndAdvance(TokenType.IN))
            {
                issues.Add(new UnexpectedTokenException(current, TokenType.IN));
            }

            var expression = parseExpression();

            if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
            {
                issues.Add(new MissingTokenException(current, TokenType.RIGHT_PAREN));
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

            var expression = parseExpression();

            if (!checkTypeAndAdvance(TokenType.SEMICOLON))
            {
                issues.Add(new MissingTokenException(current, TokenType.SEMICOLON));
            }

            statement = new ReturnStmt(expression);
            return true;
        }

        private bool tryParseExpressionStmt(out StatementBase? statement)
        {
            var expression = parseExpression();

            if (tryParseAssignmentExpression(expression, out statement))
            {
                return true;
            }

            if (tryParseFinancialFrom(expression, out statement))
            {
                return true;
            }

            if (tryParseFinancialTo(expression, out statement))
            {
                return true;
            }

            if (!checkTypeAndAdvance(TokenType.SEMICOLON))
            {
                issues.Add(new MissingTokenException(current, TokenType.SEMICOLON));
            }

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

            var operatorToken = advance();

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

            var operatorToken = advance();
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

            var typeToken = advance();
            var type = typeToken.Type != TokenType.VAR ? typeToken.Lexeme! : null;

            // variable name
            if (current.Type != TokenType.IDENTIFIER)
            {
                throw new UnexpectedTokenException(current, TokenType.IDENTIFIER);
            }

            var name = advance().Lexeme!;

            if (typeToken.Type == TokenType.VAR && !checkType(TokenType.EQUAL))
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
                issues.Add(new MissingTokenException(current, TokenType.SEMICOLON));
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

            while (checkTypeAndAdvance(TokenType.OR))
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
                var operand = Tuple.Create(advance().Type, parseAdditiveExpr());
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
                var operand = Tuple.Create(advance().Type, parseMultiplicativeExpr());
                rExpressions.Add(operand);
            }

            if (rExpressions.Count() > 0)
            {
                return new ComparativeExpr(lExpression, rExpressions);
            }

            return lExpression;
        }

        private ExpressionBase parseMultiplicativeExpr()
        {
            var lExpression = parseNegatonExpr();
            var rExpressions = new List<Tuple<TokenType, ExpressionBase>>();

            while (checkType(TokenType.STAR, TokenType.SLASH))
            {
                var operand = Tuple.Create(advance().Type, parseNegatonExpr());
                rExpressions.Add(operand);
            }

            if (rExpressions.Count() > 0)
            {
                return new ComparativeExpr(lExpression, rExpressions);
            }

            return lExpression;
        }

        private ExpressionBase parseNegatonExpr()
        {
            if (checkType(TokenType.BANG, TokenType.MINUS))
            {
                return new NegativeExpr(advance().Type, parseConversionExpr());
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

            var name = advance().Lexeme!;

            if (checkTypeAndAdvance(TokenType.LEFT_PAREN))
            {
                var arguments = parseArguments();

                if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
                {
                    issues.Add(new MissingTokenException(current, TokenType.RIGHT_PAREN));
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
            if (!checkTypeAndAdvance(TokenType.LEFT_BRACE))
            {
                newExpression = null;
                return false;
            }

            var indexExpression = parseExpression();

            if (!checkTypeAndAdvance(TokenType.RIGHT_BRACE))
            {
                issues.Add(new MissingTokenException(current, TokenType.RIGHT_BRACE));
            }

            newExpression = new ObjectIndexExpr(subExpression, indexExpression);
            return true;
        }

        private IEnumerable<ArgumentBase> parseArguments()
        {
            throw new NotImplementedException();
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
            else if (tryParseIdentifier(out term))
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
                issues.Add(new MissingTokenException(current, TokenType.RIGHT_PAREN));
            }

            return true;
        }

        private bool tryParseIdentifier(out ExpressionBase? term)
        {
            if (!checkType(TokenType.IDENTIFIER))
            {
                term = null;
                return false;
            }

            term = new Identifier(advance().Lexeme!);
            return true;
        }

        private bool tryParseLiteral(out ExpressionBase? term)
        {
            if (!checkType(TokenType.LITERAL))
            {
                term = null;
                return false;
            }

            term = new Literal(advance());
            return true;
        }

        private bool tryParseTypeOrConstructor(out ExpressionBase? term)
        {
            if (!checkType(TokenType.TYPE))
            {
                term = null;
                return false;
            }

            var typeToken = advance();

            if (checkType(TokenType.LEFT_PAREN))
            {
                var arguments = parseArguments();

                if (!checkTypeAndAdvance(TokenType.RIGHT_PAREN))
                {
                    issues.Add(new MissingTokenException(current, TokenType.RIGHT_PAREN));
                }

                term = new ConstructiorCallExpr(typeToken.Lexeme!, arguments);
                return true;
            }

            term = new Literal("Type", stringValue: typeToken.Lexeme!);
            return true;
        }

        private bool checkType(params TokenType[] types)
        {
            if (types.Contains(_lexer.Peek().Type))
            {
                return true;
            }

            return false;
        }

        private bool checkTypeAndAdvance(params TokenType[] types)
        {
            if (types.Contains(_lexer.Peek().Type))
            {
                advance();
                return true;
            }

            return false;
        }

        private Token current => _lexer.Peek();
        private Token advance() => _lexer.Read();
    }
}
