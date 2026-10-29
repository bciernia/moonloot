using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class TutorialManager : MonoBehaviour, ISaveable
{
    private const string SaveKey = "tutorials_seen";
    private static TutorialManager _instance;

    public static TutorialManager Instance => _instance;

    private readonly Queue<TutorialDefinitionSO> _queue = new();
    private readonly HashSet<string> _queuedIds = new();
    private readonly HashSet<string> _seenIds = new();

    private GameObject _canvasObject;
    private GameObject _modalRoot;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _instructionsText;
    private TextMeshProUGUI _actionHintText;
    private TextMeshProUGUI _closeButtonLabel;
    private Image _illustrationImage;
    private Coroutine _animationRoutine;
    private TutorialDefinitionSO _currentTutorial;
    private bool _pauseHeld;
    private bool _fallbackPauseHeld;
    private float _previousTimeScale = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        var managerObject = new GameObject(nameof(TutorialManager));
        managerObject.AddComponent<TutorialManager>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        BuildModal();
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        ReleasePause();
        _instance = null;
    }

    public bool TryShow(TutorialDefinitionSO tutorial, Player player)
    {
        if (tutorial == null || player == null)
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

        RequestPause();
        _modalRoot.SetActive(true);
        var font = FindTutorialFont();
        if (font == null)
            font = TMP_Settings.defaultFontAsset;
        if (font != null)
        {
            _titleText.font = font;
            _instructionsText.font = font;
            _actionHintText.font = font;
        }
        SetLocalized(_titleText, _currentTutorial.Title);
        SetLocalized(_instructionsText, _currentTutorial.Instructions);
        SetLocalized(_closeButtonLabel, "Rozumiem");

        var hasActionHint = !string.IsNullOrWhiteSpace(_currentTutorial.ActionHint);
        _actionHintText.gameObject.SetActive(hasActionHint);
        if (hasActionHint)
            SetLocalized(_actionHintText, _currentTutorial.ActionHint);

        var frames = _currentTutorial.AnimationFrames;
        if (frames != null && frames.Length > 0)
        {
            _illustrationImage.gameObject.SetActive(true);
            _illustrationImage.sprite = frames[0];
            _animationRoutine = StartCoroutine(PlayAnimation(_currentTutorial));
        }
        else
        {
            _illustrationImage.sprite = _currentTutorial.Illustration;
            _illustrationImage.gameObject.SetActive(_currentTutorial.Illustration != null);
        }
    }

    private IEnumerator PlayAnimation(TutorialDefinitionSO tutorial)
    {
        var frames = tutorial.AnimationFrames;
        if (frames == null || frames.Length == 0)
            yield break;

        var frameDelay = 1f / Mathf.Max(1f, tutorial.FramesPerSecond);
        var index = 0;
        while (_currentTutorial == tutorial)
        {
            _illustrationImage.sprite = frames[index];
            yield return new WaitForSecondsRealtime(frameDelay);

            index++;
            if (index >= frames.Length)
            {
                if (!tutorial.LoopAnimation)
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
        ReleasePause();
    }

    private void BuildModal()
    {
        _canvasObject = new GameObject("TutorialModalCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvasObject.transform.SetParent(transform, false);

        var canvas = _canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1200;

        var scaler = _canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _modalRoot = CreateObject("TutorialModal", _canvasObject.transform, typeof(Image));
        Stretch(_modalRoot.GetComponent<RectTransform>());
        var dimmer = _modalRoot.GetComponent<Image>();
        dimmer.sprite = GetUiSprite();
        dimmer.color = new Color(0.015f, 0.02f, 0.04f, 0.88f);
        dimmer.raycastTarget = true;

        var panel = CreateObject("TutorialPanel", _modalRoot.transform, typeof(Image), typeof(Outline));
        SetCentered(panel.GetComponent<RectTransform>(), new Vector2(1180f, 760f), Vector2.zero);
        var panelImage = panel.GetComponent<Image>();
        panelImage.sprite = GetUiSprite();
        panelImage.type = Image.Type.Sliced;
        panelImage.color = new Color(0.07f, 0.085f, 0.14f, 1f);
        panelImage.raycastTarget = true;
        var outline = panel.GetComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.68f, 0.3f, 0.95f);
        outline.effectDistance = new Vector2(2f, -2f);

        _titleText = CreateText("Title", panel.transform, new Vector2(1000f, 86f),
            new Vector2(0f, 310f), 44f, FontStyles.Bold, TextAlignmentOptions.Center);
        _titleText.color = new Color(1f, 0.82f, 0.49f, 1f);

        _instructionsText = CreateText("Instructions", panel.transform, new Vector2(570f, 400f),
            new Vector2(-255f, 5f), 30f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        _instructionsText.enableAutoSizing = true;
        _instructionsText.fontSizeMin = 22f;
        _instructionsText.fontSizeMax = 30f;

        _actionHintText = CreateText("ActionHint", panel.transform, new Vector2(570f, 110f),
            new Vector2(-255f, -230f), 27f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        _actionHintText.color = new Color(0.45f, 0.9f, 0.68f, 1f);
        _actionHintText.enableAutoSizing = true;
        _actionHintText.fontSizeMin = 20f;
        _actionHintText.fontSizeMax = 27f;

        var visualFrame = CreateObject("IllustrationFrame", panel.transform, typeof(Image), typeof(Outline));
        SetCentered(visualFrame.GetComponent<RectTransform>(), new Vector2(430f, 440f), new Vector2(335f, 5f));
        var visualFrameImage = visualFrame.GetComponent<Image>();
        visualFrameImage.sprite = GetUiSprite();
        visualFrameImage.type = Image.Type.Sliced;
        visualFrameImage.color = new Color(0.12f, 0.14f, 0.2f, 1f);
        visualFrameImage.raycastTarget = false;
        var visualOutline = visualFrame.GetComponent<Outline>();
        visualOutline.effectColor = new Color(0.78f, 0.57f, 0.31f, 0.72f);
        visualOutline.effectDistance = new Vector2(1f, -1f);

        var imageObject = CreateObject("TutorialIllustration", visualFrame.transform, typeof(Image));
        Stretch(imageObject.GetComponent<RectTransform>(), new Vector2(16f, 16f));
        _illustrationImage = imageObject.GetComponent<Image>();
        _illustrationImage.preserveAspect = true;
        _illustrationImage.raycastTarget = false;

        var buttonObject = CreateObject("AcknowledgeButton", panel.transform,
            typeof(Image), typeof(Button), typeof(Outline));
        SetCentered(buttonObject.GetComponent<RectTransform>(), new Vector2(330f, 74f), new Vector2(0f, -320f));
        var buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.sprite = GetUiSprite();
        buttonImage.type = Image.Type.Sliced;
        buttonImage.color = new Color(0.58f, 0.28f, 0.09f, 1f);
        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(CloseCurrentTutorial);
        var buttonOutline = buttonObject.GetComponent<Outline>();
        buttonOutline.effectColor = new Color(1f, 0.73f, 0.34f, 1f);
        buttonOutline.effectDistance = new Vector2(2f, -2f);
        _closeButtonLabel = CreateText("ButtonLabel", buttonObject.transform,
            new Vector2(310f, 66f), Vector2.zero, 26f, FontStyles.Bold, TextAlignmentOptions.Center);
        SetLocalized(_closeButtonLabel, "Rozumiem");

        EnsureEventSystem();
        _modalRoot.SetActive(false);
    }

    private TextMeshProUGUI CreateText(string objectName, Transform parent, Vector2 size,
        Vector2 position, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        var textObject = CreateObject(objectName, parent, typeof(TextMeshProUGUI));
        SetCentered(textObject.GetComponent<RectTransform>(), size, position);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        var font = FindTutorialFont();
        if (font == null)
            font = TMP_Settings.defaultFontAsset;
        if (font != null)
            text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.margin = new Vector4(12f, 8f, 12f, 8f);
        return text;
    }

    private static void SetLocalized(TMP_Text text, string source)
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLocalizedText(text, source ?? string.Empty);
        else if (text != null)
            text.text = source ?? string.Empty;
    }

    private static TMP_FontAsset FindTutorialFont()
    {
        return Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
            .FirstOrDefault(font => font != null && font.name.Contains("BreeSerif"));
    }

    private static GameObject CreateObject(string objectName, Transform parent, params System.Type[] components)
    {
        var gameObject = new GameObject(objectName, components);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void SetCentered(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect, Vector2 inset = default)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = inset;
        rect.offsetMax = -inset;
    }

    private static Sprite GetUiSprite()
    {
        return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        var eventSystemObject = new GameObject("TutorialEventSystem",
            typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemObject);
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
        _seenIds.Clear();
        if (string.IsNullOrEmpty(ES3SlotManager.selectedSlotPath))
            return;

        var settings = SaveLoadManager.Instance != null ? SaveLoadManager.Instance.GetSettings() : null;
        if (settings == null || !ES3.KeyExists(SaveKey, settings))
            return;

        var loadedIds = ES3.Load<List<string>>(SaveKey, settings);
        if (loadedIds == null)
            return;

        foreach (var id in loadedIds)
        {
            if (!string.IsNullOrWhiteSpace(id))
                _seenIds.Add(id);
        }
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
