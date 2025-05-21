using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaExit : MonoBehaviour
{
    public string sceneToLoad;
    public string areaTransitionName;
    public AreaEntrance areaEntrance;

    private void Start()
    {
        areaEntrance.transitionName = areaTransitionName;
    }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         SceneManager.LoadScene(sceneToLoad);
    //         Player.instance.areaTransitionName = areaTransitionName;
    //     }
    // }

    public void ExitArea()
    {
        SceneManager.LoadScene(sceneToLoad);
        Player.instance.areaTransitionName = areaTransitionName;
    }
}
