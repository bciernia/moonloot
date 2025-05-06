using UnityEngine;

public class PlayerUILoader : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
