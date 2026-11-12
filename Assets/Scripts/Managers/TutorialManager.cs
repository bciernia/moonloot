using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class TutorialManager : Singleton<TutorialManager>, ISaveable
{
    private const string SaveKey = "tutorials_seen";
    [Header("Tutorial asset")]
    [SerializeField] private TutorialDefinitionSO startMapTutorial;

    [Header("Manual tutorial canvas")]
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI actionHintText;
    [SerializeField] private TextMeshProUGUI pageCounterText;
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button acknowledgeButton;

    public TutorialDefinitionSO StartMapTutorial => startMapTutorial;

    private readonly Queue<TutorialDefinitionSO> _queue = new();
    private readonly HashSet<string> _queuedIds = new();
    private readonly HashSet<string> _seenIds = new();

    private GameObject _modalRoot;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _instructionsText;
    private TextMeshProUGUI _actionHintText;
    private TextMeshProUGUI _pageCounterText;
    private TextMeshProUGUI _previousButtonText;
    private TextMeshProUGUI _nextButtonText;
    private TextMeshProUGUI _acknowledgeButtonText;
    private Image _illustrationImage;
    private Button _previousButton;
    private Button _nextButton;
    private Button _acknowledgeButton;
    private Coroutine _animationRoutine;
    private TutorialDefinitionSO _currentTutorial;
    private int _currentPageIndex;
    private CursorUI _cursorAboveModal;
    private bool _pauseHeld;
    private bool _fallbackPauseHeld;
    private float _previousTimeScale = 1f;
    private bool _uiReady;
    private UIManager _uiManager;
    private bool _seenTutorialDataReady = true;

    public bool IsSeenTutorialDataReady => _seenTutorialDataReady;

    public bool HasSeenTutorial(string tutorialId)
    {
        return !string.IsNullOrWhiteSpace(tutorialId) && _seenIds.Contains(tutorialId);
    }

    public void BeginLoadingSeenTutorials()
    {
        _seenTutorialDataReady = false;
    }

    public void InitializeForNewSave()
    {
        _seenIds.Clear();
        _seenTutorialDataReady = true;
    }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        BuildModal();
        _uiManager = FindFirstObjectByType<UIManager>();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SetCursorAboveModal(false);
        ReleasePause();
    }

    public bool TryShow(TutorialDefinitionSO tutorial, Player player)
    {
        if (!_uiReady || !_seenTutorialDataReady || tutorial == null || player == null)
            return false;

        var id = tutorial.Id;
        if (tutorial.ShowOncePerSave && _seenIds.Contains(id))
            return true;

        if (_currentTutorial != null && _currentTutorial.Id == id || _queuedIds.Contains(id))
            return true;

        if (tutorial.Condition != null && !tutorial.Condition.IsMet(player))
            return false;

        _queue.Enqueue(tutorial);
        _queuedIds.Add(id);
        if (_currentTutorial == null)
            ShowNext();

        return true;
    }

    public void CloseCurrentTutorial()
    {
        if (_currentTutorial == null)
            return;

        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
            _animationRoutine = null;
        }

        if (_currentTutorial.ShowOncePerSave)
        {
            _seenIds.Add(_currentTutorial.Id);
            SaveSeenIds();
        }

        _currentTutorial = null;
        ShowNext();
    }

    private void ShowNext()
    {
        if (_queue.Count == 0)
        {
            if (_modalRoot != null)
                _modalRoot.SetActive(false);
            if (_uiManager != null)
                _uiManager.SetPlayerHudVisible(true);
            SetCursorAboveModal(false);
            ReleasePause();
            return;
        }

        _currentTutorial = _queue.Dequeue();
        _queuedIds.Remove(_currentTutorial.Id);
        if (_currentTutorial.ShowOncePerSave && _seenIds.Contains(_currentTutorial.Id))
        {
            _currentTutorial = null;
            ShowNext();
            return;
        }

        if (_currentTutorial.ShowOncePerSave)
        {
            _seenIds.Add(_currentTutorial.Id);
            SaveSeenIds();
        }

        RequestPause();
        SetCursorAboveModal(true);
        if (_uiManager != null)
            _uiManager.SetPlayerHudVisible(false);
        _modalRoot.SetActive(true);
        SetLocalizedByKey(_titleText, $"{_currentTutorial.Id}.title", _currentTutorial.Title);
        SetLocalized(_previousButtonText, "Wstecz");
        SetLocalized(_nextButtonText, "Następny");
        SetLocalized(_acknowledgeButtonText, "Zamknij");
        _currentPageIndex = 0;
        DisplayCurrentPage();
    }

    private void DisplayCurrentPage()
    {
        if (_currentTutorial == null)
            return;

        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
            _animationRoutine = null;
        }

        var pages = _currentTutorial.Pages;
        var hasPages = pages != null && pages.Count > 0;
        var pageCount = hasPages ? pages.Count : 1;
        _currentPageIndex = Mathf.Clamp(_currentPageIndex, 0, pageCount - 1);

        var page = hasPages ? pages[_currentPageIndex] : null;
        var description = page != null ? page.Description : _currentTutorial.Instructions;
        var instructionKey = page != null && !string.IsNullOrWhiteSpace(page.StepId)
            ? $"{_currentTutorial.Id}.{page.StepId}.instructions"
            : $"{_currentTutorial.Id}.instructions";
        if (_instructionsText != null) SetLocalizedByKey(_instructionsText, instructionKey, description);

        var frames = page != null ? page.AnimationFrames : _currentTutorial.AnimationFrames;
        var fps = page != null ? page.FramesPerSecond : _currentTutorial.FramesPerSecond;
        var loopAnimation = page != null ? page.LoopAnimation : _currentTutorial.LoopAnimation;
        var illustration = page != null ? page.Illustration : _currentTutorial.Illustration;
        var validFrames = frames != null ? frames.Where(frame => frame != null).ToArray() : System.Array.Empty<Sprite>();
        var hasAnimation = validFrames.Length > 0;
        var hasVisual = hasAnimation || illustration != null;

        if (_illustrationImage != null) _illustrationImage.gameObject.SetActive(hasVisual);
        if (_illustrationImage != null && hasAnimation)
        {
            _illustrationImage.sprite = validFrames[0];
            _animationRoutine = StartCoroutine(
                PlayAnimation(_currentTutorial, _currentPageIndex, validFrames, fps, loopAnimation));
        }
        else if (_illustrationImage != null)
        {
            _illustrationImage.sprite = illustration;
        }

        var hasMultiplePages = pageCount > 1;
        SetButtonVisible(_previousButton, hasMultiplePages && _currentPageIndex > 0);
        SetButtonVisible(_nextButton, hasMultiplePages && _currentPageIndex < pageCount - 1);
        SetButtonVisible(_acknowledgeButton, !hasMultiplePages || _currentPageIndex == pageCount - 1);
        if (_pageCounterText != null)
        {
            _pageCounterText.gameObject.SetActive(hasMultiplePages);
            _pageCounterText.text = hasMultiplePages ? $"{_currentPageIndex + 1} / {pageCount}" : string.Empty;
        }

        var hint = page != null ? page.Hint : _currentTutorial.ActionHint;
        var hintKey = page != null && !string.IsNullOrWhiteSpace(page.StepId)
            ? $"{_currentTutorial.Id}.{page.StepId}.hint"
            : $"{_currentTutorial.Id}.hint";
        var hasActionHint = !string.IsNullOrWhiteSpace(hint);
        if (_actionHintText != null)
            _actionHintText.gameObject.SetActive(hasActionHint);
        if (hasActionHint && _actionHintText != null)
            SetLocalizedByKey(_actionHintText, hintKey, hint);
    }

    private IEnumerator PlayAnimation(
        TutorialDefinitionSO tutorial,
        int pageIndex,
        Sprite[] frames,
        float framesPerSecond,
        bool loop)
    {
        var frameDelay = 1f / Mathf.Max(1f, framesPerSecond);
        var index = 0;
        while (_currentTutorial == tutorial && _currentPageIndex == pageIndex)
        {
            _illustrationImage.sprite = frames[index];
            yield return new WaitForSecondsRealtime(frameDelay);

            index++;
            if (index >= frames.Length)
            {
                if (!loop)
                {
                    index = frames.Length - 1;
                    _illustrationImage.sprite = frames[index];
                    _animationRoutine = null;
                    yield break;
                }

                index = 0;
            }
        }
    }

    private void ShowPreviousPage()
    {
        if (_currentTutorial == null || _currentPageIndex <= 0)
            return;

        _currentPageIndex--;
        DisplayCurrentPage();
    }

    private void ShowNextPage()
    {
        if (_currentTutorial == null)
            return;

        var pages = _currentTutorial.Pages;
        if (pages == null || _currentPageIndex >= pages.Count - 1)
            return;

        _currentPageIndex++;
        DisplayCurrentPage();
    }

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button == null)
            return;

        var canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void SetCursorAboveModal(bool enabled)
    {
        if (enabled)
        {
            if (_cursorAboveModal == null)
                _cursorAboveModal = FindFirstObjectByType<CursorUI>();
            _cursorAboveModal?.SetAboveModal(true);
            return;
        }

        if (_cursorAboveModal != null)
            _cursorAboveModal.SetAboveModal(false);
        _cursorAboveModal = null;
    }

    private void RequestPause()
    {
        if (_pauseHeld || _fallbackPauseHeld)
            return;

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.RequestPause();
            _pauseHeld = true;
            return;
        }

        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _fallbackPauseHeld = true;
    }

    private void ReleasePause()
    {
        if (_pauseHeld && PauseManager.Instance != null)
            PauseManager.Instance.ReleasePause();
        _pauseHeld = false;

        if (_fallbackPauseHeld)
            Time.timeScale = _previousTimeScale;
        _fallbackPauseHeld = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_currentTutorial == null && _queue.Count == 0)
            return;

        _queue.Clear();
        _queuedIds.Clear();
        _currentTutorial = null;
        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
            _animationRoutine = null;
        }

        if (_modalRoot != null)
            _modalRoot.SetActive(false);
        if (_uiManager != null)
            _uiManager.SetPlayerHudVisible(true);
        SetCursorAboveModal(false);
        ReleasePause();
    }

    private void BuildModal()
    {
        _modalRoot = tutorialCanvas != null ? tutorialCanvas.gameObject : null;
        _titleText = titleText;
        _instructionsText = instructionsText;
        _actionHintText = actionHintText;
        _pageCounterText = pageCounterText;
        _illustrationImage = illustrationImage;
        _previousButton = previousButton;
        _nextButton = nextButton;
        _acknowledgeButton = acknowledgeButton;
        _previousButtonText = previousButton != null ? previousButton.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        _nextButtonText = nextButton != null ? nextButton.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        _acknowledgeButtonText = acknowledgeButton != null ? acknowledgeButton.GetComponentInChildren<TextMeshProUGUI>(true) : null;

        _uiReady = tutorialCanvas != null && _modalRoot != null;
        if (!_uiReady)
        {
            Debug.LogError("TutorialManager: assign the manual Canvas in the Inspector.", this);
            return;
        }

        if (_previousButton != null) _previousButton.onClick.AddListener(ShowPreviousPage);
        if (_nextButton != null) _nextButton.onClick.AddListener(ShowNextPage);
        if (_acknowledgeButton != null) _acknowledgeButton.onClick.AddListener(CloseCurrentTutorial);
        _modalRoot.SetActive(false);
    }

    private static void SetLocalized(TMP_Text text, string source)
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLocalizedText(text, source ?? string.Empty);
        else if (text != null)
            text.text = source ?? string.Empty;
    }

    private static void SetLocalizedByKey(TMP_Text text, string key, string fallback)
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetTutorialLocalizedText(text, key, fallback);
        else if (text != null)
            text.text = fallback ?? string.Empty;
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
            return;

        var settings = SaveLoadManager.Instance != null ? SaveLoadManager.Instance.GetSettings() : null;
        if (settings == null)
            return;

        ES3.Save(SaveKey, _seenIds.ToList(), settings);
    }

    public void Load()
    {
        _seenTutorialDataReady = false;
        _seenIds.Clear();
        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
        {
            _seenTutorialDataReady = true;
            return;
        }

        var settings = SaveLoadManager.Instance != null ? SaveLoadManager.Instance.GetSettings() : null;
        if (settings == null || !ES3.KeyExists(SaveKey, settings))
        {
            _seenTutorialDataReady = true;
            return;
        }

        var loadedIds = ES3.Load<List<string>>(SaveKey, settings);
        if (loadedIds == null)
        {
            _seenTutorialDataReady = true;
            return;
        }

        foreach (var id in loadedIds)
        {
            if (!string.IsNullOrWhiteSpace(id))
                _seenIds.Add(id);
        }

        _seenTutorialDataReady = true;
    }

    private void SaveSeenIds()
    {
        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
            return;

        var settings = SaveLoadManager.Instance != null ? SaveLoadManager.Instance.GetSettings() : null;
        if (settings != null)
            ES3.Save(SaveKey, _seenIds.ToList(), settings);
    }
}
