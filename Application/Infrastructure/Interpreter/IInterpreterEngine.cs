using Application.Infrastructure.ErrorHandling;
using Application.Infrastructure.Presenters;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Values;

namespace Application.Infrastructure.Interpreter
{
    public interface IInterpreterEngine
    {
        bool InterpretProgram(ProgramRoot program);
        IValue InterpretFunctionCall(FunctionDecl declaration, IEnumerable<Parameter> parameters, IEnumerable<IValue> arguments);
        IValue InterpretLambdaCall(Lambda lambda, IEnumerable<Parameter> parameters, IEnumerable<IValue> arguments);
    }
}