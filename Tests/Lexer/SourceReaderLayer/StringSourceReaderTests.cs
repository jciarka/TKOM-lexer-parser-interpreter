using Application.Infrastructure.Lekser.Helpers;
using Application.Infrastructure.Lekser.SourceReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Lexer.SourceReaderLayer
{
    public class StringSourceReaderTests
    {
        [Fact]
        public void ShouldReturnEOFAtEndOfFile()
        {
            // arrange
            var empty = "";


            var reader = new StringSourceReader(empty);

            // act and assert

            for (int i = 0; i < 5; i++)
            {
                var peekResult = reader.Peek();
                var readResult = reader.Read();

                Assert.Equal(peekResult, CharactersHelpers.EOF);
                Assert.Equal(readResult, CharactersHelpers.EOF);
            }
        }

        [Fact]
        public void ShouldReturnNlAtEndOfLine()
        {
            // arrange
            var empty = "\n";

            var reader = new StringSourceReader(empty);

            // act and assert
            var peekResult = reader.Peek();
            var readResult = reader.Read();

            Assert.Equal(peekResult, CharactersHelpers.NL);
            Assert.Equal(readResult, CharactersHelpers.NL);
        }

        [Fact]
        public void ShouldReturnLetterAndIncrementCoulmnOnSingleLineRead()
        {
            // arrange
            var line = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

            var reader = new StringSourceReader(line);

            // act and assert
            int i = 0;

            foreach (var letter in line)
            {
                Assert.Equal(reader.Column, i);
                Assert.Equal(reader.Position, i);

                i++;

                var peekResult = reader.Peek();
                var readResult = reader.Read();

                Assert.Equal(peekResult, letter);
                Assert.Equal(readResult, letter);
            }
        }

        [Fact]
        public void ShouldIncrementLineOnMultiLineRead()
        {
            // arrange

            int linesNumbers = 5;
            var builder = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                builder.AppendLine("");
            }

            var reader = new StringSourceReader(builder.ToString());

            // act and assert
            int lineNumber = 0;

            for (int i = 0; i < linesNumbers; i++)
            {
                Assert.Equal(reader.Line, lineNumber);
                Assert.Equal(reader.Position, lineNumber * Environment.NewLine.Length);

                lineNumber++;
                reader.Read();
            }
        }

        [Fact]
        public void ShouldReturnLetterOnMultilineLineReadAndIncrementCoulmnAndLine()
        {
            // arrange
            var lines = new List<string>()
                    {
                        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                        "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.",
                        "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
                    };

            var builder = new StringBuilder();

            lines.ForEach(line => builder.AppendLine(line));

            var reader = new StringSourceReader(builder.ToString());

            // act and assert
            int letterNumber = 0;
            int lineNumber = 0;
            int columnNumber = 0;

            foreach (string line in lines)
            {
                foreach (char letter in line)
                {
                    Assert.Equal(reader.Column, columnNumber++);
                    Assert.Equal(reader.Line, lineNumber);
                    Assert.Equal(reader.Position, letterNumber++);

                    var peekResult = reader.Peek();
                    var readResult = reader.Read();

                    Assert.Equal(peekResult, letter);
                    Assert.Equal(readResult, letter);
                }

                Assert.Equal(reader.Position, letterNumber);
                columnNumber = 0;
                lineNumber++;
                letterNumber += Environment.NewLine.Length;

                var peekEndOfLine = reader.Peek();
                var readEndOfLine = reader.Read();

                Assert.Equal(peekEndOfLine, CharactersHelpers.NL);
                Assert.Equal(readEndOfLine, CharactersHelpers.NL);
            }
        }
    }
}

