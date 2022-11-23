using Application.Models.Exceptions;

namespace Application.Infrastructure.ErrorHandling
{
    public interface IErrorHandler
    {
        public void HandleError(ComputingException issue);
    }
}