namespace Application.Models.Grammar
{
    public class Identifier : TermBase
    {
        public string Name { get; }

        public Identifier(string name)
        {
            Name = name;
        }
    }
}