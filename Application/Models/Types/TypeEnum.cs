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
        GENERIC
    }

    public static class TypeName
    {
        public static string BOOL => "bool";
        public static string INT => "int";
        public static string DECIMAL => "decimal";
        public static string STRING => "string";
        public static string TYPE => "Type";
        public static string ACCOUNT => "Account";
        public static string COLLECTION => "Collection";
        public static string EXTERNAL => "External";
    }
}
