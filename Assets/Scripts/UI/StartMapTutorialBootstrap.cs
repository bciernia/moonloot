using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shows the START_MAP story card and starts its fixed Aulla objective.
/// It creates its UI at runtime so no extra scene references are required.
/// </summary>
public sealed class StartMapTutorialBootstrap : MonoBehaviour
{
    private const string TutorialSceneName = "START_MAP";
    private const string TutorialTitle = "Wiedza Aulli";
    private const string StoryText = "Biegłeś nocą przez las w poszukiwaniu schronienia, niestety potwory zdążyły cię dogonić. Znajdź kryształy Aulli, żeby dowiedzieć się, gdzie jest najbliższa wioska.";
    private const string ObjectiveText = "Znajdź trzy kryształy Aulli, aby otworzyć portal.";

    private static StartMapTutorialBootstrap _instance;
    private GameObject _introCanvas;
    private bool _pauseRequested;
    private bool _fallbackPauseRequested;
    private float _previousTimeScale = 1f;
    private bool _tutorialStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        var bootstrapObject = new GameObject(nameof(StartMapTutorialBootstrap));
        bootstrapObject.AddComponent<StartMapTutorialBootstrap>();
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
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        _instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _tutorialStarted = false;
        if (scene.name != TutorialSceneName)
        {
            DestroyIntro();
            ReleaseIntroPause();
            return;
        }

        CreateIntro();
    }

    private void CreateIntro()
    {
        DestroyIntro();

        var canvasObject = new GameObject(
            "StartMapTutorialIntro",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        _introCanvas = canvasObject;
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var dimmer = CreateImage("Dimmer", canvasObject.transform,
            new Color(0.015f, 0.02f, 0.035f, 0.82f));
        SetStretch(dimmer.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var panel = CreateImage("StoryPanel", canvasObject.transform,
            new Color(0.09f, 0.105f, 0.15f, 0.98f));
        SetStretch(panel.rectTransform, new Vector2(0.1f, 0.09f), new Vector2(0.9f, 0.91f), Vector2.zero, Vector2.zero);

        var title = CreateText("Title", panel.transform, TutorialTitle, 54f, FontStyles.Bold);
        SetAnchors(title.rectTransform, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.97f));
        title.alignment = TextAlignmentOptions.Center;

        var story = CreateText("Story", panel.transform, StoryText, 37f, FontStyles.Normal);
        SetAnchors(story.rectTransform, new Vector2(0.09f, 0.37f), new Vector2(0.91f, 0.78f));
        story.alignment = TextAlignmentOptions.Center;
        story.enableAutoSizing = true;
        story.fontSizeMin = 27f;
        story.fontSizeMax = 37f;

        var objective = CreateText("Objective", panel.transform, ObjectiveText, 32f, FontStyles.Bold);
        SetAnchors(objective.rectTransform, new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.36f));
        objective.alignment = TextAlignmentOptions.Center;
        objective.color = new Color(1f, 0.82f, 0.36f, 1f);
        objective.enableAutoSizing = true;
        objective.fontSizeMin = 23f;
        objective.fontSizeMax = 32f;

        var buttonObject = new GameObject(
            "BeginButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.045f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.045f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(340f, 82f);

        var buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.77f, 0.42f, 0.13f, 1f);
        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(BeginTutorial);

        var buttonLabel = CreateText("Label", buttonObject.transform, "Rozpocznij", 30f, FontStyles.Bold);
        SetStretch(buttonLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        buttonLabel.alignment = TextAlignmentOptions.Center;
        buttonLabel.color = Color.white;

        Localize(title, TutorialTitle);
        Localize(story, StoryText);
        Localize(objective, ObjectiveText);
        Localize(buttonLabel, "Rozpocznij");

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.RequestPause();
            _pauseRequested = true;
        }
        else
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _fallbackPauseRequested = true;
        }

        EnsureEventSystem();
    }

    private void BeginTutorial()
    {
        if (_tutorialStarted)
            return;

        if (HordeManager.Instance == null)
        {
            Debug.LogError("HordeManager is missing; cannot start the tutorial objective.");
            return;
        }

        _tutorialStarted = true;
        DestroyIntro();

        ReleaseIntroPause();

        HordeManager.Instance.StartTutorialMap();
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        var eventSystemObject = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }

    private void ReleaseIntroPause()
    {
        if (_pauseRequested && PauseManager.Instance != null)
            PauseManager.Instance.ReleasePause();
        _pauseRequested = false;

        if (_fallbackPauseRequested)
            Time.timeScale = _previousTimeScale;
        _fallbackPauseRequested = false;
    }

    private static Image CreateImage(string objectName, Transform parent, Color color)
    {
        var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        var image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return image;
    }

    private static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string source,
        float fontSize,
        FontStyles style)
    {
        var textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = source;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.margin = new Vector4(12f, 8f, 12f, 8f);
        return text;
    }

    private static void Localize(TMP_Text text, string source)
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLocalizedText(text, source);
        else
            text.text = source;
    }

    private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetStretch(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private void DestroyIntro()
    {
        if (_introCanvas != null)
            Destroy(_introCanvas);
        _introCanvas = null;
    }
}
