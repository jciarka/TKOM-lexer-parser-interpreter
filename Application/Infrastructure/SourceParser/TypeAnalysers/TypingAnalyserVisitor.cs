using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Interpreter;
using Application.Models.Exceptions;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;
using Application.Models.Values;
using System.Linq;

namespace Application.Infrastructure.Presenters
{
    public class TypingAnalyser : ITypingAnalyseVisitor
    {
        private readonly IErrorHandler _errorHandler;
        private readonly TypingAnalyserOptions _options;

        private IEnumerable<FunctionDecl> _allDeclarations = new List<FunctionDecl>();
        private FunctionCallTypeAnalyseContext? _context;

        public TypingAnalyser(IErrorHandler errorHandler, TypingAnalyserOptions? options = null)
        {
            _errorHandler = errorHandler;
            _options = options ?? new TypingAnalyserOptions();
        }

        public TypeBase Visit(ProgramRoot node)
        {
            _allDeclarations = node.FunctionDeclarations;

            foreach (var function in node.FunctionDeclarations)
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

            return new NoneType();
        }

        public TypeBase Visit(FunctionDecl node)
        {
            var parsedFunctions = _allDeclarations
                .ToDictionary(x => (FunctionSignature)new FixedArgumentsFunctionSignature(x), x => x.Type);

            var nativeFunctions = _options.NativeCallables
                .ToDictionary(x => x.Signature, x => x.Signature.ReturnType);

            var functions = nativeFunctions
                .Union(parsedFunctions)
                .ToDictionary(x => x.Key, x => x.Value);

            var prototypes = _options.NativeClasses
                .ToDictionary(x => x.Create().Name, x => (IClassPrototype)x);

            _context = new FunctionCallTypeAnalyseContext(node, new CallableAnalyseSet(functions), new ClassAnalyseSet(prototypes));

            node.Block.Accept(this);

            return new NoneType();
        }

        public TypeBase Visit(BlockStmt node)
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

            return new NoneType();
        }

        public TypeBase Visit(IdentifierAssignmentStatement node)
        {
            if (!_context!.Scope.TryFind(node.Identifier.Name, out var variableType))
            {
                _errorHandler.HandleError(new NotDefinedVariableException(node.Identifier.Name));
            }

            var expressionType = node.Expression.Accept(this);

            if (variableType != expressionType)
            {
                _errorHandler.HandleError(new InvalidTypeException(expressionType, node.Position, variableType!.Type));
            }

            return new NoneType();
        }

        public TypeBase Visit(PropertyAssignmentStatement propertyAssignmentStatement)
        {
            return new NoneType();
        }

        public TypeBase Visit(IndexAssignmentStatement indexAssignmentStatement)
        {
            return new NoneType();
        }

        public TypeBase Visit(DeclarationStmt node)
        {
            var expressionType = node.Expression != null ? node.Expression.Accept(this) : null;

            if (expressionType == null && node.Type == null)
            {
                _errorHandler.HandleError(new UnresolvableVarTypeException(node.Identifier.Name, node.Position));
            }
            else if (node.Type == null)
            {
                node.Type = expressionType;
            }
            else if (!node.Type.Equals(expressionType))
            {
                _errorHandler.HandleError(new InvalidTypeException(expressionType, node.Position, node.Type.Type));
            }

            if (!_context!.Scope.TryAdd(node.Identifier.Name, node.Type!))
            {
                _errorHandler.HandleError(new VariableRedefiniitionException(node.Identifier.Name, node.Position));
            }

            return new NoneType();
        }

        public TypeBase Visit(ReturnStmt node)
        {
            var returnExpressionType = node.ReturnExpression != null ? node.ReturnExpression.Accept(this) : new NoneType();

            if (!_context!.CheckReturnType(returnExpressionType))
            {
                if (_context.ReturnType == new NoneType())
                {
                    _errorHandler.HandleError(new InvalidTypeException(returnExpressionType, node.Position, _context!.ReturnType.Type));
                }
                else
                {
                    _errorHandler.HandleError(new InvalidTypeException(returnExpressionType, node.Position, TypeEnum.VOID));
                }
            }

            return returnExpressionType;
        }

