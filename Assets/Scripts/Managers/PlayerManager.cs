using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;

    private void Start()
    {
        if (Player.instance == null)
        {
            Instantiate(player);
        }
    }
}
