using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
public static class MethodRetrievalService
{

    /// <summary>
    /// Function that grabs all the assemblies and finds all the functions with given method of type TAttribute.
    /// </summary>
    /// <typeparam name="TAttribute"> The type of the attribute that you want. </typeparam>
    /// <returns> IEnumerable of Method Infos that have the attribute. </returns>
    public static IEnumerable<MethodInfo> GetStaticMethodsWithAttribute<TAttribute>() where TAttribute : Attribute
    {
        List<MethodInfo> methodsWithAttribute = new List<MethodInfo>();

        // Get all assemblies that are loaded in the current AppDomain
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            // Get all types in the assembly
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // Get all static methods of the type
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var method in methods)
                {
                    // Check if the method has the specified attribute
                    if (method.GetCustomAttributes(typeof(TAttribute), false).Any())
                    {
                        methodsWithAttribute.Add(method);
                    }
                }
            }
        }

        return methodsWithAttribute;
    }
}
