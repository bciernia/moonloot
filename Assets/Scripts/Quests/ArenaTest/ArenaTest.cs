using UnityEngine;

public class ArenaTest : MonoBehaviour
{
    [SerializeField] private GameObject ArenaStarter;
    [SerializeField] private Transform ArenaStarterPosition;
    
    public void CreateEnemiesSpawner(int chosenDifficultyLevel)
    {
        var arenaStarter = Instantiate(ArenaStarter, ArenaStarterPosition.position, Quaternion.identity);
        arenaStarter.GetComponent<ArenaSpawner>().DifficultyLevel = (DifficultyLevel)chosenDifficultyLevel;
    }

    public bool IsArenaStarted()
    {
        return (bool)GameObject.FindGameObjectWithTag("ArenaStartButton");
    }
}
