using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Interpreter;
using Application.Models.Exceptions;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;


namespace Application.Infrastructure.Presenters
{
    public class TypingAnalyseVisitor : ITypingAnalyseVisitor
    {
        private readonly IErrorHandler _errorHandler;

        private FunctionCallTypeAnalyseContext? _context;

        public TypingAnalyseVisitor(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public TypeBase? Visit(ProgramRoot node)
        {
            foreach (var function in node.FunctionDeclaration)
            {
                try
                {
                    function.Accept(this);
                }
                catch (ComputingException issue)
                {
                    _errorHandler.HandleError(issue);
                }
            }

            return null;
        }

        public TypeBase? Visit(FunctionDecl node)
        {
            _context = new FunctionCallTypeAnalyseContext(node);

            node.Block.Accept(this);

            return null;
        }

        public TypeBase? Visit(BlockStmt node)
        {
            try
            {
                _context!.PushScope();

                foreach (var statement in node.Statements)
                {
                    statement.Accept(this);
                }
            }
            catch (ComputingException issue)
            {
                _errorHandler?.HandleError(issue);
            }
            finally
            {
                _context!.PopScope();
            }

            return null;
        }

        public TypeBase? Visit(IdentifierAssignmentStatement node)
        {
            if (!_context!.Scope.TryFind(node.Identifier.Name, out var variableType))
            {
                _errorHandler.HandleError(new NotDefinedVariableException(node.Identifier.Name));
            }

            var expressionType = node.Expression.Accept(this);

            if (variableType != expressionType)
            {
                _errorHandler.HandleError(new InvalidTypeException(expressionType, variableType!.Type));
            }

            return null;
        }

        public TypeBase? Visit(PropertyAssignmentStatement propertyAssignmentStatement)
        {
            return null;
        }

        public TypeBase? Visit(IndexAssignmentStatement indexAssignmentStatement)
        {
            return null;
        }

        public TypeBase? Visit(DeclarationStmt node)
        {
            var expressionType = node.Expression != null ? node.Expression.Accept(this) : null;

            if (expressionType == null && node.Type == null)
            {
                _errorHandler.HandleError(new UnresolvableVarTypeException(node.Identifier.Name));
            }
            else if (node.Type == null)
            {
                node.Type = expressionType;
            }
            else if (node.Type != expressionType)
            {
                _errorHandler.HandleError(new InvalidTypeException(expressionType, node.Type.Type));
            }

            if (!_context!.Scope.TryAdd(node.Identifier.Name, node.Type!))
            {
                _errorHandler.HandleError(new VariableRedefiniitionException(node.Identifier.Name));
            }

            return null;
        }

        public TypeBase? Visit(ReturnStmt node)
        {
            var returnExpressionType = node.ReturnExpression != null ? node.ReturnExpression.Accept(this) : null;

            if (returnExpressionType != _context!.ReturnType)
            {
                if (_context.ReturnType != null)
                {
                    _errorHandler.HandleError(new InvalidTypeException(returnExpressionType, _context!.ReturnType.Type));
                }
                else
                {
                    _errorHandler.HandleError(new InvalidTypeException(returnExpressionType, TypeEnum.NONE));
                }
            }

            return returnExpressionType;
        }

        public TypeBase? Visit(WhileStmt node)
        {
            var conditionType = node.Condition.Accept(this);

            if (!checkType(conditionType, TypeEnum.BOOL))
            {
                _errorHandler.HandleError(new InvalidTypeException(conditionType, TypeEnum.BOOL));
            }

            node.Statement.Accept(this);

            return null;
        }

        public TypeBase? Visit(IfStmt node)
        {
            var conditionType = node.Condition.Accept(this);

            if (!checkType(conditionType, TypeEnum.BOOL))
            {
                _errorHandler.HandleError(new InvalidTypeException(conditionType, TypeEnum.BOOL));
            }

            node.ThenStatement.Accept(this);

            if (node.ElseStatement != null)
            {
                node.ElseStatement.Accept(this);
            }

            return null;
        }

        public TypeBase? Visit(ForeachStmt node)
        {
            var collectionType = node.CollectionExpression.Accept(this);

            if (collectionType!.GetType() != typeof(GenericType))
            {
                _errorHandler.HandleError(new InvalidTypeException(collectionType, TypeEnum.GENERIC));
            }

            var genericCollectionType = (GenericType)collectionType;

            if (node.Parameter.Type != genericCollectionType)
            {
                _errorHandler.HandleError(new InvalidTypeException(genericCollectionType, node.Parameter.Type.Type));
            }

            node.Statement.Accept(this);

            return null;
        }

        public TypeBase? Visit(FinancialToStmt node)
        {
            var accountExpression = node.AccountExpression.Accept(this);

            if (!checkType(accountExpression, TypeEnum.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, TypeEnum.ACCOUNT));
            }

            var valueExpression = node.ValueExpression.Accept(this);

            if (!checkType(valueExpression, TypeEnum.CURRENCY))
            {
                _errorHandler.HandleError(new InvalidTypeException(valueExpression, TypeEnum.CURRENCY));
            }

            return null;
        }

