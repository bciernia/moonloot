using UnityEngine;

public class WorldMapEvent : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string areaTransitionName;
    public AreaEntrance areaEntrance;

    private BoxCollider2D _boxCollider2D;

    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        areaEntrance.transitionName = areaTransitionName;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GoToEventScene();
        }
    }

    private void GoToEventScene()
    {
        LoadingSceneManager.Instance.LoadScene(sceneToLoad);
        FindAnyObjectByType<Player>().areaTransitionName = areaTransitionName;
    }
}
