using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    private Canvas _cursorCanvas;
    private Graphic _cursorGraphic;
    private bool _addedCursorCanvas;
    private RenderMode _previousCursorRenderMode;
    private bool _previousCursorOverrideSorting;
    private int _previousCursorSortingOrder;
    private bool _previousCursorRaycastTarget;
    private bool _pauseRequested;
    private bool _fallbackPauseRequested;
    private float _previousTimeScale = 1f;
    private bool _tutorialStarted;
    private Coroutine _entranceRoutine;
    private PlayerInput _entrancePlayerInput;
    private PlayerMovement _entrancePlayerMovement;
    private Rigidbody2D _entranceRigidbody;
    private PlayerAnimations _entranceAnimations;
    private Transform _entranceRunner;
    private Animator _previewAnimator;
    private AnimatorCullingMode _previousPreviewCullingMode;
    private Renderer[] _playerRenderers;
    private readonly List<bool> _playerRendererStates = new();
    private Collider2D[] _playerColliders;
    private readonly List<bool> _playerColliderStates = new();
    private bool _playerBodyWasSimulated;
    private bool _playerInputWasEnabled;
    private bool _playerMovementWasEnabled;
    private AnimatorUpdateMode _previousPreviewAnimatorUpdateMode;
    private bool _previewAnimatorWasPrepared;
    private bool _cursorGraphicWasEnabled;
    private bool _previousSystemCursorVisible;
    private Graphic _entranceCursorGraphic;
    private readonly List<Canvas> _hiddenCanvases = new();
    private readonly List<bool> _hiddenCanvasStates = new();

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
        if (_entranceRoutine != null)
        {
            StopCoroutine(_entranceRoutine);
            _entranceRoutine = null;
            RestoreEntranceState();
            CameraFocusManager.Instance?.UnlockCameraFollow();
            ReleaseIntroPause();
        }

        _tutorialStarted = false;
        if (scene.name != TutorialSceneName)
        {
            DestroyIntro();
            ReleaseIntroPause();
            MainMenuManager.ClearSelectedHeroForTutorialIntro();
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
        canvas.sortingOrder = 32766;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var dimmer = CreateImage("Dimmer", canvasObject.transform,
            Color.black);
        SetStretch(dimmer.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var panel = CreateImage("StoryPanel", canvasObject.transform,
            Color.black);
        SetStretch(panel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

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

        BringCustomCursorAboveIntro();

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
        _entranceRoutine = StartCoroutine(PlayEntranceThenStartTutorial());
    }

    private IEnumerator PlayEntranceThenStartTutorial()
    {
        var player = Player.Instance;
        if (player == null)
        {
            DestroyIntro();
            ReleaseIntroPause();
            HordeManager.Instance.StartTutorialMap();
            _entranceRoutine = null;
            yield break;
        }

        var selectedHero = MainMenuManager.SelectedHeroForTutorialIntro;
        if (selectedHero == null || selectedHero.Character == null)
        {
            Debug.LogWarning("The selected hero has no character prefab for the START_MAP entrance. Starting the tutorial without the entrance animation.");
            DestroyIntro();
            ReleaseIntroPause();
            MainMenuManager.ClearSelectedHeroForTutorialIntro();
            HordeManager.Instance.StartTutorialMap();
            _entranceRoutine = null;
            yield break;
        }

        HideGameplayCanvases();

        _entrancePlayerInput = player.GetComponent<PlayerInput>();
        if (_entrancePlayerInput != null)
        {
            _playerInputWasEnabled = _entrancePlayerInput.enabled;
            _entrancePlayerInput.enabled = false;
        }

        _entrancePlayerMovement = player.GetComponent<PlayerMovement>();
        if (_entrancePlayerMovement != null)
        {
            _playerMovementWasEnabled = _entrancePlayerMovement.enabled;
            _entrancePlayerMovement.enabled = false;
        }

        _entranceRigidbody = player.GetComponent<Rigidbody2D>();
        _entranceAnimations = player.GetComponent<PlayerAnimations>();

        _entranceCursorGraphic = _cursorGraphic;
        if (_entranceCursorGraphic != null)
        {
            _cursorGraphicWasEnabled = _entranceCursorGraphic.enabled;
            _entranceCursorGraphic.enabled = false;
        }
        _previousSystemCursorVisible = Cursor.visible;
        Cursor.visible = false;

        CameraFocusManager.Instance?.LockCameraAtPlayerPosition(player.transform);

        var endPosition = player.transform.position;
        var startPosition = endPosition + Vector3.left * 8f;
        var gameplayCamera = Camera.main;
        if (gameplayCamera != null)
        {
            var depth = Mathf.Abs(gameplayCamera.transform.position.z - endPosition.z);
            endPosition = gameplayCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
            endPosition.z = player.transform.position.z;
            startPosition = gameplayCamera.ViewportToWorldPoint(new Vector3(-0.08f, 0.5f, depth));
            startPosition.z = endPosition.z;
        }

        player.transform.position = endPosition;
        if (_entranceRigidbody != null)
        {
            _playerBodyWasSimulated = _entranceRigidbody.simulated;
            _entranceRigidbody.simulated = false;
        }

        _playerRenderers = player.GetComponentsInChildren<Renderer>(true);
        _playerRendererStates.Clear();
        foreach (var renderer in _playerRenderers)
        {
            _playerRendererStates.Add(renderer != null && renderer.enabled);
            if (renderer != null)
                renderer.enabled = false;
        }

        _playerColliders = player.GetComponentsInChildren<Collider2D>(true);
        _playerColliderStates.Clear();
        foreach (var collider in _playerColliders)
        {
            _playerColliderStates.Add(collider != null && collider.enabled);
            if (collider != null)
                collider.enabled = false;
        }

        var previewRoot = new GameObject("TutorialEntranceHero");
        previewRoot.SetActive(false);
        var previewObject = Instantiate(selectedHero.Character, startPosition, Quaternion.identity, previewRoot.transform);
        previewObject.name = selectedHero.Character.name;
        PreparePreviewCharacter(previewObject, player);

        // Keep the story card up until the temporary character is safe to show.
        yield return null;

        previewRoot.transform.position = Vector3.zero;
        previewRoot.SetActive(true);
        _entranceRunner = previewObject.transform;
        SetPreviewMoving(_previewAnimator, true);
        DestroyIntro(restoreCursor: false);

        const float entranceDuration = 1.6f;
        var elapsed = 0f;
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / entranceDuration);
            _entranceRunner.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        SetPreviewMoving(_previewAnimator, false);
        previewRoot.SetActive(false);
        Destroy(previewRoot);
        _entranceRunner = null;
        if (_entranceRigidbody != null)
            _entranceRigidbody.position = endPosition;
        player.transform.position = endPosition;
        if (_entranceAnimations != null)
        {
            _entranceAnimations.SetMoveAnimation(Vector2.right);
            _entranceAnimations.SetIsMovingAnimation(false);
        }

        RestoreCustomCursor();
        RestoreEntranceState();
        CameraFocusManager.Instance?.UnlockCameraFollow();
        ReleaseIntroPause();
        MainMenuManager.ClearSelectedHeroForTutorialIntro();
        HordeManager.Instance.StartTutorialMap();
        _entranceRoutine = null;
    }

    private void PreparePreviewCharacter(GameObject previewObject, Player player)
    {
        foreach (var behaviour in previewObject.GetComponentsInChildren<Behaviour>(true))
        {
            if (behaviour == null)
                continue;

            if (behaviour is Animator || behaviour is PlayerAnimations)
                continue;

            if (behaviour is MonoBehaviour)
                Destroy(behaviour);
            else
                behaviour.enabled = false;
        }

        foreach (var collider in previewObject.GetComponentsInChildren<Collider2D>(true))
            collider.enabled = false;

        foreach (var body in previewObject.GetComponentsInChildren<Rigidbody2D>(true))
            body.simulated = false;

        _previewAnimator = previewObject.GetComponentInChildren<Animator>(true);
        if (_previewAnimator != null)
        {
            _previousPreviewAnimatorUpdateMode = _previewAnimator.updateMode;
            _previousPreviewCullingMode = _previewAnimator.cullingMode;
            _previewAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            _previewAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            _previewAnimatorWasPrepared = true;
        }

        var playerSpriteRenderer = player.GetComponentInChildren<SpriteRenderer>(true);
        SetLayerRecursively(previewObject, player.gameObject.layer);
        if (playerSpriteRenderer == null)
            return;

        var previewSpriteRenderer = previewObject.GetComponentInChildren<SpriteRenderer>(true);
        if (previewSpriteRenderer == null)
            return;

        var baseSortingOrder = previewSpriteRenderer.sortingOrder;
        foreach (var spriteRenderer in previewObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            spriteRenderer.sortingLayerID = playerSpriteRenderer.sortingLayerID;
            spriteRenderer.sortingOrder += playerSpriteRenderer.sortingOrder - baseSortingOrder;
        }
    }

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private static void SetPreviewMoving(Animator animator, bool moving)
    {
        if (animator == null)
            return;

        foreach (var parameter in animator.parameters)
        {
            if ((parameter.name == "MoveX" || parameter.name == "MovingX") &&
                parameter.type == AnimatorControllerParameterType.Float)
                animator.SetFloat(parameter.nameHash, 1f);
            else if ((parameter.name == "MoveY" || parameter.name == "MovingY") &&
                     parameter.type == AnimatorControllerParameterType.Float)
                animator.SetFloat(parameter.nameHash, 0f);
            else if (parameter.name == "IsMoving" && parameter.type == AnimatorControllerParameterType.Bool)
                animator.SetBool(parameter.nameHash, moving);
        }

        if (moving)
        {
            var walkStateHash = Animator.StringToHash("Base Layer.Zombie_Walk");
            if (animator.HasState(0, walkStateHash))
                animator.Play(walkStateHash, 0, 0f);
        }

    }

    private void HideGameplayCanvases()
    {
        _hiddenCanvases.Clear();
        _hiddenCanvasStates.Clear();

        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas == null || canvas.gameObject == _introCanvas || canvas == _cursorCanvas)
                continue;

            if (canvas.renderMode == RenderMode.WorldSpace)
                continue;

            _hiddenCanvases.Add(canvas);
            _hiddenCanvasStates.Add(canvas.enabled);
            canvas.enabled = false;
        }
    }

    private void RestoreEntranceState()
    {
        for (var i = 0; i < _hiddenCanvases.Count; i++)
        {
            if (_hiddenCanvases[i] != null)
                _hiddenCanvases[i].enabled = _hiddenCanvasStates[i];
        }
        _hiddenCanvases.Clear();
        _hiddenCanvasStates.Clear();

        if (_previewAnimator != null && _previewAnimatorWasPrepared)
        {
            _previewAnimator.updateMode = _previousPreviewAnimatorUpdateMode;
            _previewAnimator.cullingMode = _previousPreviewCullingMode;
        }
        _previewAnimatorWasPrepared = false;

        if (_entranceRunner != null)
        {
            _entranceRunner.root.gameObject.SetActive(false);
            Destroy(_entranceRunner.root.gameObject);
        }
        _entranceRunner = null;

        if (_entranceRigidbody != null)
            _entranceRigidbody.simulated = _playerBodyWasSimulated;

        if (_playerRenderers != null)
        {
            for (var i = 0; i < _playerRenderers.Length; i++)
            {
                if (_playerRenderers[i] != null)
                    _playerRenderers[i].enabled = _playerRendererStates[i];
            }
        }
        _playerRenderers = null;
        _playerRendererStates.Clear();

        if (_playerColliders != null)
        {
            for (var i = 0; i < _playerColliders.Length; i++)
            {
                if (_playerColliders[i] != null)
                    _playerColliders[i].enabled = _playerColliderStates[i];
            }
        }
        _playerColliders = null;
        _playerColliderStates.Clear();

        if (_entrancePlayerMovement != null)
            _entrancePlayerMovement.enabled = _playerMovementWasEnabled;
        if (_entrancePlayerInput != null)
            _entrancePlayerInput.enabled = _playerInputWasEnabled;

        if (_entranceCursorGraphic != null)
            _entranceCursorGraphic.enabled = _cursorGraphicWasEnabled;
        _entranceCursorGraphic = null;
        Cursor.visible = _previousSystemCursorVisible;

        _entrancePlayerInput = null;
        _entrancePlayerMovement = null;
        _entranceRigidbody = null;
        _entranceAnimations = null;
        _previewAnimator = null;
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

    private void BringCustomCursorAboveIntro()
    {
        var cursor = FindFirstObjectByType<CursorUI>();
        if (cursor == null)
            return;

        _cursorCanvas = cursor.GetComponent<Canvas>();
        _addedCursorCanvas = _cursorCanvas == null;
        if (_addedCursorCanvas)
            _cursorCanvas = cursor.gameObject.AddComponent<Canvas>();

        _previousCursorRenderMode = _cursorCanvas.renderMode;
        _previousCursorOverrideSorting = _cursorCanvas.overrideSorting;
        _previousCursorSortingOrder = _cursorCanvas.sortingOrder;
        _cursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _cursorCanvas.overrideSorting = true;
        _cursorCanvas.sortingOrder = 32767;

        _cursorGraphic = cursor.GetComponent<Graphic>();
        if (_cursorGraphic != null)
        {
            _previousCursorRaycastTarget = _cursorGraphic.raycastTarget;
            _cursorGraphic.raycastTarget = false;
        }
    }

    private void RestoreCustomCursor()
    {
        if (_cursorGraphic != null)
            _cursorGraphic.raycastTarget = _previousCursorRaycastTarget;

        if (_cursorCanvas != null)
        {
            if (_addedCursorCanvas)
                Destroy(_cursorCanvas);
            else
            {
                _cursorCanvas.renderMode = _previousCursorRenderMode;
                _cursorCanvas.overrideSorting = _previousCursorOverrideSorting;
                _cursorCanvas.sortingOrder = _previousCursorSortingOrder;
            }
        }

        _cursorCanvas = null;
        _cursorGraphic = null;
        _addedCursorCanvas = false;
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

    private void DestroyIntro(bool restoreCursor = true)
    {
        if (restoreCursor)
            RestoreCustomCursor();
        if (_introCanvas != null)
            Destroy(_introCanvas);
        _introCanvas = null;
    }
}
