using UnityEngine;

public class AreaEntrance : MonoBehaviour
{
    public string transitionName;

    void Start()
    {
        if (transitionName == Player.instance.areaTransitionName)
        {
            Player.instance.transform.position = transform.position;
        }
    }
}
