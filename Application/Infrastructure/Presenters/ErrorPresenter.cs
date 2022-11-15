using Application.Models;
using Application.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Infrastructure.Presenters
{
    public class ErrorConsolePresenter : IDisposable
    {
        private readonly IRandomSourceReader _reader;

        public ErrorConsolePresenter(IRandomSourceReader reader)
        {
            _reader = reader;
        }


        public void Present(ComputingException exception)
        {
            Console.WriteLine(exception.Message);
            if (_reader.TryReadLineFromPosition(exception.Position.LinePosition, out var line))
            {
                showLineWithError(exception.Position, line);
            }
            Console.WriteLine();
        }

        public void Present(IEnumerable<ComputingException> exceptions)
        {
            var linesMappings = _reader.ReadManyLinesFromPositions(exceptions.Select(x => x.Position.LinePosition));


            foreach (var exception in exceptions)
            {
                Console.WriteLine(exception.Message);
                if (linesMappings.ContainsKey(exception.Position.LinePosition))
                {
                    showLineWithError(exception.Position, linesMappings[exception.Position.LinePosition]);
                }
            }

            Console.WriteLine();

        }

        private void showLineWithError(CharacterPosition position, string? line)
        {
            Console.WriteLine($"Linia: {line}");

            StringBuilder builder = new();
            builder.Append(Enumerable.Repeat(' ', 7 + (int)position.Column).ToArray());
            builder.Append("^ HERE !");

            Console.WriteLine(builder.ToString());
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
