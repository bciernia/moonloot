using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyUI : MonoBehaviour, IShowEnemyInfo
{
    [SerializeField] private TextMeshProUGUI text;
    private EnemyBrain _enemyBrain;
    private bool ShouldShowAttackCooldown { get; set; }
    private float _lastDisplayedValue = -1f;

    public bool _isShownEnemyCooldown;
    private Coroutine _showCooldownRoutine;
    
    private void Awake()
    {
        _enemyBrain = GetComponentInParent<EnemyBrain>();
    }

    private void Update()
    {
        if (_enemyBrain == null)
            return;

        if (!ShouldShowAttackCooldown)
        {
            text.gameObject.SetActive(false);
            return;
        }

        var cooldown = _enemyBrain.AttackCooldown;

        if (cooldown > 0f)
        {
            var rounded = Mathf.Ceil(cooldown * 10f) / 10f;

            if (Mathf.Abs(rounded - _lastDisplayedValue) > 0.01f)
            {
                text.text = rounded.ToString("F1");
                _lastDisplayedValue = rounded;
            }

            text.gameObject.SetActive(true);
        }
        else
        {
            text.gameObject.SetActive(false);
        }
    }

    public void ShowEnemyCooldown(float duration)
    {
        if (_isShownEnemyCooldown)
        {
            if (_showCooldownRoutine != null)
                StopCoroutine(_showCooldownRoutine);
        }

        _showCooldownRoutine = StartCoroutine(ShowCooldownRoutine(duration));
    }

    private IEnumerator ShowCooldownRoutine(float duration)
    {
        _isShownEnemyCooldown = true;
        ShouldShowAttackCooldown = true;

        yield return new WaitForSeconds(duration);

        _isShownEnemyCooldown = false;
        ShouldShowAttackCooldown = false;
    }
}