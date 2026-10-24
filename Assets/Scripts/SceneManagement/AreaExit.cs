using UnityEngine;

public class AreaExit : MonoBehaviour
{
    public string sceneToLoad;
    public string areaTransitionName;
    public AreaEntrance areaEntrance;

    private void Start()
    {
        areaEntrance.transitionName = areaTransitionName;
    }

    public void ExitArea()
    {
        QuestObjectiveEvents.RaiseCurrentMapCompleted();
        LoadingSceneManager.Instance.LoadScene(sceneToLoad);
        FindAnyObjectByType<Player>().areaTransitionName = areaTransitionName;
    }
}
