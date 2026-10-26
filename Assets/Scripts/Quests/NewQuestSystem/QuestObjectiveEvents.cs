using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime events used by the data-driven quest journal.
/// </summary>
public static class QuestObjectiveEvents
{
    public static event Action<ItemSO, int> ItemCollected;
    public static event Action<string> MapCompleted;

    public static void RaiseItemCollected(ItemSO item, int quantity)
    {
        if (item != null && quantity > 0)
            ItemCollected?.Invoke(item, quantity);
    }

    public static void RaiseCurrentMapCompleted()
    {
        MapCompleted?.Invoke(SceneManager.GetActiveScene().name);
    }
}
