using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Types
{
    public enum TypeEnum
    {
        INT,
        DECIMAL,
        BOOL,
        STRING,
        ACCOUNT,
        TYPE,
        COLLECTION,
        CURRENCY,
        GENERIC,
        NULL
    }

    public static class TypeName
    {
        public const string BOOL = "bool";
        public const string INT = "int";
        public const string DECIMAL = "decimal";
        public const string STRING = "string";
        public const string TYPE = "Type";
        public const string ACCOUNT = "Account";
        public const string COLLECTION = "Collection";
        public const string EXTERNAL = "External";
        public const string NULL = "null";
        public const string LAMBDA = "Lambda";
    }
}
