using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace AscentLanguage.Var
{
    [Serializable]
    public class ImportVar
    {
        public string name = "";
        public string value = "";
        public ImportType type;

        public void Set(object settingValue)
        {
            value = settingValue?.ToString();
        }

        public object Get()
        {
            switch (type)
            {
                case ImportType.Float:
                    return float.Parse(value);
                case ImportType.String:
                    return value;
                case ImportType.Bool:
                    return bool.Parse(value);
#if UNITY_5_3_OR_NEWER
            case ImportType.Vector2:
                var trimmed2 = value.Substring(1, value.Length - 2);
                var split2 = trimmed2.Split(',');
                return new Vector2(float.Parse(split2[0]), float.Parse(split2[1]));
            case ImportType.Vector3:
                var trimmed3 = value.Substring(1, value.Length - 2);
                var split3 = trimmed3.Split(',');
                return new Vector3(float.Parse(split3[0]), float.Parse(split3[1]), float.Parse(split3[2]));
            case ImportType.Color:
                var trimmedCol = value.Substring(5, value.Length - 6);
                var splitCol = trimmedCol.Split(',');
                return new Color(float.Parse(splitCol[0]), float.Parse(splitCol[1]), float.Parse(splitCol[2]), float.Parse(splitCol[3]));
#endif
                default:
                    return null;
            }
        }

        public enum ImportType
        {
            Float,
            String,
            Bool,
#if UNITY_5_3_OR_NEWER
            Vector2,
            Vector3,
            Color,
#endif
        }
    }
}
