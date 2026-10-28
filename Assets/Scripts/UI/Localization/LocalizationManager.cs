using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameLanguage
{
    Polish = 0,
    English = 1,
    French = 2,
    Spanish = 3,
    Italian = 4,
    German = 5,
    Russian = 6
}

public sealed class LocalizationManager : MonoBehaviour
{
    private const string LanguagePreferenceKey = "Moonloot.Language";
    private static LocalizationManager _instance;
    private static readonly Dictionary<string, string[]> Translations = BuildTranslations();

    private readonly Dictionary<TMP_Text, TextState> _textStates = new();

    public static LocalizationManager Instance => _instance;
    public GameLanguage CurrentLanguage { get; private set; }

    private sealed class TextState
    {
        public string Source;
        public string LastApplied;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        var managerObject = new GameObject(nameof(LocalizationManager));
        managerObject.AddComponent<LocalizationManager>();
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
        CurrentLanguage = (GameLanguage)Mathf.Clamp(
            PlayerPrefs.GetInt(LanguagePreferenceKey, (int)GameLanguage.Polish),
            (int)GameLanguage.Polish,
            (int)GameLanguage.Russian);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        ApplySceneTexts();
        ConfigureLanguageDropdowns(SceneManager.GetActiveScene());
        StartCoroutine(RefreshSceneTexts());
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        _instance = null;
    }

    public void SetLanguage(GameLanguage language)
    {
        if (CurrentLanguage == language)
        {
            ApplySceneTexts();
            return;
        }

        CurrentLanguage = language;
        PlayerPrefs.SetInt(LanguagePreferenceKey, (int)language);
        PlayerPrefs.Save();
        ApplySceneTexts();
    }

    public void SetLocalizedText(TMP_Text text, string source)
    {
        if (text == null)
            return;

        if (!_textStates.TryGetValue(text, out var state))
        {
            state = new TextState();
            _textStates.Add(text, state);
        }

        state.Source = source ?? string.Empty;
        state.LastApplied = Translate(state.Source);
        text.text = state.LastApplied;
    }

