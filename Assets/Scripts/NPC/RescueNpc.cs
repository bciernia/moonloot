using System.Collections;
using UnityEngine;

public class RescueNpc : MonoBehaviour
{
    private bool _alreadySaved = false;
    [SerializeField] private GameObject RescuePanel;

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
        
        RescuePanel.SetActive(true);
        
        HordeManager.Instance.SetRescuedNPC(_runtime);
        StartCoroutine(HideNpc());
    }
    
    private IEnumerator HideNpc()
    {
        yield return new WaitForSeconds(3f);
        RescuePanel.SetActive(false);
        gameObject.SetActive(false);
    }
}