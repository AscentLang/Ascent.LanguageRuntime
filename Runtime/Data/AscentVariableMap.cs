using AscentLanguage.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscentLanguage
{
    public class AscentVariableMap
    {
        public AscentVariableMap(Dictionary<string, Var> queryVariables)
        {
            QueryVariables = queryVariables;
        }

        public AscentVariableMap()
        {
        }

        public Dictionary<string, ImportVar> ImportVariables { get; set; }
        public Dictionary<string, ImportVarUnity> ImportVariablesUnity { get; set; }
        public Dictionary<string, Var> QueryVariables { get; set; }
    }
}
