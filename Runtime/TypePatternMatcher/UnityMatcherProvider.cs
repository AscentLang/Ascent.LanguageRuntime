using System;
using System.Reflection;

public class UnityMatcherProvider : IMatcherProvider
{
	public string[] GetWhiteListedAssemblies()
	{
		return new string[] { "Assembly-CSharp", "UnityEngine", "UnityEngine.CoreModule", "System", "mscorlib", "System.Xml", "Unity.InputSystem" };
	}

	public bool DefaultBlacklistAllMethods(Type type)
	{
		return false;
	}

	public bool DefaultBlacklistAllTypes(Assembly assembly)
	{
		return true;
	}

	public string[] GetBlacklistedMethods(Type type)
	{
		return new string[] { };
	}

	public string[] GetBlacklistedTypes(Assembly assembly)
	{
		return new string[] { };
	}

	public string[] GetWhitelistedMethods(Type type)
	{
		return new string[] { "ToString", "Equals", "GetHashCode", "GetType" };
	}

	public string[] GetWhitelistedTypes(Assembly assembly)
	{
		Console.WriteLine(assembly.GetName().Name);
		switch (assembly.GetName().Name)
		{
			case "UnityEngine.CoreModule":
				return new string[] { "Camera", "Vector2", "Vector3", "Vector4", "Color", "Mathf", "GameObject", "Transform", "Component", "Object" };
				break;
			default:
				break;
		}
		return new string[] { };
	}
}
