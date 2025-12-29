using UnityEngine;

public class AreaEntrance : MonoBehaviour
{
    public string transitionName;

    void Start()
    {
        if (transitionName == Player.Instance.areaTransitionName)
        {
            Player.Instance.transform.position = transform.position;
        }
    }
}
