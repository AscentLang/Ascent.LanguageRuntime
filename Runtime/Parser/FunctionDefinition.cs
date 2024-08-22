using System.Collections.Generic;

namespace AscentLanguage.Parser
{
    public class FunctionDefinition
    {
        public FunctionDefinition(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public List<string> Args { get; set; } = new();
        public Expression[] Contents { get; set; }
    }
}
