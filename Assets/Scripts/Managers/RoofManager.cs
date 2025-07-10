using UnityEngine;

public class RoofManager : MonoBehaviour
{
    void Start()
    {
        foreach (Transform roof in transform)
        {
            roof.gameObject.SetActive(true);
        } 
    }
}
