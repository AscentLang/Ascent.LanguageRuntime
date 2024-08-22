#if UNITY_5_3_OR_NEWER
using System;

namespace AscentLanguage.Var
{
    [Serializable]
    public class ImportVarUnity
    {
        public string name = "";
        public UnityEngine.Object value;

        public void Set(UnityEngine.Object settingValue)
        {
            value = settingValue;
        }
    }
}
#endif