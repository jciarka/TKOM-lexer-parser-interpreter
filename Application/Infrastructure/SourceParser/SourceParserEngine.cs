using Application.Infrastructure.Lekser;
using Application.Models.Exceptions;
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
        }

        private ICollection<ComputingException> issues;

        public ProgramRoot Parse(IEnumerable<ComputingException> parseIssues)
        {
            issues = new List<ComputingException>();

            var functions = parseFunctionDeclarations();

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
            }
            catch (ComputingException ex)
            {
                issues.Add(ex);
            }

            return functions;
        }

        private bool tryParseFunctionDeclaration(out FunctionDecl? function)
        {
            if (_lexer.Peek().Type != TokenType.TYPE)
            {
                function = null;
                return false;
            }

            var type = _lexer.Read().Lexeme!;

            if (_lexer.Peek().Type != TokenType.IDENTIFIER)
            {
                throw new ComputingException(_lexer.Read().Position!);
            }

            var name = _lexer.Read().Lexeme!;

            if (_lexer.Peek().Type != TokenType.LEFT_PAREN)
            {
                issues.Add(new ComputingException(_lexer.Read().Position!)); // CHANGE
            }

            parseParameters(out IEnumerable<Parameter> parameters);

            if (_lexer.Peek().Type != TokenType.RIGHT_PAREN)
            {
                issues.Add(new ComputingException(_lexer.Read().Position!)); // CHANGE
            }

            parseBlock(out BlockStmt block);

            function = new FunctionDecl(type, name, parameters, block);
            return true;
        }

        private void parseBlock(out BlockStmt block)
        {
            throw new NotImplementedException();
        }

        private void parseParameters(out IEnumerable<Parameter> parameters)
        {
            throw new NotImplementedException();
        }
    }
}
