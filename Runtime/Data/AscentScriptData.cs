using AscentLanguage.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscentLanguage
{
    public class AscentScriptData
    {
        public AscentScriptData()
        {
            Variables = new Dictionary<string, Var>();
            Predicates = new List<string>();
        }

        public AscentScriptData Clone()
        {
            var clone = new AscentScriptData();
            clone.Variables = new Dictionary<string, Var>(Variables);
            clone.Predicates = new List<string>(Predicates);
            return clone;
        }

        public Dictionary<string, Var> Variables { get; set; }
        public List<string> Predicates { get; set; }
		public Dictionary<string, FunctionDefinition> Functions { get; set; } = new Dictionary<string, FunctionDefinition>();
	}
}
