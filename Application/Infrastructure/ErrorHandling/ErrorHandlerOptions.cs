namespace Application.Infrastructure.Presenters
{
    public class ErrorHandlerOptions
    {
        public int MaxErrorCount { get; } = 20;

        public ErrorHandlerOptions()
        {

        }

        public ErrorHandlerOptions(int maxErrorCount)
        {
            MaxErrorCount = maxErrorCount;
        }
    }
}