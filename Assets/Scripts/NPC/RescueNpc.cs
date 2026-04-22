using UnityEngine;

public class RescueNpc : MonoBehaviour
{
    public void SetSaveNpc()
    {
        HordeManager.Instance.SetRescuedNPC();
    }
}