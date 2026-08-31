using System.Collections;
using UnityEngine;

public class MimicChest : MonoBehaviour
{
    [SerializeField] private ChestInteraction chestInteraction;

    [Header("Loot")]
    [SerializeField] private GameObject lootPrefab;

    [Header("Mimic Visual")]
    [SerializeField] private Color _mimicColor = Color.red;
    [SerializeField] private float _mimicScale = 1.25f;
    [SerializeField] private float _revealDuration = 2f;

    private bool _isMimic;
    private bool _opened;

    private SpriteRenderer _spriteRenderer;
    private Vector3 _originalScale;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }

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
            StartCoroutine(MimicRevealCoroutine());
        }
        else
        {
            SpawnLoot();

            Debug.Log("Loot chest!");

            Destroy(gameObject);
        }
    }

    private IEnumerator MimicRevealCoroutine()
    {
        var elapsed = 0f;

        var startScale = _originalScale;
        var targetScale = _originalScale * _mimicScale;

        while (elapsed < _revealDuration)
        {
            elapsed += Time.deltaTime;

            var t = Mathf.Clamp01(
                elapsed / _revealDuration);

            // Łagodne zwiększanie skali
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t);

            // Stopniowe przechodzenie w czerwień
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color =
                    Color.Lerp(
                        Color.white,
                        _mimicColor,
                        t);
            }

            yield return null;
        }

        transform.localScale = targetScale;

        if (_spriteRenderer != null)
            _spriteRenderer.color = _mimicColor;

        SpawnEnemies();

        HordeManager.Instance.AddObjectiveProgress(1);

        Debug.Log("Mimic revealed!");

        Destroy(gameObject);
    }

    private void SpawnLoot()
    {
        var lootAmount =
            RNGManager.Instance.GetRandomInt(2, 4);

        for (var i = 0; i < lootAmount; i++)
        {
            var offset =
                Random.insideUnitSphere * 2f;

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
        var enemiesNumber =
            RNGManager.Instance.GetRandomInt(3, 6);

        for (var i = 0; i < enemiesNumber; i++)
        {
            var offset =
                Random.insideUnitSphere * 3f;

            offset.y = 0f;

            var spawnPos =
                transform.position + offset;

            HordeManager.Instance.SpawnMimicEnemy(
                spawnPos
            );
        }
    }
}