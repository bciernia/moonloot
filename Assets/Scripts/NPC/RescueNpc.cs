using UnityEngine;

public class RescueNpc : MonoBehaviour
{
    public bool IsInTown;
    private bool _alreadySaved = false;

    private VillageNpcRuntime _runtime;

    public bool IsSaved() => LoadingSceneManager.Instance.IsSceneTown(); 
    public VillageNpcRuntime GetRuntime() => _runtime;
    
    public void SetRuntime(VillageNpcRuntime runtime)
    {
        _runtime = runtime;
    }
    
    public void SetSaveNpc()
    {
        if (_runtime == null || _alreadySaved)
            return;

        _alreadySaved = true;

        HordeManager.Instance.SetRescuedNPC(_runtime);
    }

    public void HideNpc()
    {
        gameObject.SetActive(false);
    }
}