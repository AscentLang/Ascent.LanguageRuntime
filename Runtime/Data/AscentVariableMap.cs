using System.Collections.Generic;

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
#if UNITY_5_3_OR_NEWER
        public Dictionary<string, ImportVarUnity> ImportVariablesUnity { get; set; }
#endif
        public Dictionary<string, Var> QueryVariables { get; set; }
    }
}
