using Application.Models.Exceptions;

namespace Application.Infrastructure.ErrorHandling
{
    public interface IErrorHandler
    {
        public int ErrorCount();
        public void HandleError(ComputingException issue);
    }
}