using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Presenters;
using Application.Models.Exceptions;
using Application.Models.Exceptions.Interpreter;
using Application.Models.Exceptions.SourseParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Values;
using Application.Models.Values.BasicTypeValues;
using Application.Models.Values.NativeLibrary;

namespace Application.Infrastructure.Interpreter
{
    public class InterpreterEngine : IVisitor, IInterpreterEngine
    {
        private readonly IErrorHandler _errorHandler;
        private readonly InterpreterEngineOptions _options;

        private IEnumerable<FunctionDecl> _allDeclarations = new List<FunctionDecl>();
        public FunctionCallContext? _functionContext;
        public ProgramCallContext? _programContext;
        public Stack<IValue> _stack;

        public InterpreterEngine(IErrorHandler errorHandler, InterpreterEngineOptions? options = null)
        {
            _errorHandler = errorHandler;
            _options = options ?? new InterpreterEngineOptions();
            _stack = new Stack<IValue>();
        }

        public bool InterpretProgram(ProgramRoot program)
        {
            try
            {
                program.Accept(this);
            }
            catch (RuntimeException ex)
            {
                _errorHandler.HandleError(ex);
                return false;
            }

            return true;
        }
        public IValue InterpretFunctionCall(FunctionDecl declaration, IEnumerable<Parameter> parameters, IEnumerable<IValue> arguments)
        {
            var oldContext = _functionContext;
            _functionContext = new FunctionCallContext(parameters, arguments);

            IValue? value = null;
            try
            {
                accept(declaration);
            }
            catch (ReturnValue returnValue)
            {
                value = returnValue.Value;
            }
            catch (ComputingException)
            {
                throw;
            }
            finally
            {
                _functionContext = oldContext;
            }

            return value!;
        }

        public IValue InterpretLambdaCall(Lambda lambda, IEnumerable<Parameter> parameters, IEnumerable<IValue> arguments)
        {
            IValue? value = null;

            _functionContext!.PushScope();

            foreach (var paramValue in parameters.Zip(arguments))
            {
                _functionContext.Scope.Add(paramValue.First.Identifier, paramValue.Second);
            }

            try
            {
                value = accept(lambda.Stmt);
            }
            catch (ReturnValue returnValue)
            {
                value = returnValue.Value;
            }
            catch (ComputingException)
            {
                throw;
            }
            finally
            {
                _functionContext.PopScope();
            }

            return value!;
        }

        public void Visit(ProgramRoot node)
        {
            _allDeclarations = node.FunctionDeclarations.Values;

            var parsedFunctions = _allDeclarations
                .ToDictionary(x => (FunctionSignature)new FixedArgumentsFunctionSignature(x), x => new Callable(x) as ICallable);

            var nativeFunctions = _options.NativeCallables
                .ToDictionary(x => x.Signature, x => x as ICallable);

            var functions = nativeFunctions
                .Union(parsedFunctions)
                .ToDictionary(x => x.Key, x => x.Value);

            var prototypes = _options.NativeClasses
                .ToDictionary(x => x.Create().Name, x => (IClassPrototype)x);

            _programContext = new ProgramCallContext(new CallableSet(functions), new ClassSet(prototypes));

            var main = _allDeclarations.FirstOrDefault(x => x.Name.Equals("main"));

            InterpretFunctionCall(main!, new Parameter[] { }, new IValue[] { });
        }

        public void Visit(FunctionDecl functionDecl)
        {
            push(accept(functionDecl.Block));
        }

        public void Visit(BlockStmt node)
        {
            try
            {
                _functionContext!.PushScope();

                foreach (var statement in node.Statements)
                {
                    statement.Accept(this);
                }
            }
            catch (ComputingException)
            {
                throw;
            }
            finally
            {
                _functionContext!.PopScope();
            }

            push(new EmptyValue());
        }

        public void Visit(IdentifierAssignmentStatement node)
        {
            _functionContext!.Scope.Set(node.Identifier.Name, accept(node.Expression));
            push(new EmptyValue());
        }

        public void Visit(PropertyAssignmentStatement node)
        {
            var newValue = accept(node.Expression);
            var @object = (Reference)accept(node.Property.Object);

            if (@object.Instance == null)
            {
                throw new RuntimeNullReferenceException(node.Position, $"Property {node.Property.Property}");
            }

            @object.Instance.SetProperty(node.Property.Property, newValue);
            push(new EmptyValue());
        }

        public void Visit(IndexAssignmentStatement node)
        {
            var newValue = accept(node.Expression);
            var @object = (Reference)accept(node.IndexExpr.Object);
            var index = ((IntValue)accept(node.IndexExpr.IndexExpression)).Value;

            if (@object.Instance == null)
            {
                throw new RuntimeNullReferenceException(node.Position, $"Index {index}");
            }

            var collection = (CollectionInstance)@object.Instance;

            if (index >= collection.Values.Count())
            {
                throw new ReferenceOutOfRangeException(index, node.Position);
            }

            collection.Values[index] = newValue;
            push(new EmptyValue());
        }

