using System.Collections.Generic;
using UnityEngine;

public class StarterItemSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> _possibleItems;

    private readonly string _stateKey = "starter_item_spawned";
    
    private void Start()
    {
        if (WorldStateManager.Instance.HasState(_stateKey))
            return;

        SpawnRandomItem();

        WorldStateManager.Instance.SetState(_stateKey);
    }

    private void SpawnRandomItem()
    {
        if (_possibleItems.Count == 0)
            return;

        var randomItem = _possibleItems[Random.Range(0, _possibleItems.Count)];

        Instantiate(randomItem, gameObject.transform.position, Quaternion.identity);
        
    }
}