using System;

namespace AscentLanguage
{
    [Serializable]
    public class Var
    {
        public Var() { }

        public Var(VarType varType, object value)
        {
            _serializedType = varType;
            _value = value;
        }

        public VarType Type => _serializedType;
        public object Value => _value;

        private object _value;
        private VarType _serializedType;

        public T GetValue<T>()
        {
            if (_value is T value)
            {
                return value;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast {_value.GetType().Name} to {typeof(T).Name}");
            }
        }

        public object GetValue(Type type)
        {
            if (type.IsInstanceOfType(_value))
            {
                return _value;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast {_value.GetType().Name} to {type.Name}");
            }
        }

        public void SetValue<T>(T value)
        {
            _value = value;
            _serializedType = DetermineVarType(typeof(T));
        }

        public void SetValue(object value, VarType type)
        {
            _value = value;
            _serializedType = type;
        }

        public static VarType DetermineVarType(Type type)
        {
            if (type == typeof(float) || type == typeof(double)) return VarType.Float;
            if (type == typeof(string)) return VarType.String;
            if (type == typeof(bool)) return VarType.Bool;
#if UNITY
            if (type == typeof(Vector2)) return VarType.Vector2;
            if (type == typeof(Vector3)) return VarType.Vector3;
            if (type == typeof(Color)) return VarType.Color;
#endif
            return VarType.Object;
        }

        // Implicit conversion operators
        public static implicit operator Var(float value) => new Var(VarType.Float, value);
        public static implicit operator Var(string value) => new Var(VarType.String, value);
        public static implicit operator Var(bool value) => new Var(VarType.Bool, value);

        public override string ToString() => _value?.ToString() ?? "null";
    }

    public enum VarType
    {
        Float,
        String,
        Bool,
        Vector2,
        Vector3,
        Color,
        Object
    }
}