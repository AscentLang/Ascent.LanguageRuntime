using System.Collections.Generic;
using AscentLanguage.Var;

namespace AscentLanguage.Data
{
    public class AscentVariableMap
    {
        public AscentVariableMap(Dictionary<string, Variable> queryVariables)
        {
            QueryVariables = queryVariables;
        }

        public Dictionary<string, ImportVar> ImportVariables { get; set; }
#if UNITY_5_3_OR_NEWER
        public Dictionary<string, ImportVarUnity> ImportVariablesUnity { get; set; }
#endif
        public Dictionary<string, Variable> QueryVariables { get; set; }
    }
}
