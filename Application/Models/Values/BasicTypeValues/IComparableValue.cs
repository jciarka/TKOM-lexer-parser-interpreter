namespace Application.Models.Values.BasicTypeValues
{
    public interface IComparableValue
    {
        public BoolValue Greater(IValue other);
        public BoolValue GreaterEqual(IValue other);
        public BoolValue Less(IValue other);
        public BoolValue LessEqual(IValue other);
    }
}
