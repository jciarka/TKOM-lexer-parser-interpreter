﻿using Application.Models.Exceptions.ConfigurationParser;
using Application.Models.Grammar;
using Application.Models.Grammar.Expressions.Terms;
using Application.Models.Tokens;
using Application.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.SourseParser
{
    public class UnresolvableVarTypeException : TypeVerifierException
    {
        public string? Variable { get; }

        public UnresolvableVarTypeException(string variable, RulePosition position)
            : base(new CharacterPosition(position), prepareMessage(position, variable))
        {
            Variable = variable;
        }

        private static string prepareMessage(RulePosition position, string variable)
        {
            return $"(LINE: {position.Line}) " +
                $"Can't resolve VAR type for variable {variable} - expression is missing or does not return a value";
        }
    }
}
