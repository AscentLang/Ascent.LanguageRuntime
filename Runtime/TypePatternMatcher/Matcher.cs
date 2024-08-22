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
            if (_matchedQualifiedTypes != null) return _matchedQualifiedTypes.AsReadOnly();
            _matchedQualifiedTypes = new List<string>();
            ResetAndMatch();
            return _matchedQualifiedTypes.AsReadOnly(); 
        }
    }

    private static List<string> _matchedQualifiedTypes = null;

    private static void ResetAndMatch()
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
        var types = new List<string>();

        foreach (var typeName in ReadOnlyMatchedQualifiedTypes)
        {
            var type = Type.GetType(typeName);
            if (type?.Namespace == null) continue;
            if (type.Namespace.StartsWith(ns))
            {
                types.Add(type.AssemblyQualifiedName);
            }
        }

        return types.ToArray();
    }

    private static bool GetType(string[] qualifiedTypes, string type, out Type sysType)
    {
        foreach (var qualifiedType in qualifiedTypes)
        {
            sysType = Type.GetType(qualifiedType);
            if (sysType?.FullName == type || sysType?.Name == type)
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
                string ns = predicate[..(predicate.IndexOf("*", StringComparison.Ordinal) - 1)];

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
