using UnityEngine;

public class GameRoot : MonoBehaviour
{
#pragma warning disable UDR0001
    private static GameRoot instance;
#pragma warning restore UDR0001

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}