        public TypeBase Visit(WhileStmt node)
        {
            var conditionType = node.Condition.Accept(this);

            if (!checkType(conditionType, TypeEnum.BOOL))
            {
                _errorHandler.HandleError(new InvalidTypeException(conditionType, node.Position, TypeEnum.BOOL));
            }

            node.Statement.Accept(this);

            return new NoneType();
        }

        public TypeBase Visit(IfStmt node)
        {
            var conditionType = node.Condition.Accept(this);

            if (!checkType(conditionType, TypeEnum.BOOL))
            {
                _errorHandler.HandleError(new InvalidTypeException(conditionType, node.Position, TypeEnum.BOOL));
            }

            node.ThenStatement.Accept(this);

            if (node.ElseStatement != null)
            {
                node.ElseStatement.Accept(this);
            }

            return new NoneType();
        }

        public TypeBase Visit(ForeachStmt node)
        {
            var collectionType = node.CollectionExpression.Accept(this);

            if (collectionType!.GetType() != typeof(GenericType))
            {
                _errorHandler.HandleError(new InvalidTypeException(collectionType, node.Position, TypeEnum.ACCOUNT));
            }

            var genericCollectionType = (GenericType)collectionType;

            if (node.Parameter.Type != genericCollectionType)
            {
                _errorHandler.HandleError(new InvalidTypeException(genericCollectionType, node.Position, node.Parameter.Type.Type));
            }

            node.Statement.Accept(this);

            return new NoneType();
        }

        public TypeBase Visit(FinancialToStmt node)
        {
            var accountExpression = node.AccountExpression.Accept(this);

            if (!accountExpression.Name.Equals(TypeName.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, node.Position, TypeEnum.ACCOUNT));
            }

            var valueExpression = node.ValueExpression.Accept(this);

            if (node.Operator == TokenType.TRANSFER_TO)
            {
                if (!checkType(valueExpression, TypeEnum.CURRENCY))
                {
                    _errorHandler.HandleError(new InvalidTypeException(valueExpression, node.Position, TypeEnum.CURRENCY));
                }
            }
            else
            {
                if (!checkType(valueExpression, TypeEnum.INT, TypeEnum.DECIMAL))
                {
                    _errorHandler.HandleError(new InvalidTypeException(valueExpression, node.Position, TypeEnum.CURRENCY));
                }
            }

