using System;

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
            if (Value is T value)
            {
                return value;
            }
            throw new InvalidCastException($"Cannot cast {Value.GetType().Name} to {typeof(T).Name}");
        }

        public object GetValue(Type type)
        {
            if (type.IsInstanceOfType(Value))
            {
                return Value;
            }
            throw new InvalidCastException($"Cannot cast {Value.GetType().Name} to {type.Name}");
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