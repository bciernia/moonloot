using UnityEngine;

public class ActionPutTotem : FSMAction
{
    public GameObject healTotemPrefab;
    public GameObject iceTotemPrefab;
    public GameObject fireTotemPrefab;

    public LayerMask allyLayer;

    private EnemyBrain _enemyBrain;
    private EnemyStatistics _enemyStatistics;

    private readonly System.Random rng = new System.Random();

    private void Awake()
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override void Act()
    {
        TryPlaceTotem();
    }

    private void TryPlaceTotem()
    {
        if (!_enemyBrain.CanAttack())
            return;

        _enemyBrain.ResetAttackCooldown();

        var rolledType = (TotemType)rng.Next(0, 3);

        Transform target = rolledType == TotemType.Heal
            ? FindAllyNeedingHeal()
            : _enemyBrain.Player;

        if (target == null)
        {
            target = _enemyBrain.Player;
            rolledType = (TotemType)rng.Next(1, 3);
        }

        GameObject prefab = GetPrefab(rolledType);

        if (!prefab)
        {
            Debug.LogError("Totem prefab is missing!");
            return;
        }
        
        Instantiate(prefab, transform.position, Quaternion.identity);
    }

    private GameObject GetPrefab(TotemType type)
    {
        return type switch
        {
            TotemType.Heal => healTotemPrefab,
            TotemType.Ice  => iceTotemPrefab,
            TotemType.Fire => fireTotemPrefab,
            _ => null
        };
    }

    private Transform FindAllyNeedingHeal()
    {
        var range = _enemyStatistics.AttackRange * 3f;

        var hits = Physics2D.OverlapCircleAll(transform.position, range, allyLayer);

        Transform best = null;
        var bestDist = Mathf.Infinity;

        foreach (var h in hits)
        {
            var stats = h.GetComponent<EnemyStatistics>();
            if (!stats || stats.CurrentHP >= stats.MaxHP) continue;

            var dist = Vector2.Distance(transform.position, h.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = h.transform;
            }
        }

        return best;
    }
}
