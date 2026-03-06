using System.Reflection;

namespace NetworkMapper.Application.Shared.Helpers;

public class ReflectionHelper
{
    public static string[] GetProperties<T>()
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(prop => prop.Name)
            .ToArray();
    }
}