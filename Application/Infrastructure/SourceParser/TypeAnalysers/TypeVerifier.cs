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
    public class TypeVerifier : IVisitor
    {
        private readonly IErrorHandler _errorHandler;
        private readonly TypeVerifierOptions _options;

        private IEnumerable<FunctionDecl> _allDeclarations = new List<FunctionDecl>();
        private FunctionCallTypeAnalyseContext? _context;
        private Stack<TypeBase> _stack;

        public TypeVerifier(IErrorHandler errorHandler, TypeVerifierOptions? options = null)
        {
            _errorHandler = errorHandler;
            _options = options ?? new TypeVerifierOptions();
            _stack = new Stack<TypeBase>();
        }

        public void Visit(ProgramRoot node)
        {
            _allDeclarations = node.FunctionDeclarations.Values;

            foreach (var function in node.FunctionDeclarations.Values)
            {
                try
                {
                    accept(function);
                }
                catch (ComputingException issue)
                {
                    _errorHandler.HandleError(issue);
                }
            }

            push(new NoneType());
        }

        public void Visit(FunctionDecl node)
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

            accept(node.Block);

            push(new NoneType());
        }

        public void Visit(BlockStmt node)
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

            push(new NoneType());
        }

        public void Visit(IdentifierAssignmentStatement node)
        {
            if (!_context!.Scope.TryFind(node.Identifier.Name, out var variableType))
            {
                _errorHandler.HandleError(new NotDefinedVariableException(node.Identifier.Name));
            }

            var expressionType = accept(node.Expression);

            if (variableType != expressionType)
            {
                _errorHandler.HandleError(new InvalidTypeException(expressionType, node.Position, variableType!.Type));
            }

            push(new NoneType());
        }

        public void Visit(PropertyAssignmentStatement node)
        {
            var exprType = accept(node.Expression);
            var propertyType = accept(node.Property);

            if (!exprType.Equals(propertyType))
                _errorHandler.HandleError(new InvalidTypeException(exprType, node.Position, propertyType.Type));

            push(new NoneType());
        }

        public void Visit(IndexAssignmentStatement node)
        {
            var exprType = accept(node.IndexExpr);

            if (!checkType(exprType, TypeEnum.INT))
            {
                _errorHandler.HandleError(new InvalidTypeException(exprType, node.Position, TypeEnum.INT));
            }

            push(new NoneType());
        }

        public void Visit(DeclarationStmt node)
        {
            var expressionType = node.Expression != null ? accept(node.Expression) : null;

            if (expressionType == null && node.Type == null)
            {
                _errorHandler.HandleError(new UnresolvableVarTypeException(node.Identifier.Name, node.Position));
            }
            else if (node.Type == null)
            {
                node.Type = expressionType;
            }
            else if (expressionType != null && !node.Type.Equals(expressionType))
            {
                _errorHandler.HandleError(new InvalidTypeException(expressionType, node.Position, node.Type.Type));
            }

            if (!_context!.Scope.TryAdd(node.Identifier.Name, node.Type!))
            {
                _errorHandler.HandleError(new VariableRedefiniitionException(node.Identifier.Name, node.Position));
            }

            push(new NoneType());
        }

        public void Visit(ReturnStmt node)
        {
            var returnExpressionType = node.ReturnExpression != null ? accept(node.ReturnExpression) : new NoneType();

            if (!_context!.CheckReturnType(returnExpressionType))
            {
                if (_context.ReturnType == new NoneType())
                {
                    _errorHandler.HandleError(new InvalidTypeException(returnExpressionType, node.Position, _context!.ReturnType.Type));
                }
                else
                {
                    _errorHandler.HandleError(new InvalidTypeException(returnExpressionType, node.Position, TypeEnum.NULL));
                }
            }

            push(returnExpressionType);
        }

        public void Visit(WhileStmt node)
        {
            var conditionType = accept(node.Condition);

            if (!checkType(conditionType, TypeEnum.BOOL))
            {
                _errorHandler.HandleError(new InvalidTypeException(conditionType, node.Position, TypeEnum.BOOL));
            }

            node.Statement.Accept(this);

            push(new NoneType());
        }

        public void Visit(IfStmt node)
        {
            var conditionType = accept(node.Condition);

            if (!checkType(conditionType, TypeEnum.BOOL))
            {
                _errorHandler.HandleError(new InvalidTypeException(conditionType, node.Position, TypeEnum.BOOL));
            }

            node.ThenStatement.Accept(this);

            if (node.ElseStatement != null)
            {
                node.ElseStatement.Accept(this);
            }

            push(new NoneType());
        }

        public void Visit(ForeachStmt node)
        {
            var collectionType = accept(node.CollectionExpression);

            if (collectionType!.GetType() != typeof(GenericType) || !collectionType.Name.Equals(TypeName.COLLECTION))
            {
                _errorHandler.HandleError(new InvalidTypeException(collectionType, node.Position, TypeEnum.COLLECTION));
            }

            var genericCollectionType = (GenericType)collectionType;

            if (!node.Parameter.Type.Equals(genericCollectionType.ParametrisingType))
            {
                _errorHandler.HandleError(new InvalidTypeException(genericCollectionType, node.Position, node.Parameter.Type.Type));
            }

            _context!.PushScope();
            _context!.Scope.TryAdd(node.Parameter.Identifier, node.Parameter.Type);
            try
            {
                node.Statement.Accept(this);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _context!.PopScope();
            }

            push(new NoneType());
        }

        public void Visit(FinancialToStmt node)
        {
            var accountExpression = accept(node.AccountExpression);

            if (!accountExpression.Name.Equals(TypeName.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, node.Position, TypeEnum.ACCOUNT));
            }

            var valueExpression = accept(node.ValueExpression);

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

            push(new NoneType());
        }

        public void Visit(FinancialFromStmt node)
        {
            var accountExpression = accept(node.AccountFromExpression);

            if (!accountExpression.Name.Equals(TypeName.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, node.Position, TypeEnum.ACCOUNT));
            }

            var valueExpression = accept(node.ValueExpression);

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
                var accountToExpression = accept(node.AccountToExpression);

                if (!accountExpression.Name.Equals(TypeName.ACCOUNT))
                {
                    _errorHandler.HandleError(new InvalidTypeException(accountToExpression, node.Position, TypeEnum.ACCOUNT));
                }
            }

            push(new NoneType());
        }

        public void Visit(Parameter parameter)
        {
            push(parameter.Type);
        }

        public void Visit(Lambda lambda)
        {
            if (lambda.Stmt.GetType() == typeof(ExpressionStmt))
            {
                push(visitExpressionLambda(lambda));
                return;
            }
            else
            {
                push(visitBlockLambda(lambda));
                return;
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
                type = accept(lambda.Stmt);
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

        public void Visit(Identifier identifier)
        {
            if (!_context!.Scope.TryFind(identifier.Name, out var variableType))
            {
                throw new NotDefinedVariableException(identifier.Name);
            }

            push(variableType!);
        }

        public void Visit(ExpressionArgument expressionArgument)
        {
            push(accept(expressionArgument.Expression));
        }

        public void Visit(ExpressionStmt expressionStmt)
        {
            push(accept(expressionStmt.RightExpression));
        }

        public void Visit(OrExpr node)
        {
            push(new BasicType(TypeName.BOOL, TypeEnum.BOOL));
        }

        public void Visit(AdditiveExpr node)
        {
            var first = accept(node.FirstOperand);

            if (!checkType(first, TypeEnum.INT, TypeEnum.DECIMAL, TypeEnum.CURRENCY))
            {
                _errorHandler.HandleError(new InvalidTypeException(null, node.Position, TypeEnum.INT, TypeEnum.DECIMAL, TypeEnum.CURRENCY));
            }

            foreach (var operand in node.Operands)
            {
                var next = accept(operand.Item2);

                if (first!.Name != next?.Name)
                {
                    _errorHandler.HandleError(new InvalidTypeException(next, node.Position, first!.Type));
                }
            }

            push(first);
        }

        public void Visit(MultiplicativeExpr node)
        {
            var first = accept(node.FirstOperand);

            if (!checkType(first, TypeEnum.INT, TypeEnum.DECIMAL))
            {
                _errorHandler.HandleError(new InvalidTypeException(null, node.Position, TypeEnum.INT, TypeEnum.DECIMAL));
            }

            foreach (var operand in node.Operands)
            {
                var next = accept(operand.Item2);

                if (first!.Type != next?.Type)
                {
                    _errorHandler.HandleError(new InvalidTypeException(next, node.Position, first!.Type));
                }
            }

            push(first);
        }

        public void Visit(NegativeExpr node)
        {
            var first = accept(node.Operand);

            if (node.Operator == TokenType.MINUS)
            {
                if (checkType(first, TypeEnum.INT, TypeEnum.DECIMAL))
                {
                    push(first);
                    return;
                }

                _errorHandler.HandleError(new InvalidTypeException(first, node.Position, TypeEnum.INT, TypeEnum.DECIMAL));

                push(new BasicType(TypeName.INT, TypeEnum.INT));
            }

            if (node.Operator == TokenType.BANG)
            {
                if (checkType(first, TypeEnum.BOOL))
                {
                    push(first);
                    return;
                }

                _errorHandler.HandleError(new InvalidTypeException(first, node.Position, TypeEnum.BOOL));
                push(new BasicType(TypeName.BOOL, TypeEnum.BOOL));
                return;
            }

            push(new NoneType());
        }

        public void Visit(ConversionExpr node)
        {
            var next = accept(node.TypeExpression);

            if (!checkType(next, TypeEnum.TYPE))
            {
                _errorHandler.HandleError(new InvalidTypeException(next, node.Position, TypeEnum.TYPE));
            }

            push(((TypeType)next!).OfType);
        }

        public void Visit(AndExpr andExpr)
        {
            push(new BasicType(TypeName.BOOL, TypeEnum.BOOL));
        }

        public void Visit(FunctionCallExpr node)
        {
            var argumentTypes = node.Arguments.Select(x => accept(x));
            var callDescription = new FunctionCallExprDescription(node.Name, argumentTypes);

            if (!_context!.CallableSet.TryFind(callDescription, out var returnType))
            {
                _errorHandler?.HandleError(new FunctionNotDeclaredException(callDescription, node.Position));
            }

            push(returnType!);
        }

        public void Visit(ComparativeExpr node)
        {
            var first = accept(node.FirstOperand);

            if (first?.Type == null)
            {
                _errorHandler.HandleError(new InvalidTypeException(null, node.Position, TypeEnum.DECIMAL));
            }

            foreach (var operand in node.Operands)
            {
                var next = accept(operand.Item2);

                if (first!.Type != next?.Type)
                {
                    if (!checkOperator(operand.Item1, TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL) || next?.Type != TypeEnum.NULL)
                    {
                        _errorHandler.HandleError(new InvalidTypeException(next, node.Position, TypeEnum.INT, TypeEnum.DECIMAL));
                    }
                }

                if (!checkOperator(operand.Item1, TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL)
                    && !checkType(next!, TypeEnum.INT, TypeEnum.DECIMAL, TypeEnum.CURRENCY))
                {
                    // only comparable type expected
                    _errorHandler.HandleError(new InvalidTypeException(next, node.Position, TypeEnum.INT, TypeEnum.DECIMAL, TypeEnum.CURRENCY));
                }
            }

            push(new BasicType(TypeName.BOOL, TypeEnum.BOOL));
        }

        private bool checkOperator(TokenType given, params TokenType[] expected)
        {
            if (expected.Contains(given))
                return true;

            return false;
        }

        public void Visit(PrctOfExpr node)
        {
            var accountExpression = accept(node.FirstOperand);

            if (!accountExpression.Name.Equals(TypeName.ACCOUNT))
            {
                _errorHandler.HandleError(new InvalidTypeException(accountExpression, node.Position, TypeEnum.GENERIC));
            }

            var prctExpression = accept(node.SecondOperand);

            if (!checkType(prctExpression, TypeEnum.INT, TypeEnum.DECIMAL))
            {
                _errorHandler.HandleError(new InvalidTypeException(prctExpression, node.Position, TypeEnum.INT, TypeEnum.DECIMAL));
            }

            push(new BasicType(TypeName.DECIMAL, TypeEnum.DECIMAL));
        }

        public void Visit(ConstructiorCallExpr node)
        {
            var parameterTypes = node.Arguments.Select(x => accept(x));

            if (!_context!.ClassSet.TryFindConstructor(node.Type, parameterTypes))
            {
                _errorHandler?.HandleError(new ClassNotDeclaredException(node.Type.Name, node.Position));
            }

            push(node.Type);
        }

        public void Visit(ObjectPropertyExpr node)
        {
            var objectType = accept(node.Object);

            if (!_context!.ClassSet.TryFindProperty(objectType, node.Property, out var propertyType))
            {
                _errorHandler?.HandleError(new PropertyNotDeclaredException(objectType.Name, node.Property, node.Position));
            }

            push(propertyType!);
        }

        public void Visit(ObjectIndexExpr node)
        {
            var objectType = accept(node.Object);

            if (!objectType.Name.Equals(TypeName.COLLECTION))
            {
                _errorHandler?.HandleError(new ClassNotDeclaredException(objectType.Name, node.Position));
            }

            push((objectType as GenericType)!.ParametrisingType);
        }

        public void Visit(ObjectMethodExpr node)
        {
            var objectType = accept(node.Object);
            var parameterTypes = node.Arguments.Select(x => accept(x));

            if (!_context!.ClassSet.TryFindMethod(
                objectType, new FunctionCallExprDescription(node.Method, parameterTypes), out var returnType))
            {
                _errorHandler?.HandleError(new MethodNotDeclaredException(objectType.Name, node.Method, node.Position));
            }

            push(returnType!);
        }

        public void Visit(BracedExprTerm node)
        {
            push(accept(node.Expression));
        }

        public void Visit(Literal node)
        {
            push(node.Type);
        }

        public void Visit(BasicType node)
        {
            push(new TypeType(node));
        }

        public void Visit(GenericType node)
        {
            push(new TypeType(node));
        }

        public void Visit(TypeType node)
        {
            push(node);
        }

        public void Visit(NoneType noneType)
        {
            // logic should never enter this method
            throw new NotImplementedException();
        }

        public bool checkType(TypeBase type, params TypeEnum[] types)
        {
            if (type == null)
            {
                return false;
            }

            return types.Any(t => type?.Type == t);
        }

        public void push(TypeBase type)
        {
            _stack.Push(type);
        }

        public TypeBase accept(IVisitable node)
        {
            node.Accept(this);
            return _stack.Pop();
        }
    }
}
