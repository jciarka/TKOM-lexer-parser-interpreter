using Application.Infrastructure.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Values.NativeLibrary
{
    public interface INativeCallable : ICallable
    {
        public FunctionSignature Signature { get; }
    }
}
