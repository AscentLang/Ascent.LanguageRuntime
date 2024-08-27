using System;
using UnityEngine;

namespace AscentLanguage.Var
{
    [Serializable]
    public class Variable
    {
        public Variable() { }

        public Variable(VarType varType, object value)
        {
            Type = varType;
            Value = value;
        }

        public VarType Type { get; private set; }

        public object Value { get; private set; }

        public T GetValue<T>()
        {
            object result = typeof(T) switch
            {
                Type t when t == typeof(float) => ToFloat(),
                Type t when t == typeof(string) => ToStringValue(),
                Type t when t == typeof(bool) => ToBool(),
                Type t when t.IsInstanceOfType(Value) => Value,
                _ => throw new InvalidCastException($"Cannot cast {Type} to {typeof(T).Name}")
            };

            return (T)result;
        }

        public object GetValue(Type type)
        {
            if (type == typeof(float)) return ToFloat();
            if (type == typeof(string)) return ToStringValue();
            if (type == typeof(bool)) return ToBool();
            if (type.IsInstanceOfType(Value)) return Value;
            
            throw new InvalidCastException($"Cannot cast {Type} to {type.Name}");
        }

        public void SetValue<T>(T value)
        {
            Value = value;
            Type = DetermineVarType(typeof(T));
        }

        public void SetValue(object value, VarType type)
        {
            Value = value;
            Type = type;
        }

        public static VarType DetermineVarType(Type type)
        {
            if (type == typeof(float) || type == typeof(double)) return VarType.Float;
            if (type == typeof(string)) return VarType.String;
            if (type == typeof(bool)) return VarType.Bool;
#if UNITY_5_3_OR_NEWER
            if (type == typeof(UnityEngine.Vector2)) return VarType.Vector2;
            if (type == typeof(UnityEngine.Vector3)) return VarType.Vector3;
            if (type == typeof(UnityEngine.Color)) return VarType.Color;
#endif
            return VarType.Object;
        }
        
        // Explicit conversion methods
        public float ToFloat()
        {
            return Type switch
            {
                VarType.Float => Convert.ToSingle(Value),
                VarType.Bool => (bool)Value ? 1f : 0f,
                VarType.String => float.TryParse((string)Value, out float result) ? result : throw new InvalidCastException($"Cannot convert {Value} to float"),
                _ => throw new InvalidCastException($"Cannot convert {Type} to float")
            };
        }

        public string ToStringValue()
        {
            return Value?.ToString() ?? "null";
        }

        public bool ToBool()
        {
            return Type switch
            {
                VarType.Bool => (bool)Value,
                VarType.Float => Convert.ToSingle(Value) != 0f,
                VarType.String => bool.TryParse((string)Value, out bool result) ? result : throw new InvalidCastException($"Cannot convert {Value} to bool"),
                _ => throw new InvalidCastException($"Cannot convert {Type} to bool")
            };
        }

        // Implicit conversion operators
        public static implicit operator Variable(float value) => new (VarType.Float, value);
        public static implicit operator Variable(string value) => new (VarType.String, value);
        public static implicit operator Variable(bool value) => new (VarType.Bool, value);

        public override string ToString() => Value?.ToString() ?? "null";
    }

    public enum VarType
    {
        Float,
        String,
        Bool,
#if UNITY_5_3_OR_NEWER
        Vector2,
        Vector3,
        Color,
#endif
        Object
    }
}