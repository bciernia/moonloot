using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;

    private void Start()
    {
        if (FindAnyObjectByType<Player>() == null)
        {
            Instantiate(player);
        }
    }
}
