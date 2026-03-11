using UnityEngine;

public class WorldStateObject : MonoBehaviour
{
    public string requiredState;
    public bool showIfStateIs = true;

    void Start()
    {
        gameObject.SetActive(WorldStateManager.Instance.HasState(requiredState) == showIfStateIs);
    }
}