using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


/*
 - Interface for providing the type pattern matcher with the necessary information to match types.
 - The type pattern matcher uses this information to determine which types are allowed and which are not.
Order:
 - GetWhiteListedAssemblies, all assemblies that can be used
  - DefaultBlacklistAllTypes, if true, all types are blacklisted by default
   - GetWhitelistedTypes, removes types from the blacklist
   - GetBlacklistedTypes, adds types to the blacklist
  - DefaultBlacklistAllMethods, if true, all methods are blacklisted by default
   - GetWhitelistedMethods, removes methods from the blacklist
   - GetBlacklistedMethods, adds methods from the blacklist
*/
public interface IMatcherProvider
{
    public string[] GetWhiteListedAssemblies();
    public bool DefaultBlacklistAllTypes(Assembly assembly);
    public string[] GetWhitelistedTypes(Assembly assembly);
    public string[] GetBlacklistedTypes(Assembly assembly);
    public bool DefaultBlacklistAllMethods(Type type);
    public string[] GetWhitelistedMethods(Type type);
    public string[] GetBlacklistedMethods(Type type);
}
