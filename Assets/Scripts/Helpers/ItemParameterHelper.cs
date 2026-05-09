using System.Collections.Generic;

public static class ItemParameterHelper
{
    public static float GetParameterValue(
        List<ItemParameter> parameters,
        ItemParameterSO parameter)
    {
        if (parameters == null)
            return 0f;

        foreach (var p in parameters)
        {
            if (p.itemParameter == parameter)
            {
                return p.value;
            }
        }

        return 0f;
    }
}