        public TypeBase? Visit(FinancialFromStmt node)
        {
            var accountExpression = node.AccountFromExpression.Accept(this);

            if (!checkType(accountExpression, TypeEnum.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, TypeEnum.ACCOUNT));
            }

            var valueExpression = node.ValueExpression.Accept(this);

            if (!checkType(valueExpression, TypeEnum.CURRENCY))
            {
                _errorHandler.HandleError(new InvalidTypeException(valueExpression, TypeEnum.CURRENCY));
            }

            if (node.AccountToExpression != null)
            {
                var accountToExpression = node.AccountToExpression.Accept(this);

                if (!checkType(accountToExpression, TypeEnum.ACCOUNT))
                {
                    _errorHandler.HandleError(new InvalidTypeException(accountToExpression, TypeEnum.ACCOUNT));
                }
            }

            return null;
        }

        public TypeBase? Visit(Parameter parameter)
        {
            return parameter.Type;
        }

        public TypeBase? Visit(Lambda lambda)
        {
            // TO DO:
            throw new NotImplementedException();
        }

        public TypeBase? Visit(Identifier identifier)
        {
            if (!_context!.Scope.TryFind(identifier.Name, out var variableType))
            {
                throw new NotDefinedVariableException(identifier.Name);
            }

            return variableType;
        }

        public TypeBase? Visit(ExpressionArgument expressionArgument)
        {
            return expressionArgument.Accept(this);
        }

        public TypeBase? Visit(ExpressionStmt expressionStmt)
        {
            return expressionStmt.Accept(this);
        }

        public TypeBase? Visit(OrExpr node)
        {
            return new BasicType(TypeName.BOOL, TypeEnum.BOOL);
        }

        public TypeBase? Visit(AdditiveExpr node)
        {
            var first = node.FirstOperand.Accept(this);

            if (!checkType(first, TypeEnum.INT, TypeEnum.DECIMAL, TypeEnum.CURRENCY))
            {
                _errorHandler.HandleError(new InvalidTypeException(null, TypeEnum.INT, TypeEnum.DECIMAL, TypeEnum.CURRENCY));
            }

            foreach (var operand in node.Operands)
            {
                var next = operand.Item2.Accept(this);

                if (first!.Name != next?.Name)
                {
                    _errorHandler.HandleError(new InvalidTypeException(next, first!.Type));
                }
            }

            return first;
        }

        public TypeBase? Visit(MultiplicativeExpr node)
        {
            var first = node.FirstOperand.Accept(this);

            if (!checkType(first, TypeEnum.INT, TypeEnum.DECIMAL))
            {
                _errorHandler.HandleError(new InvalidTypeException(null, TypeEnum.INT, TypeEnum.DECIMAL));
            }

            foreach (var operand in node.Operands)
            {
                var next = operand.Item2.Accept(this);

                if (first!.Type != next?.Type)
                {
                    _errorHandler.HandleError(new InvalidTypeException(next, first!.Type));
                }
            }

            return first;
        }

