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
    public class InvalidTypeException : TypingAnalyseException
    {
        public IEnumerable<TypeEnum> ExpectedTypes { get; }
        public TypeBase? CurrentType { get; }

        public InvalidTypeException(TypeBase? currentType, params TypeEnum[] expectedTypes)
            : base(new CharacterPosition(), prepareMessage(new CharacterPosition(), currentType, expectedTypes))
        {
            ExpectedTypes = expectedTypes;
            CurrentType = currentType;
        }

        private static string prepareMessage(CharacterPosition position, TypeBase? currentType, IEnumerable<TypeEnum> expectedTypes)
        {
            string current = currentType != null ? currentType.Type.ToString() : "void";

            return $"(LINE: {position.Line}, column: {position.Column}) " +
                $"Unexpected type of expression: \"{current}\", expected \"{string.Join(" / ", expectedTypes)}\"";
        }
    }
}