    public static string GetLanguageName(GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.English: return "English";
            case GameLanguage.French: return "Français";
            case GameLanguage.Spanish: return "Español";
            case GameLanguage.Italian: return "Italiano";
            case GameLanguage.German: return "Deutsch";
            case GameLanguage.Russian: return "Русский";
            default: return "Polski";
        }
    }

    public string Translate(string source)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        if (Translations.TryGetValue(source, out var values))
            return ValueForLanguage(values, source);

        var lines = source.Split('\n');
        if (lines.Length > 1)
        {
            for (var i = 0; i < lines.Length; i++)
                lines[i] = Translate(lines[i]);
            return string.Join("\n", lines);
        }

        // Quest labels include live counts and names, so translate their leading
        // action while retaining the item, enemy, map name and numeric progress.
        var progressStart = source.LastIndexOf(' ');
        var suffix = string.Empty;
        var actionText = source;
        if (progressStart >= 0 && source.Substring(progressStart + 1).Contains("/"))
        {
            suffix = source.Substring(progressStart);
            actionText = source.Substring(0, progressStart);
        }

        var dynamicResult = TranslateActionPrefix(actionText);
        return dynamicResult == null ? source : dynamicResult + suffix;
    }

    private string ValueForLanguage(string[] values, string fallback)
    {
        var translated = values[(int)CurrentLanguage];
        return string.IsNullOrEmpty(translated) ? fallback : translated;
    }

    private string TranslateActionPrefix(string text)
    {
        var prefixes = new[]
        {
            new[] { "Wróć do ", "Return to ", "Retournez voir ", "Vuelve con ", "Torna da ", "Kehre zu ", "Вернитесь к " },
            new[] { "Ukończ ", "Complete ", "Terminez ", "Completa ", "Completa ", "Schließe ", "Завершите " },
            new[] { "Znajdź ", "Find ", "Trouve ", "Encuentra ", "Trova ", "Finde ", "Найдите " },
            new[] { "Zabij ", "Kill ", "Tuez ", "Elimina ", "Uccidi ", "Töte ", "Убейте " },
            new[] { "Zbierz ", "Collect ", "Collectez ", "Recoge ", "Raccogli ", "Sammle ", "Соберите " },
            new[] { "Zbierz ", "Gather ", "Ramassez ", "Reúne ", "Raccogli ", "Sammle ", "Соберите " }
        };

        foreach (var translations in prefixes)
        {
            foreach (var prefix in translations)
            {
                if (text.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    return translations[(int)CurrentLanguage] + text.Substring(prefix.Length);
            }
        }

        return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _textStates.Clear();
        ApplySceneTexts();
        ConfigureLanguageDropdowns(scene);
    }

    private IEnumerator RefreshSceneTexts()
    {
        var wait = new WaitForSecondsRealtime(0.35f);
        while (true)
        {
            ApplySceneTexts();
            yield return wait;
        }
    }

    private void ApplySceneTexts()
    {
        var allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (var text in allTexts)
        {
            if (text == null || !text.gameObject.scene.IsValid() || IsInsideDropdown(text.transform))
                continue;

            if (!_textStates.TryGetValue(text, out var state))
            {
                state = new TextState { Source = text.text };
                _textStates.Add(text, state);
            }
            else if (text.text != state.LastApplied)
            {
                // UI scripts often replace labels after Start; remember that fresh source text.
                state.Source = text.text;
            }

            state.LastApplied = Translate(state.Source);
            if (text.text != state.LastApplied)
                text.text = state.LastApplied;
        }
    }

    private static bool IsInsideDropdown(Transform current)
    {
        while (current != null)
        {
            if (current.GetComponent<TMP_Dropdown>() != null)
                return true;
            current = current.parent;
        }
        return false;
    }

    private static void ConfigureLanguageDropdowns(Scene scene)
    {
        // Prefer the explicit component already serialized in the menu scenes.
        // It may be disabled in the scene, so find it with Resources rather than
        // relying on active hierarchy/name searches.
        var languageSelectors = Resources.FindObjectsOfTypeAll<LanguageDropdown>();
        foreach (var selector in languageSelectors)
        {
            if (selector == null || selector.gameObject.scene != scene)
                continue;

            selector.InitializeFromHierarchy();
        }

        var dropdowns = Resources.FindObjectsOfTypeAll<TMP_Dropdown>();
        foreach (var dropdown in dropdowns)
        {
            if (dropdown == null || dropdown.gameObject.scene != scene || !IsLanguageDropdown(dropdown.transform))
                continue;

            var languageDropdown = dropdown.GetComponent<LanguageDropdown>();
            if (languageDropdown != null)
                continue;

            languageDropdown = dropdown.gameObject.AddComponent<LanguageDropdown>();

            languageDropdown.Initialize(dropdown);
        }
    }

    private static bool IsLanguageDropdown(Transform current)
    {
        for (var depth = 0; current != null && depth < 5; depth++, current = current.parent)
        {
            if (current.name.ToLowerInvariant().Contains("language"))
                return true;
        }
        return false;
    }

    private static Dictionary<string, string[]> BuildTranslations()
    {
        var result = new Dictionary<string, string[]>(System.StringComparer.Ordinal);
        Add(result, "Graj", "Play", "Jouer", "Jugar", "Gioca", "Spielen", "Играть");
        Add(result, "Wczytaj", "Load", "Charger", "Cargar", "Carica", "Laden", "Загрузить");
        Add(result, "Opcje", "Options", "Options", "Opciones", "Opzioni", "Optionen", "Настройки");
        Add(result, "Twórcy", "Credits", "Crédits", "Créditos", "Crediti", "Mitwirkende", "Авторы");
        Add(result, "Wyjście", "Exit", "Quitter", "Salir", "Esci", "Beenden", "Выход");
        Add(result, "Wyjdź z gry", "Exit game", "Quitter le jeu", "Salir del juego", "Esci dal gioco", "Spiel beenden", "Выйти из игры");
        Add(result, "Gra autorstwa", "Game by", "Jeu créé par", "Un juego de", "Gioco di", "Spiel von", "Игра создана");
        Add(result, "Stworzenie i rozwój", "Created & Developed by", "Création et développement", "Creación y desarrollo", "Creazione e sviluppo", "Entwicklung", "Создание и разработка");
        Add(result, "Grafika", "Art", "Illustrations", "Arte", "Grafica", "Grafik", "Графика");
        Add(result, "Muzyka i dźwięk", "Music & Audio", "Musique et audio", "Música y sonido", "Musica e audio", "Musik und Audio", "Музыка и звук");
        Add(result, "Silnik", "Engine", "Moteur", "Motor", "Motore", "Engine", "Движок");
        Add(result, "Narzędzia", "Tools", "Outils", "Herramientas", "Strumenti", "Werkzeuge", "Инструменты");
        Add(result, "Specjalne podziękowania dla wszystkich wspierających ten projekt!", "Special thanks to everyone supporting this project!", "Un grand merci à toutes les personnes qui soutiennent ce projet !", "¡Gracias a todas las personas que apoyan este proyecto!", "Un ringraziamento speciale a chiunque sostenga questo progetto!", "Ein besonderer Dank an alle, die dieses Projekt unterstützen!", "Особая благодарность всем, кто поддерживает этот проект!");
        Add(result, "Specjalne podziękowania dla wszystkich wspierających ten projekt!", "Special thanks to all everyone supporting this project!", "Un grand merci à toutes les personnes qui soutiennent ce projet !", "¡Gracias a todas las personas que apoyan este proyecto!", "Un ringraziamento speciale a chiunque sostenga questo progetto!", "Ein besonderer Dank an alle, die dieses Projekt unterstützen!", "Особая благодарность всем, кто поддерживает этот проект!");
        Add(result, "Naciśnij ESC, aby wrócić do menu głównego", "Press ESC to get back to Main Menu", "Appuyez sur ÉCHAP pour revenir au menu principal", "Pulsa ESC para volver al menú principal", "Premi ESC per tornare al menu principale", "Drücke ESC, um zum Hauptmenü zurückzukehren", "Нажмите ESC, чтобы вернуться в главное меню");
        Add(result, "Powrót", "Return", "Retour", "Volver", "Indietro", "Zurück", "Назад");
        Add(result, "Wróć", "Back", "Retour", "Atrás", "Indietro", "Zurück", "Назад");
        Add(result, "Start", "Start", "Commencer", "Empezar", "Inizia", "Starten", "Начать");
        Add(result, "Kontynuuj", "Continue", "Continuer", "Continuar", "Continua", "Weiter", "Продолжить");
        Add(result, "Nowa gra", "New Game", "Nouvelle partie", "Nueva partida", "Nuova partita", "Neues Spiel", "Новая игра");
        Add(result, "Ustawienia", "Settings", "Paramètres", "Ajustes", "Impostazioni", "Einstellungen", "Настройки");
        Add(result, "Język", "Language", "Langue", "Idioma", "Lingua", "Sprache", "Язык");
        Add(result, "Dźwięk", "Sound", "Son", "Sonido", "Audio", "Ton", "Звук");
        Add(result, "Muzyka", "Music", "Musique", "Música", "Musica", "Musik", "Музыка");
        Add(result, "Efekty dźwiękowe", "Sound Effects", "Effets sonores", "Efectos de sonido", "Effetti sonori", "Soundeffekte", "Звуковые эффекты");
        Add(result, "Sterowanie", "Controls", "Commandes", "Controles", "Comandi", "Steuerung", "Управление");
        Add(result, "Klawiatura", "Keyboard", "Clavier", "Teclado", "Tastiera", "Tastatur", "Клавиатура");
        Add(result, "Mysz", "Mouse", "Souris", "Ratón", "Mouse", "Maus", "Мышь");
        Add(result, "Zapisano", "Saved", "Enregistré", "Guardado", "Salvato", "Gespeichert", "Сохранено");
        Add(result, "Anuluj", "Cancel", "Annuler", "Cancelar", "Annulla", "Abbrechen", "Отмена");
        Add(result, "Potwierdź", "Confirm", "Confirmer", "Confirmar", "Conferma", "Bestätigen", "Подтвердить");
        Add(result, "Tak", "Yes", "Oui", "Sí", "Sì", "Ja", "Да");
        Add(result, "Nie", "No", "Non", "No", "No", "Nein", "Нет");
        Add(result, "Ekwipunek", "Inventory", "Inventaire", "Inventario", "Inventario", "Inventar", "Инвентарь");
        Add(result, "Zadania", "Quests", "Quêtes", "Misiones", "Missioni", "Aufgaben", "Задания");
        Add(result, "Dziennik zadań", "Quest Journal", "Journal des quêtes", "Diario de misiones", "Diario delle missioni", "Questtagebuch", "Журнал заданий");
        Add(result, "Zakończono", "Completed", "Terminé", "Completado", "Completato", "Abgeschlossen", "Выполнено");
        Add(result, "Nieukończone", "Incomplete", "Inachevé", "Incompleto", "Incompleto", "Unvollständig", "Не выполнено");
        Add(result, "Wróć do zleceniodawcy", "Return to the quest giver", "Retournez voir le donneur de quête", "Vuelve con quien te dio la misión", "Torna da chi ti ha affidato la missione", "Kehre zum Auftraggeber zurück", "Вернитесь к заказчику");
        Add(result, "Złoto", "Gold", "Or", "Oro", "Oro", "Gold", "Золото");
        Add(result, "Zdrowie", "Health", "Santé", "Salud", "Salute", "Gesundheit", "Здоровье");
        Add(result, "Mana", "Mana", "Mana", "Maná", "Mana", "Mana", "Мана");
        Add(result, "Obrażenia", "Damage", "Dégâts", "Daño", "Danni", "Schaden", "Урон");
        Add(result, "Szybkość", "Speed", "Vitesse", "Velocidad", "Velocità", "Geschwindigkeit", "Скорость");
        Add(result, "Atak", "Attack", "Attaque", "Ataque", "Attacco", "Angriff", "Атака");
        Add(result, "Obrona", "Defense", "Défense", "Defensa", "Difesa", "Verteidigung", "Защита");
        Add(result, "Kup", "Buy", "Acheter", "Comprar", "Compra", "Kaufen", "Купить");
        Add(result, "Sprzedaj", "Sell", "Vendre", "Vender", "Vendi", "Verkaufen", "Продать");
        Add(result, "Wytwórz", "Craft", "Fabriquer", "Fabricar", "Crea", "Herstellen", "Создать");
        Add(result, "Ulepsz", "Upgrade", "Améliorer", "Mejorar", "Potenzia", "Verbessern", "Улучшить");
        Add(result, "Brak miejsca", "Not enough space", "Pas assez de place", "No hay espacio suficiente", "Spazio insufficiente", "Nicht genug Platz", "Недостаточно места");
        Add(result, "Brak złota", "Not enough gold", "Pas assez d'or", "No tienes suficiente oro", "Oro insufficiente", "Nicht genug Gold", "Недостаточно золота");
        Add(result, "Naciśnij, aby kontynuować", "Press to continue", "Appuyez pour continuer", "Pulsa para continuar", "Premi per continuare", "Zum Fortfahren drücken", "Нажмите, чтобы продолжить");
        Add(result, "Naciśnij, aby interaktować", "Press to interact", "Appuyez pour interagir", "Pulsa para interactuar", "Premi per interagire", "Zum Interagieren drücken", "Нажмите для взаимодействия");
        Add(result, "Wybierz bohatera", "Choose a hero", "Choisissez un héros", "Elige un héroe", "Scegli un eroe", "Wähle einen Helden", "Выберите героя");
        Add(result, "Powrót do menu głównego", "Go to main menu", "Retour au menu principal", "Volver al menú principal", "Torna al menu principale", "Zurück zum Hauptmenü", "Вернуться в главное меню");
        Add(result, "Opcja A", "Option A", "Option A", "Opción A", "Opzione A", "Option A", "Вариант A");
        Add(result, "Nowy tekst", "New Text", "Nouveau texte", "Texto nuevo", "Nuovo testo", "Neuer Text", "Новый текст");
        Add(result, "Draven, Barbarzyńca", "Draven, the Barbarian", "Draven, le Barbare", "Draven, el Bárbaro", "Draven, il Barbaro", "Draven, der Barbar", "Дрейвен, Варвар");
        Add(result, "Viggo, Bard", "Viggo, the Bard", "Viggo, le Barde", "Viggo, el Bardo", "Viggo, il Bardo", "Viggo, der Barde", "Вигго, Бард");
        Add(result, "Jimbo, Rekrut", "Jimbo, the Recruit", "Jimbo, la Recrue", "Jimbo, el Recluta", "Jimbo, la Recluta", "Jimbo, der Rekrut", "Джимбо, Новобранец");
        Add(result, "Yersantil, Łowca", "Yersantil, the Ranger", "Yersantil, le Rôdeur", "Yersantil, el Explorador", "Yersantil, il Ranger", "Yersantil, der Waldläufer", "Ерсантиль, Следопыт");
        Add(result, "Duży zasięg i wysoka wytrzymałość, ale niska szybkość.", "Long reach, very tough, but slow.", "Très longue portée et très résistant, mais lent.", "Gran alcance y mucha resistencia, pero lento.", "Lunga portata e molta resistenza, ma lento.", "Große Reichweite und sehr robust, aber langsam.", "Большая дальность и высокая прочность, но низкая скорость.");
        Add(result, "Pięknie śpiewa.", "Sings beautifully.", "Il chante à merveille.", "Canta de maravilla.", "Canta magnificamente.", "Er singt wunderschön.", "Прекрасно поёт.");
        Add(result, "Gość ubrany w cieliste ubrania.", "The guy in the flesh-colored clothes.", "Le type vêtu de vêtements couleur chair.", "El tipo vestido con ropa color carne.", "Il tipo vestito di color carne.", "Der Typ in hautfarbener Kleidung.", "Парень в одежде телесного цвета.");
        Add(result, "Świetny strzelec.", "Great shooter.", "Excellent tireur.", "Gran tirador.", "Ottimo tiratore.", "Ein hervorragender Schütze.", "Отличный стрелок.");
        Add(result, "Gość ubrany w cieliste ubrania.", "\rThe guy in the flesh-colored clothes.", "Le type vêtu de vêtements couleur chair.", "El tipo vestido con ropa color carne.", "Il tipo vestito di color carne.", "Der Typ in hautfarbener Kleidung.", "Парень в одежде телесного цвета.");
        Add(result, "Wybierz mapę", "Choose a map", "Choisissez une carte", "Elige un mapa", "Scegli una mappa", "Wähle eine Karte", "Выберите карту");
        Add(result, "Nieodblokowane", "Locked", "Verrouillé", "Bloqueado", "Bloccato", "Gesperrt", "Заблокировано");
        Add(result, "Odblokowane", "Unlocked", "Débloqué", "Desbloqueado", "Sbloccato", "Freigeschaltet", "Разблокировано");
        return result;
    }

    private static void Add(Dictionary<string, string[]> map, params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrEmpty(value))
                map[value] = values;
        }
    }
}
