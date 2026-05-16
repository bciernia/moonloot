using UnityEngine;

public class MimicChest : MonoBehaviour
{
    [SerializeField] private ChestInteraction chestInteraction;

    [Header("Loot")]
    [SerializeField] private GameObject lootPrefab;

    private bool _isMimic;
    private bool _opened;

    public void Initialize(bool isMimic)
    {
        _isMimic = isMimic;
    }

    public void OpenChest()
    {
        if (_opened)
            return;

        _opened = true;

        if (_isMimic)
        {
            SpawnEnemies();

            HordeManager.Instance.AddObjectiveProgress(1);

            Debug.Log("Mimic found!");
        }
        else
        {
            SpawnLoot();

            Debug.Log("Loot chest!");
        }

        Destroy(gameObject);
    }

    private void SpawnLoot()
    {
        var lootAmount = RNGManager.Instance.GetRandomInt(2, 4);

        for (var i = 0; i < lootAmount; i++)
        {
            var offset = Random.insideUnitSphere * 2f;
            offset.y = 0f;

            Instantiate(
                lootPrefab,
                transform.position + offset,
                Quaternion.identity
            );
        }
    }

    private void SpawnEnemies()
    {
        var enemiesNumber = RNGManager.Instance.GetRandomInt(3, 6);
        
        for (var i = 0; i < enemiesNumber; i++)
        {
            var offset = Random.insideUnitSphere * 3f;
            offset.y = 0f;

            var spawnPos = transform.position + offset;

            HordeManager.Instance.SpawnMimicEnemy(
                spawnPos
            );
        }
    }
}