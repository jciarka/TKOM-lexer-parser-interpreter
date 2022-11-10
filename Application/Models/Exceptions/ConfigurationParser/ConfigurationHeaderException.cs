using Application.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Exceptions.ConfigurationParser
{
    public class ConfigurationException : ComputingException
    {
        public ConfigurationParsingSevernity Severnity { get; private set; }
        public Token Token { get; private set; }

        public ConfigurationException(Token token, ConfigurationParsingSevernity severnity) : base(token.Position!)
        {
            Severnity = severnity;
            Token = token;
        }

        public ConfigurationException(Token token, ConfigurationParsingSevernity severnity, string? message) : base(token!.Position!, message)
        {
            Token = token;
            Severnity = severnity;
        }
    }

    public enum ConfigurationParsingSevernity
    {
        CRITICAL,
        CONDITIONAL
    }
}
