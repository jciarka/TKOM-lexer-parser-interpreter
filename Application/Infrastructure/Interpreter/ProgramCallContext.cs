namespace Application.Infrastructure.Interpreter
{
    public class ProgramCallContext
    {
        public ICallableSet CallableSet { get; }
        public IClassSet ClassSet { get; }

        public ProgramCallContext(ICallableSet signaturesSet, IClassSet classAnalyseSet)
        {
            CallableSet = signaturesSet ?? new CallableSet(new());
            ClassSet = classAnalyseSet ?? new ClassSet(new());
        }
    }
}