        public void Visit(DeclarationStmt node)
        {
            var expressionValue = node.Expression != null ? accept(node.Expression) : ValuesFactory.GetDefaultValue(node.Type!);
            _functionContext!.Scope.Add(node.Identifier.Name, expressionValue);
            push(new EmptyValue());
        }

        public void Visit(ReturnStmt node)
        {
            var value = node.ReturnExpression != null ? accept(node.ReturnExpression) : new EmptyValue();
            throw new ReturnValue(value);
        }

        public void Visit(WhileStmt node)
        {
            while (((BoolValue)accept(node.Condition)).Value)
            {
                accept(node.Statement);
            }

            push(new EmptyValue());
        }

        public void Visit(IfStmt node)
        {
            if (((BoolValue)accept(node.Condition)).Value)
            {
                accept(node.ThenStatement);
            }
            else if (node.ElseStatement != null)
            {
                accept(node.ElseStatement);
            }

            push(new EmptyValue());
        }

        public void Visit(ForeachStmt node)
        {
            var collection = (CollectionInstance)((Reference)accept(node.CollectionExpression)).Instance!;

            foreach (var item in collection.Values)
            {
                _functionContext!.PushScope();

                _functionContext.Scope.Add(node.Parameter.Identifier, item);
                node.Statement.Accept(this);

                _functionContext!.PopScope();
            }

            push(new EmptyValue());
        }

        public void Visit(FinancialToStmt node)
        {
            var accountFrom = (AccountInstace)((Reference)accept(node.AccountExpression)).Instance!;

            var value = accept(node.ValueExpression);

            if (node.Operator == TokenType.TRANSFER_PRCT_TO)
            {
                value = accountFrom.PrctOf(value)
                    .To(new TypeValue(accountFrom.Type), null!);
            }

            accountFrom.Fund(_options.CurrencyTypesInfo, value);

            push(new NullValue());
        }

        public void Visit(FinancialFromStmt node)
        {
            var accountFrom = (AccountInstace)((Reference)accept(node.AccountFromExpression)).Instance!;

            var accountTo = node.AccountToExpression != null ?
                (AccountInstace)((Reference)accept(node.AccountToExpression)).Instance! : null;

            var value = accept(node.ValueExpression);

            if (node.Operator == TokenType.TRANSFER_PRCT_FROM)
            {
                value = accountFrom.PrctOf(value)
                    .To(new TypeValue(accountFrom.Type), null!);
            }

            accountFrom.Withdraw(_options.CurrencyTypesInfo, value, accountTo);

            push(new NullValue());
        }

        public void Visit(Parameter parameter)
        {
            throw new NotSupportedException();
        }

        public void Visit(Lambda node)
        {
            push(new Reference(new DelegateInstance(node.Parameter.Type, node)));
        }

        public void Visit(Identifier identifier)
        {
            _functionContext!.Scope.TryFind(identifier.Name, out var value);
            push(value!);
        }

        public void Visit(ExpressionArgument node)
        {
            push(accept(node.Expression));
        }

        public void Visit(ExpressionStmt stmt)
        {
            push(accept(stmt.RightExpression));
        }

        public void Visit(OrExpr node)
        {
            var value = accept(node.FirstOperand);

            foreach (var operand in node.Operands)
            {
                value = ((BoolValue)value).Or(accept(operand));
            }

            push(value);
        }

        public void Visit(AdditiveExpr node)
        {
            var value = accept(node.FirstOperand);

            foreach (var operand in node.Operands)
            {
                var next = accept(operand.Item2);

                switch (operand.Item1)
                {
                    case Models.Tokens.TokenType.PLUS:
                        value = ((IArthmeticValue)value).Add(next);
                        break;
                    case Models.Tokens.TokenType.MINUS:
                        value = ((IArthmeticValue)value).Sub(next);
                        break;
                }
            }

            push(value);
        }

        public void Visit(MultiplicativeExpr node)
        {
            var value = accept(node.FirstOperand);

            foreach (var operand in node.Operands)
            {
                var next = accept(operand.Item2);

                switch (operand.Item1)
                {
                    case Models.Tokens.TokenType.STAR:
                        value = ((IArthmeticValue)value).Mul(next);
                        break;
                    case Models.Tokens.TokenType.SLASH:
                        value = ((IArthmeticValue)value).Div(next);
                        break;
                }
            }

            push(value);
        }

        public void Visit(NegativeExpr node)
        {
            var value = accept(node.Operand);
            push(((INegatableValue)value).Negate());
        }

        public void Visit(ConversionExpr node)
        {
            var valueFrom = accept(node.OryginalExpression);
            var typeTo = accept(node.TypeExpression);
            push(valueFrom.To(typeTo, _options.CurrencyTypesInfo));
        }

        public void Visit(AndExpr node)
        {
            var value = accept(node.FirstOperand);

            foreach (var operand in node.Operands)
            {
                value = ((BoolValue)value).And(accept(operand));
            }

            push(value);
        }

