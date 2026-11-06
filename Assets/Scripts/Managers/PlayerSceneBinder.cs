using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Rebinds scene-dependent systems to the player created for each gameplay scene.
/// </summary>
public sealed class PlayerSceneBinder : MonoBehaviour
{
    private static PlayerSceneBinder _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBinder()
    {
        if (_instance != null)
            return;

        var binderObject = new GameObject(nameof(PlayerSceneBinder));
        binderObject.AddComponent<PlayerSceneBinder>();
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
        StopAllCoroutines();
        StartCoroutine(BindAfterSceneInitialization());
    }

    private IEnumerator BindAfterSceneInitialization()
    {
        // The sceneLoaded callback runs after Awake, but some player prefabs are
        // instantiated by a scene bootstrapper on its first frame.
        var timeout = 2f;
        Player player = null;
        while (timeout > 0f && player == null)
        {
            player = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();
            if (player != null)
                break;

            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        Bind(player);
    }

    private static void Bind(Player player)
    {
        if (player == null)
            return;

        var gameManager = GameManager.Instance;
        if (gameManager != null)
            gameManager.BindPlayerContext(player);

        var cameraManager = CameraFocusManager.Instance;
        if (cameraManager != null)
            cameraManager.BindPlayerContext(player);

        var skillsManager = SkillsManager.Instance;
        if (skillsManager != null)
            skillsManager.BindPlayerContext(player);

        var dialogueManager = DialogueManager.Instance;
        if (dialogueManager != null)
            dialogueManager.BindPlayerContext(player);

        var outfitManager = OutfitManager.Instance;
        if (outfitManager != null)
            outfitManager.BindPlayerContext(player);

        var uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
            uiManager.BindPlayerContext(player);

        var interactionManagers = FindObjectsOfType<InteractionManager>(true);
        foreach (var interactionManager in interactionManagers)
        {
            if (interactionManager != null)
                interactionManager.BindPlayerContext(player);
        }
    }
}
