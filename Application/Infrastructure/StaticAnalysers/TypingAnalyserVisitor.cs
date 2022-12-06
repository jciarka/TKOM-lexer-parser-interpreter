using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Interpreter;
using Application.Models.Exceptions;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public class TypingAnalyserVisitor : ITypingAnalyserVisitor
    {
        private readonly IErrorHandler _errorHandler;

        private FunctionCallContext? _context;

        public TypingAnalyserVisitor(IErrorHandler errorHandler)
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
            _context = new FunctionCallContext(node);

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
                throw new NotImplementedException();
            }

            if (variableType != node.Expression.Accept(this))
            {
                throw new NotImplementedException();
            }

            return null;
        }

        public TypeBase? Visit(PropertyAssignmentStatement propertyAssignmentStatement)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(IndexAssignmentStatement indexAssignmentStatement)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(DeclarationStmt node)
        {
            var expressionType = node.Expression != null ? node.Expression.Accept(this) : null;

            if (expressionType == null && node.Type == null)
            {
                throw new NotImplementedException();
            }
            else if (node.Type == null)
            {
                node.Type = expressionType;
            }
            else if (node.Type != expressionType)
            {
                throw new NotImplementedException();
            }

            _context!.Scope.Add(node.Identifier.Name, node.Type!);
            return null;
        }

        public TypeBase? Visit(ReturnStmt node)
        {
            var returnExpressionType = node.ReturnExpression != null ? node.ReturnExpression.Accept(this) : null;

            if (returnExpressionType != _context!.ReturnType)
            {
                throw new NotImplementedException();
            }

            return returnExpressionType;
        }

        public TypeBase? Visit(WhileStmt node)
        {
            if (node.Condition.Accept(this) != new BasicType("bool"))
            {
                throw new NotImplementedException();
            }

            node.Statement.Accept(this);

            return null;
        }

        public TypeBase? Visit(IfStmt node)
        {
            if (node.Condition.Accept(this) != new BasicType("bool"))
            {
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }

            var genericCollectionType = (GenericType)collectionType;

            if (node.Parameter.Type != genericCollectionType)
            {
                throw new NotImplementedException();
            }

            node.Statement.Accept(this);

            return null;
        }

        public TypeBase? Visit(FinancialToStmt node)
        {
            if (node.AccountExpression.Accept(this) != new BasicType("Account"))
            {
                throw new NotImplementedException();
            }

            // END HERE -- finish
        }

        public TypeBase? Visit(FinancialFromStmt financialFromStmt)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(Parameter parameter)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(Lambda lambda)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(Identifier identifier)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ExpressionArgument expressionArgument)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ExpressionStmt expressionStmt)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(OrExpr orExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(AdditiveExpr additiveExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(MultiplicativeExpr multiplicativeExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(NegativeExpr negativeExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ConversionExpr conversionExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(AndExpr andExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(FunctionCallExpr functionCallExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ComparativeExpr comparativeExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(PrctOfExpr prctOfExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ConstructiorCallExpr constructiorCallExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ObjectPropertyExpr objectPropertyExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ObjectIndexExpr objectIndexExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(ObjectMethodExpr objectMethodExpr)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(BracedExprTerm bracedExprTerm)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(Literal literal)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(BasicType literal)
        {
            throw new NotImplementedException();
        }

        public TypeBase? Visit(GenericType literal)
        {
            throw new NotImplementedException();
        }
    }
}
