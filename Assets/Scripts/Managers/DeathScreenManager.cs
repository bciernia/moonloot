using System.Collections;
using TMPro;
using UnityEngine;

public class DeathScreenManager : Singleton<DeathScreenManager>
{
    [Header("Root")]
    [SerializeField] private GameObject deathScreen;

    [Header("UI")]
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private TextMeshProUGUI topLabel;

    [SerializeField] private GameObject returnButton;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    [SerializeField] private float buttonDelay = 2f;

    private SpriteRenderer _playerRenderer;

    private string _originalSortingLayer;
    private int _originalOrder;
    
    protected override void Awake()
    {
        base.Awake();
        deathScreen.SetActive(false);
        panelCanvasGroup.alpha = 0f;
        returnButton.SetActive(false);
    }

    public void ShowEndScreen(string title, int score)
    {
        deathScreen.SetActive(true);

        topLabel.text = title;
        scoreLabel.text = $"Score: {score}";

        _playerRenderer = Player.Instance.GetComponent<SpriteRenderer>();

        if (_playerRenderer != null)
        {
            _originalSortingLayer = _playerRenderer.sortingLayerName;
            _originalOrder = _playerRenderer.sortingOrder;

            _playerRenderer.sortingLayerName = "UI";
            _playerRenderer.sortingOrder = 10;
        }

        StartCoroutine(DeathRoutine());
    }

    public void ShowDeathScreen(int score)
    {
        ShowEndScreen("You died", score);
    }

    public void ShowWinScreen(int score)
    {
        ShowEndScreen("You won", score);
    }
    
    private IEnumerator DeathRoutine()
    {
        var timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            panelCanvasGroup.alpha =
                Mathf.Lerp(0f, 1f, timer / fadeDuration);

            yield return null;
        }

        panelCanvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(buttonDelay);

        returnButton.SetActive(true);
    }

    public void ReturnToMenu()
    {
        if (_playerRenderer != null)
        {
            _playerRenderer.sortingLayerName = _originalSortingLayer;
            _playerRenderer.sortingOrder = _originalOrder;
        }
        
        Time.timeScale = 1f;

        deathScreen.SetActive(false);
        
        LoadingSceneManager.Instance.LoadMainMenu();
        
        Destroy(GameObject.FindWithTag("GameRoot"));
        Destroy(GameObject.FindWithTag("Player"));
    }
}