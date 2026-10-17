using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private const string DEMO_START_SCENE = "Base_Demo";
    private const string MAIN_MENU = "MainMenu";
    [SerializeField] private GameObject[] _buttonsMenu;

    [Header("Main menu character selection")]
    [SerializeField] private RectTransform _characterPanel;
    [SerializeField] private Transform _characterCardsContainer;
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private TMP_Text _characterDescription;
    [SerializeField] private GameObject _characterPreviewObject;
    [SerializeField] private Button _characterStartButton;
    [SerializeField] private Button _characterBackButton;
    [SerializeField] private float _panelSlideDistance = 1800f;
    [SerializeField] private float _panelSlideDuration = 0.45f;
    [SerializeField] private string _idleStateName = "Idle";
    [SerializeField, Min(0.1f)] private float _characterPreviewZoom = 1.5f;
    [SerializeField] private TitleAndButtonsIntro2D _titleAndButtonsIntro;
    
    private GameObject _loadPanel;
    private GameObject _optionsPanel;
    private Vector2 _characterPanelVisiblePosition;
    private Coroutine _panelTransition;
    private GameObject _characterPreviewInstance;
    private Camera _characterPreviewCamera;
    private RenderTexture _characterPreviewTexture;
    private Material _characterPreviewMaterial;
    private RawImage _characterPreviewImage;
    private NPCHero _selectedHero;
    private bool _isStartingGame;

    public static NPCHero SelectedHeroForNewGame { get; private set; }

    public static void ClearSelectedHeroForNewGame()
    {
        SelectedHeroForNewGame = null;
    }

    private void Start()
    {
        InitializeCharacterSelection();
    }

    private void OnDestroy()
    {
        if (_characterPreviewInstance != null)
            Destroy(_characterPreviewInstance);
        if (_characterPreviewCamera != null)
            Destroy(_characterPreviewCamera.gameObject);
        if (_characterPreviewTexture != null)
        {
            _characterPreviewTexture.Release();
            Destroy(_characterPreviewTexture);
        }
        if (_characterPreviewMaterial != null)
            Destroy(_characterPreviewMaterial);
    }

    // Existing Play button entry point now opens hero selection.
    public void StartGame()
    {
        if (_characterPanel != null && _characterPanel.gameObject.activeInHierarchy)
        {
            ConfirmCharacterSelection();
            return;
        }

        OpenCharacterSelection();
    }

    public void OpenCharacterSelection()
    {
        if (_characterPanel == null)
        {
            Debug.LogError("Main menu character panel is not assigned.", this);
            return;
        }

        ChangeButtonsVisibility(false);
        _characterPanel.gameObject.SetActive(true);
        StartPanelTransition(_characterPanelVisiblePosition);
        _titleAndButtonsIntro?.SetCharacterSelectionTitle(true);
        if (_selectedHero == null)
            SelectDefaultCharacter();
    }

    public void ReturnToMainMenuButtons()
    {
        if (_characterPanel == null)
        {
            ChangeButtonsVisibility(true);
            return;
        }

        StartPanelTransition(_characterPanelVisiblePosition + Vector2.right * _panelSlideDistance,
            () =>
            {
                _characterPanel.gameObject.SetActive(false);
                ChangeButtonsVisibility(true);
            });
        _titleAndButtonsIntro?.SetCharacterSelectionTitle(false);
    }

    public void ConfirmCharacterSelection()
    {
        if (_isStartingGame)
            return;

        if (_selectedHero == null)
        {
            Debug.LogWarning("Select a hero before starting a new game.", this);
            return;
        }

        _isStartingGame = true;
        SelectedHeroForNewGame = _selectedHero;
        var gameRoot = GameObject.FindWithTag("GameRoot");
        var player = GameObject.FindWithTag("Player");

        if (gameRoot != null) Destroy(gameRoot);
        if (player != null) Destroy(player);
        
        // ES3.DeleteFile();
        
        SaveLoadManager.Instance.CreateNewSaveForNewGame();
        LoadingSceneManager.Instance.StartNewGame(DEMO_START_SCENE);
        ChangeButtonsVisibility(false);
    }

    private void InitializeCharacterSelection()
    {
        if (_titleAndButtonsIntro == null)
            _titleAndButtonsIntro = FindAnyObjectByType<TitleAndButtonsIntro2D>();

        if (_characterPanel != null)
        {
            _characterPanelVisiblePosition = _characterPanel.anchoredPosition;
            _characterPanel.anchoredPosition += Vector2.right * _panelSlideDistance;
            _characterPanel.gameObject.SetActive(false);
        }

        if (_characterStartButton != null)
        {
            _characterStartButton.interactable = false;
            _characterStartButton.onClick.AddListener(ConfirmCharacterSelection);
        }

        if (_characterBackButton != null)
            _characterBackButton.onClick.AddListener(ReturnToMainMenuButtons);

        if (_characterCardsContainer == null)
            return;

        foreach (var card in _characterCardsContainer.GetComponentsInChildren<CharacterMenuUI>(true))
        {
            card.Setup(card.Hero);
            card.OnCharacterSelected += HandleCharacterSelected;
        }
    }

    private void HandleCharacterSelected(CharacterMenuUI selectedCard, NPCHero hero)
    {
        if (hero == null || hero.Character == null)
            return;

        _selectedHero = hero;
        if (_characterStartButton != null)
            _characterStartButton.interactable = true;

        if (_characterName != null)
            _characterName.text = $"{hero.Name}, the {hero.Profession} ";

        if (_characterDescription != null)
        {
            if (!string.IsNullOrWhiteSpace(hero.MenuDescription))
            {
                _characterDescription.text = hero.MenuDescription;
            }
        }

        foreach (var card in _characterCardsContainer.GetComponentsInChildren<CharacterMenuUI>(true))
            card.SetSelected(card == selectedCard);

        ShowCharacterPreview(hero, selectedCard);
    }

    private void SelectDefaultCharacter()
    {
        if (_characterCardsContainer == null)
            return;

        foreach (var card in _characterCardsContainer.GetComponentsInChildren<CharacterMenuUI>(true))
        {
            if (card.IsLocked || card.Hero == null || card.Hero.Character == null)
                continue;

            card.SelectCharacter();
            return;
        }
    }

    private void ShowCharacterPreview(NPCHero hero, CharacterMenuUI selectedCard)
    {
        if (_characterPreviewObject == null)
        {
            Debug.LogWarning("Assign the Character GameObject as the character preview target.", this);
            return;
        }

        _characterPreviewImage = _characterPreviewObject.GetComponent<RawImage>();
        if (_characterPreviewImage == null)
        {
            var previewChild = _characterPreviewObject.transform.Find("AnimatedPreview");
            if (previewChild == null)
            {
                var previewObject = new GameObject(
                    "AnimatedPreview",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage));
                previewObject.transform.SetParent(_characterPreviewObject.transform, false);
                var previewRect = previewObject.GetComponent<RectTransform>();
                previewRect.anchorMin = Vector2.zero;
                previewRect.anchorMax = Vector2.one;
                previewRect.offsetMin = Vector2.zero;
                previewRect.offsetMax = Vector2.zero;
                previewChild = previewObject.transform;
            }

            _characterPreviewImage = previewChild.GetComponent<RawImage>();
        }
        _characterPreviewImage.raycastTarget = false;

        if (_characterPreviewInstance != null)
            Destroy(_characterPreviewInstance);

        EnsureCharacterPreviewCamera();

        const int previewLayer = 31;
        _characterPreviewInstance = Instantiate(hero.Character, new Vector3(10000f, 10000f, 0f),
            Quaternion.identity);
        SetLayerRecursively(_characterPreviewInstance, previewLayer);

        foreach (var behaviour in _characterPreviewInstance.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (!(behaviour is Animator))
                behaviour.enabled = false;
        }

        var animator = _characterPreviewInstance.GetComponentInChildren<Animator>(true);
        if (animator != null && !string.IsNullOrWhiteSpace(_idleStateName))
        {
            animator.Play(_idleStateName, 0, 0f);
            animator.Update(0f);
        }

        var renderers = _characterPreviewInstance.GetComponentsInChildren<SpriteRenderer>(true);
        var visibleRenderers = new List<SpriteRenderer>();
        var previewMaterial = GetCharacterPreviewMaterial();
        foreach (var spriteRenderer in renderers)
        {
            if (spriteRenderer.sprite != null)
            {
                if (previewMaterial != null)
                    spriteRenderer.sharedMaterial = previewMaterial;
                visibleRenderers.Add(spriteRenderer);
            }
        }

        if (visibleRenderers.Count == 0)
        {
            Debug.LogWarning(
                $"Hero prefab '{hero.name}' has no visible SpriteRenderer after entering '{_idleStateName}'.",
                this);
            return;
        }

        var bounds = visibleRenderers[0].bounds;
        for (var i = 1; i < visibleRenderers.Count; i++)
            bounds.Encapsulate(visibleRenderers[i].bounds);

        selectedCard.SetPortrait(visibleRenderers[0].sprite);

        var center = bounds.center;
        _characterPreviewCamera.transform.position = center + Vector3.back * 20f;
        _characterPreviewCamera.transform.rotation = Quaternion.identity;
        _characterPreviewCamera.orthographic = true;

        var rect = _characterPreviewImage.rectTransform.rect;
        var aspect = rect.height > 0f ? rect.width / rect.height : 1f;
        _characterPreviewCamera.aspect = aspect;
        _characterPreviewCamera.orthographicSize = Mathf.Max(
            bounds.extents.y,
            bounds.extents.x / Mathf.Max(aspect, 0.01f)) * 1.2f / Mathf.Max(_characterPreviewZoom, 0.1f);

    }

    private Material GetCharacterPreviewMaterial()
    {
        if (_characterPreviewMaterial != null)
            return _characterPreviewMaterial;

        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogWarning("Could not find an unlit sprite shader for the character preview.", this);
            return null;
        }

        _characterPreviewMaterial = new Material(shader)
        {
            name = "Character Preview Unlit Material"
        };
        return _characterPreviewMaterial;
    }

    private void EnsureCharacterPreviewCamera()
    {
        if (_characterPreviewCamera == null)
        {
            var cameraObject = new GameObject("CharacterPreviewCamera");
            cameraObject.transform.SetParent(transform, false);
            _characterPreviewCamera = cameraObject.AddComponent<Camera>();
            _characterPreviewCamera.clearFlags = CameraClearFlags.SolidColor;
            _characterPreviewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _characterPreviewCamera.cullingMask = 1 << 31;
            _characterPreviewCamera.nearClipPlane = 0.01f;
            _characterPreviewCamera.farClipPlane = 100f;
        }

        if (_characterPreviewTexture == null)
        {
            _characterPreviewTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            _characterPreviewTexture.Create();
        }

        _characterPreviewCamera.targetTexture = _characterPreviewTexture;
        _characterPreviewImage.texture = _characterPreviewTexture;
    }

    private void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void StartPanelTransition(Vector2 targetPosition, System.Action completed = null)
    {
        if (_panelTransition != null)
            StopCoroutine(_panelTransition);
        _panelTransition = StartCoroutine(AnimateCharacterPanel(targetPosition, completed));
    }

    private IEnumerator AnimateCharacterPanel(Vector2 targetPosition, System.Action completed)
    {
        var startPosition = _characterPanel.anchoredPosition;
        var elapsed = 0f;
        while (elapsed < _panelSlideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _panelSlideDuration));
            _characterPanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        _characterPanel.anchoredPosition = targetPosition;
        _panelTransition = null;
        completed?.Invoke();
    }

    public void ExitGame()
    {
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void GoToMainMenu()
    {
        LoadingSceneManager.Instance.LoadScene("MainMenu");
        
        Destroy(GameObject.FindWithTag("GameRoot"));
        Destroy(GameObject.FindWithTag("Player"));
    }
    
    private void OnEnable()
    {
#pragma warning disable UDR0005
        SceneManager.sceneLoaded += OnSceneLoaded;
#pragma warning restore UDR0005
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMode(scene.name == MAIN_MENU ? GameMode.MainMenu : GameMode.Location);
        }
        else
        {
            ChangeButtonsVisibility(true);
            // MainMenuSoundManager.Instance.PlayMainMenuMusic();
        }
    }

    private void ChangeButtonsVisibility(bool shouldBeVisible)
    {
        foreach (var btn in _buttonsMenu)
        {
            btn.gameObject.SetActive(shouldBeVisible);
        }
    }
    
    public void SetActiveLoadPanel(bool isActive)
    {
        if (_loadPanel == null)
        {
            FindLoadPanel();
        }

        if (_loadPanel != null)
        {
            _loadPanel.SetActive(isActive);
        }
    }    
    
    public void SetActiveOptionsPanel(bool isActive)
    {
        if (_optionsPanel == null)
        {
            FindOptionsPanel();
        }

        if (_optionsPanel != null)
        {
            _optionsPanel.SetActive(isActive);
        }
    }    
    
    private void FindLoadPanel()
    {
        var transforms =
            FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (var t in transforms)
        {
            if (!t.CompareTag("LoadSlots"))
                continue;

            _loadPanel = t.gameObject;
            return;
        }

        Debug.LogError("LoadPanel not found!");
    }
    
    private void FindOptionsPanel()
    {
        var transforms =
            FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (var t in transforms)
        {
            if (!t.CompareTag("OptionsMenu"))
                continue;

            _optionsPanel = t.gameObject;
            return;
        }

        Debug.LogError("OptionsPanel not found!");
    }
}
