using System.Collections.Generic;
using AscentLanguage.Parser;
using AscentLanguage.Var;

namespace AscentLanguage.Data
{
    public class AscentScriptData
    {
        public AscentScriptData Clone()
        {
            var clone = new AscentScriptData
            {
                Variables = new Dictionary<string, Variable>(Variables),
                Predicates = new List<string>(Predicates)
            };
            return clone;
        }

        public Dictionary<string, Variable> Variables { get; set; } = new();
        public List<string> Predicates { get; set; } = new();
        public Dictionary<string, FunctionDefinition> Functions { get; set; } = new();
	}
}
