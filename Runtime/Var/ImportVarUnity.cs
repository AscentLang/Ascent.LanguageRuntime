#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;

[Serializable]
public class ImportVarUnity
{
    public string name = "";
    public UnityEngine.Object value;

    public void Set(UnityEngine.Object value)
    {
        this.value = value;
    }
}
#endif