        public TypeBase? Visit(NegativeExpr node)
        {
            var first = node.Operand.Accept(this);

            if (node.Operator == TokenType.MINUS)
            {
                if (checkType(first, TypeEnum.INT, TypeEnum.DECIMAL))
                {
                    return first;
                }

                _errorHandler.HandleError(new InvalidTypeException(first, TypeEnum.INT, TypeEnum.DECIMAL));

                return new BasicType(TypeName.INT, TypeEnum.INT);
            }

            if (node.Operator == TokenType.BANG)
            {
                if (checkType(first, TypeEnum.BOOL))
                {
                    return first;
                }

                _errorHandler.HandleError(new InvalidTypeException(first, TypeEnum.BOOL));
                return new BasicType(TypeName.BOOL, TypeEnum.BOOL);
            }

            return null;
        }

        public TypeBase? Visit(ConversionExpr node)
        {
            var first = node.OryginalExpression.Accept(this);

            if (!checkType(first, TypeEnum.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(null, TypeEnum.ACCOUNT));
            }

            var next = node.TypeExpression.Accept(this);

            if (!checkType(next, TypeEnum.TYPE))
            {
                _errorHandler.HandleError(new InvalidTypeException(next, TypeEnum.TYPE));
            }

            return ((TypeType)next!).OfType;
        }

        public TypeBase? Visit(AndExpr andExpr)
        {
            return new BasicType(TypeName.BOOL, TypeEnum.BOOL);
        }

        public TypeBase? Visit(FunctionCallExpr functionCallExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ComparativeExpr node)
        {
            var first = node.FirstOperand.Accept(this);

            if (first?.Type == null)
            {
                _errorHandler.HandleError(new InvalidTypeException(null, TypeEnum.DECIMAL));
            }

            foreach (var operand in node.Operands)
            {
                var next = operand.Item2.Accept(this);

                if (first!.Type != next?.Type)
                {
                    _errorHandler.HandleError(new InvalidTypeException(next, TypeEnum.INT, TypeEnum.DECIMAL));
                }
            }

            return new BasicType(TypeName.BOOL, TypeEnum.BOOL);
        }

        public TypeBase? Visit(PrctOfExpr node)
        {
            var accountExpression = node.FirstOperand.Accept(this);

            if (!checkType(accountExpression, TypeEnum.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, TypeEnum.ACCOUNT));
            }

            var prctExpression = node.SecondOperand.Accept(this);

            if (!checkType(prctExpression, TypeEnum.INT, TypeEnum.DECIMAL))
            {
                _errorHandler.HandleError(new InvalidTypeException(prctExpression, TypeEnum.INT, TypeEnum.DECIMAL));
            }

            return new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL);
        }

        public TypeBase? Visit(ConstructiorCallExpr node)
        {
            // TO DO check constructors parameters 
            return node.Type;
        }

        public TypeBase? Visit(ObjectPropertyExpr objectPropertyExpr)
        {
            // TO DO: check parameter types
            // TO DO: return method return type
            return null;
        }

        public TypeBase? Visit(ObjectIndexExpr objectIndexExpr)
        {
            // TO DO: check parameter types
            // TO DO: return method return type
            return null;
        }

        public TypeBase? Visit(ObjectMethodExpr node)
        {
            // TO DO: check parameter types
            // TO DO: return method return type
            return null;
        }

        public TypeBase? Visit(BracedExprTerm node)
        {
            return node.Expression.Accept(this);
        }

        public TypeBase? Visit(Literal node)
        {
            return node.Type;
        }

        public TypeBase? Visit(BasicType node)
        {
            return new TypeType(node);
        }

        public TypeBase? Visit(GenericType node)
        {
            return new TypeType(node);
        }

        public TypeBase? Visit(TypeType node)
        {
            return node;
        }

        public bool checkType(TypeBase? type, params TypeEnum[] types)
        {
            if (type == null)
            {
                return false;
            }

            return types.Any(t => type?.Type == t);
        }
    }
}
