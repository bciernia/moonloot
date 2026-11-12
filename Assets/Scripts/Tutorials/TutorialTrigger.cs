using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public sealed class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialDefinitionSO tutorial;
    [SerializeField, Min(0.05f)] private float conditionRetryInterval = 0.25f;
    [SerializeField] private bool disableAfterAccepted = true;

    private Player _playerInside;
    private float _nextConditionCheck;
    private bool _accepted;
    private Coroutine _savedStateCheckRoutine;

    private void Awake()
    {
        var triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        CheckSavedTutorialState();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        _playerInside = player;
        CheckSavedTutorialState();
        TryShowTutorial();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_accepted || Time.unscaledTime < _nextConditionCheck)
            return;

        var player = other.GetComponentInParent<Player>();
        if (player == null || (_playerInside != null && _playerInside != player))
            return;

        _playerInside = player;
        TryShowTutorial();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponentInParent<Player>();
        if (player == null || player != _playerInside)
            return;

        _playerInside = null;
        _nextConditionCheck = 0f;
        if (!disableAfterAccepted && tutorial != null && !tutorial.ShowOncePerSave)
            _accepted = false;
    }

    private void TryShowTutorial()
    {
        _nextConditionCheck = Time.unscaledTime + Mathf.Max(0.05f, conditionRetryInterval);

        if (_accepted || tutorial == null || _playerInside == null || TutorialManager.Instance == null)
            return;

        var tutorialManager = TutorialManager.Instance;
        if (!tutorialManager.IsSeenTutorialDataReady)
        {
            CheckSavedTutorialState();
            return;
        }

        if (tutorial.ShowOncePerSave && tutorialManager.HasSeenTutorial(tutorial.Id))
        {
            _accepted = true;
            Destroy(gameObject);
            return;
        }

        if (!tutorialManager.TryShow(tutorial, _playerInside))
            return;

        _accepted = true;
        if (disableAfterAccepted)
            gameObject.SetActive(false);
    }

    private void CheckSavedTutorialState()
    {
        var tutorialManager = TutorialManager.Instance;
        if (tutorial == null || tutorialManager == null)
            return;

        if (!tutorialManager.IsSeenTutorialDataReady)
        {
            if (_savedStateCheckRoutine == null)
                _savedStateCheckRoutine = StartCoroutine(WaitForSavedTutorialState());
            return;
        }

        if (tutorial.ShowOncePerSave && tutorialManager.HasSeenTutorial(tutorial.Id))
        {
            _accepted = true;
            Destroy(gameObject);
            return;
        }

        if (_playerInside != null)
            TryShowTutorial();
    }

    private IEnumerator WaitForSavedTutorialState()
    {
        while (TutorialManager.Instance == null || !TutorialManager.Instance.IsSeenTutorialDataReady)
            yield return null;

        _savedStateCheckRoutine = null;
        CheckSavedTutorialState();
    }
}
