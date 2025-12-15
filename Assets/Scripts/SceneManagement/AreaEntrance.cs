using UnityEngine;

public class AreaEntrance : MonoBehaviour
{
    public string transitionName;

    void Start()
    {
        if (transitionName == FindAnyObjectByType<Player>().areaTransitionName)
        {
            FindAnyObjectByType<Player>().transform.position = transform.position;
        }
    }
}
