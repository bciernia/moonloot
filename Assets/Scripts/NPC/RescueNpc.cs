using System;
using UnityEngine;

public class RescueNpc : MonoBehaviour
{
    public bool IsInTown;

    public bool IsSaved() => LoadingSceneManager.Instance.IsSceneTown(); 
    
    public void SetSaveNpc()
    {
        HordeManager.Instance.SetRescuedNPC();
    }

    public void HideNpc()
    {
        gameObject.SetActive(false);
    }
}