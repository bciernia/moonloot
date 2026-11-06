using UnityEngine;

public abstract class TutorialConditionSO : ScriptableObject
{
    public abstract bool IsMet(Player player);
}