        public void Visit(FunctionCallExpr node)
        {
            var argumentValues = node.Arguments.Select(x => accept(x));
            var argumentTypes = argumentValues.Select(x => x.Type);

            var callDescription = new FunctionCallExprDescription(node.Name, argumentTypes);

            _programContext!.CallableSet.TryFind(callDescription, out var callable);

            push(callable!.Call(this, argumentValues.ToArray())!);
        }

        public void Visit(ComparativeExpr node)
        {
            var value = accept(node.FirstOperand);

            foreach (var operand in node.Operands)
            {
                var next = accept(operand.Item2);

                switch (operand.Item1)
                {
                    case Models.Tokens.TokenType.EQUAL_EQUAL:
                        value = value.EqualEqual(next);
                        break;
                    case Models.Tokens.TokenType.BANG_EQUAL:
                        value = value.BangEqual(next);
                        break;
                    case Models.Tokens.TokenType.GREATER:
                        value = ((IComparableValue)value).Greater(next);
                        break;
                    case Models.Tokens.TokenType.GREATER_EQUAL:
                        value = ((IComparableValue)value).GreaterEqual(next);
                        break;
                    case Models.Tokens.TokenType.LESS:
                        value = ((IComparableValue)value).Less(next);
                        break;
                    case Models.Tokens.TokenType.LESS_EQUAL:
                        value = ((IComparableValue)value).LessEqual(next);
                        break;
                }
            }

            push(value);
        }

        public void Visit(PrctOfExpr prctOfExpr)
        {
            throw new NotImplementedException();
        }

        public void Visit(ConstructiorCallExpr node)
        {
            var arguments = node.Arguments.Select(x => accept(x));
            var argumentTypes = arguments.Select(x => x.Type);

            _programContext!.ClassSet.TryFindConstructor(node.Type, argumentTypes, out var constructor);

            push(constructor!.Call(this, arguments));
        }

        public void Visit(ObjectPropertyExpr node)
        {
            var @object = (Reference)accept(node.Object);

            if (@object.Instance == null)
            {
                throw new RuntimeNullReferenceException(node.Position, $"Property {node.Property}");
            }

            push(@object.Instance.GetProperty(node.Property));
        }

        public void Visit(ObjectIndexExpr node)
        {
            var @object = (Reference)accept(node.Object);
            var index = ((IntValue)accept(node.IndexExpression)).Value;

            if (@object.Instance == null)
            {
                throw new RuntimeNullReferenceException(node.Position, $"Index {index}");
            }

            var collection = (CollectionInstance)@object.Instance;

            if (index >= collection.Values.Count())
            {
                throw new ReferenceOutOfRangeException(index, node.Position);
            }

            push(collection.Values[index]);
        }

        public void Visit(ObjectMethodExpr node)
        {
            var @object = (Reference)accept(node.Object);
            var arguments = node.Arguments.Select(x => accept(x));
            var argumentsTypes = arguments.Select(x => x.Type);

            if (@object.Instance == null)
            {
                throw new RuntimeNullReferenceException(node.Position, $"Method {node.Method}");
            }

            var signature = new FixedArgumentsFunctionSignature(null!, node.Method, argumentsTypes);

            var method = @object.Instance.Class.Methods[signature].Item2;

            push(method.Call(this, (new IValue[] { @object }).Union(arguments)));
        }

        public void Visit(BracedExprTerm node)
        {
            push(accept(node.Expression));
        }

        public void Visit(Literal literal)
        {
            switch (literal.Type.Type)
            {
                case Models.Types.TypeEnum.INT:
                    push(new IntValue((int)literal.IntValue!));
                    return;
                case Models.Types.TypeEnum.DECIMAL:
                    push(new DecimalValue((decimal)literal.DecimalValue!)); ;
                    return;
                case Models.Types.TypeEnum.BOOL:
                    push(new BoolValue((bool)literal.BoolValue!)); ;
                    return;
                case Models.Types.TypeEnum.STRING:
                    push(new StringValue(literal.StringValue!)); ;
                    return;
                case Models.Types.TypeEnum.TYPE:
                    push(new TypeValue(literal.ValueType!)); ;
                    return;
                case Models.Types.TypeEnum.CURRENCY:
                    push(new CurrencyValue(literal.Type.Name, (decimal)literal.DecimalValue!)); ;
                    return;
                case Models.Types.TypeEnum.NULL:
                    push(new NullValue());
                    return;
            }

            throw new NotSupportedException();
        }

        public void Visit(NoneType node)
        {
            push(new TypeValue(node));
        }

        public void Visit(BasicType node)
        {
            push(new TypeValue(node));
        }

        public void Visit(GenericType node)
        {
            push(new TypeValue(node));
        }

        public void Visit(TypeType typeType)
        {
            throw new NotSupportedException();
        }

        public void push(IValue value)
        {
            _stack.Push(value);
        }

        public IValue accept(IVisitable node)
        {
            try
            {
                node.Accept(this);
            }
            catch (RuntimeException ex)
            {
                ex.AddToStackTrace((GrammarRuleBase)node);
                throw ex;
            }

            return _stack.Pop();
        }
    }
}