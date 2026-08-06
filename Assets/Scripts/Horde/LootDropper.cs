using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [SerializeField]
    private ItemPoolSO itemPool;

    [SerializeField]
    private int itemCount = 3;

    [SerializeField]
    private float dropRadius = 1f;

    [SerializeField]
    private float minDistanceBetweenDrops = 0.3f;

    [SerializeField]
    private int maxAttempts = 10;

    [SerializeField]
    private LayerMask dropItemMask;

    public void DropItems()
    {
        if (itemPool == null)
            return;

        for (var i = 0; i < itemCount; i++)
        {
            var prefab =
                itemPool.GetRandomItem(HordeManager.Instance.GetCurrentHordeNumber());

            if (prefab == null)
                continue;

            SpawnItem(prefab);
        }
    }
    
    private void SpawnItem(GameObject prefab)
    {
        var dropPosition =
            FindFreeDropPosition();

        var drop = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity);

        var mover =
            drop.GetComponent<LootDropMover>();

        if (mover != null)
        {
            mover.MoveToPosition(
                dropPosition,
                Random.Range(0.25f, 0.5f));
        }
        else
        {
            drop.transform.position =
                dropPosition;
        }
    }

    private Vector3 FindFreeDropPosition()
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomOffset =
                Random.insideUnitCircle * dropRadius;

            var testPos =
                transform.position +
                new Vector3(
                    randomOffset.x,
                    randomOffset.y,
                    0f);

            var hit =
                Physics2D.OverlapCircle(
                    testPos,
                    minDistanceBetweenDrops,
                    dropItemMask);

            if (hit == null)
                return testPos;
        }

        return transform.position;
    }
}