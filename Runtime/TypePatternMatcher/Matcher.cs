using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

public static class Matcher
{
    public static ReadOnlyCollection<string> ReadOnlyMatchedQualifiedTypes
    {
        get 
        {
            if (_matchedQualifiedTypes == null)
            {
                _matchedQualifiedTypes = new List<string>();
				ResetAndMatch();
			}
			return _matchedQualifiedTypes.AsReadOnly(); 
        }
    }

    private static List<string> _matchedQualifiedTypes = null;
    
    public static void ResetAndMatch()
    {
        var matcherProvider = AscentDomain.matcherProvider;

        _matchedQualifiedTypes.Clear();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
        {
            return matcherProvider.GetWhiteListedAssemblies().Contains(assembly.GetName().Name);
        }).ToArray();

        foreach (Assembly assembly in assemblies)
        {
            HashSet<Type> matchTypes = new HashSet<Type>();

            if (!matcherProvider.DefaultBlacklistAllTypes(assembly))
            {
				assembly.GetTypes().All(type => matchTypes.Add(type));
            }

            string[] whiteListedTypes = matcherProvider.GetWhitelistedTypes(assembly);
            string[] blackListedTypes = matcherProvider.GetBlacklistedTypes(assembly);

            foreach (Type type in assembly.GetTypes())
            {
                if (whiteListedTypes.Contains(type.Name) || whiteListedTypes.Contains(type.FullName))
                {
                    if (!matchTypes.Contains(type))
                    {
                        matchTypes.Add(type);
                    }
                }
                if (blackListedTypes.Contains(type.Name) || blackListedTypes.Contains(type.FullName))
                {
                    if (matchTypes.Contains(type))
                    {
                        matchTypes.Remove(type);
                    }
                }
            }

            foreach (Type type in matchTypes)
            {
                if (!_matchedQualifiedTypes.Contains(type.FullName))
                {
                    _matchedQualifiedTypes.Add(type.AssemblyQualifiedName);
                }
            }
        }
    }

    private static string[] GetTypesFromNamespace(string ns)
    {
        List<string> types = new List<string>();

        for (int i = 0; i < _matchedQualifiedTypes.Count; i++)
        {
            var type = Type.GetType(_matchedQualifiedTypes[i]);
            if (type.Namespace.StartsWith(ns))
            {
                types.Add(type.AssemblyQualifiedName);
            }
        }

        return types.ToArray();
    }

    private static bool GetType(string[] qualifiedTypes, string type, out Type sysType)
    {
        for (int i = 0; i < qualifiedTypes.Length; i++)
        {
            sysType = Type.GetType(qualifiedTypes[i]);
            if (sysType.FullName == type || sysType.Name == type)
            {
                return true;
            }
        }
        sysType = null;
        return false;
    }

    // This method is used to get all types from the predicates
    // Ex. [ "UnityEngine.*", "System.Time", "System.IO.*" ] will give all types from the UnityEngine namespace, the Time class from System, all types under the System IO namespace and subnamespaces.
    public static string[] GetTypesFromPredicates(string[] predicates)
    {
        List<string> types = new List<string>();

        foreach (string predicate in predicates)
        {
            if (predicate.Contains("*"))
            {
                string ns = predicate.Substring(0, predicate.IndexOf("*") - 1);

                string[] nsTypes = GetTypesFromNamespace(ns);

                foreach (string type in nsTypes)
                {
                    types.Add(type);
                }
            }
            else
            {
                //Check if each matched type to see if the predicate has been matched as a type.
                if (GetType(_matchedQualifiedTypes.ToArray(), predicate, out Type sysType))
                {
                    types.Add(sysType.AssemblyQualifiedName);
                }
            }
        }

        return types.ToArray();
    }

    public static Type GetType(string typeName, string[] usingPredicates)
    {
        //Check for primitive types
        switch (typeName)
        {
            case "float":
                return typeof(float);
            case "bool":
                return typeof(bool);
            case "string":
                return typeof(string);
        }
        if (typeName == null || typeName == "")
        {
            return null;
        }

        string[] types = GetTypesFromPredicates(usingPredicates); // Includes namespace while typeName does not

        if (GetType(types, typeName ?? "", out Type type))
        {
            return type;
        }
        return null;
    }
}
