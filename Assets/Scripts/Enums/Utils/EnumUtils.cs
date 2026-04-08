using UnityEngine;

public static class EnumUtils
{
    public static T GetRandomEnum<T>()
    {
        var values = System.Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Range(0, values.Length));
    }
}