            return new NoneType();
        }

        public TypeBase Visit(FinancialFromStmt node)
        {
            var accountExpression = node.AccountFromExpression.Accept(this);

            if (!accountExpression.Name.Equals(TypeName.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, node.Position, TypeEnum.ACCOUNT));
            }

            var valueExpression = node.ValueExpression.Accept(this);

            if (node.Operator == TokenType.TRANSFER_FROM)
            {
                if (!checkType(valueExpression, TypeEnum.CURRENCY))
                {
                    _errorHandler.HandleError(new InvalidTypeException(valueExpression, node.Position, TypeEnum.CURRENCY));
                }
            }
            else
            {
                if (!checkType(valueExpression, TypeEnum.INT, TypeEnum.DECIMAL))
                {
                    _errorHandler.HandleError(new InvalidTypeException(valueExpression, node.Position, TypeEnum.CURRENCY));
                }
            }

            if (node.AccountToExpression != null)
            {
                var accountToExpression = node.AccountToExpression.Accept(this);

                if (!accountExpression.Name.Equals(TypeName.ACCOUNT))
                {
                    _errorHandler.HandleError(new InvalidTypeException(accountToExpression, node.Position, TypeEnum.ACCOUNT));
                }
            }

            return new NoneType();
        }

        public TypeBase Visit(Parameter parameter)
        {
            return parameter.Type;
        }

        public TypeBase Visit(Lambda lambda)
        {
            if (lambda.Stmt.GetType() == typeof(ExpressionStmt))
            {
                return visitExpressionLambda(lambda);
            }
            else
            {
                return visitBlockLambda(lambda);
            }
        }

        private TypeBase visitBlockLambda(Lambda lambda)
        {
            var oldContext = _context;
            TypeBase type = new NoneType();

            try
            {
                _context = new FunctionCallTypeAnalyseContext(lambda, oldContext!);
                type = _context.ReturnType;
            }
            catch (ComputingException issue)
            {
                _errorHandler?.HandleError(issue);
            }
            finally
            {
                _context = oldContext;
            }

            return new GenericType(TypeName.LAMBDA, type);
        }

        private TypeBase visitExpressionLambda(Lambda lambda)
        {
            TypeBase type = new NoneType();

            try
            {
                _context!.PushScope();
                _context.Scope.TryAdd(lambda.Parameter.Identifier, lambda.Parameter.Type);
                type = lambda.Stmt.Accept(this);
            }
            catch (ComputingException issue)
            {
                _errorHandler?.HandleError(issue);
            }
            finally
            {
                _context!.PopScope();
            }

            return new GenericType(TypeName.LAMBDA, type);
        }

        public TypeBase Visit(Identifier identifier)
        {
            if (!_context!.Scope.TryFind(identifier.Name, out var variableType))
            {
                throw new NotDefinedVariableException(identifier.Name);
            }

            return variableType!;
        }

        public TypeBase Visit(ExpressionArgument expressionArgument)
        {
            return expressionArgument.Expression.Accept(this);
        }

        public TypeBase Visit(ExpressionStmt expressionStmt)
        {
            return expressionStmt.RightExpression.Accept(this);
        }

        public TypeBase Visit(OrExpr node)
        {
            return new BasicType(TypeName.BOOL, TypeEnum.BOOL);
        }

        public TypeBase Visit(AdditiveExpr node)
        {
            var first = node.FirstOperand.Accept(this);

            if (!checkType(first, TypeEnum.INT, TypeEnum.DECIMAL, TypeEnum.CURRENCY))
            {
                _errorHandler.HandleError(new InvalidTypeException(null, node.Position, TypeEnum.INT, TypeEnum.DECIMAL, TypeEnum.CURRENCY));
            }

            foreach (var operand in node.Operands)
            {
                var next = operand.Item2.Accept(this);

                if (first!.Name != next?.Name)
                {
                    _errorHandler.HandleError(new InvalidTypeException(next, node.Position, first!.Type));
                }
            }

            return first;
        }

        public TypeBase Visit(MultiplicativeExpr node)
        {
            var first = node.FirstOperand.Accept(this);

            if (!checkType(first, TypeEnum.INT, TypeEnum.DECIMAL))
            {
                _errorHandler.HandleError(new InvalidTypeException(null, node.Position, TypeEnum.INT, TypeEnum.DECIMAL));
            }

            foreach (var operand in node.Operands)
            {
                var next = operand.Item2.Accept(this);

                if (first!.Type != next?.Type)
                {
                    _errorHandler.HandleError(new InvalidTypeException(next, node.Position, first!.Type));
                }
            }

            return first;
        }

        public TypeBase Visit(NegativeExpr node)
        {
            var first = node.Operand.Accept(this);

            if (node.Operator == TokenType.MINUS)
            {
                if (checkType(first, TypeEnum.INT, TypeEnum.DECIMAL))
                {
                    return first;
                }

                _errorHandler.HandleError(new InvalidTypeException(first, node.Position, TypeEnum.INT, TypeEnum.DECIMAL));

                return new BasicType(TypeName.INT, TypeEnum.INT);
            }

            if (node.Operator == TokenType.BANG)
            {
                if (checkType(first, TypeEnum.BOOL))
                {
                    return first;
                }

                _errorHandler.HandleError(new InvalidTypeException(first, node.Position, TypeEnum.BOOL));
                return new BasicType(TypeName.BOOL, TypeEnum.BOOL);
            }

            return new NoneType();
        }

        public TypeBase Visit(ConversionExpr node)
        {
            var next = node.TypeExpression.Accept(this);

            if (!checkType(next, TypeEnum.TYPE))
            {
                _errorHandler.HandleError(new InvalidTypeException(next, node.Position, TypeEnum.TYPE));
            }

            return ((TypeType)next!).OfType;
        }

        public TypeBase Visit(AndExpr andExpr)
        {
            return new BasicType(TypeName.BOOL, TypeEnum.BOOL);
        }

        public TypeBase Visit(FunctionCallExpr node)
        {
            var argumentTypes = node.Arguments.Select(x => x.Accept(this));
            var callDescription = new FunctionCallExprDescription(node.Name, argumentTypes);

            if (!_context!.CallableSet.TryFind(callDescription, out var returnType))
            {
                _errorHandler?.HandleError(new FunctionNotDeclaredException(callDescription, node.Position));
            }

            return returnType!;
        }

        public TypeBase Visit(ComparativeExpr node)
        {
            var first = node.FirstOperand.Accept(this);

            if (first?.Type == null)
            {
                _errorHandler.HandleError(new InvalidTypeException(null, node.Position, TypeEnum.DECIMAL));
            }

            foreach (var operand in node.Operands)
            {
                var next = operand.Item2.Accept(this);

                if (first!.Type != next?.Type)
                {
                    _errorHandler.HandleError(new InvalidTypeException(next, node.Position, TypeEnum.INT, TypeEnum.DECIMAL));
                }
            }

            return new BasicType(TypeName.BOOL, TypeEnum.BOOL);
        }

        public TypeBase Visit(PrctOfExpr node)
        {
            var accountExpression = node.FirstOperand.Accept(this);

            if (!accountExpression.Name.Equals(TypeName.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, node.Position, TypeEnum.GENERIC));
            }

            var prctExpression = node.SecondOperand.Accept(this);

            if (!checkType(prctExpression, TypeEnum.INT, TypeEnum.DECIMAL))
            {
                _errorHandler.HandleError(new InvalidTypeException(prctExpression, node.Position, TypeEnum.INT, TypeEnum.DECIMAL));
            }

            return new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL);
        }

        public TypeBase Visit(ConstructiorCallExpr node)
        {
            var parameterTypes = node.Arguments.Select(x => x.Accept(this));

            if (!_context!.ClassSet.TryFindConstructor(node.Type, parameterTypes))
            {
                _errorHandler?.HandleError(new ClassNotDeclaredException(node.Type.Name, node.Position));
            }

            return node.Type;
        }

        public TypeBase Visit(ObjectPropertyExpr node)
        {
            var objectType = node.Object.Accept(this);

            if (!_context!.ClassSet.TryFindProperty(objectType, node.Property, out var propertyType))
            {
                _errorHandler?.HandleError(new PropertyNotDeclaredException(objectType.Name, node.Property, node.Position));
            }

            return propertyType!;
        }

        public TypeBase Visit(ObjectIndexExpr node)
        {
            var objectType = node.Object.Accept(this);

            if (!objectType.Name.Equals(TypeName.COLLECTION))
            {
                _errorHandler?.HandleError(new ClassNotDeclaredException(objectType.Name, node.Position));
            }

            return (objectType as GenericType)!.ParametrisingType;
        }

        public TypeBase Visit(ObjectMethodExpr node)
        {
            var objectType = node.Object.Accept(this);
            var parameterTypes = node.Arguments.Select(x => x.Accept(this));

            if (!_context!.ClassSet.TryFindMethod(
                objectType, new FunctionCallExprDescription(node.Method, parameterTypes), out var returnType))
            {
                _errorHandler?.HandleError(new ClassNotDeclaredException(objectType.Name, node.Position));
            }

            return returnType!;
        }

        public TypeBase Visit(BracedExprTerm node)
        {
            return node.Expression.Accept(this);
        }

        public TypeBase Visit(Literal node)
        {
            return node.Type;
        }

        public TypeBase Visit(BasicType node)
        {
            return new TypeType(node);
        }

        public TypeBase Visit(GenericType node)
        {
            return new TypeType(node);
        }

        public TypeBase Visit(TypeType node)
        {
            return node;
        }

        public bool checkType(TypeBase type, params TypeEnum[] types)
        {
            if (type == null)
            {
                return false;
            }

            return types.Any(t => type?.Type == t);
        }
    }
}
