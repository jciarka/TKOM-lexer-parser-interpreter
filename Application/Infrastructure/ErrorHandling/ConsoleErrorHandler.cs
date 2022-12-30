using Application.Infrastructure.ErrorHandling;
using Application.Models;
using Application.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public class ConsoleErrorHandler : IErrorHandler
    {
        private readonly IRandomSourceReader _reader;
        private readonly ErrorHandlerOptions _options;
        private readonly ICollection<ComputingException> _errors = new List<ComputingException>();

        public IEnumerable<ComputingException> Errors => _errors;

        public ConsoleErrorHandler(IRandomSourceReader reader, ErrorHandlerOptions? options = null)
        {
            _reader = reader;
            _options = options ?? new ErrorHandlerOptions();
        }

        public void HandleError(ComputingException issue)
        {
            _errors.Add(issue);

            present(issue);

            if (_errors.Count() >= _options.MaxErrorCount)
            {
                throw new BreakAndFinishComputingException();
            }
        }

        private void present(ComputingException exception)
        {
            Console.WriteLine(exception.Message);
            if (_reader.TryReadLineFromPosition(exception.Position.LinePosition, out var line))
            {
                showLineWithError(exception.Position, line);
            }
            Console.WriteLine();
        }


        private void showLineWithError(CharacterPosition position, string? line)
        {
            Console.WriteLine($"Line: {line}");

            StringBuilder builder = new();
            builder.Append(Enumerable.Repeat(' ', 6 + (int)position.Column).ToArray());
            builder.Append("^ HERE !");

            Console.WriteLine(builder.ToString());
        }

        public int ErrorCount()
        {
            return _errors.Count();
        }
    }
}
