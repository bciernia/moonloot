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

    [Header("Victory outro")]
    [SerializeField, Min(0f)] private float victoryScreenHoldDuration = 4f;
    [SerializeField, Min(0.01f)] private float outroTransitionDuration = 0.65f;

    private SpriteRenderer _playerRenderer;

    private string _originalSortingLayer;
    private int _originalOrder;
    private Coroutine _endScreenRoutine;

    private static readonly string[] WonTitleTranslations =
    {
        "Zwycięstwo!",
        "You won",
        "Victoire !",
        "¡Has ganado!",
        "Hai vinto!",
        "Du hast gewonnen!",
        "Победа!"
    };

    private static readonly string[] ThankYouTitleTranslations =
    {
        "Dziękujemy za grę!",
        "Thank you for playing!",
        "Merci d’avoir joué !",
        "¡Gracias por jugar!",
        "Grazie per aver giocato!",
        "Danke fürs Spielen!",
        "Спасибо за игру!"
    };

    private static readonly string[] ThankYouBodyTranslations =
    {
        "Jeśli podobała Ci się przygoda, dodaj pełną wersję MOONLOOT do swojej listy życzeń na Steamie.\n\nPozdrawia cały zespół VS&G.",
        "If you enjoyed the adventure, add the full version of MOONLOOT to your Steam Wishlist.\n\nWarm wishes,\nThe whole VS&G team.",
        "Si l’aventure t’a plu, ajoute la version complète de MOONLOOT à ta liste de souhaits Steam.\n\nAvec les salutations de toute l’équipe VS&G.",
        "Si disfrutaste de la aventura, añade la versión completa de MOONLOOT a tu lista de deseados de Steam.\n\nUn saludo de todo el equipo de VS&G.",
        "Se l’avventura ti è piaciuta, aggiungi la versione completa di MOONLOOT alla tua lista dei desideri di Steam.\n\nUn saluto da tutto il team VS&G.",
        "Wenn dir das Abenteuer gefallen hat, setze die Vollversion von MOONLOOT auf deine Steam-Wunschliste.\n\nViele Grüße vom gesamten Team VS&G.",
        "Если тебе понравилось это приключение, добавь полную версию MOONLOOT в список желаемого Steam.\n\nС наилучшими пожеланиями, команда VS&G."
    };
    
    protected override void Awake()
    {
        base.Awake();
        deathScreen.SetActive(false);
        panelCanvasGroup.alpha = 0f;
        returnButton.SetActive(false);
    }

    public void ShowEndScreen(string title, int score)
    {
        PrepareEndScreen(title, score);
        StartEndScreenRoutine(DeathRoutine());
    }

    public void ShowDeathScreen(int score)
    {
        ShowEndScreen("You died", score);
    }

    public void ShowWinScreen(int score)
    {
        PrepareEndScreen("You won", score);
        SetLocalized(topLabel, "You won", WonTitleTranslations);
        StartEndScreenRoutine(WinAndThankYouRoutine());
    }

    private void PrepareEndScreen(string title, int score)
    {
        deathScreen.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        returnButton.SetActive(false);

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
    }

    private void StartEndScreenRoutine(IEnumerator routine)
    {
        if (_endScreenRoutine != null)
            StopCoroutine(_endScreenRoutine);

        _endScreenRoutine = StartCoroutine(routine);
    }

    private IEnumerator WinAndThankYouRoutine()
    {
        yield return FadePanel(0f, 1f, fadeDuration);
        yield return new WaitForSecondsRealtime(victoryScreenHoldDuration);
        yield return FadePanel(1f, 0f, outroTransitionDuration);

        SetLocalized(topLabel, "Thank you for playing!", ThankYouTitleTranslations);
        SetLocalized(scoreLabel, ThankYouBodyTranslations[0], ThankYouBodyTranslations);
        ConfigureThankYouLayout();

        yield return FadePanel(0f, 1f, outroTransitionDuration);
        yield return new WaitForSecondsRealtime(buttonDelay);

        returnButton.SetActive(true);
        _endScreenRoutine = null;
    }

    private IEnumerator FadePanel(float from, float to, float duration)
    {
        var safeDuration = Mathf.Max(0.01f, duration);
        var elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / safeDuration);
            yield return null;
        }

        panelCanvasGroup.alpha = to;
    }

    private void ConfigureThankYouLayout()
    {
        topLabel.enableAutoSizing = true;
        topLabel.fontSizeMin = 32f;
        topLabel.fontSizeMax = 64f;

        scoreLabel.enableWordWrapping = true;
        scoreLabel.enableAutoSizing = true;
        scoreLabel.fontSizeMin = 22f;
        scoreLabel.fontSizeMax = 36f;
        scoreLabel.alignment = TextAlignmentOptions.Center;

        var bodyRect = scoreLabel.rectTransform;
        bodyRect.anchorMin = new Vector2(0.12f, 0.23f);
        bodyRect.anchorMax = new Vector2(0.88f, 0.58f);
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;
    }

    private static void SetLocalized(TMP_Text target, string source, string[] translations)
    {
        if (target == null)
            return;

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLocalizedText(target, source, translations);
        else
            target.text = translations[1];
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
        _endScreenRoutine = null;
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
