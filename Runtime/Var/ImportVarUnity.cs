using System;
using System.Collections;
using System.Collections.Generic;
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
