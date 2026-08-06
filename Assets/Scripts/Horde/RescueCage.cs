using UnityEngine;

public class RescueCage : MonoBehaviour
{
    [SerializeField]
    private RescueNpc rescueNpc;
    private RescueNpc _rescueNpc;

    public void Initialize(RescueNpc rescueNpc)
    {
        _rescueNpc = rescueNpc;

        GetComponent<ItemStatistics>().OnDestroyed += OnDestroyed;
    }

    private void OnDestroyed()
    {
        _rescueNpc?.Rescue();
    }

    private void OnDisable()
    {
        var itemStatistics = GetComponent<ItemStatistics>();

        if (itemStatistics != null)
            itemStatistics.OnDestroyed -= OnDestroyed;
    }
}