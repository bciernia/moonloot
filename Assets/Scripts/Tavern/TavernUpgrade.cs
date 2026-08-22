using System.Collections;
using UnityEngine;

public class TavernUpgrade : MonoBehaviour
{
    [SerializeField] private Transform _tavernOverviewPoint;
    [SerializeField] private GameObject _tavernOverviewPanel;
    
    public void OpenTavernUpgrade()
    {
        StartCoroutine(OpenTavernUpgradeCoroutine());
    }

    private IEnumerator OpenTavernUpgradeCoroutine()
    {
        CameraFocusManager.Instance.SetFocus(_tavernOverviewPoint, 32f);

        yield return new WaitUntil(() =>
            CameraFocusManager.Instance.IsAtTarget());
        
        _tavernOverviewPanel.SetActive(true);
    }

    public void CloseTavernUpgrade()
    {
        _tavernOverviewPanel.SetActive(false);

        CameraFocusManager.Instance.FocusPlayer();

        DialogueManager.Instance.ContinueDialogue();
    }
}