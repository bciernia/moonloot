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
    Russian = 6,
    PortugueseBrazilian = 7
}

public sealed class LocalizationManager : MonoBehaviour
{
    private const string LanguagePreferenceKey = "Moonloot.Language";
    private static LocalizationManager _instance;
    private static readonly Dictionary<string, string[]> Translations = BuildTranslations();
    private static readonly Dictionary<string, string[]> TutorialTranslations = BuildTutorialTranslations();

    private readonly Dictionary<TMP_Text, TextState> _textStates = new();

    public static LocalizationManager Instance => _instance;
    public GameLanguage CurrentLanguage { get; private set; }

    private sealed class TextState
    {
        public string Source;
        public string LastApplied;
        public string[] LanguageOverrides;
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
            (int)GameLanguage.PortugueseBrazilian);
        SyncEasyTalkLanguage();
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
            SyncEasyTalkLanguage();
            ApplySceneTexts();
            return;
        }

        CurrentLanguage = language;
        SyncEasyTalkLanguage();
        PlayerPrefs.SetInt(LanguagePreferenceKey, (int)language);
        PlayerPrefs.Save();
        ApplySceneTexts();
    }

    private void SyncEasyTalkLanguage()
    {
        var languageCode = CurrentLanguage switch
        {
            GameLanguage.Polish => "pl",
            GameLanguage.English => "en",
            GameLanguage.French => "fr",
            GameLanguage.Spanish => "es",
            GameLanguage.Italian => "it",
            GameLanguage.German => "de",
            GameLanguage.Russian => "ru",
            GameLanguage.PortugueseBrazilian => "pt-BR",
            _ => "en"
        };

        // Use the overriding setter so EasyTalk does not reset the selected game
        // language to a dialogue asset's original language when playback starts.
        EasyTalk.Controller.EasyTalkGameState.Instance.Language = languageCode;
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
        state.LanguageOverrides = null;
        state.LastApplied = Translate(state.Source);
        text.text = state.LastApplied;
    }

    public void SetLocalizedText(TMP_Text text, string source, string[] languageOverrides)
    {
        if (text == null)
            return;

        if (!_textStates.TryGetValue(text, out var state))
        {
            state = new TextState();
            _textStates.Add(text, state);
        }

        state.Source = source ?? string.Empty;
        state.LanguageOverrides = languageOverrides;
        state.LastApplied = GetLocalizedStateText(state);
        text.text = state.LastApplied;
    }

    public void SetTutorialLocalizedText(TMP_Text text, string key, string fallback)
    {
        if (TutorialTranslations.TryGetValue(key ?? string.Empty, out var values))
            SetLocalizedText(text, fallback, values);
        else
            SetLocalizedText(text, fallback);
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
            case GameLanguage.PortugueseBrazilian: return "Português (Brasil)";
            default: return "Polski";
        }
    }

    public string Translate(string source)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        if (Translations.TryGetValue(source, out var values))
            return ValueForLanguage(values, source);

        // ScriptableObject text fields may contain line breaks or repeated spaces
        // introduced by the Inspector/YAML serialization. Treat those as equivalent.
        var normalizedSource = NormalizeTranslationKey(source);
        if (!string.Equals(normalizedSource, source, System.StringComparison.Ordinal) &&
            Translations.TryGetValue(normalizedSource, out values))
            return ValueForLanguage(values, normalizedSource);

        var progressLabelSeparator = source.LastIndexOf(": ", System.StringComparison.Ordinal);
        if (progressLabelSeparator >= 0)
        {
            var label = source.Substring(0, progressLabelSeparator);
            var progress = source.Substring(progressLabelSeparator + 2);
            if (Translations.TryGetValue(label, out values))
                return ValueForLanguage(values, label) + ": " + progress;
        }

        // Bonus rows contain a changing number followed by a stable stat label,
        // for example "+15% Move Speed". Keep the value and translate the label.
        if (source.StartsWith("+", System.StringComparison.Ordinal))
        {
            var labelStart = source.IndexOf(' ');
            if (labelStart >= 0)
            {
                var value = source.Substring(0, labelStart);
                var label = source.Substring(labelStart + 1);
                if (Translations.TryGetValue(label, out values))
                    return value + " " + ValueForLanguage(values, label);
            }
        }

        var savedPrefixes = new[]
        {
            new[] { "Uratowano ", "Saved ", "Sauvé : ", "Salvado: ", "Salvato: ", "Gerettet: ", "Спасён: " }
        };
        foreach (var prefixSet in savedPrefixes)
        {
            foreach (var prefix in prefixSet)
            {
                if (source.StartsWith(prefix, System.StringComparison.Ordinal))
                    return prefixSet[(int)CurrentLanguage] + source.Substring(prefix.Length);
            }
        }

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
        var index = (int)CurrentLanguage;
        var translated = index < values.Length ? values[index] : string.Empty;
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

            state.LastApplied = GetLocalizedStateText(state);
            if (text.text != state.LastApplied)
                text.text = state.LastApplied;
        }
    }

    private string GetLocalizedStateText(TextState state)
    {
        if (state.LanguageOverrides != null && (int)CurrentLanguage < state.LanguageOverrides.Length)
        {
            var localized = state.LanguageOverrides[(int)CurrentLanguage];
            if (!string.IsNullOrWhiteSpace(localized))
                return localized;
        }

        return Translate(state.Source);
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
        Add(result, "KUP ({0} kryształów Aulli)", "BUY ({0} Aulla Crystals)", "ACHETER ({0} cristaux d’Aulla)", "COMPRAR ({0} cristales de Aulla)", "ACQUISTA ({0} cristalli di Aulla)", "KAUFEN ({0} Aulla-Kristalle)", "КУПИТЬ ({0} кристаллов Ауллы)");
        Add(result, "Nie masz wystarczającej liczby kryształów Aulli.", "You don't have enough Aulla Crystals.", "Vous n’avez pas assez de cristaux d’Aulla.", "No tienes suficientes cristales de Aulla.", "Non hai abbastanza cristalli di Aulla.", "Du hast nicht genug Aulla-Kristalle.", "Недостаточно кристаллов Ауллы.");
        Add(result, "Odblokowano umiejętność", "Skill unlocked", "Compétence débloquée", "Habilidad desbloqueada", "Abilità sbloccata", "Fertigkeit freigeschaltet", "Навык разблокирован");
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
        Add(result, "Rozumiem", "Got it", "Compris", "Entendido", "Capito", "Verstanden", "Понятно");
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
        Add(result, "Statystyki", "Statistics", "Statistiques", "Estadísticas", "Statistiche", "Statistiken", "Характеристики");
        Add(result, "Odporność fizyczna", "Damage resistance", "Résistance physique", "Resistencia física", "Resistenza fisica", "Physischer Widerstand", "Физическое сопротивление");
        Add(result, "Szybkość ruchu", "Move speed", "Vitesse de déplacement", "Velocidad de movimiento", "Velocità di movimento", "Bewegungsgeschwindigkeit", "Скорость передвижения");
        Add(result, "Redukcja czasu odnowienia", "Cooldown reduction", "Réduction du temps de recharge", "Reducción del tiempo de recarga", "Riduzione del tempo di recupero", "Abklingzeitverringerung", "Сокращение перезарядки");
        Add(result, "Redukcja obrażeń", "Damage reduction", "Réduction des dégâts", "Reducción de daño", "Riduzione dei danni", "Schadensreduktion", "Снижение урона");
        Add(result, "Mnożnik trafienia krytycznego", "Critical attack multiplier", "Multiplicateur de coup critique", "Multiplicador de golpe crítico", "Moltiplicatore del colpo critico", "Kritischer Schadensmultiplikator", "Множитель критического удара");
        Add(result, "Szansa na trafienie krytyczne", "Critical attack chance", "Chance de coup critique", "Probabilidad de golpe crítico", "Probabilità di colpo critico", "Kritische Trefferchance", "Шанс критического удара");
        Add(result, "Szybkość", "Speed", "Vitesse", "Velocidad", "Velocità", "Geschwindigkeit", "Скорость");
        Add(result, "Atak", "Attack", "Attaque", "Ataque", "Attacco", "Angriff", "Атака");
        Add(result, "Obrona", "Defense", "Défense", "Defensa", "Difesa", "Verteidigung", "Защита");
        Add(result, "Nagrody", "Rewards", "Récompenses", "Recompensas", "Ricompense", "Belohnungen", "Награды");
        Add(result, "Wyniki nocy", "Night results", "Résultats de la nuit", "Resultados de la noche", "Risultati della notte", "Nachtergebnisse", "Итоги ночи");
        Add(result, "Summary", "Night summary", "Bilan de la nuit", "Resumen de la noche", "Riepilogo della notte", "Nachtübersicht", "Итоги ночи");
        Add(result, "Wróć do wioski", "Return to village", "Retourner au village", "Volver a la aldea", "Torna al villaggio", "Zurück ins Dorf", "Вернуться в деревню");
        Add(result, "Noc przetrwana!", "Night survived!", "Nuit survécue !", "¡Noche superada!", "Notte superata!", "Nacht überlebt!", "Ночь пережита!");
        Add(result, "Zadane obrażenia", "Dealt damage", "Dégâts infligés", "Daño infligido", "Danni inflitti", "Verursachter Schaden", "Нанесённый урон");
        Add(result, "Zebrane złoto", "Earned gold", "Or gagné", "Oro obtenido", "Oro ottenuto", "Erhaltenes Gold", "Получено золота");
        Add(result, "Pokonani zwykli przeciwnicy", "Defeated minions", "Ennemis vaincus", "Enemigos derrotados", "Nemici sconfitti", "Besiegte Gegner", "Побеждено обычных врагов");
        Add(result, "Pokonani elitarni przeciwnicy", "Defeated elites", "Élites vaincues", "Élites derrotadas", "Élite sconfitti", "Besiegte Elitegegner", "Побеждено элитных врагов");
        Add(result, "Pokonani bossowie", "Defeated bosses", "Boss vaincus", "Jefes derrotados", "Boss sconfitti", "Besiegte Bosse", "Побеждено боссов");
        Add(result, "Przebyty dystans", "Distance traveled", "Distance parcourue", "Distancia recorrida", "Distanza percorsa", "Zurückgelegte Strecke", "Пройденное расстояние");
        Add(result, "Nie udało ci się znaleźć wieśniaka.", "You did not find the peasant.", "Tu n'as pas trouvé le paysan.", "No encontraste al campesino.", "Non hai trovato il contadino.", "Du hast den Bauern nicht gefunden.", "Ты не нашёл крестьянина.");
        Add(result, "Uratowano nowego wieśniaka!", "New villager saved!", "Un nouvel habitant a été sauvé !", "¡Has salvado a un nuevo aldeano!", "Hai salvato un nuovo abitante!", "Ein neuer Dorfbewohner wurde gerettet!", "Спасён новый житель!");
        Add(result, "Uratowano ", "Saved ", "Sauvé : ", "Salvado: ", "Salvato: ", "Gerettet: ", "Спасён: ");
        Add(result, "Move Speed", "Szybkość ruchu", "Vitesse de déplacement", "Velocidad de movimiento", "Velocità di movimento", "Bewegungsgeschwindigkeit", "Скорость передвижения");
        Add(result, "Crit Chance", "Szansa na trafienie krytyczne", "Chance de coup critique", "Probabilidad de crítico", "Probabilità di critico", "Kritische Trefferchance", "Шанс критического удара");
        Add(result, "Crit Multiplier", "Mnożnik trafienia krytycznego", "Multiplicateur critique", "Multiplicador crítico", "Moltiplicatore critico", "Kritischer Multiplikator", "Критический множитель");
        Add(result, "Damage", "Obrażenia", "Dégâts", "Daño", "Danni", "Schaden", "Урон");
        Add(result, "Physical Resistance", "Odporność fizyczna", "Résistance physique", "Resistencia física", "Resistenza fisica", "Physischer Widerstand", "Физическое сопротивление");
        Add(result, "Critical Damage", "Mnożnik trafienia krytycznego", "Dégâts critiques", "Daño crítico", "Danno critico", "Kritischer Schaden", "Критический урон");
        Add(result, "Max HP", "Maks. zdrowie", "PV max", "Salud máx.", "PS max", "Max. LP", "Макс. здоровье");
        Add(result, "Max Mana", "Maks. mana", "Mana max", "Maná máx.", "Mana max", "Max. Mana", "Макс. мана");
        Add(result, "Attack Speed", "Szybkość ataku", "Vitesse d'attaque", "Velocidad de ataque", "Velocità d'attacco", "Angriffsgeschwindigkeit", "Скорость атаки");
        Add(result, "Health Regeneration", "Regeneracja zdrowia", "Régénération de santé", "Regeneración de salud", "Rigenerazione della salute", "Gesundheitsregeneration", "Регенерация здоровья");
        Add(result, "HP/s", "HP/s", "PV/s", "PS/s", "PS/s", "LP/s", "HP/с");
        // Player statistics and level-up cards. Values are ordered by GameLanguage.
        Add(result, "Obrażenia", "Damage", "Dégâts", "Daño", "Danni", "Schaden", "Урон", "Dano");
        Add(result, "Odporność fizyczna", "Damage resistance", "Résistance physique", "Resistencia física", "Resistenza fisica", "Physischer Widerstand", "Физическое сопротивление", "Resistência física");
        Add(result, "Szybkość ruchu", "Move speed", "Vitesse de déplacement", "Velocidad de movimiento", "Velocità di movimento", "Bewegungsgeschwindigkeit", "Скорость передвижения", "Velocidade de movimento");
        Add(result, "Redukcja czasu między atakami", "Cooldown reduction", "Réduction du délai d'attaque", "Reducción del tiempo entre ataques", "Riduzione del tempo tra gli attacchi", "Angriffsabklingzeitverringerung", "Сокращение времени между атаками", "Redução do intervalo entre ataques");
        Add(result, "Redukcja obrażeń", "Damage reduction", "Réduction des dégâts", "Reducción de daño", "Riduzione dei danni", "Schadensreduktion", "Снижение урона", "Redução de dano");
        Add(result, "Mnożnik trafienia krytycznego", "Critical attack multiplier", "Multiplicateur de coup critique", "Multiplicador de golpe crítico", "Moltiplicatore del colpo critico", "Kritischer Schadensmultiplikator", "Множитель критического удара", "Multiplicador de acerto crítico");
        Add(result, "Szansa na trafienie krytyczne", "Critical attack chance", "Chance de coup critique", "Probabilidad de golpe crítico", "Probabilità di colpo critico", "Kritische Trefferchance", "Шанс критического удара", "Chance de acerto crítico");
        Add(result, "Regeneracja zdrowia", "HP regeneration", "Régénération de santé", "Regeneración de salud", "Rigenerazione della salute", "Gesundheitsregeneration", "Регенерация здоровья", "Regeneração de vida");
        Add(result, "Szansa na trafienie krytyczne", "Crit chance", "Chance de coup critique", "Probabilidad de golpe crítico", "Probabilità di colpo critico", "Kritische Trefferchance", "Шанс критического удара", "Chance de acerto crítico");
        Add(result, "Mnożnik trafienia krytycznego", "Crit Multiplier", "Multiplicateur critique", "Multiplicador crítico", "Moltiplicatore critico", "Kritischer Multiplikator", "Критический множитель", "Multiplicador crítico");
        Add(result, "Zdrowie", "Hp", "Vie", "Salud", "Salute", "Leben", "Здоровье", "Vida");
        Add(result, "Regeneracja HP/s", "HP/s Regeneration", "Régénération PV/s", "Regeneración PS/s", "Rigenerazione PS/s", "LP/s-Regeneration", "Регенерация HP/с", "Regeneração de vida/s");
        Add(result, "Zwiększa zadawane obrażenia.", "Increase your damage.", "Augmente les dégâts infligés.", "Aumenta el daño infligido.", "Aumenta i danni inflitti.", "Erhöht deinen verursachten Schaden.", "Увеличивает наносимый урон.", "Aumenta o dano causado.");
        Add(result, "Poruszasz się szybciej po polu walki.", "Move faster across the battlefield.", "Déplace-toi plus vite sur le champ de bataille.", "Muévete más rápido por el campo de batalla.", "Muoviti più velocemente sul campo di battaglia.", "Bewege dich schneller über das Schlachtfeld.", "Передвигайся быстрее по полю боя.", "Mova-se mais rápido pelo campo de batalha.");
        Add(result, "Zwiększa szansę na trafienie krytyczne.", "Increase your chance to land a critical hit.", "Augmente les chances de coup critique.", "Aumenta la probabilidad de golpe crítico.", "Aumenta la probabilità di colpo critico.", "Erhöht deine Chance auf einen kritischen Treffer.", "Повышает шанс критического удара.", "Aumenta a chance de acerto crítico.");
        Add(result, "Trafienia krytyczne zadają większe obrażenia.", "Deal more damage with critical hits.", "Les coups critiques infligent plus de dégâts.", "Los golpes críticos infligen más daño.", "I colpi critici infliggono più danni.", "Kritische Treffer verursachen mehr Schaden.", "Критические удары наносят больше урона.", "Acertos críticos causam mais dano.");
        Add(result, "Zwiększa maksymalne zdrowie.", "Increase your maximum health.", "Augmente la santé maximale.", "Aumenta la salud máxima.", "Aumenta la salute massima.", "Erhöht deine maximale Gesundheit.", "Увеличивает максимальное здоровье.", "Aumenta a vida máxima.");
        Add(result, "Zwiększa maksymalną manę.", "Increase your maximum mana.", "Augmente le mana maximal.", "Aumenta el maná máximo.", "Aumenta il mana massimo.", "Erhöht dein maximales Mana.", "Увеличивает максимальный запас маны.", "Aumenta a mana máxima.");
        Add(result, "Skraca czas między atakami.", "Reduce the time between attacks.", "Réduit le délai entre les attaques.", "Reduce el tiempo entre ataques.", "Riduce il tempo tra gli attacchi.", "Verkürzt die Zeit zwischen Angriffen.", "Сокращает время между атаками.", "Reduz o intervalo entre ataques.");
        Add(result, "Zmniejsza otrzymywane obrażenia.", "Reduce the damage you receive.", "Réduit les dégâts subis.", "Reduce el daño recibido.", "Riduce i danni subiti.", "Verringert den erlittenen Schaden.", "Уменьшает получаемый урон.", "Reduz o dano recebido.");
        Add(result, "Przywraca zdrowie z czasem.", "Restore health over time.", "Restaure progressivement la santé.", "Recupera salud con el tiempo.", "Ripristina salute nel tempo.", "Stellt mit der Zeit Gesundheit wieder her.", "Постепенно восстанавливает здоровье.", "Recupera vida com o tempo.");
        Add(result, "Odnawiasz dodatkowe 0,1 HP na sekundę. Regeneracja jest ograniczona do 1 HP/s.", "Regenerate an additional 0.1 HP every second. Total regeneration is capped at 1 HP/s.", "Régénère 0,1 PV supplémentaire par seconde. La régénération est limitée à 1 PV/s.", "Regeneras 0,1 PS adicionales por segundo. La regeneración está limitada a 1 PS/s.", "Rigeneri 0,1 PS aggiuntivi al secondo. La rigenerazione è limitata a 1 PS/s.", "Regeneriere zusätzlich 0,1 LP pro Sekunde. Die Regeneration ist auf 1 LP/s begrenzt.", "Дополнительно восстанавливает 0,1 HP в секунду. Предел регенерации — 1 HP/с.", "Regenera mais 0,1 de vida por segundo. A regeneração é limitada a 1 de vida/s.");
        Add(result, "Damage Reduction", "Redukcja obrażeń", "Réduction des dégâts", "Reducción de daño", "Riduzione dei danni", "Schadensreduktion", "Снижение урона");
        Add(result, "Kup", "Buy", "Acheter", "Comprar", "Compra", "Kaufen", "Купить");
        Add(result, "Sprzedaj", "Sell", "Vendre", "Vender", "Vendi", "Verkaufen", "Продать");
        Add(result, "Cena zakupu", "Buy price", "Prix d’achat", "Precio de compra", "Prezzo d’acquisto", "Kaufpreis", "Цена покупки", "Preço de compra");
        Add(result, "Cena sprzedaży", "Sell price", "Prix de vente", "Precio de venta", "Prezzo di vendita", "Verkaufspreis", "Цена продажи", "Preço de venda");
        Add(result, "Wytwórz", "Craft", "Fabriquer", "Fabricar", "Crea", "Herstellen", "Создать");
        Add(result, "Ulepsz", "Upgrade", "Améliorer", "Mejorar", "Potenzia", "Verbessern", "Улучшить");
        Add(result, "Brak miejsca", "Not enough space", "Pas assez de place", "No hay espacio suficiente", "Spazio insufficiente", "Nicht genug Platz", "Недостаточно места");
        Add(result, "Brak złota", "Not enough gold", "Pas assez d'or", "No tienes suficiente oro", "Oro insufficiente", "Nicht genug Gold", "Недостаточно золота");
        Add(result, "Naciśnij, aby kontynuować", "Press to continue", "Appuyez pour continuer", "Pulsa para continuar", "Premi per continuare", "Zum Fortfahren drücken", "Нажмите, чтобы продолжить");
        Add(result, "Naciśnij, aby interaktować", "Press to interact", "Appuyez pour interagir", "Pulsa para interactuar", "Premi per interagire", "Zum Interagieren drücken", "Нажмите для взаимодействия");
        Add(result, "Wybierz bohatera", "Choose a hero", "Choisissez un héros", "Elige un héroe", "Scegli un eroe", "Wähle einen Helden", "Выберите героя");
        Add(result, "Wiedza Aulli", "Aulla's Knowledge", "Le savoir d'Aulla", "El conocimiento de Aulla", "La conoscenza di Aulla", "Aullas Wissen", "Знания Ауллы");
        Add(result, "Znajdź portal", "Find the portal", "Trouve le portail", "Encuentra el portal", "Trova il portale", "Finde das Portal", "Найди портал");
        Add(result, "Biegłeś nocą przez las w poszukiwaniu schronienia, niestety potwory zdążyły cię dogonić. Znajdź kryształy Aulli, żeby dowiedzieć się, gdzie jest najbliższa wioska.", "You ran through the forest at night looking for shelter, but the monsters caught up with you. Find Aulla's crystals to learn where the nearest village is.", "Tu as couru dans la forêt pendant la nuit à la recherche d'un abri, mais les monstres t'ont rattrapé. Trouve les cristaux d'Aulla pour découvrir où se trouve le village le plus proche.", "Corriste por el bosque de noche buscando refugio, pero los monstruos te alcanzaron. Encuentra los cristales de Aulla para averiguar dónde está la aldea más cercana.", "Hai corso nella foresta di notte in cerca di riparo, ma i mostri ti hanno raggiunto. Trova i cristalli di Aulla per scoprire dove si trova il villaggio più vicino.", "Du bist nachts auf der Suche nach einem Unterschlupf durch den Wald gerannt, doch die Monster haben dich eingeholt. Finde Aullas Kristalle, um herauszufinden, wo das nächste Dorf liegt.", "Ночью ты бежал через лес в поисках укрытия, но монстры настигли тебя. Найди кристаллы Ауллы, чтобы узнать, где находится ближайшая деревня.");
        Add(result, "Znajdź trzy kryształy Aulli, aby otworzyć portal.", "Find three Aulla crystals to open the portal.", "Trouve trois cristaux d'Aulla pour ouvrir le portail.", "Encuentra tres cristales de Aulla para abrir el portal.", "Trova tre cristalli di Aulla per aprire il portale.", "Finde drei Aulla-Kristalle, um das Portal zu öffnen.", "Найди три кристалла Ауллы, чтобы открыть портал.");
        Add(result, "Znajdź trzy niebieskie kryształy Aulli. Niebieskie kropki na mapie pokazują, gdzie ich szukać.", "Find three blue Aulla crystals. Blue dots on the map show where to look.", "Trouve trois cristaux bleus d’Aulla. Les points bleus sur la carte indiquent où chercher.", "Encuentra tres cristales azules de Aulla. Los puntos azules del mapa indican dónde buscarlos.", "Trova tre cristalli blu di Aulla. I punti blu sulla mappa indicano dove cercarli.", "Finde drei blaue Aulla-Kristalle. Blaue Punkte auf der Karte zeigen ihre Fundorte.", "Найди три синих кристалла Ауллы. Синие точки на карте показывают, где их искать.");
        Add(result, "Czerwone kropki oznaczają przeciwników. Po zebraniu wszystkich trzech kryształów pojawi się portal. Wejdź do niego, aby wrócić do wioski.", "Red dots mark enemies. Once you collect all three crystals, a portal will appear. Enter it to return to the village.", "Les points rouges indiquent les ennemis. Lorsque tu auras récupéré les trois cristaux, un portail apparaîtra. Traverse-le pour retourner au village.", "Los puntos rojos indican enemigos. Cuando recojas los tres cristales, aparecerá un portal. Entra en él para volver a la aldea.", "I punti rossi indicano i nemici. Quando avrai raccolto tutti e tre i cristalli, apparirà un portale. Entraci per tornare al villaggio.", "Rote Punkte markieren Gegner. Sobald du alle drei Kristalle gesammelt hast, erscheint ein Portal. Betritt es, um ins Dorf zurückzukehren.", "Красные точки обозначают врагов. Когда соберёшь все три кристалла, появится портал. Войди в него, чтобы вернуться в деревню.");
        Add(result, "Uważaj: im dłużej pozostajesz na mapie, tym więcej potworów przybywa i tym silniejsze się stają. Dłuższy pobyt pozwala zebrać więcej łupów przydatnych w dalszej przygodzie.", "Be careful: the longer you stay on the map, the more monsters arrive and the stronger they become. Staying longer lets you gather more loot for the rest of your adventure.", "Attention : plus tu restes longtemps sur la carte, plus les monstres arrivent nombreux et deviennent puissants. En restant, tu peux récupérer davantage de butin pour la suite de l’aventure.", "Ten cuidado: cuanto más tiempo permanezcas en el mapa, más monstruos llegarán y más fuertes serán. Si te quedas, podrás reunir más botín para el resto de la aventura.", "Attenzione: più a lungo resti sulla mappa, più mostri arriveranno e più diventeranno forti. Restando più a lungo potrai raccogliere più bottino per il resto dell’avventura.", "Vorsicht: Je länger du auf der Karte bleibst, desto mehr Monster kommen und desto stärker werden sie. Bei längerem Aufenthalt kannst du mehr Beute für dein weiteres Abenteuer sammeln.", "Осторожно: чем дольше ты остаёшься на карте, тем больше и сильнее становится монстров. Зато за это время можно собрать больше добычи для дальнейшего приключения.");
        Add(result, "Poprzednia", "Previous", "Précédent", "Anterior", "Indietro", "Zurück", "Назад");
        Add(result, "Dalej", "Next", "Suivant", "Siguiente", "Avanti", "Weiter", "Далее");
        Add(result, "Wstecz", "Back", "Retour", "Atrás", "Indietro", "Zurück", "Назад");
        Add(result, "Następny", "Next", "Suivant", "Siguiente", "Successivo", "Weiter", "Далее");
        Add(result, "Zamknij", "Close", "Fermer", "Cerrar", "Chiudi", "Schließen", "Закрыть");
        Add(result, "Rozpocznij", "Begin", "Commencer", "Comenzar", "Inizia", "Beginnen", "Начать");
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
        AddItemTranslations(result);
        AddQuestTranslations(result);
        return result;
    }

    private static void AddItemTranslations(Dictionary<string, string[]> result)
    {
        // Foods and potions. Entries use the same language order as GameLanguage.
        Add(result, "Barszcz", "Beet soup", "Soupe de betterave", "Sopa de remolacha", "Zuppa di barbabietole", "Rote-Bete-Suppe", "Свекольный суп", "Sopa de beterraba");
        Add(result, "Zupa z buraków znacznie zwiększa maksymalne zdrowie na kolejną noc hordy.", "Beetroot soup significantly increases your HP for the next horde.", "La soupe de betterave augmente considérablement vos PV pour la prochaine horde.", "La sopa de remolacha aumenta considerablemente tus PV para la próxima horda.", "La zuppa di barbabietole aumenta notevolmente i tuoi PV per la prossima orda.", "Rote-Bete-Suppe erhöht deine LP für die nächste Horde deutlich.", "Свекольный суп значительно увеличивает здоровье перед следующей ордой.", "A sopa de beterraba aumenta bastante seus PV para a próxima horda.");
        Add(result, "Pieczone mięso", "Cooked meat", "Viande cuite", "Carne cocinada", "Carne cotta", "Gebratenes Fleisch", "Приготовленное мясо", "Carne cozida");
        Add(result, "Pieczony kurczak lub inne mięso. Zwiększa obrażenia o 7,5% i maksymalne zdrowie o 50 na następną noc.", "Cooked chicken or other meat. Increases damage by 7.5% and maximum HP by 50 for the next night.", "Du poulet ou une autre viande cuite. Augmente les dégâts de 7,5 % et les PV max. de 50 pour la prochaine nuit.", "Pollo cocinado u otra carne. Aumenta el daño un 7,5 % y los PV máximos en 50 durante la próxima noche.", "Pollo cotto o altra carne. Aumenta i danni del 7,5% e i PV massimi di 50 per la prossima notte.", "Gebratenes Hähnchen oder anderes Fleisch. Erhöht den Schaden um 7,5 % und die max. LP für die nächste Nacht um 50.", "Приготовленная курица или другое мясо. Увеличивает урон на 7,5% и максимум здоровья на 50 до следующей ночи.", "Frango ou outra carne cozida. Aumenta o dano em 7,5% e o máximo de PV em 50 durante a próxima noite.");
        Add(result, "Grillowany łosoś", "Grilled salmon", "Saumon grillé", "Salmón a la parrilla", "Salmone grigliato", "Gegrillter Lachs", "Жареный лосось", "Salmão grelhado");
        Add(result, "Grillowany łosoś smakuje lepiej, gdy pamiętasz, jak szybko uciekłeś przed niedźwiedziem. Zwiększa też szybkość ruchu.", "Grilled salmon tastes better when you remember how fast you ran from a bear. It also gives you movement speed.", "Le saumon grillé est encore meilleur quand on se souvient de sa fuite devant un ours. Il augmente aussi la vitesse de déplacement.", "El salmón a la parrilla sabe mejor al recordar lo rápido que huiste de un oso. También aumenta la velocidad de movimiento.", "Il salmone grigliato è più buono se ricordi quanto velocemente sei scappato da un orso. Aumenta anche la velocità di movimento.", "Gegrillter Lachs schmeckt besser, wenn du an deine Flucht vor dem Bären denkst. Er erhöht außerdem dein Bewegungstempo.", "Жареный лосось вкуснее, если вспомнить, как быстро ты убегал от медведя. Также повышает скорость передвижения.", "O salmão grelhado fica ainda melhor quando você lembra da corrida para escapar do urso. Também aumenta a velocidade de movimento.");
        Add(result, "Pizza", "Pizza", "Pizza", "Pizza", "Pizza", "Pizza", "Пицца", "Pizza");
        Add(result, "Mamma mia! Pyszna pizza bez ananasa, która zwiększa wszystkie twoje statystyki!", "Mamma mia! Delicious pizza without pineapple which increases all of your stats!", "Mamma mia ! Une délicieuse pizza sans ananas qui augmente toutes vos statistiques !", "¡Mamma mía! Una pizza deliciosa sin piña que aumenta todas tus estadísticas.", "Mamma mia! Una pizza deliziosa senza ananas che aumenta tutte le tue statistiche.", "Mamma mia! Eine köstliche Pizza ohne Ananas, die all deine Werte erhöht.", "Мамма миа! Вкусная пицца без ананаса, повышающая все характеристики.", "Mamma mia! Uma pizza deliciosa sem abacaxi que aumenta todos os seus atributos.");
        Add(result, "Zupa pomidorowa", "Tomato soup", "Soupe à la tomate", "Sopa de tomate", "Zuppa di pomodoro", "Tomatensuppe", "Томатный суп", "Sopa de tomate");
        Add(result, "Sprawia, że stajesz się twardy jak skała albo niedojrzały pomidor.", "Makes you hard as a rock or an unripe tomato.", "Vous rend dur comme la pierre ou comme une tomate pas mûre.", "Te deja duro como una roca o como un tomate verde.", "Ti rende duro come una roccia o come un pomodoro acerbo.", "Macht dich hart wie einen Felsen oder eine unreife Tomate.", "Делает тебя крепким, как скала или неспелый помидор.", "Deixa você duro como uma rocha ou um tomate verde.");
        Add(result, "Onigiri wędrowca", "Wayfarer's Onigiri", "Onigiri du voyageur", "Onigiri del viajero", "Onigiri del viandante", "Onigiri des Wanderers", "Онигири путника", "Onigiri do viajante");
        Add(result, "Przywraca 10 HP i zwiększa szybkość ruchu o 8% do końca bieżącego runu.", "Restores 10 HP and increases movement speed by 8% for the rest of the run.", "Rend 10 PV et augmente la vitesse de déplacement de 8 % jusqu’à la fin de la partie.", "Restaura 10 PV y aumenta la velocidad de movimiento un 8 % durante el resto de la partida.", "Ripristina 10 PV e aumenta la velocità di movimento dell’8% per il resto della partita.", "Stellt 10 LP wieder her und erhöht das Bewegungstempo für den Rest des Durchlaufs um 8 %.", "Восстанавливает 10 HP и повышает скорость передвижения на 8% до конца забега.", "Recupera 10 PV e aumenta a velocidade de movimento em 8% até o fim da partida.");
        Add(result, "Łosoś", "Salmon", "Saumon", "Salmón", "Salmone", "Lachs", "Лосось", "Salmão");
        Add(result, "Łosoś smakuje lepiej, gdy pamiętasz, jak szybko uciekłeś przed niedźwiedziem. Zwiększa też szybkość ruchu.", "Salmon tastes better when you remember how fast you ran from a bear. It also gives you movement speed.", "Le saumon est encore meilleur quand on se souvient de sa fuite devant un ours. Il augmente aussi la vitesse de déplacement.", "El salmón sabe mejor al recordar lo rápido que huiste de un oso. También aumenta la velocidad de movimiento.", "Il salmone è più buono se ricordi quanto velocemente sei scappato da un orso. Aumenta anche la velocità di movimento.", "Lachs schmeckt besser, wenn du an deine Flucht vor dem Bären denkst. Er erhöht außerdem dein Bewegungstempo.", "Лосось вкуснее, если вспомнить, как быстро ты убегал от медведя. Также повышает скорость передвижения.", "O salmão fica ainda melhor quando você lembra da corrida para escapar do urso. Também aumenta a velocidade de movimento.");
        Add(result, "Burak", "Beetroot", "Betterave", "Remolacha", "Barbabietola", "Rote Bete", "Свёкла", "Beterraba");
        Add(result, "Kupione od handlarza gazet.", "Bought from a paper seller.", "Acheté à un vendeur de journaux.", "Comprado a un vendedor de periódicos.", "Comprata da un venditore di giornali.", "Bei einem Zeitungsverkäufer gekauft.", "Куплена у газетчика.", "Comprada de um jornaleiro.");
        Add(result, "Jagody", "Berries", "Baies", "Bayas", "Bacche", "Beeren", "Ягоды", "Frutas silvestres");
        Add(result, "Jagody. Mnóstwo jagód.", "Berries. Lots of berries.", "Des baies. Beaucoup de baies.", "Bayas. Un montón de bayas.", "Bacche. Tante bacche.", "Beeren. Jede Menge Beeren.", "Ягоды. Очень много ягод.", "Frutas silvestres. Muitas delas.");
        Add(result, "Papryczka chili", "Chilli", "Piment", "Chile", "Peperoncino", "Chili", "Перец чили", "Pimenta-malagueta");
        Add(result, "Bardzo ostra.", "Very spicy.", "Très piquant.", "Muy picante.", "Molto piccante.", "Sehr scharf.", "Очень острый.", "Muito picante.");
        Add(result, "Kukurydza", "Corn", "Maïs", "Maíz", "Mais", "Mais", "Кукуруза", "Milho");
        Add(result, "Chrupiąca kukurydza.", "Crispy corn.", "Du maïs croustillant.", "Maíz crujiente.", "Mais croccante.", "Knuspriger Mais.", "Хрустящая кукуруза.", "Milho crocante.");
        Add(result, "Bakłażan", "Eggplant", "Aubergine", "Berenjena", "Melanzana", "Aubergine", "Баклажан", "Berinjela");
        Add(result, "Fioletowy i soczysty.", "Purple and juicy.", "Violet et juteux.", "Morado y jugoso.", "Viola e succosa.", "Violett und saftig.", "Фиолетовый и сочный.", "Roxo e suculento.");
        Add(result, "Ziemniak", "Potato", "Pomme de terre", "Patata", "Patata", "Kartoffel", "Картофель", "Batata");
        Add(result, "Podstawa pożywienia podróżników.", "The basis of food for travelers.", "La base de l’alimentation des voyageurs.", "La base de la alimentación de los viajeros.", "La base dell’alimentazione dei viaggiatori.", "Grundnahrungsmittel für Reisende.", "Основа рациона путешественников.", "A base da alimentação dos viajantes.");
        Add(result, "Dynia", "Pumpkin", "Citrouille", "Calabaza", "Zucca", "Kürbis", "Тыква", "Abóbora");
        Add(result, "Dynia na Halloween albo na kolację.", "Pumpkin, for Halloween or for dinner.", "Une citrouille, pour Halloween ou pour le dîner.", "Calabaza, para Halloween o para cenar.", "Zucca, per Halloween o per cena.", "Kürbis, für Halloween oder zum Abendessen.", "Тыква — на Хэллоуин или на ужин.", "Abóbora, para o Halloween ou para o jantar.");
        Add(result, "Pomidor", "Tomato", "Tomate", "Tomate", "Pomodoro", "Tomate", "Помидор", "Tomate");
        Add(result, "Soczysty i czerwony.", "Juicy and red.", "Juteux et rouge.", "Jugoso y rojo.", "Succoso e rosso.", "Saftig und rot.", "Сочный и красный.", "Suculento e vermelho.");
        Add(result, "Rzepa", "Turnip", "Navet", "Nabo", "Rapa", "Rübe", "Репа", "Nabo");
        Add(result, "Zdrowa rzepa. Pychota.", "Healthy turnip. Yummy.", "Un navet bon pour la santé. Miam.", "Un nabo saludable. Qué rico.", "Una rapa sana. Che bontà.", "Gesunde Rübe. Lecker.", "Полезная репа. Вкусно.", "Nabo saudável. Que delícia.");
        Add(result, "Ziele budzące", "Awakening herb", "Herbe d’éveil", "Hierba del despertar", "Erba del risveglio", "Erweckungskraut", "Пробуждающая трава", "Erva do despertar");
        Add(result, "Można użyć jej do przygotowania mikstury wybudzającej z magicznego snu.", "You can use it to make a potion to wake up from a magical sleep.", "Elle permet de préparer une potion pour sortir d’un sommeil magique.", "Sirve para preparar una poción que despierta de un sueño mágico.", "Puoi usarla per preparare una pozione che risveglia dal sonno magico.", "Damit lässt sich ein Trank gegen magischen Schlaf brauen.", "Из неё можно приготовить зелье, пробуждающее от магического сна.", "Pode ser usada para preparar uma poção que desperta do sono mágico.");
        Add(result, "Mikstura przebudzenia", "Awakening potion", "Potion d’éveil", "Poción del despertar", "Pozione del risveglio", "Erweckungstrank", "Зелье пробуждения", "Poção do despertar");
        Add(result, "Mikstura, która może obudzić cię z wiecznego snu.", "A potion that can wake you up from eternal sleep.", "Une potion capable de vous tirer du sommeil éternel.", "Una poción capaz de despertarte del sueño eterno.", "Una pozione capace di svegliarti dal sonno eterno.", "Ein Trank, der dich aus dem ewigen Schlaf wecken kann.", "Зелье, способное пробудить тебя от вечного сна.", "Uma poção capaz de despertar você do sono eterno.");
        Add(result, "Duża mikstura zdrowia", "Large health potion", "Grande potion de santé", "Poción de salud grande", "Pozione della salute grande", "Großer Heiltrank", "Большое зелье здоровья", "Poção de vida grande");
        Add(result, "Naprawdę duża mikstura.", "So big potion.", "Une potion vraiment grande.", "Una poción muy grande.", "Una pozione davvero grande.", "Ein sehr großer Trank.", "Очень большое зелье.", "Uma poção bem grande.");
        Add(result, "Mikstura zdrowia", "Health potion", "Potion de santé", "Poción de salud", "Pozione della salute", "Heiltrank", "Зелье здоровья", "Poção de vida");
        Add(result, "Może ocalić ci życie, chyba że to mimik. Wtedy zginiesz. Ta mikstura ma średnią moc.", "It can save your life, unless it is a mimic. In that case, you'll die. This one is medium.", "Elle peut vous sauver, sauf si c’est un mimique. Dans ce cas, vous mourrez. Celle-ci est de puissance moyenne.", "Puede salvarte la vida, salvo si es un mímico. En ese caso, morirás. Esta tiene una potencia media.", "Può salvarti la vita, a meno che non sia un mimetico. In quel caso morirai. Questa è di media potenza.", "Sie kann dein Leben retten, außer es ist ein Mimic. Dann stirbst du. Dieser Trank ist mittelstark.", "Она может спасти тебе жизнь, если это не мимик. В таком случае ты погибнешь. Это зелье средней силы.", "Ela pode salvar sua vida, a menos que seja um mímico. Nesse caso, você morre. Esta poção tem efeito médio.");
        Add(result, "Mała mikstura zdrowia", "Minor health potion", "Petite potion de santé", "Poción de salud menor", "Pozione della salute minore", "Kleiner Heiltrank", "Малое зелье здоровья", "Poção de vida pequena");
        Add(result, "Mała mikstura zdrowia.", "Tiny health potion.", "Une toute petite potion de santé.", "Una poción de salud diminuta.", "Una piccola pozione della salute.", "Ein winziger Heiltrank.", "Крошечное зелье здоровья.", "Uma poção de vida pequenina.");
        Add(result, "Duża mikstura many", "Large mana potion", "Grande potion de mana", "Poción de maná grande", "Pozione del mana grande", "Großer Manatrank", "Большое зелье маны", "Poção de mana grande");
        Add(result, "Przywraca energię, by móc rzucać więcej kul ognia. Ta mikstura jest duża.", "Restores your energy so you can throw more fireballs. This one is large.", "Restaure votre énergie pour lancer plus de boules de feu. Cette potion est grande.", "Restaura tu energía para lanzar más bolas de fuego. Esta poción es grande.", "Ripristina energia per lanciare più palle di fuoco. Questa pozione è grande.", "Stellt Energie wieder her, damit du mehr Feuerbälle werfen kannst. Dieser Trank ist groß.", "Восстанавливает энергию, чтобы метать больше огненных шаров. Это большое зелье.", "Recupera sua energia para lançar mais bolas de fogo. Esta poção é grande.");
        Add(result, "Mikstura many", "Mana potion", "Potion de mana", "Poción de maná", "Pozione del mana", "Manatrank", "Зелье маны", "Poção de mana");
        Add(result, "Przywraca energię, by móc rzucać więcej kul ognia. Ta mikstura ma średnią moc.", "Restores your energy so you can throw more fireballs. This one is medium.", "Restaure votre énergie pour lancer plus de boules de feu. Cette potion est de puissance moyenne.", "Restaura tu energía para lanzar más bolas de fuego. Esta poción tiene una potencia media.", "Ripristina energia per lanciare più palle di fuoco. Questa pozione è di media potenza.", "Stellt Energie wieder her, damit du mehr Feuerbälle werfen kannst. Dieser Trank ist mittelstark.", "Восстанавливает энергию, чтобы метать больше огненных шаров. Это зелье средней силы.", "Recupera sua energia para lançar mais bolas de fogo. Esta poção tem efeito médio.");
        Add(result, "Mała mikstura many", "Minor mana potion", "Petite potion de mana", "Poción de maná menor", "Pozione del mana minore", "Kleiner Manatrank", "Малое зелье маны", "Poção de mana pequena");
        Add(result, "Przywraca energię, by móc rzucać więcej kul ognia.", "Restores your energy so you can throw more fireballs.", "Restaure votre énergie pour lancer plus de boules de feu.", "Restaura tu energía para lanzar más bolas de fuego.", "Ripristina energia per lanciare più palle di fuoco.", "Stellt Energie wieder her, damit du mehr Feuerbälle werfen kannst.", "Восстанавливает энергию, чтобы метать больше огненных шаров.", "Recupera sua energia para lançar mais bolas de fogo.");
        Add(result, "Mała mikstura szybkości", "Minor speed potion", "Petite potion de vitesse", "Poción de velocidad menor", "Pozione della velocità minore", "Kleiner Geschwindigkeitstrank", "Малое зелье скорости", "Poção de velocidade pequena");
        Add(result, "Zwiększa szybkość.", "Increases your speed.", "Augmente votre vitesse.", "Aumenta tu velocidad.", "Aumenta la tua velocità.", "Erhöht dein Tempo.", "Увеличивает скорость.", "Aumenta sua velocidade.");
        Add(result, "Mała mikstura siły", "Minor strength potion", "Petite potion de force", "Poción de fuerza menor", "Pozione della forza minore", "Kleiner Stärketrank", "Малое зелье силы", "Poção de força pequena");
        Add(result, "Zwiększa zadawane obrażenia.", "Increases your damage.", "Augmente les dégâts que vous infligez.", "Aumenta el daño que infliges.", "Aumenta i danni che infliggi.", "Erhöht deinen verursachten Schaden.", "Увеличивает наносимый урон.", "Aumenta o dano que você causa.");

        // Armour, helmets and footwear.
        Add(result, "Napierśnik Nocnej Straży", "Nightwatch Cuirass", "Cuirasse de la Garde nocturne", "Coraza de la Guardia Nocturna", "Corazza della Guardia Notturna", "Harnisch der Nachtwache", "Кираса Ночного дозора", "Couraça da Vigília Noturna");
        Add(result, "Warstwowa, ciemna skóra stworzona na najniebezpieczniejsze warty na skraju lasu.", "Dark layered hide is built for the deadliest watches at the edge of the forest.", "Cette épaisse cuirasse sombre est faite pour les gardes les plus périlleuses en lisière de forêt.", "Esta armadura oscura de cuero por capas está hecha para las guardias más peligrosas en el límite del bosque.", "Una corazza scura a strati, pensata per i turni di guardia più pericolosi ai margini della foresta.", "Dunkles, mehrlagiges Leder für die gefährlichsten Wachen am Waldrand.", "Тёмная многослойная кожа для самых опасных дозоров на опушке леса.", "Couro escuro em camadas, feito para as vigílias mais perigosas à beira da floresta.");
        Add(result, "Kaftan Straży Cierniowej", "Briarwatch Hauberk", "Haubert de la Garde des Ronces", "Cota de la Guardia de Zarzas", "Usbergo della Guardia dei Rovi", "Dornwacht-Hauberk", "Хауберк Шипастого дозора", "Cota da Guarda dos Espinheiros");
        Add(result, "Warstwowa skóra wzmocniona płytkami z cierniowego drewna łączy ochronę ze swobodą ruchu.", "Layered leather reinforced with thornwood plates balances protection with freedom of movement.", "Un cuir multicouche renforcé de plaques de bois épineux allie protection et liberté de mouvement.", "El cuero por capas reforzado con placas de madera espinosa combina protección y libertad de movimiento.", "Il cuoio a strati rinforzato con placche di legno spinoso bilancia protezione e libertà di movimento.", "Mehrlagiges Leder mit Dornholzplatten verbindet Schutz mit Bewegungsfreiheit.", "Многослойная кожа с накладками из колючего дерева сочетает защиту и свободу движений.", "O couro em camadas, reforçado com placas de madeira espinhosa, equilibra proteção e liberdade de movimento.");
        Add(result, "Napierśnik z jeleniej skóry", "Staghide Cuirass", "Cuirasse en peau de cerf", "Coraza de piel de ciervo", "Corazza in pelle di cervo", "Hirschlederharnisch", "Кираса из оленьей шкуры", "Couraça de couro de cervo");
        Add(result, "Gruba skóra jelenia odpiera silne ciosy, ale jej ciężar może utrudnić odwrót.", "Dense stag hide turns aside hard blows, though its weight can hinder a quick retreat.", "La peau épaisse du cerf dévie les coups puissants, mais son poids gêne les retraites rapides.", "La gruesa piel de ciervo desvía golpes fuertes, aunque su peso dificulta una retirada rápida.", "La spessa pelle di cervo devia i colpi pesanti, ma il suo peso può ostacolare una ritirata rapida.", "Dickes Hirschleder hält schwere Schläge ab, erschwert durch sein Gewicht aber die schnelle Flucht.", "Плотная оленья шкура отражает сильные удары, но её вес мешает быстрому отступлению.", "O couro espesso de cervo desvia golpes fortes, mas seu peso pode atrapalhar uma retirada rápida.");
        Add(result, "Napierśnik Żelaznego Korzenia", "Ironroot Cuirass", "Cuirasse de la Racine de fer", "Coraza de Raíz de Hierro", "Corazza della Radice di Ferro", "Eisenwurzelharnisch", "Кираса Железного корня", "Couraça da Raiz de Ferro");
        Add(result, "Gęsta, ciemna skóra i łuski z żelaznego drewna pochłaniają silne ciosy kosztem szybkości.", "Dense, dark hide and ironwood scales absorb heavy blows at the cost of a slower stride.", "Le cuir sombre et épais, renforcé d’écailles de bois de fer, absorbe les coups puissants au prix d’une démarche plus lente.", "El cuero oscuro y denso, con escamas de madera de hierro, absorbe golpes fuertes a costa de reducir la velocidad.", "Il cuoio scuro e fitto, con scaglie di legnoferro, assorbe i colpi pesanti a scapito della velocità.", "Dunkles, dichtes Leder und Eisenholzschuppen fangen schwere Schläge ab, machen dich aber langsamer.", "Плотная тёмная кожа и чешуя из железного дерева поглощают сильные удары, но замедляют шаг.", "Couro escuro e denso, com escamas de madeira-ferro, absorve golpes fortes ao custo de uma passada mais lenta.");
        Add(result, "Kaftan Leśnego Strażnika", "Forestguard Jerkin", "Pourpoint du Gardien forestier", "Jubón del Guardián del Bosque", "Giubba della Guardia della Foresta", "Waldwächter-Wams", "Куртка Лесного стража", "Gibão do Guardião da Floresta");
        Add(result, "Wzmocniona skóra zapewnia niezawodną ochronę podczas patroli poza murami wioski.", "Toughened hide offers dependable protection on long patrols beyond the village walls.", "Le cuir renforcé offre une protection fiable lors des longues patrouilles au-delà des murs du village.", "El cuero endurecido ofrece protección fiable en patrullas largas más allá de las murallas.", "Il cuoio rinforzato offre una protezione affidabile durante le lunghe pattuglie oltre le mura del villaggio.", "Gehärtetes Leder bietet zuverlässigen Schutz auf langen Patrouillen außerhalb des Dorfes.", "Укреплённая кожа надёжно защищает во время долгих патрулей за стенами деревни.", "O couro reforçado oferece proteção confiável em longas patrulhas além das muralhas da vila.");
        Add(result, "Kaftan tropiciela z jeleniej skóry", "Scout's Deerskin Jerkin", "Pourpoint en peau de cerf de l’éclaireur", "Jubón de piel de ciervo del explorador", "Giubba da ricognitore in pelle di cervo", "Späherwams aus Hirschleder", "Куртка следопыта из оленьей шкуры", "Gibão de couro de cervo do batedor");
        Add(result, "Miękka skóra jelenia chroni przed chłodem, nie spowalniając kroku tropiciela.", "Soft deerskin keeps the chill off without slowing a scout's stride.", "La peau de cerf souple protège du froid sans ralentir l’éclaireur.", "La suave piel de ciervo abriga del frío sin ralentizar al explorador.", "La morbida pelle di cervo ripara dal freddo senza rallentare il ricognitore.", "Weiches Hirschleder hält die Kälte ab, ohne den Späher zu bremsen.", "Мягкая оленья кожа защищает от холода и не замедляет следопыта.", "O couro macio de cervo protege do frio sem reduzir o passo do batedor.");
        Add(result, "Kaftan Wędrowca", "Wayfarer's Jerkin", "Pourpoint du voyageur", "Jubón del viajero", "Giubba del viandante", "Wams des Wanderers", "Куртка путника", "Gibão do viajante");
        Add(result, "Elastyczny kaftan dla tropicieli, którzy wolą szybką ucieczkę od ciężkiej ochrony. Jego żywe włókna powoli leczą rany.", "A supple hide jerkin for scouts who value a quick escape over heavy protection. Its living fibers slowly mend wounds.", "Un pourpoint souple pour les éclaireurs qui préfèrent fuir vite plutôt que porter une lourde armure. Ses fibres vivantes soignent lentement les blessures.", "Un jubón flexible para exploradores que prefieren escapar rápido antes que llevar una protección pesada. Sus fibras vivas curan las heridas poco a poco.", "Una giubba flessibile per ricognitori che preferiscono una fuga rapida a una protezione pesante. Le sue fibre vive curano lentamente le ferite.", "Ein geschmeidiger Lederwams für Späher, die eine schnelle Flucht schwerer Rüstung vorziehen. Seine lebenden Fasern heilen Wunden langsam.", "Гибкая куртка для следопытов, предпочитающих быстрое отступление тяжёлой броне. Её живые волокна медленно затягивают раны.", "Um gibão flexível para batedores que preferem uma fuga rápida a uma proteção pesada. Suas fibras vivas curam ferimentos lentamente.");
        Add(result, "Kaftan Jasnej Skóry", "Palehide Hauberk", "Haubert en cuir pâle", "Cota de piel pálida", "Usbergo in pelle chiara", "Heller Lederhauberk", "Хауберк из светлой кожи", "Cota de couro claro");
        Add(result, "Jasna, starannie wyprawiona skóra łączy solidną ochronę z rozsądnym ciężarem.", "Pale, tightly worked leather balances reliable protection with a manageable weight.", "Le cuir pâle soigneusement travaillé offre une bonne protection sans être trop lourd.", "El cuero claro y bien trabajado ofrece una protección fiable sin demasiado peso.", "Il cuoio chiaro e lavorato con cura offre una protezione affidabile senza troppo peso.", "Helles, sorgfältig verarbeitetes Leder bietet zuverlässigen Schutz bei tragbarem Gewicht.", "Светлая тщательно выделанная кожа надёжно защищает и при этом не слишком тяжёлая.", "O couro claro e bem trabalhado oferece proteção confiável sem pesar demais.");

        Add(result, "Hełm Nocnej Straży", "Nightwatch Helm", "Heaume de la Garde nocturne", "Yelmo de la Guardia Nocturna", "Elmo della Guardia Notturna", "Helm der Nachtwache", "Шлем Ночного дозора", "Elmo da Vigília Noturna");
        Add(result, "Ciemny, wzmocniony hełm dla strażników, którzy mierzą się z niebezpieczeństwami po zmroku.", "A dark, reinforced helm made for guards who face danger after sundown.", "Un heaume sombre et renforcé conçu pour les gardes qui affrontent les dangers après la tombée de la nuit.", "Un yelmo oscuro y reforzado, hecho para guardias que afrontan peligros después del anochecer.", "Un elmo scuro e rinforzato, pensato per le guardie che affrontano i pericoli dopo il tramonto.", "Ein dunkler, verstärkter Helm für Wachen, die sich nach Sonnenuntergang Gefahren stellen.", "Тёмный усиленный шлем для стражей, встречающих опасность после заката.", "Um elmo escuro e reforçado, feito para guardas que enfrentam perigos após o anoitecer.");
        Add(result, "Czepiec Straży Cierniowej", "Briarwatch Coif", "Coiffe de la Garde des Ronces", "Almófar de la Guardia de Zarzas", "Camaglio della Guardia dei Rovi", "Dornwacht-Kettenhaube", "Койф Шипастого дозора", "Coifa da Guarda dos Espinheiros");
        Add(result, "Wyściełany czepiec z zachodzącymi na siebie skórzanymi osłonami skroni i karku.", "A padded coif with overlapping leather guards for the temples and neck.", "Une coiffe rembourrée, avec des renforts de cuir superposés aux tempes et à la nuque.", "Un almófar acolchado con protectores de cuero superpuestos para las sienes y el cuello.", "Un camaglio imbottito con protezioni di cuoio sovrapposte per tempie e collo.", "Eine gepolsterte Haube mit überlappenden Lederschutzen für Schläfen und Nacken.", "Стёганый койф с накладными кожаными защитами висков и шеи.", "Uma coifa acolchoada com proteções de couro sobrepostas nas têmporas e no pescoço.");
        Add(result, "Hełm z jeleniej skóry", "Staghide Helm", "Heaume en peau de cerf", "Yelmo de piel de ciervo", "Elmo in pelle di cervo", "Hirschlederhelm", "Шлем из оленьей шкуры", "Elmo de couro de cervo");
        Add(result, "Warstwy twardej skóry jelenia chronią głowę przed rykoszetującymi ciosami.", "Firmly layered stag hide protects the head from a glancing strike.", "Les couches rigides de peau de cerf protègent la tête des coups obliques.", "Las capas firmes de piel de ciervo protegen la cabeza de golpes de refilón.", "Gli strati rigidi di pelle di cervo proteggono la testa dai colpi di striscio.", "Fest geschichtetes Hirschleder schützt den Kopf vor Streifschlägen.", "Плотные слои оленьей шкуры защищают голову от скользящих ударов.", "As camadas firmes de couro de cervo protegem a cabeça de golpes de raspão.");
        Add(result, "Hełm Żelaznego Korzenia", "Ironroot Helm", "Heaume de la Racine de fer", "Yelmo de Raíz de Hierro", "Elmo della Radice di Ferro", "Eisenwurzelhelm", "Шлем Железного корня", "Elmo da Raiz de Ferro");
        Add(result, "Wzmocniony hełm podszyty ciemną skórą, zaprojektowany, by odbijać ciosy z bliska.", "A reinforced helm lined with dark hide, built to turn aside a close-range strike.", "Un heaume renforcé doublé de cuir sombre, conçu pour dévier les coups rapprochés.", "Un yelmo reforzado y forrado de cuero oscuro, diseñado para desviar golpes a corta distancia.", "Un elmo rinforzato foderato di cuoio scuro, progettato per deviare i colpi ravvicinati.", "Ein verstärkter, dunkel gefütterter Helm, der Schläge aus nächster Nähe abwehrt.", "Усиленный шлем с подкладкой из тёмной кожи, отводящий удары вблизи.", "Um elmo reforçado e forrado com couro escuro, feito para aparar golpes de perto.");
        Add(result, "Czapka Leśnego Strażnika", "Forestguard Cap", "Calotte du Gardien forestier", "Gorra del Guardián del Bosque", "Berretto della Guardia della Foresta", "Waldwächterkappe", "Шапка Лесного стража", "Boné do Guardião da Floresta");
        Add(result, "Solidna skórzana czapka z dodatkową wyściółką na czole i skroniach.", "A sturdy leather cap with extra padding over the brow and temples.", "Une solide calotte de cuir, rembourrée au niveau du front et des tempes.", "Una resistente gorra de cuero con acolchado adicional en la frente y las sienes.", "Un robusto berretto di cuoio con imbottitura extra sulla fronte e sulle tempie.", "Eine robuste Lederkappe mit zusätzlicher Polsterung an Stirn und Schläfen.", "Прочная кожаная шапка с дополнительной подкладкой на лбу и висках.", "Um boné de couro resistente, com acolchoamento extra na testa e nas têmporas.");
        Add(result, "Kaptur tropiciela z jeleniej skóry", "Scout's Deerskin Hood", "Capuche en peau de cerf de l’éclaireur", "Capucha de piel de ciervo del explorador", "Cappuccio da ricognitore in pelle di cervo", "Späherkapuze aus Hirschleder", "Капюшон следопыта из оленьей шкуры", "Capuz de couro de cervo do batedor");
        Add(result, "Miękki kaptur chroni głowę przed deszczem i gałęziami, nie obciążając noszącego.", "A soft hood shields the head from rain and branches without weighing the wearer down.", "Une capuche souple protège de la pluie et des branches sans alourdir celui qui la porte.", "Una capucha suave protege de la lluvia y las ramas sin cargar a quien la lleva.", "Un cappuccio morbido ripara da pioggia e rami senza appesantire chi lo indossa.", "Eine weiche Kapuze schützt vor Regen und Ästen, ohne den Träger zu beschweren.", "Мягкий капюшон защищает от дождя и веток, не утяжеляя голову.", "Um capuz macio protege da chuva e dos galhos sem pesar para quem o usa.");
        Add(result, "Kaptur Wędrowca", "Wayfarer's Hood", "Capuche du voyageur", "Capucha del viajero", "Cappuccio del viandante", "Kapuze des Wanderers", "Капюшон путника", "Capuz do viajante");
        Add(result, "Dopasowany kaptur chroni oczy przed deszczem i gałęziami.", "A close-fitting hood keeps rain and branches away from a traveller's eyes.", "Une capuche ajustée protège les yeux du voyageur de la pluie et des branches.", "Una capucha ajustada protege los ojos del viajero de la lluvia y las ramas.", "Un cappuccio aderente protegge gli occhi del viandante da pioggia e rami.", "Eine eng anliegende Kapuze hält Regen und Äste von den Augen des Wanderers fern.", "Плотно сидящий капюшон защищает глаза путника от дождя и веток.", "Um capuz justo protege os olhos do viajante da chuva e dos galhos.");
        Add(result, "Czepiec Jasnej Skóry", "Palehide Coif", "Coiffe en cuir pâle", "Almófar de piel pálida", "Camaglio in pelle chiara", "Heller Lederhaube", "Койф из светлой кожи", "Coifa de couro claro");
        Add(result, "Dopasowany czepiec chroni głowę i kark, nie ograniczając pola widzenia.", "A close-fitting coif protects the head and neck while keeping a clear field of view.", "Une coiffe ajustée protège la tête et le cou sans gêner la vision.", "Un almófar ajustado protege la cabeza y el cuello sin limitar la visión.", "Un camaglio aderente protegge testa e collo senza limitare la visuale.", "Eine eng anliegende Haube schützt Kopf und Nacken und lässt das Sichtfeld frei.", "Плотный койф защищает голову и шею, не ограничивая обзор.", "Uma coifa justa protege a cabeça e o pescoço sem limitar o campo de visão.");

        Add(result, "Buty Nocnej Straży", "Nightwatch Boots", "Bottes de la Garde nocturne", "Botas de la Guardia Nocturna", "Stivali della Guardia Notturna", "Stiefel der Nachtwache", "Сапоги Ночного дозора", "Botas da Vigília Noturna");
        Add(result, "Ciemne, dobrze dopasowane buty stworzone do szybkiego poruszania się podczas nocnych patroli.", "Dark, well-fitted boots built for swift movement on night patrols.", "Des bottes sombres et bien ajustées, faites pour se déplacer vite lors des patrouilles nocturnes.", "Botas oscuras y bien ajustadas, hechas para moverse rápido durante las patrullas nocturnas.", "Stivali scuri e ben calzanti, pensati per muoversi rapidamente durante le ronde notturne.", "Dunkle, gut sitzende Stiefel für schnelle Bewegungen auf Nachtpatrouillen.", "Тёмные удобные сапоги для быстрого передвижения во время ночных дозоров.", "Botas escuras e bem ajustadas, feitas para se mover rapidamente nas rondas noturnas.");
        Add(result, "Buty Lisiego Kroku", "Foxstep Boots", "Bottes Pas-de-renard", "Botas Paso de Zorro", "Stivali Passo di Volpe", "Fuchsschrittstiefel", "Сапоги Лисьего шага", "Botas Passo de Raposa");
        Add(result, "Lekkie, elastyczne buty do cichego przemierzania lasu bez utraty tempa.", "Light, flexible boots made for slipping through the woods without losing pace.", "Des bottes légères et souples pour traverser les bois sans perdre de vitesse.", "Botas ligeras y flexibles para cruzar el bosque sin perder el ritmo.", "Stivali leggeri e flessibili per attraversare il bosco senza perdere passo.", "Leichte, flexible Stiefel, um den Wald zu durchqueren, ohne langsamer zu werden.", "Лёгкие гибкие сапоги позволяют бесшумно идти по лесу, не сбавляя шага.", "Botas leves e flexíveis para atravessar a floresta sem perder o ritmo.");
        Add(result, "Nagolenniki z jeleniej skóry", "Staghide Greaves", "Grèves en peau de cerf", "Grebas de piel de ciervo", "Schinieri in pelle di cervo", "Hirschleder-Beinschienen", "Поножи из оленьей шкуры", "Grevas de couro de cervo");
        Add(result, "Wzmocnione obuwie stabilizuje kostkę na nierównym podłożu.", "Reinforced footwear supports the ankle on uneven ground.", "Ces chaussures renforcées soutiennent la cheville sur les terrains accidentés.", "El calzado reforzado sujeta el tobillo en terrenos irregulares.", "La calzatura rinforzata sostiene la caviglia sui terreni irregolari.", "Verstärktes Schuhwerk stützt den Knöchel auf unebenem Boden.", "Укреплённая обувь поддерживает голеностоп на неровной земле.", "O calçado reforçado dá suporte ao tornozelo em terrenos irregulares.");
        Add(result, "Buty Leśnego Strażnika", "Forestguard Boots", "Bottes du Gardien forestier", "Botas del Guardián del Bosque", "Stivali della Guardia della Foresta", "Waldwächterstiefel", "Сапоги Лесного стража", "Botas do Guardião da Floresta");
        Add(result, "Wytrzymałe skórzane buty na długie warty i trudne leśne ścieżki.", "Reliable leather boots for long watches and rough woodland paths.", "Des bottes en cuir fiables pour les longues gardes et les sentiers forestiers difficiles.", "Botas de cuero fiables para largas guardias y caminos boscosos difíciles.", "Stivali di cuoio affidabili per lunghe ronde e sentieri boschivi impervi.", "Zuverlässige Lederstiefel für lange Wachen und raue Waldwege.", "Надёжные кожаные сапоги для долгих дозоров и трудных лесных троп.", "Botas de couro confiáveis para longas vigílias e trilhas acidentadas na floresta.");
        Add(result, "Buty tropiciela z jeleniej skóry", "Scout's Deerskin Boots", "Bottes en peau de cerf de l’éclaireur", "Botas de piel de ciervo del explorador", "Stivali da ricognitore in pelle di cervo", "Späherstiefel aus Hirschleder", "Сапоги следопыта из оленьей шкуры", "Botas de couro de cervo do batedor");
        Add(result, "Lekkie buty z jeleniej skóry pomagają tropicielowi zachować zwinność na leśnych szlakach.", "Light deerskin boots keep a scout nimble on forest trails.", "Des bottes légères en peau de cerf gardent l’éclaireur agile sur les sentiers forestiers.", "Las botas ligeras de piel de ciervo mantienen ágil al explorador por los senderos del bosque.", "Gli stivali leggeri in pelle di cervo mantengono agile il ricognitore sui sentieri della foresta.", "Leichte Hirschlederstiefel halten Späher auf Waldpfaden beweglich.", "Лёгкие сапоги из оленьей шкуры помогают следопыту сохранять подвижность на лесных тропах.", "Botas leves de couro de cervo mantêm o batedor ágil nas trilhas da floresta.");
        Add(result, "Buty Wędrowca przez Mokradła", "Mirewalker Boots", "Bottes du marcheur des marais", "Botas del Caminante del Pantano", "Stivali del camminatore di palude", "Moorläuferstiefel", "Сапоги болотника", "Botas do Andarilho do Pântano");
        Add(result, "Przyczepna podeszwa i stabilne wsparcie kostki pomagają poruszać się po błocie i nierównym terenie.", "Gripped soles and firm ankle support keep the wearer moving through mud and rough ground.", "Les semelles adhérentes et le bon maintien de la cheville facilitent les déplacements dans la boue et sur les terrains accidentés.", "La suela adherente y el buen soporte del tobillo ayudan a avanzar por el barro y el terreno irregular.", "La suola antiscivolo e il solido supporto alla caviglia aiutano a muoversi nel fango e sui terreni sconnessi.", "Rutschfeste Sohlen und stabiler Knöchelschutz helfen im Schlamm und auf unebenem Boden.", "Цепкая подошва и надёжная поддержка голеностопа помогают идти по грязи и пересечённой местности.", "Solas aderentes e bom suporte para o tornozelo ajudam a avançar na lama e em terrenos acidentados.");
        Add(result, "Nagolenniki Burzowego Kroku", "Stormstride Greaves", "Grèves Pas-de-tempête", "Grebas Paso de Tormenta", "Schinieri Passo di Tempesta", "Sturmtritt-Beinschienen", "Поножи Штормового шага", "Grevas Passo da Tempestade");
        Add(result, "Lekkie jak pióro i sprężyste podeszwy pomagają uciec przed niebezpieczeństwem.", "A feather-light fit and springy soles help the wearer outrun danger.", "Leur légèreté et leurs semelles souples aident à semer le danger.", "Su ligereza y sus suelas elásticas ayudan a dejar atrás el peligro.", "La calzata leggerissima e le suole elastiche aiutano a seminare il pericolo.", "Federleicht und mit federnden Sohlen helfen sie, Gefahren davonzulaufen.", "Лёгкость и упругие подошвы помогают убежать от опасности.", "Leves como uma pena e com solas flexíveis, ajudam a escapar do perigo.");
        Add(result, "Buty Jasnej Skóry", "Palehide Boots", "Bottes en cuir pâle", "Botas de piel pálida", "Stivali in pelle chiara", "Helle Lederstiefel", "Сапоги из светлой кожи", "Botas de couro claro");
        Add(result, "Miękka, jasna skóra sprawia, że każdy krok jest lekki i pewny.", "Supple pale leather keeps each step light and steady.", "Le cuir pâle et souple rend chaque pas léger et assuré.", "El cuero claro y flexible hace que cada paso sea ligero y firme.", "Il cuoio chiaro e morbido rende ogni passo leggero e sicuro.", "Geschmeidiges, helles Leder sorgt für leichte und sichere Schritte.", "Мягкая светлая кожа делает каждый шаг лёгким и уверенным.", "O couro claro e flexível deixa cada passo leve e firme.");

        // Melee and ranged weapons.
        Add(result, "Tasak Żarzącego Ostrza", "Cinder Cleaver", "Couperet de braise", "Cuchilla de Ascuas", "Mannaia di Brace", "Glutspalter", "Тесак Тлеющего угля", "Cutelo Braseiro");
        Add(result, "Ciężki tasak z rozżarzonym ostrzem, które może podpalić trafionego przeciwnika.", "A heavy cleaver with an ember-hot edge that can set a struck enemy ablaze.", "Un lourd couperet au tranchant ardent qui peut enflammer l’ennemi touché.", "Un cuchillo pesado con un filo al rojo vivo que puede prender fuego al enemigo alcanzado.", "Una mannaia pesante dal filo rovente che può incendiare il nemico colpito.", "Ein schweres Beil mit glühender Schneide, das getroffene Gegner entzünden kann.", "Тяжёлый тесак с раскалённым лезвием может поджечь поражённого врага.", "Um cutelo pesado com fio em brasa que pode incendiar o inimigo atingido.");
        Add(result, "Topór Przecinający Szlak", "Trailbreaker Axe", "Hache Brise-sentier", "Hacha Rompesendas", "Ascia Spaccasentieri", "Pfadbrecher-Axt", "Топор Пролома", "Machado Quebra-Trilha");
        Add(result, "Niezawodny topór drwala o wyważonej głowicy i zdecydowanym zamachu.", "A dependable woodsman's axe with a balanced head and a decisive swing.", "Une hache de bûcheron fiable, à la tête équilibrée et au coup assuré.", "Un hacha fiable de leñador, con una cabeza equilibrada y un golpe firme.", "Un’ascia da boscaiolo affidabile, con testa bilanciata e colpo deciso.", "Eine zuverlässige Holzfälleraxt mit ausgewogenem Kopf und kraftvollem Schwung.", "Надёжный топор лесоруба со сбалансированной головой и решительным ударом.", "Um machado confiável de lenhador, com cabeça equilibrada e golpe certeiro.");
        Add(result, "Sztylet Jadowego Kła", "Venomfang Dagger", "Dague Croc-venimeux", "Daga Colmillo Venenoso", "Pugnale Zanna Velenosa", "Giftzahn-Dolch", "Кинжал Ядовитого клыка", "Adaga Presa Venenosa");
        Add(result, "Lekka, wąska klinga pokryta jadem, który może zatruć cel.", "A swift, narrow blade coated in venom that can poison its target.", "Une lame étroite et rapide, enduite de venin qui peut empoisonner sa cible.", "Una hoja ligera y estrecha cubierta de veneno que puede envenenar a su objetivo.", "Una lama rapida e stretta, ricoperta di veleno che può avvelenare il bersaglio.", "Eine schmale, schnelle Klinge mit Gift, das das Ziel vergiften kann.", "Узкий быстрый клинок, покрытый ядом, способным отравить цель.", "Uma lâmina estreita e veloz, revestida de veneno capaz de envenenar o alvo.");
        Add(result, "Nóż Wędrowca", "Wayfarer's Knife", "Couteau du voyageur", "Cuchillo del viajero", "Coltello del viandante", "Messer des Wanderers", "Нож путника", "Faca do viajante");
        Add(result, "Poręczne ostrze, które szybko atakuje i łatwo zabrać w każdą podróż.", "A compact blade that strikes quickly and is easy to carry on any journey.", "Une lame compacte qui frappe vite et se transporte facilement en voyage.", "Una hoja compacta que golpea rápido y es fácil de llevar en cualquier viaje.", "Una lama compatta che colpisce rapidamente ed è facile da portare in viaggio.", "Eine kompakte Klinge, die schnell zuschlägt und auf jede Reise passt.", "Компактный клинок для быстрых ударов, который удобно брать в путешествие.", "Uma lâmina compacta que golpeia rápido e é fácil de levar em qualquer jornada.");
        Add(result, "Bez broni", "Unarmed", "Sans arme", "Desarmado", "Disarmato", "Unbewaffnet", "Без оружия", "Desarmado");
        Add(result, "Nie masz broni. Gdy nie ma innego wyjścia, walcz pięściami.", "No weapon equipped. Fight with your fists when there is no other choice.", "Aucune arme équipée. Battez-vous à mains nues en dernier recours.", "No llevas un arma. Lucha con los puños cuando no haya otra opción.", "Nessuna arma equipaggiata. Combatti a pugni quando non hai altra scelta.", "Keine Waffe ausgerüstet. Kämpfe mit den Fäusten, wenn es keinen anderen Ausweg gibt.", "Оружие не экипировано. Когда другого выбора нет, сражайся кулаками.", "Nenhuma arma equipada. Lute com os punhos quando não houver outra opção.");
        Add(result, "Cep Bijący Mrozem", "Frostchain Flail", "Fléau Chaîne-de-givre", "Mayal Cadena de Escarcha", "Flagello Catena di Brina", "Frostkettenflegel", "Морозный цеп", "Mangual Corrente Gélida");
        Add(result, "Broń łańcuchowa zahartowana mrozem. Jej lodowate ciosy mogą zamrozić wroga.", "A frost-tempered chain weapon whose chilling strikes can freeze an enemy.", "Une arme à chaîne forgée dans le givre, dont les coups glacials peuvent figer l’ennemi.", "Un arma de cadena templada por la escarcha cuyos golpes helados pueden congelar al enemigo.", "Un’arma a catena temprata dal gelo, i cui colpi gelidi possono congelare il nemico.", "Eine frostgehärtete Kettenwaffe, deren eisige Schläge Gegner einfrieren können.", "Цепное оружие, закалённое морозом: его ледяные удары могут заморозить врага.", "Uma arma de corrente temperada pelo gelo, cujos golpes congelantes podem congelar o inimigo.");
        Add(result, "Żelazny Cep Łańcuchowy", "Ironchain Flail", "Fléau à chaîne de fer", "Mayal de Cadena de Hierro", "Flagello a Catena di Ferro", "Eisenkettenflegel", "Железный цеп", "Mangual de Corrente de Ferro");
        Add(result, "Ciężka żelazna głowica na łańcuchu wymienia szybkie ataki na miażdżącą siłę.", "A weighted iron head on a chain trades quick attacks for punishing impact.", "Une tête de fer lestée au bout d’une chaîne sacrifie la vitesse pour des impacts dévastateurs.", "Una pesada cabeza de hierro encadenada sacrifica rapidez por impactos contundentes.", "Una pesante testa di ferro su catena sacrifica la rapidità per colpi devastanti.", "Ein beschwerter Eisenkopf an einer Kette tauscht schnelle Angriffe gegen wuchtige Treffer.", "Тяжёлая железная головка на цепи медленнее, зато наносит сокрушительные удары.", "Uma pesada cabeça de ferro presa a uma corrente troca velocidade por impactos devastadores.");
        Add(result, "Długi Miecz Żarzącej Marki", "Emberbrand Longsword", "Épée longue de la Marque ardente", "Espada larga Marca de Ascua", "Spada lunga Marchio di Brace", "Langschwert der Glutmal-Klinge", "Длинный меч Тлеющего клейма", "Espada longa Marca de Brasa");
        Add(result, "Długie ostrze o tlącej się krawędzi, które może podpalić trafionego wroga.", "A long blade with a smouldering edge that can ignite enemies on impact.", "Une longue lame au tranchant incandescent qui peut enflammer les ennemis à l’impact.", "Una hoja larga con un filo incandescente que puede prender fuego a los enemigos al impactar.", "Una lunga lama dal filo ardente che può incendiare i nemici all’impatto.", "Eine lange Klinge mit glimmender Schneide, die Gegner beim Treffer entzünden kann.", "Длинный клинок с тлеющей кромкой может поджечь врага при попадании.", "Uma lâmina longa com fio incandescente que pode incendiar inimigos ao acertar.");
        Add(result, "Włócznia Odmrożenia", "Frostbite Spear", "Lance Morsure-de-givre", "Lanza Mordedura Gélida", "Lancia Morso di Gelo", "Frostbiss-Speer", "Копьё Ледяного укуса", "Lança Mordida de Geada");
        Add(result, "Kuta na zimno włócznia, której lodowy grot może zamrozić przebitego wroga.", "A cold-forged spear whose icy point can freeze the enemy it pierces.", "Une lance forgée à froid dont la pointe glacée peut geler l’ennemi qu’elle transperce.", "Una lanza forjada en frío cuya punta helada puede congelar al enemigo que atraviesa.", "Una lancia forgiata a freddo, la cui punta gelida può congelare il nemico trafitto.", "Ein kaltgeschmiedeter Speer, dessen eisige Spitze durchbohrte Gegner einfrieren kann.", "Закалённое холодом копьё: ледяное острие может заморозить пронзённого врага.", "Uma lança forjada a frio cuja ponta gelada pode congelar o inimigo que perfura.");
        Add(result, "Włócznia Dalekiego Zasięgu", "Longreach Spear", "Lance longue portée", "Lanza de Largo Alcance", "Lancia Lunga Portata", "Weitreichen-Speer", "Копьё Дальнего боя", "Lança de Longo Alcance");
        Add(result, "Dobrze wyważona włócznia pozwala atakować z bezpieczniejszej odległości.", "A balanced spear that lets its wielder strike from a safer distance.", "Une lance équilibrée qui permet de frapper à une distance plus sûre.", "Una lanza equilibrada que permite atacar desde una distancia más segura.", "Una lancia bilanciata che permette di colpire da una distanza più sicura.", "Ein ausgewogener Speer, mit dem du aus sicherer Entfernung angreifen kannst.", "Сбалансированное копьё позволяет атаковать с более безопасного расстояния.", "Uma lança equilibrada que permite atacar de uma distância mais segura.");
        Add(result, "Rodowy Mroźny Miecz", "Heirloom Frostblade", "Lame de givre ancestrale", "Hoja de Escarcha Heredada", "Lama di Brina Ereditaria", "Erbstück Frostklinge", "Родовой Ледяной клинок", "Lâmina Gélida de Família");
        Add(result, "Stare rodzinne ostrze z chłodem w klindze, które może zamrozić trafionego wroga.", "An old family blade with a chill in its edge that can freeze a struck foe.", "Une ancienne lame familiale au tranchant glacé, capable de geler l’ennemi touché.", "Una antigua hoja familiar con un filo gélido capaz de congelar al enemigo alcanzado.", "Un’antica lama di famiglia dal filo gelido, capace di congelare il nemico colpito.", "Eine alte Familienklinge mit eisiger Schneide, die getroffene Gegner einfrieren kann.", "Старинный семейный клинок с морозной кромкой может заморозить поражённого врага.", "Uma antiga lâmina de família com fio gélido, capaz de congelar o inimigo atingido.");
        Add(result, "Popielaty Topór Wojenny", "Ashen Waraxe", "Hache de guerre cendrée", "Hacha de Guerra Cenicienta", "Ascia da Guerra Cinerea", "Aschgraue Streitaxt", "Пепельная боевая секира", "Machado de Guerra Cinéreo");
        Add(result, "Ciężki topór wojenny z rozgrzanym ostrzem, które może podpalić cel.", "A heavy waraxe with a heated edge that can leave its target burning.", "Une lourde hache de guerre au tranchant chauffé qui peut brûler sa cible.", "Un hacha de guerra pesada con un filo ardiente que puede quemar a su objetivo.", "Una pesante ascia da guerra dal filo rovente che può lasciare il bersaglio in fiamme.", "Eine schwere Streitaxt mit erhitzter Schneide, die ihr Ziel in Brand setzen kann.", "Тяжёлая секира с раскалённым лезвием может поджечь цель.", "Um machado de guerra pesado com fio aquecido que pode deixar o alvo em chamas.");
        Add(result, "Topór Wojenny Watażki", "Warlord's Waraxe", "Hache de guerre du seigneur de guerre", "Hacha de Guerra del Señor de la Guerra", "Ascia da Guerra del Signore della Guerra", "Streitaxt des Kriegsherrn", "Секира военачальника", "Machado de Guerra do Senhor da Guerra");
        Add(result, "Brutalny, dwuręczny topór, który zadaje ogromne obrażenia powolnym zamachem.", "A brutal two-handed axe that delivers tremendous damage with a deliberate swing.", "Une hache brutale à deux mains qui inflige d’énormes dégâts avec un coup lent et maîtrisé.", "Un hacha brutal de dos manos que inflige un daño enorme con un golpe pausado.", "Una brutale ascia a due mani che infligge danni enormi con un colpo ponderato.", "Eine brutale Zweihandaxt, die mit einem bedächtigen Schwung enormen Schaden verursacht.", "Жестокая двуручная секира наносит огромный урон размеренным ударом.", "Um machado brutal de duas mãos que causa muito dano com um golpe calculado.");
        Add(result, "Wiązka z Rotodrzewa", "Rotwood Lash", "Fouet de bois putride", "Látigo de Madera Corrupta", "Frusta di Legnomarcio", "Moderholz-Peitsche", "Кнут из гнилого дерева", "Chicote de Madeira Apodrecida");
        Add(result, "Ciernisty bicz nasączony trucizną, która może przeniknąć do rany.", "A thorned lash treated with poison that can seep into a wounded target.", "Un fouet épineux enduit de poison, capable de pénétrer dans les blessures.", "Un látigo con espinas impregnado de veneno que puede filtrarse en la herida.", "Una frusta spinata intrisa di veleno che può penetrare nelle ferite.", "Eine Dornenpeitsche mit Gift, das in die Wunde eindringen kann.", "Шипастый кнут пропитан ядом, который проникает в раны.", "Um chicote farpado coberto de veneno que pode penetrar nos ferimentos.");
        Add(result, "Bicz Wędrowca", "Wayfarer Whip", "Fouet du voyageur", "Látigo del viajero", "Frusta del viandante", "Peitsche des Wanderers", "Кнут путника", "Chicote do viajante");
        Add(result, "Elastyczny bicz o zasięgu pozwalającym uderzyć, zanim wróg podejdzie bliżej.", "A flexible lash with enough reach to strike before an enemy closes in.", "Un fouet souple assez long pour frapper avant que l’ennemi ne s’approche.", "Un látigo flexible con alcance suficiente para golpear antes de que el enemigo se acerque.", "Una frusta flessibile abbastanza lunga da colpire prima che il nemico si avvicini.", "Eine flexible Peitsche mit genug Reichweite, um zuzuschlagen, bevor der Gegner herankommt.", "Гибкий кнут позволяет ударить врага, прежде чем тот приблизится.", "Um chicote flexível com alcance para golpear antes que o inimigo se aproxime.");

        Add(result, "Łuk Jesionowy", "Ashwood Bow", "Arc en frêne", "Arco de Fresno", "Arco di Frassino", "Eschenbogen", "Ясеневый лук", "Arco de Freixo");
        Add(result, "Lekki i wytrzymały łuk wykonany z sezonowanego jesionu.", "A light, resilient bow carved from seasoned ash.", "Un arc léger et résistant, taillé dans du frêne séché.", "Un arco ligero y resistente tallado en fresno curado.", "Un arco leggero e resistente ricavato da frassino stagionato.", "Ein leichter, robuster Bogen aus abgelagertem Eschenholz.", "Лёгкий и прочный лук из выдержанного ясеня.", "Um arco leve e resistente feito de freixo curado.");
        Add(result, "Elficki Długi Łuk", "Elven Longbow", "Arc long elfique", "Arco largo élfico", "Arco lungo elfico", "Elfischer Langbogen", "Эльфийский длинный лук", "Arco longo élfico");
        Add(result, "Starannie wyważony długi łuk, który posyła potężne strzały na dużą odległość.", "A finely balanced longbow that sends powerful arrows over a great distance.", "Un arc long parfaitement équilibré qui envoie des flèches puissantes à grande distance.", "Un arco largo muy equilibrado que lanza flechas potentes a gran distancia.", "Un arco lungo ben bilanciato che scaglia frecce potenti a grande distanza.", "Ein fein ausbalancierter Langbogen, der mächtige Pfeile weit trägt.", "Тщательно сбалансированный длинный лук посылает мощные стрелы на большое расстояние.", "Um arco longo bem equilibrado que lança flechas poderosas a grandes distâncias.");
        Add(result, "Łuk Skrzydła Żaru", "Emberwing Bow", "Arc Aile-de-braise", "Arco Ala de Ascua", "Arco Ala di Brace", "Glutflügelbogen", "Лук Крыла Углей", "Arco Asa de Brasa");
        Add(result, "Jesionowy łuk, którego ogniste strzały mogą podpalić trafionego wroga.", "An ashwood bow whose fiery arrows can set a struck enemy ablaze.", "Un arc en frêne dont les flèches enflammées peuvent embraser l’ennemi touché.", "Un arco de fresno cuyas flechas ardientes pueden prender fuego al enemigo alcanzado.", "Un arco di frassino le cui frecce infuocate possono incendiare il nemico colpito.", "Ein Eschenbogen, dessen Feuerpfeile getroffene Gegner entzünden können.", "Ясеневый лук с огненными стрелами, способными поджечь врага.", "Um arco de freixo cujas flechas flamejantes podem incendiar o inimigo atingido.");
        Add(result, "Łuk Mroźnego Drewna", "Frostwood Bow", "Arc de bois de givre", "Arco de Madera Gélida", "Arco di Legnogelo", "Frostholzbogen", "Лук Морозного дерева", "Arco de Madeira Gélida");
        Add(result, "Łuk z jasnego drewna, którego lodowe strzały mogą zamrozić cel.", "A palewood bow whose cold arrows can freeze their target.", "Un arc en bois pâle dont les flèches glacées peuvent geler leur cible.", "Un arco de madera clara cuyas flechas heladas pueden congelar al objetivo.", "Un arco di legno chiaro le cui frecce gelide possono congelare il bersaglio.", "Ein Bogen aus hellem Holz, dessen Frostpfeile das Ziel einfrieren können.", "Лук из светлого дерева, чьи ледяные стрелы могут заморозить цель.", "Um arco de madeira clara cujas flechas geladas podem congelar o alvo.");
        Add(result, "Mocny łuk myśliwski", "Hunter's Hardbow", "Arc robuste du chasseur", "Arco Reforzado del Cazador", "Arco Robusto del Cacciatore", "Starker Jagdbogen", "Усиленный лук охотника", "Arco Reforçado do Caçador");
        Add(result, "Solidny łuk stworzony do strzelania cięższymi strzałami z dużą siłą.", "A sturdy bow built to draw a heavier arrow with a forceful shot.", "Un arc solide conçu pour bander des flèches lourdes et tirer avec force.", "Un arco resistente diseñado para tensar flechas pesadas y disparar con fuerza.", "Un arco robusto, progettato per scoccare con forza frecce più pesanti.", "Ein stabiler Bogen für schwere Pfeile und kraftvolle Schüsse.", "Прочный лук для тяжёлых стрел и сильных выстрелов.", "Um arco resistente feito para disparar flechas pesadas com força.");
        Add(result, "Długi Łuk Tropiciela", "Ranger's Longbow", "Arc long du rôdeur", "Arco Largo del Explorador", "Arco Lungo del Ricognitore", "Langbogen des Waldläufers", "Длинный лук следопыта", "Arco Longo do Batedor");
        Add(result, "Wysoki, potężny łuk, który nagradza pewny cel mocnym strzałem.", "A tall, powerful bow that rewards a steady aim with a hard-hitting shot.", "Un arc haut et puissant qui récompense une visée stable par un tir redoutable.", "Un arco alto y potente que recompensa la buena puntería con un disparo contundente.", "Un arco alto e potente che premia la mira salda con un tiro devastante.", "Ein hoher, kraftvoller Bogen, der ruhiges Zielen mit einem wuchtigen Schuss belohnt.", "Высокий мощный лук вознаграждает твёрдую руку сильным выстрелом.", "Um arco alto e poderoso que recompensa a mira firme com um disparo contundente.");
        Add(result, "Łuk Cierniowego Grzbietu", "Thornspine Bow", "Arc Épine-dorsale", "Arco Espina de Zarza", "Arco Spina Spinosa", "Dornrückenbogen", "Лук Тернового хребта", "Arco Espinha de Espinho");
        Add(result, "Łuk ozdobiony cierniami, którego kolczaste strzały mogą zatruć cel.", "A thorn-marked bow whose barbed arrows can poison their target.", "Un arc orné d’épines dont les flèches barbelées peuvent empoisonner leur cible.", "Un arco marcado con espinas cuyas flechas con púas pueden envenenar al objetivo.", "Un arco ornato di spine le cui frecce uncinate possono avvelenare il bersaglio.", "Ein dornverzierter Bogen, dessen Widerhakenpfeile das Ziel vergiften können.", "Лук с шипами, чьи зазубренные стрелы могут отравить цель.", "Um arco marcado por espinhos cujas flechas farpadas podem envenenar o alvo.");
        Add(result, "Łuk Myśliwego", "Hunter's Bow", "Arc du chasseur", "Arco del cazador", "Arco del cacciatore", "Jagdbogen", "Лук охотника", "Arco do caçador");
        Add(result, "Prosty i niezawodny łuk, dobry na pierwsze polowanie.", "A simple, reliable bow suited to a first hunt.", "Un arc simple et fiable, idéal pour une première chasse.", "Un arco sencillo y fiable, ideal para la primera cacería.", "Un arco semplice e affidabile, adatto alla prima caccia.", "Ein einfacher, zuverlässiger Bogen für die erste Jagd.", "Простой надёжный лук для первой охоты.", "Um arco simples e confiável, ideal para a primeira caçada.");
        Add(result, "Kostur Ziemi", "Earth staff", "Bâton de terre", "Bastón de tierra", "Bastone della terra", "Erdstab", "Посох земли", "Cajado da terra");
        Add(result, "Miota kamieniami.", "Shooting with rocks.", "Lance des pierres.", "Lanza piedras.", "Lancia pietre.", "Verschießt Steine.", "Метает камни.", "Dispara pedras.");
        Add(result, "Kostur Ognia", "Fire staff", "Bâton de feu", "Bastón de fuego", "Bastone del fuoco", "Feuerstab", "Огненный посох", "Cajado de fogo");
        Add(result, "Może podpalić początkującego czarodzieja.", "It can burn a beginner wizard.", "Il peut brûler un sorcier débutant.", "Puede quemar a un mago novato.", "Può bruciare un mago alle prime armi.", "Er kann einen Zauberlehrling verbrennen.", "Может поджечь начинающего волшебника.", "Pode queimar um mago iniciante.");
        Add(result, "Kostur Lodu", "Ice staff", "Bâton de glace", "Bastón de hielo", "Bastone di ghiaccio", "Eisstab", "Ледяной посох", "Cajado de gelo");
        Add(result, "Zamrażająco zimny.", "Freeeeeezing", "Glacial.", "Congelante.", "Gelidissimo.", "Eiskalt.", "Леденящий холод.", "Frio congelante.");

        // Keys, crafting materials, trophies, currency and quest crystals.
        Add(result, "Klucz do celi", "Key to cell", "Clé de la cellule", "Llave de la celda", "Chiave della cella", "Zellenschlüssel", "Ключ от камеры", "Chave da cela");
        Add(result, "Stary klucz zabrany hersztowi bandytów.", "Old key, taken from bandit chief.", "Une vieille clé prise au chef des bandits.", "Una llave antigua tomada del jefe de los bandidos.", "Una vecchia chiave sottratta al capo dei banditi.", "Ein alter Schlüssel, dem Banditenhauptmann abgenommen.", "Старый ключ, отнятый у главаря бандитов.", "Uma chave antiga tomada do chefe dos bandidos.");
        Add(result, "Kryształ ognia", "Fire crystal", "Cristal de feu", "Cristal de fuego", "Cristallo di fuoco", "Feuerkristall", "Огненный кристалл", "Cristal de fogo");
        Add(result, "Może zakląć broń lub pancerz.", "Can enchant a weapon or armor.", "Peut enchanter une arme ou une armure.", "Puede encantar un arma o una armadura.", "Può incantare un’arma o un’armatura.", "Kann eine Waffe oder Rüstung verzaubern.", "Может зачаровать оружие или броню.", "Pode encantar uma arma ou armadura.");
        Add(result, "Pole warzywne", "Vegetables field", "Potager", "Huerto", "Campo di verdure", "Gemüsefeld", "Огород", "Horta");
        Add(result, "Brudny klucz do pola warzyw.", "Dirty key to vegetables field.", "Une clé sale du potager.", "Una llave sucia del huerto.", "Una chiave sporca del campo di verdure.", "Ein schmutziger Schlüssel zum Gemüsefeld.", "Грязный ключ от огорода.", "Uma chave suja da horta.");
        Add(result, "Prosty klucz", "Simple key", "Clé simple", "Llave sencilla", "Chiave semplice", "Einfacher Schlüssel", "Простой ключ", "Chave simples");
        Add(result, "Prosty klucz do drzwi.", "Simple key to doors.", "Une simple clé de porte.", "Una llave sencilla para puertas.", "Una semplice chiave per le porte.", "Ein einfacher Schlüssel für Türen.", "Простой ключ от дверей.", "Uma chave simples para portas.");
        Add(result, "Klucz do ogrodu zielarki", "Herbalist's garden key", "Clé du jardin de l’herboriste", "Llave del jardín de la herborista", "Chiave del giardino dell’erborista", "Schlüssel zum Kräutergarten", "Ключ от сада травницы", "Chave do jardim da herbalista");
        Add(result, "Brudny klucz do ogrodu zielarki.", "Dirty key to herbalist's garden.", "Une clé sale du jardin de l’herboriste.", "Una llave sucia del jardín de la herborista.", "Una chiave sporca del giardino dell’erborista.", "Ein schmutziger Schlüssel zum Kräutergarten.", "Грязный ключ от сада травницы.", "Uma chave suja do jardim da herbalista.");
        Add(result, "Klucz do magazynu", "Warehouse key", "Clé de l’entrepôt", "Llave del almacén", "Chiave del magazzino", "Lagerschlüssel", "Ключ от склада", "Chave do depósito");
        Add(result, "Stary klucz do magazynu w Moonveil.", "Old key to warehouse in Moonveil.", "Une vieille clé de l’entrepôt de Moonveil.", "Una llave antigua del almacén de Moonveil.", "Una vecchia chiave del magazzino di Moonveil.", "Ein alter Schlüssel zum Lagerhaus in Moonveil.", "Старый ключ от склада в Мунвейле.", "Uma chave antiga do depósito em Moonveil.");
        Add(result, "Lunar", "Lunar", "Lunar", "Lunar", "Lunare", "Lunar", "Лунар", "Lunar");
        Add(result, "Moneta średniej wartości.", "Medium coin.", "Une pièce de valeur moyenne.", "Moneda de valor medio.", "Moneta di valore medio.", "Münze von mittlerem Wert.", "Монета средней ценности.", "Moeda de valor médio.");
        Add(result, "Odłamek", "Shard", "Éclat", "Fragmento", "Frammento", "Splitter", "Осколок", "Fragmento");
        Add(result, "Moneta o najniższej wartości.", "The least valuable coin.", "La pièce de moindre valeur.", "La moneda de menor valor.", "La moneta di minor valore.", "Die Münze mit dem geringsten Wert.", "Самая дешёвая монета.", "A moeda de menor valor.");
        Add(result, "Solar", "Solar", "Solaire", "Solar", "Solare", "Solar", "Солар", "Solar");
        Add(result, "Solar, najcenniejsza moneta w całym świecie Lunaris.", "Solar, the most valuable coin in the whole world of Lunaris.", "Le Solar, la pièce la plus précieuse du monde de Lunaris.", "El solar, la moneda más valiosa de todo el mundo de Lunaris.", "Il Solar, la moneta più preziosa di tutto il mondo di Lunaris.", "Der Solar, die wertvollste Münze in ganz Lunaris.", "Солар — самая ценная монета во всём мире Лунарис.", "O Solar, a moeda mais valiosa de todo o mundo de Lunaris.");
        Add(result, "Wiedza Aulli", "Aulla's Knowledge", "Savoir d’Aulla", "Conocimiento de Aulla", "Conoscenza di Aulla", "Aullas Wissen", "Знание Ауллы", "Conhecimento de Aulla");
        Add(result, "Wiedza Aulli pozwala opuścić tę mapę.", "Aulla's knowledge lets you leave this map.", "Le savoir d’Aulla vous permet de quitter cette carte.", "El conocimiento de Aulla te permite salir de este mapa.", "La conoscenza di Aulla ti permette di lasciare questa mappa.", "Aullas Wissen lässt dich diese Karte verlassen.", "Знание Ауллы позволяет покинуть эту карту.", "O conhecimento de Aulla permite que você saia deste mapa.");
        Add(result, "Surowe mięso", "Raw meat", "Viande crue", "Carne cruda", "Carne cruda", "Rohes Fleisch", "Сырое мясо", "Carne crua");
        Add(result, "Surowe mięso. Możesz je przyrządzić w kuchni.", "Raw meat. You can cook it in the kitchen.", "De la viande crue. Vous pouvez la cuisiner dans la cuisine.", "Carne cruda. Puedes cocinarla en la cocina.", "Carne cruda. Puoi cucinarla in cucina.", "Rohes Fleisch. Du kannst es in der Küche zubereiten.", "Сырое мясо. Его можно приготовить на кухне.", "Carne crua. Você pode prepará-la na cozinha.");
        Add(result, "Ruda żelaza", "Iron ore", "Minerai de fer", "Mineral de hierro", "Minerale di ferro", "Eisenerz", "Железная руда", "Minério de ferro");
        Add(result, "Twardsza od skały.", "Harder than rock.", "Plus dur que la roche.", "Más duro que la roca.", "Più duro della roccia.", "Härter als Stein.", "Твёрже камня.", "Mais duro que a rocha.");
        Add(result, "Lina", "Rope", "Corde", "Cuerda", "Corda", "Seil", "Верёвка", "Corda");
        Add(result, "Przyda się, jeśli chcesz gdzieś się wspiąć.", "Useful if you want to climb somewhere.", "Utile pour escalader un endroit.", "Útil para trepar a algún sitio.", "Utile per arrampicarsi da qualche parte.", "Nützlich, wenn du irgendwo hinaufklettern willst.", "Пригодится, если нужно куда-нибудь забраться.", "Útil para subir em algum lugar.");
        Add(result, "Kamień", "Stone", "Pierre", "Piedra", "Pietra", "Stein", "Камень", "Pedra");
        Add(result, "Twarda skała.", "Hard rock.", "Une roche dure.", "Roca dura.", "Roccia dura.", "Harter Stein.", "Твёрдая порода.", "Rocha dura.");
        Add(result, "Drewno", "Wood", "Bois", "Madera", "Legno", "Holz", "Древесина", "Madeira");
        Add(result, "Dębowe drewno.", "Oak wood.", "Bois de chêne.", "Madera de roble.", "Legno di quercia.", "Eichenholz.", "Дубовая древесина.", "Madeira de carvalho.");
        Add(result, "Pokrzyk", "Belladonna", "Belladone", "Belladona", "Belladonna", "Tollkirsche", "Белладонна", "Beladona");
        Add(result, "Świeżo zebrana belladonna — niebezpieczny, lecz cenny składnik dla doświadczonych alchemików.", "Freshly gathered belladonna, a dangerous but valuable ingredient for experienced alchemists.", "De la belladone fraîchement cueillie, un ingrédient dangereux mais précieux pour les alchimistes expérimentés.", "Belladona recién recolectada, un ingrediente peligroso pero valioso para alquimistas experimentados.", "Belladonna appena raccolta, un ingrediente pericoloso ma prezioso per alchimisti esperti.", "Frisch gepflückte Tollkirsche, eine gefährliche, aber wertvolle Zutat für erfahrene Alchemisten.", "Свежесобранная белладонна — опасный, но ценный ингредиент для опытных алхимиков.", "Beladona recém-colhida, um ingrediente perigoso, mas valioso para alquimistas experientes.");
        Add(result, "Lulek", "Henbane", "Jusquiame", "Beleño", "Giusquiamo", "Bilsenkraut", "Белена", "Meimendro");
        Add(result, "Trująca roślina o silnym, nieprzewidywalnym działaniu. Doświadczeni alchemicy wykorzystują ją do tworzenia niebezpiecznych mikstur.", "A poisonous herb with potent and unpredictable effects. Experienced alchemists use it to prepare dangerous mixtures.", "Une herbe toxique aux effets puissants et imprévisibles. Les alchimistes expérimentés l’utilisent pour préparer des mélanges dangereux.", "Una hierba venenosa de efectos potentes e impredecibles. Los alquimistas expertos la usan para preparar mezclas peligrosas.", "Un’erba velenosa dagli effetti potenti e imprevedibili. Gli alchimisti esperti la usano per preparare miscele pericolose.", "Ein giftiges Kraut mit starker, unberechenbarer Wirkung. Erfahrene Alchemisten bereiten damit gefährliche Mixturen zu.", "Ядовитая трава с сильным и непредсказуемым действием. Опытные алхимики готовят из неё опасные смеси.", "Uma erva venenosa de efeitos fortes e imprevisíveis. Alquimistas experientes a usam para preparar misturas perigosas.");
        Add(result, "Mandragora", "Mandrake", "Mandragore", "Mandrágora", "Mandragola", "Alraune", "Мандрагора", "Mandrágora");
        Add(result, "Dziwny korzeń, któremu przypisuje się potężne właściwości magiczne. Alchemicy cenią go za możliwość warzenia silnych mikstur.", "A strange root said to hold potent magical properties. Highly valued by alchemists for brewing powerful concoctions.", "Une racine étrange aux puissantes propriétés magiques, très prisée des alchimistes pour leurs préparations.", "Una raíz extraña con poderosas propiedades mágicas, muy apreciada por los alquimistas para sus pociones.", "Una radice misteriosa dalle potenti proprietà magiche, molto apprezzata dagli alchimisti per i loro preparati.", "Eine seltsame Wurzel mit starken magischen Eigenschaften, die Alchemisten für mächtige Tränke schätzen.", "Странный корень, которому приписывают сильные магические свойства. Алхимики ценят его для создания мощных зелий.", "Uma raiz estranha com poderosas propriedades mágicas, muito valorizada por alquimistas em suas poções.");
        Add(result, "Grzyb", "Mushroom", "Champignon", "Seta", "Fungo", "Pilz", "Гриб", "Cogumelo");
        Add(result, "Dziki grzyb zebrany na leśnym runie. Zależnie od przygotowania może być przydatnym składnikiem alchemicznym.", "A wild mushroom gathered from the forest floor. Depending on its preparation, it can serve as a useful ingredient in alchemy.", "Un champignon sauvage cueilli au sol de la forêt. Selon sa préparation, il peut être utile en alchimie.", "Una seta silvestre del suelo del bosque. Según cómo se prepare, puede ser un ingrediente útil para la alquimia.", "Un fungo selvatico raccolto sul terreno della foresta. A seconda della preparazione, può essere utile in alchimia.", "Ein wilder Pilz vom Waldboden. Je nach Zubereitung eignet er sich als Zutat für die Alchemie.", "Дикий гриб с лесной подстилки. В зависимости от приготовления может стать полезным ингредиентом для алхимии.", "Um cogumelo silvestre do chão da floresta. Dependendo do preparo, pode ser um ingrediente útil para alquimia.");
        Add(result, "Mak", "Poppy", "Pavot", "Amapola", "Papavero", "Mohn", "Мак", "Papoula");
        Add(result, "Ziele alchemiczna używana do przygotowywania kojących i innych specjalistycznych mikstur.", "An alchemical herb used to prepare soothing mixtures and other specialised concoctions.", "Une herbe alchimique utilisée pour préparer des mélanges apaisants et d’autres concoctions spécialisées.", "Una hierba alquímica usada para preparar mezclas calmantes y otras pociones especializadas.", "Un’erba alchemica usata per preparare miscele lenitive e altri composti speciali.", "Ein alchemistisches Kraut für beruhigende Mischungen und andere Spezialtränke.", "Алхимическая трава для приготовления успокаивающих и других особых смесей.", "Uma erva alquímica usada para preparar misturas calmantes e outras fórmulas especiais.");
        Add(result, "Krew", "Blood", "Sang", "Sangre", "Sangue", "Blut", "Кровь", "Sangue");
        Add(result, "Odrobina świeżej krwi. Mroczny składnik alchemiczny do nietypowych mikstur.", "A little fresh blood. A dark alchemical ingredient used in unusual concoctions.", "Un peu de sang frais, un ingrédient alchimique sombre pour des préparations insolites.", "Un poco de sangre fresca, un ingrediente alquímico oscuro para mezclas inusuales.", "Un po’ di sangue fresco, un ingrediente alchemico oscuro per preparati insoliti.", "Etwas frisches Blut, eine düstere alchemistische Zutat für ungewöhnliche Mixturen.", "Немного свежей крови — мрачный алхимический ингредиент для необычных смесей.", "Um pouco de sangue fresco, um ingrediente alquímico sombrio para fórmulas incomuns.");
        Add(result, "Kość", "Bone", "Os", "Hueso", "Osso", "Knochen", "Кость", "Osso");
        Add(result, "Czysta kość, przydatna do rzemiosła i eksperymentów alchemicznych.", "A clean bone. Useful for crafting and alchemical experiments.", "Un os propre, utile à l’artisanat et aux expériences alchimiques.", "Un hueso limpio, útil para fabricar objetos y hacer experimentos alquímicos.", "Un osso pulito, utile per creare oggetti e fare esperimenti alchemici.", "Ein sauberer Knochen, nützlich fürs Handwerk und alchemistische Experimente.", "Чистая кость, пригодная для ремесла и алхимических экспериментов.", "Um osso limpo, útil para criação e experimentos alquímicos.");
        Add(result, "Kieł", "Fang", "Croc", "Colmillo", "Zanna", "Fang", "Клык", "Presa");
        Add(result, "Ostry kieł ceniony przez łowców i przydatny do tworzenia wytrzymałego ekwipunku.", "A sharp fang. Valued by hunters and useful for crafting rugged equipment.", "Un croc acéré, apprécié des chasseurs et utile pour fabriquer de l’équipement robuste.", "Un colmillo afilado, apreciado por cazadores y útil para fabricar equipo resistente.", "Una zanna affilata, apprezzata dai cacciatori e utile per creare equipaggiamento robusto.", "Ein scharfer Fang, bei Jägern begehrt und nützlich für robuste Ausrüstung.", "Острый клык ценят охотники; он пригодится для создания прочного снаряжения.", "Uma presa afiada, valorizada por caçadores e útil para criar equipamentos resistentes.");
        Add(result, "Pióro", "Feather", "Plume", "Pluma", "Piuma", "Feder", "Перо", "Pena");
        Add(result, "Miękkie pióro przydatne do rzemiosła i ozdób.", "A soft feather. Useful for crafting and decorative items.", "Une plume douce, utile à l’artisanat et aux décorations.", "Una pluma suave, útil para fabricar objetos y adornos.", "Una piuma morbida, utile per creare oggetti e decorazioni.", "Eine weiche Feder, nützlich fürs Handwerk und für Verzierungen.", "Мягкое перо, пригодное для ремесла и украшений.", "Uma pena macia, útil para criação e itens decorativos.");
        Add(result, "Mięso", "Meat", "Viande", "Carne", "Carne", "Fleisch", "Мясо", "Carne");
        Add(result, "Świeże mięso, które można przyrządzić jako pożywny posiłek.", "Fresh meat. Can be cooked into a nourishing meal.", "De la viande fraîche à cuisiner pour en faire un repas nourrissant.", "Carne fresca que puedes cocinar para preparar una comida nutritiva.", "Carne fresca da cucinare per preparare un pasto nutriente.", "Frisches Fleisch, das du zu einer nahrhaften Mahlzeit zubereiten kannst.", "Свежее мясо, из которого можно приготовить сытное блюдо.", "Carne fresca que pode ser preparada como uma refeição nutritiva.");
        Add(result, "Wilcza skóra", "Wolf skin", "Peau de loup", "Piel de lobo", "Pelle di lupo", "Wolfsfell", "Волчья шкура", "Pele de lobo");
        Add(result, "Skóra wilka.", "Skin from a wolf.", "Peau provenant d’un loup.", "Piel obtenida de un lobo.", "Pelle ricavata da un lupo.", "Fell eines Wolfs.", "Шкура волка.", "Pele obtida de um lobo.");
        Add(result, "Kryształ Aulli", "Aulla crystal", "Cristal d’Aulla", "Cristal de Aulla", "Cristallo di Aulla", "Aulla-Kristall", "Кристалл Ауллы", "Cristal de Aulla");
        Add(result, "Służy do ulepszania osady.", "Used to upgrade your settlement.", "Sert à améliorer votre village.", "Sirve para mejorar tu asentamiento.", "Serve a migliorare il tuo insediamento.", "Wird zum Ausbau deiner Siedlung verwendet.", "Используется для улучшения поселения.", "Usado para aprimorar seu assentamento.");

        Add(result, "Książka testowa", "TestBook", "Livre de test", "Libro de prueba", "Libro di prova", "Testbuch", "Тестовая книга", "Livro de teste");
        Add(result, "Opis testowy.", "Desc", "Description de test.", "Descripción de prueba.", "Descrizione di prova.", "Testbeschreibung.", "Тестовое описание.", "Descrição de teste.");
        Add(result, "List bandyty", "Bandit's letter", "Lettre du bandit", "Carta del bandido", "Lettera del bandito", "Brief des Banditen", "Письмо бандита", "Carta do bandido");
        Add(result, "Niedawno napisany list wciąż pachnie męskimi perfumami i wrogością.", "The recently written letter still reeks of male perfume and hostility.", "La lettre récemment écrite empeste encore le parfum masculin et l’hostilité.", "La carta recién escrita aún apesta a perfume masculino y hostilidad.", "La lettera appena scritta puzza ancora di profumo maschile e ostilità.", "Der kürzlich geschriebene Brief riecht noch immer nach Herrenparfüm und Feindseligkeit.", "От недавно написанного письма всё ещё веет мужским одеколоном и враждебностью.", "A carta recém-escrita ainda cheira a perfume masculino e hostilidade.");
        Add(result, "List do Hendrika", "Letter to Hendrik", "Lettre à Hendrik", "Carta para Hendrik", "Lettera a Hendrik", "Brief an Hendrik", "Письмо Хендрику", "Carta para Hendrik");
        Add(result, "List jest lekko nadpalony, ale jego treść ocalała.", "The letter is a bit burnt, but the content is preserved.", "La lettre est un peu brûlée, mais son contenu est intact.", "La carta está algo quemada, pero su contenido se conserva.", "La lettera è un po’ bruciata, ma il contenuto è salvo.", "Der Brief ist etwas verbrannt, aber sein Inhalt ist erhalten geblieben.", "Письмо немного обгорело, но его содержание сохранилось.", "A carta está um pouco queimada, mas o conteúdo foi preservado.");

        // Alternate source spellings preserved in existing ScriptableObject text.
        Add(result, "Może zakląć broń lub pancerz.", "Can enchant weapon or armor.", "Peut enchanter une arme ou une armure.", "Puede encantar un arma o una armadura.", "Può incantare un’arma o un’armatura.", "Kann eine Waffe oder Rüstung verzaubern.", "Может зачаровать оружие или броню.", "Pode encantar uma arma ou armadura.");
        Add(result, "Mamma mia! Pyszna pizza bez ananasa, która zwiększa wszystkie statystyki!", "Mamma mia! Delicious pizza without pineapple which increase all of your stats!", "Mamma mia ! Une délicieuse pizza sans ananas qui augmente toutes vos statistiques !", "¡Mamma mía! Una pizza deliciosa sin piña que aumenta todas tus estadísticas.", "Mamma mia! Una pizza deliziosa senza ananas che aumenta tutte le tue statistiche.", "Mamma mia! Eine köstliche Pizza ohne Ananas, die all deine Werte erhöht.", "Мамма миа! Вкусная пицца без ананаса, повышающая все характеристики.", "Mamma mia! Uma pizza deliciosa sem abacaxi que aumenta todos os seus atributos.");
        Add(result, "Kupione od handlarza gazet", "Bought from a paper seller", "Acheté à un vendeur de journaux", "Comprado a un vendedor de periódicos", "Comprata da un venditore di giornali", "Bei einem Zeitungsverkäufer gekauft", "Куплена у газетчика", "Comprada de um jornaleiro");
        Add(result, "Fioletowy i soczysty", "Purple and juicy", "Violet et juteux", "Morado y jugoso", "Viola e succosa", "Violett und saftig", "Фиолетовый и сочный", "Roxo e suculento");
        Add(result, "Dynia na Halloween albo na kolację.", "Pumpkin, for halloween or for dinner.", "Une citrouille, pour Halloween ou pour le dîner.", "Calabaza, para Halloween o para cenar.", "Zucca, per Halloween o per cena.", "Kürbis, für Halloween oder zum Abendessen.", "Тыква — на Хэллоуин или на ужин.", "Abóbora, para o Halloween ou para o jantar.");
        Add(result, "Soczysty i czerwony, jak hemoroidy.", "Juicy and red, like your hemorrhoids.", "Juteux et rouge, comme vos hémorroïdes.", "Jugoso y rojo, como tus hemorroides.", "Succoso e rosso, come le tue emorroidi.", "Saftig und rot wie deine Hämorrhoiden.", "Сочный и красный, как твой геморрой.", "Suculento e vermelho como suas hemorroidas.");
        Add(result, "Zdrowa rzepa. Pychota.", "Healthy turnip. Yummy.", "Un navet bon pour la santé. Miam.", "Un nabo saludable. Qué rico.", "Una rapa sana. Che bontà.", "Gesunde Rübe. Lecker.", "Полезная репа. Вкусно.", "Nabo saudável. Que delícia.");
        Add(result, "Mała mikstura zdrowia.", "Tiny health potion", "Une toute petite potion de santé.", "Una poción de salud diminuta.", "Una piccola pozione della salute.", "Ein winziger Heiltrank.", "Крошечное зелье здоровья.", "Uma poção de vida pequenina.");
        Add(result, "Przywraca energię, by móc rzucać więcej kul ognia. Ta mikstura jest duża.", "Restores your energy. So you can throw more fireballs. But it is large.", "Restaure votre énergie pour lancer plus de boules de feu. Cette potion est grande.", "Restaura tu energía para lanzar más bolas de fuego. Esta poción es grande.", "Ripristina energia per lanciare più palle di fuoco. Questa pozione è grande.", "Stellt Energie wieder her, damit du mehr Feuerbälle werfen kannst. Dieser Trank ist groß.", "Восстанавливает энергию, чтобы метать больше огненных шаров. Это большое зелье.", "Recupera sua energia para lançar mais bolas de fogo. Esta poção é grande.");
        Add(result, "Przywraca energię, by móc rzucać więcej kul ognia. Ta mikstura ma średnią moc.", "Restores your energy. So you can throw more fireballs. This one is medium.", "Restaure votre énergie pour lancer plus de boules de feu. Cette potion est de puissance moyenne.", "Restaura tu energía para lanzar más bolas de fuego. Esta poción tiene una potencia media.", "Ripristina energia per lanciare più palle di fuoco. Questa pozione è di media potenza.", "Stellt Energie wieder her, damit du mehr Feuerbälle werfen kannst. Dieser Trank ist mittelstark.", "Восстанавливает энергию, чтобы метать больше огненных шаров. Это зелье средней силы.", "Recupera sua energia para lançar mais bolas de fogo. Esta poção tem efeito médio.");
        Add(result, "Przywraca energię, by móc rzucać więcej kul ognia.", "Restores your energy. So you can throw more fireballs.", "Restaure votre énergie pour lancer plus de boules de feu.", "Restaura tu energía para lanzar más bolas de fuego.", "Ripristina energia per lanciare più palle di fuoco.", "Stellt Energie wieder her, damit du mehr Feuerbälle werfen kannst.", "Восстанавливает энергию, чтобы метать больше огненных шаров.", "Recupera sua energia para lançar mais bolas de fogo.");
        Add(result, "Zwiększ swoją szybkość", "Increase your speed", "Augmentez votre vitesse", "Aumenta tu velocidad", "Aumenta la tua velocità", "Erhöhe dein Tempo", "Увеличь скорость", "Aumente sua velocidade");
        Add(result, "Zwiększ swoje obrażenia.", "Increase your damage.", "Augmentez vos dégâts.", "Aumenta tu daño.", "Aumenta i tuoi danni.", "Erhöhe deinen Schaden.", "Увеличь наносимый урон.", "Aumente seu dano.");
        Add(result, "Skóra warstwowa wzmocniona płytkami z cierniowego drewna łączy ochronę ze swobodą ruchu.", "Layered leather reinforced with thornwood plates, balancing protection with freedom of movement.", "Un cuir multicouche renforcé de plaques de bois épineux allie protection et liberté de mouvement.", "El cuero por capas reforzado con placas de madera espinosa combina protección y libertad de movimiento.", "Il cuoio a strati rinforzato con placche di legno spinoso bilancia protezione e libertà di movimento.", "Mehrlagiges Leder mit Dornholzplatten verbindet Schutz mit Bewegungsfreiheit.", "Многослойная кожа с накладками из колючего дерева сочетает защиту и свободу движений.", "O couro em camadas, reforçado com placas de madeira espinhosa, equilibra proteção e liberdade de movimento.");
        Add(result, "Dopasowany kaptur chroni oczy podróżnika przed deszczem i gałęziami.", "A close-fitting hood that keeps rain and branches away from a traveller's eyes.", "Une capuche ajustée qui protège les yeux du voyageur de la pluie et des branches.", "Una capucha ajustada que protege los ojos del viajero de la lluvia y las ramas.", "Un cappuccio aderente che protegge gli occhi del viandante da pioggia e rami.", "Eine eng anliegende Kapuze, die Regen und Äste von den Augen des Reisenden fernhält.", "Плотно сидящий капюшон защищает глаза путника от дождя и веток.", "Um capuz justo que protege os olhos do viajante da chuva e dos galhos.");
        Add(result, "Prosty klucz do drzwi.", "Simple key to doors", "Une simple clé de porte.", "Una llave sencilla para puertas.", "Una semplice chiave per le porte.", "Ein einfacher Schlüssel für Türen.", "Простой ключ от дверей.", "Uma chave simples para portas.");
        Add(result, "Solar, najcenniejsza moneta w całym świecie Lunaris.", "Solar, the most valuable coin in whole Lunaris World.", "Le Solar, la pièce la plus précieuse de tout le monde de Lunaris.", "El solar, la moneda más valiosa de todo el mundo de Lunaris.", "Il Solar, la moneta più preziosa di tutto il mondo di Lunaris.", "Der Solar, die wertvollste Münze in ganz Lunaris.", "Солар — самая ценная монета во всём мире Лунарис.", "O Solar, a moeda mais valiosa de todo o mundo de Lunaris.");
        Add(result, "Wiedza Aulli pozwala opuścić tę mapę.", "Aullas knowledge to leave this map", "Le savoir d’Aulla permet de quitter cette carte.", "El conocimiento de Aulla permite salir de este mapa.", "La conoscenza di Aulla permette di lasciare questa mappa.", "Aullas Wissen lässt dich diese Karte verlassen.", "Знание Ауллы позволяет покинуть эту карту.", "O conhecimento de Aulla permite sair deste mapa.");
        Add(result, "Surowe mięso. Możesz je przyrządzić w kuchni.", "Raw meat. You can cook it at chef's room.", "De la viande crue. Vous pouvez la cuisiner dans la cuisine.", "Carne cruda. Puedes cocinarla en la cocina.", "Carne cruda. Puoi cucinarla in cucina.", "Rohes Fleisch. Du kannst es in der Küche zubereiten.", "Сырое мясо. Его можно приготовить на кухне.", "Carne crua. Você pode prepará-la na cozinha.");
        Add(result, "Przyda się, jeśli chcesz gdzieś się wspiąć.", "If you want to climb somewhere.", "Utile pour escalader un endroit.", "Útil para trepar a algún sitio.", "Utile per arrampicarsi da qualche parte.", "Nützlich, wenn du irgendwo hinaufklettern willst.", "Пригодится, если нужно куда-нибудь забраться.", "Útil para subir em algum lugar.");
        Add(result, "Twarda skała.", "Hard rock", "Une roche dure.", "Roca dura.", "Roccia dura.", "Harter Stein.", "Твёрдая порода.", "Rocha dura.");
        Add(result, "Dębowe drewno.", "Oak wood", "Bois de chêne.", "Madera de roble.", "Legno di quercia.", "Eichenholz.", "Дубовая древесина.", "Madeira de carvalho.");
        Add(result, "Odrobina świeżej krwi. Mroczny składnik alchemiczny do nietypowych mikstur.", "A little bit of fresh blood. A dark alchemical ingredient used in unusual concoctions.", "Un peu de sang frais, un ingrédient alchimique sombre pour des préparations insolites.", "Un poco de sangre fresca, un ingrediente alquímico oscuro para mezclas inusuales.", "Un po’ di sangue fresco, un ingrediente alchemico oscuro per preparati insoliti.", "Etwas frisches Blut, eine düstere alchemistische Zutat für ungewöhnliche Mixturen.", "Немного свежей крови — мрачный алхимический ингредиент для необычных смесей.", "Um pouco de sangue fresco, um ingrediente alquímico sombrio para fórmulas incomuns.");
        Add(result, "Skóra wilka.", "Skin from wolf", "Peau provenant d’un loup.", "Piel obtenida de un lobo.", "Pelle ricavata da un lupo.", "Fell eines Wolfs.", "Шкура волка.", "Pele obtida de um lobo.");
        Add(result, "Służy do ulepszania osady.", "Use to upgrade your base", "Sert à améliorer votre village.", "Sirve para mejorar tu asentamiento.", "Serve a migliorare il tuo insediamento.", "Wird zum Ausbau deiner Siedlung verwendet.", "Используется для улучшения поселения.", "Usado para aprimorar seu assentamento.");
        Add(result, "Kryształ Nythera", "Nythera crystal", "Cristal de Nythera", "Cristal de Nythera", "Cristallo di Nythera", "Nythera-Kristall", "Кристалл Нитеры", "Cristal de Nythera");
        Add(result, "Kryształ Ottona", "Otton crystal", "Cristal d’Otton", "Cristal de Otton", "Cristallo di Otton", "Otton-Kristall", "Кристалл Оттона", "Cristal de Otton");
        Add(result, "Kryształ Sylvara", "Sylvar crystal", "Cristal de Sylvar", "Cristal de Sylvar", "Cristallo di Sylvar", "Sylvar-Kristall", "Кристалл Сильвара", "Cristal de Sylvar");
        Add(result, "Kryształ Udrosa", "Udros crystal", "Cristal d’Udros", "Cristal de Udros", "Cristallo di Udros", "Udros-Kristall", "Кристалл Удроса", "Cristal de Udros");
        Add(result, "Służy do ulepszania osady.", "Used to upgrade your settlement.", "Sert à améliorer votre village.", "Sirve para mejorar tu asentamiento.", "Serve a migliorare il tuo insediamento.", "Wird zum Ausbau deiner Siedlung verwendet.", "Используется для улучшения поселения.", "Usado para aprimorar seu assentamento.");
    }

    private static void AddQuestTranslations(Dictionary<string, string[]> map)
    {
        // NEW QUESTS are authored in English. Keep the translations in this existing
        // localization table so the Quest ScriptableObjects need no JSON sidecars.
        Add(map, "Halloween", "Halloween", "Halloween", "Halloween", "Halloween", "Halloween", "Хэллоуин");
        Add(map, "Ucichnij, Ostrze", "Silence the Blade", "Faire taire la Lame", "Silencia la Hoja", "Metti a tacere la Lama", "Bring die Klinge zum Schweigen", "Утихомирить Клинок");
        Add(map, "Ucichnij Ogary", "Quiet the Hounds", "Faire taire les Chiens", "Silencia a los Sabuesos", "Metti a tacere i Segugi", "Beruhige die Jagdhunde", "Утихомирить гончих");
        Add(map, "Szlak Mrocznego Lasu", "The Dark Forest Trail", "Le sentier de la forêt sombre", "El sendero del Bosque Oscuro", "Il sentiero della Foresta Oscura", "Der Pfad durch den Dunkelwald", "Тропа Тёмного леса");
        Add(map, "Rozbij patrol orków", "Break the Orc Patrol", "Brise la patrouille des orcs", "Acaba con la patrulla de orcos", "Spezza la pattuglia degli orchi", "Zerschlage die Orkpatrouille", "Разбей патруль орков");
        Add(map, "W głąb lochu", "Down Into the Dungeon", "Dans les profondeurs du donjon", "A las profundidades de la mazmorra", "Nel profondo del dungeon", "Hinab in den Dungeon", "В глубины подземелья");
        Add(map, "Przebiegnij przez loch", "Run through the dungeon", "Traversée du donjon", "Atraviesa la mazmorra", "Attraversa il dungeon", "Durch den Dungeon", "Пробег по подземелью");
        Add(map, "Leśny trakt", "The Forest Road", "La route forestière", "El camino del bosque", "La strada nella foresta", "Der Waldweg", "Лесная дорога");
        Add(map, "Nieco lepki problem", "A Rather Sticky Problem", "Un problème plutôt collant", "Un problema bastante pegajoso", "Un problema piuttosto appiccicoso", "Ein ziemlich klebriges Problem", "Довольно липкая проблема");
        Add(map, "Zabij zombie", "Kill zombies", "Tuez les zombies", "Mata zombis", "Uccidi gli zombie", "Töte Zombies", "Убейте зомби");
        Add(map, "Ziemniaki do kotła", "Potatoes for the Pot", "Des pommes de terre pour la marmite", "Patatas para la olla", "Patate per la pentola", "Kartoffeln für den Topf", "Картофель в котёл");
        Add(map, "Porządki z czerwoną mazią", "Red Slime Cleanup", "Nettoyage de la gelée rouge", "Limpieza de baba roja", "Pulizia della melma rossa", "Rote Schleime beseitigen", "Уборка красной слизи");
        Add(map, "Echa z podziemi", "Echoes Below", "Échos souterrains", "Ecos bajo tierra", "Echi dal sottosuolo", "Echos aus der Tiefe", "Эхо под городом");
        Add(map, "Brak rzepy", "The Turnip Shortage", "La pénurie de navets", "La escasez de nabos", "La carenza di rape", "Die Rübenknappheit", "Нехватка репы");
        Add(map, "Podążaj tropem wargów", "Follow the Warg Tracks", "Suis les traces des wargs", "Sigue las huellas de los huargos", "Segui le tracce dei warg", "Folge den Spuren der Warge", "Иди по следу варгов");
        Add(map, "Wilczy problem", "Wolf Trouble", "Problème de loups", "Problemas con lobos", "Problemi con i lupi", "Ärger mit Wölfen", "Волчья напасть");

        Add(map, "Przynieś 10 dyń", "Bring 10 pumpkins", "Apporte 10 citrouilles", "Trae 10 calabazas", "Porta 10 zucche", "Bringe 10 Kürbisse", "Принеси 10 тыкв");
        Add(map, "Zabij 5 Ostrzy Bractwa", "Kill 5 Brotherhood Blades", "Tue 5 Lames de la Confrérie", "Mata a 5 Hojas de la Hermandad", "Uccidi 5 Lame della Fratellanza", "Töte 5 Klingen der Bruderschaft", "Убей 5 клинков Братства");
        Add(map, "Zabij 5 Ogary Bractwa", "Kill 5 Brotherhood Hounds", "Tue 5 chiens de la Confrérie", "Mata a 5 sabuesos de la Hermandad", "Uccidi 5 segugi della Fratellanza", "Töte 5 Jagdhunde der Bruderschaft", "Убей 5 гончих Братства");
        Add(map, "Ukończ Mroczny Las", "Complete DarkForest", "Termine la Forêt sombre", "Completa el Bosque Oscuro", "Completa la Foresta Oscura", "Schließe den Dunkelwald ab", "Заверши Тёмный лес");
        Add(map, "Zabij 8 mrocznych orków", "Kill 8 dark orcs", "Tue 8 orcs sombres", "Mata a 8 orcos oscuros", "Uccidi 8 orchi oscuri", "Töte 8 dunkle Orks", "Убей 8 тёмных орков");
        Add(map, "Ukończ Loch", "Complete Dungeon", "Termine le donjon", "Completa la mazmorra", "Completa il dungeon", "Schließe den Dungeon ab", "Заверши подземелье");
        Add(map, "Ukończ mapę „Dungeon”", "Finish \"Dungeon\" map", "Termine la carte « Dungeon »", "Completa el mapa «Dungeon»", "Completa la mappa «Dungeon»", "Schließe die Karte „Dungeon“ ab", "Заверши карту «Dungeon»");
        Add(map, "Ukończ mapę Forest", "Complete the Forest map", "Termine la carte Forest", "Completa el mapa Forest", "Completa la mappa Forest", "Schließe die Karte Forest ab", "Заверши карту Forest");
        Add(map, "Zabij 12 zielonych szlamów", "Kill 12 green slimes", "Tue 12 gelées vertes", "Mata a 12 limos verdes", "Uccidi 12 melme verdi", "Töte 12 grüne Schleime", "Убей 12 зелёных слизней");
        Add(map, "Zabij 10 zombie", "Kill 10 zombies", "Tuez 10 zombies", "Mata 10 zombis", "Uccidi 10 zombie", "Töte 10 Zombies", "Убей 10 зомби");
        Add(map, "Przynieś 10 ziemniaków", "Bring 10 potatoes", "Apporte 10 pommes de terre", "Trae 10 patatas", "Porta 10 patate", "Bringe 10 Kartoffeln", "Принеси 10 картофелин");
        Add(map, "Zabij 10 czerwonych szlamów", "Kill 10 red slimes", "Tue 10 gelées rouges", "Mata a 10 limos rojos", "Uccidi 10 melme rosse", "Töte 10 rote Schleime", "Убей 10 красных слизней");
        Add(map, "Ukończ mapę Kanałów", "Complete the Sewers map", "Termine la carte des Égouts", "Completa el mapa de las Cloacas", "Completa la mappa delle Fogne", "Schließe die Kanalisation ab", "Заверши карту канализации");
        Add(map, "Przynieś 8 rzep", "Bring 8 turnips", "Apporte 8 navets", "Trae 8 nabos", "Porta 8 rape", "Bringe 8 Rüben", "Принеси 8 реп");
        Add(map, "Zabij 5 wargów", "Kill 5 wargs", "Tue 5 wargs", "Mata a 5 huargos", "Uccidi 5 warg", "Töte 5 Warge", "Убей 5 варгов");
        Add(map, "Zabij 6 wilków", "Kill 6 wolves", "Tue 6 loups", "Mata a 6 lobos", "Uccidi 6 lupi", "Töte 6 Wölfe", "Убей 6 волков");

        Add(map, "Dynie: ", "Pumpkins: ", "Citrouilles : ", "Calabazas: ", "Zucche: ", "Kürbisse: ", "Тыквы: ");
        Add(map, "Ostrza Bractwa:", "Brotherhood Blades:", "Lames de la Confrérie :", "Hojas de la Hermandad:", "Lame della Fratellanza:", "Klingen der Bruderschaft:", "Клинки Братства:");
        Add(map, "Ogary Bractwa:", "Brotherhood Hounds:", "Chiens de la Confrérie :", "Sabuesos de la Hermandad:", "Segugi della Fratellanza:", "Jagdhunde der Bruderschaft:", "Гончие Братства:");
        Add(map, "Ukończ Mroczny Las", "Complete Dark Forest", "Termine la Forêt sombre", "Completa el Bosque Oscuro", "Completa la Foresta Oscura", "Schließe den Dunkelwald ab", "Заверши Тёмный лес");
        Add(map, "Mroczne orki:", "Dark orcs:", "Orcs sombres :", "Orcos oscuros:", "Orchi oscuri:", "Dunkle Orks:", "Тёмные орки:");
        Add(map, "Zombie:", "Zombies:", "Zombies :", "Zombis:", "Zombie:", "Zombies:", "Зомби:");
        Add(map, "Ukończ Loch:", "Complete Dungeon:", "Termine le donjon :", "Completa la mazmorra:", "Completa il dungeon:", "Dungeon abschließen:", "Заверши подземелье:");
        Add(map, "Ukończ Las:", "Complete Forest:", "Termine la forêt :", "Completa el bosque:", "Completa la foresta:", "Schließe den Wald ab:", "Заверши лес:");
        Add(map, "Zielone szlamy:", "Green slimes:", "Gelées vertes :", "Limos verdes:", "Melme verdi:", "Grüne Schleime:", "Зелёные слизни:");
        Add(map, "Ziemniaki:", "Potatoes:", "Pommes de terre :", "Patatas:", "Patate:", "Kartoffeln:", "Картофель:");
        Add(map, "Czerwone szlamy:", "Red slimes:", "Gelées rouges :", "Limos rojos:", "Melme rosse:", "Rote Schleime:", "Красные слизни:");
        Add(map, "Ukończ Kanały:", "Complete Sewers:", "Termine les égouts :", "Completa las cloacas:", "Completa le fogne:", "Schließe die Kanalisation ab:", "Заверши канализацию:");
        Add(map, "Rzepy:", "Turnips:", "Navets :", "Nabos:", "Rape:", "Rüben:", "Репа:");
        Add(map, "Wargi:", "Wargs:", "Wargs :", "Huargos:", "Warg:", "Warge:", "Варги:");
        Add(map, "Wilki:", "Wolves:", "Loups :", "Lobos:", "Lupi:", "Wölfe:", "Волки:");
        Add(map, "Ukończono", "Completed", "Terminé", "Completado", "Completato", "Abgeschlossen", "Выполнено");
        Add(map, "Wróć do {0}", "Return to {0}", "Retourne voir {0}", "Vuelve con {0}", "Torna da {0}", "Kehre zu {0} zurück", "Вернись к {0}");
        Add(map, "Ukończono zadanie „{0}”. Pozwól, że wręczę ci nagrodę.", "You have completed {0}. Let me give you your reward.", "Tu as terminé « {0} ». Laisse-moi te remettre ta récompense.", "Has completado «{0}». Déjame darte tu recompensa.", "Hai completato «{0}». Lascia che ti dia la ricompensa.", "Du hast „{0}“ abgeschlossen. Lass mich dir deine Belohnung geben.", "Ты выполнил задание «{0}». Позволь вручить тебе награду.");
        Add(map, "Dziękuję za ukończenie zadania „{0}”. Twoja pomoc wiele dla mnie znaczy.", "Thank you for completing {0}. Your help means a great deal to me.", "Merci d'avoir terminé « {0} ». Ton aide compte beaucoup pour moi.", "Gracias por completar «{0}». Tu ayuda significa mucho para mí.", "Grazie per aver completato «{0}». Il tuo aiuto significa molto per me.", "Danke, dass du „{0}“ abgeschlossen hast. Deine Hilfe bedeutet mir viel.", "Спасибо, что выполнил задание «{0}». Твоя помощь очень много для меня значит.");
        Add(map, "Wciąż pamiętam, jak pomogłeś mi z zadaniem „{0}”. Dziękuję.", "I still remember how you helped me with {0}. Thank you.", "Je me souviens encore de ton aide pour « {0} ». Merci.", "Aún recuerdo cómo me ayudaste con «{0}». Gracias.", "Ricordo ancora come mi hai aiutato con «{0}». Grazie.", "Ich erinnere mich noch, wie du mir bei „{0}“ geholfen hast. Danke.", "Я помню, как ты помог мне с заданием «{0}». Спасибо.");
        Add(map, "Wciąż wszyscy mówią o tym, co zrobiłeś dla zadania „{0}”.", "People are still talking about what you did for {0}.", "On parle encore de ce que tu as fait pour « {0} ».", "La gente sigue hablando de lo que hiciste en «{0}».", "Si parla ancora di ciò che hai fatto per «{0}».", "Die Leute reden noch immer darüber, was du für „{0}“ getan hast.", "Люди всё ещё говорят о том, что ты сделал для задания «{0}».");
        Add(map, "Nie udałoby mi się ukończyć zadania „{0}” bez ciebie.", "I couldn't have finished {0} without you.", "Je n'aurais pas pu terminer « {0} » sans toi.", "No habría podido completar «{0}» sin ti.", "Non avrei potuto completare «{0}» senza di te.", "Ohne dich hätte ich „{0}“ nicht geschafft.", "Без тебя я бы не справился с заданием «{0}».");
        Add(map, "Teraz jest znacznie lepiej, gdy zadanie „{0}” mamy za sobą. Jeszcze raz dziękuję.", "Things are much better now that {0} is behind us. Thanks again.", "Tout va bien mieux maintenant que « {0} » est derrière nous. Encore merci.", "Todo va mucho mejor ahora que hemos dejado atrás «{0}». Gracias de nuevo.", "Ora va molto meglio, con «{0}» alle spalle. Grazie ancora.", "Jetzt, da „{0}“ hinter uns liegt, ist alles viel besser. Nochmals danke.", "Теперь, когда задание «{0}» позади, всё стало намного лучше. Ещё раз спасибо.");
        Add(map, "Ukończono zadanie: ", "Quest completed: ", "Quête terminée : ", "Misión completada: ", "Missione completata: ", "Quest abgeschlossen: ", "Задание выполнено: ");
        Add(map, "Cele zadania ukończone: ", "Quest objectives complete: ", "Objectifs terminés : ", "Objetivos completados: ", "Obiettivi completati: ", "Questziele abgeschlossen: ", "Цели задания выполнены: ");
        Add(map, " Wróć do ", " Return to ", " Retourne voir ", " Vuelve con ", " Torna da ", " Kehre zu ", " Вернись к ");

        Add(map, "Ktoś potrzebuje 10 dyń. Pomożesz mi i przyniesiesz je?", "Someone needs 10 pumpkins. Can you help me and bring them to me?", "Quelqu'un a besoin de 10 citrouilles. Peux-tu m'aider et me les apporter ?", "Alguien necesita 10 calabazas. ¿Puedes ayudarme y traérmelas?", "Qualcuno ha bisogno di 10 zucche. Puoi aiutarmi a portargliele?", "Jemand braucht 10 Kürbisse. Kannst du mir helfen und sie herbringen?", "Кому-то нужны 10 тыкв. Поможешь мне и принесёшь их?");
        Add(map, "Zbiory prawie się skończyły. Przynieś mi te dziesięć dyń, zanim spiżarnia opustoszeje.", "The harvest is nearly gone. Please bring me those ten pumpkins before the pantry is empty.", "La récolte est presque épuisée. Apporte-moi ces dix citrouilles avant que le garde-manger ne soit vide.", "La cosecha casi se ha acabado. Tráeme esas diez calabazas antes de que se vacíe la despensa.", "Il raccolto è quasi finito. Portami quelle dieci zucche prima che la dispensa si svuoti.", "Die Ernte ist fast aufgebraucht. Bring mir bitte die zehn Kürbisse, bevor die Speisekammer leer ist.", "Урожай почти закончился. Принеси мне эти десять тыкв, пока кладовая не опустела.");
        Add(map, "Przyniosłeś dynie w samą porę. Dzięki zbiorom wieś będzie miała co jeść. Jestem ci wdzięczny.", "You brought the pumpkins just in time. The harvest will feed the village, and I am grateful.", "Tu as apporté les citrouilles juste à temps. La récolte nourrira le village, et je t'en suis reconnaissant.", "Has traído las calabazas justo a tiempo. La cosecha alimentará al pueblo y te estoy muy agradecido.", "Hai portato le zucche appena in tempo. Il raccolto sfamerà il villaggio e ti sono grato.", "Du hast die Kürbisse gerade noch rechtzeitig gebracht. Die Ernte wird das Dorf ernähren. Ich bin dir dankbar.", "Ты принёс тыквы как раз вовремя. Урожай прокормит деревню, и я тебе благодарен.");
        Add(map, "Te dynie zapewnią wiosce jedzenie. Dziękuję ci.", "These pumpkins will keep the village fed. You have my thanks.", "Ces citrouilles permettront de nourrir le village. Merci à toi.", "Estas calabazas ayudarán a alimentar al pueblo. Te lo agradezco.", "Queste zucche sfameranno il villaggio. Ti ringrazio.", "Diese Kürbisse werden das Dorf satt halten. Ich danke dir.", "Эти тыквы помогут прокормить деревню. Спасибо тебе.");
        Add(map, "Kucharz już planuje kolejny posiłek. Sam bym sobie bez ciebie nie poradził.", "The cook is already planning the next meal. I could not have done it without you.", "Le cuisinier prépare déjà le prochain repas. Je n'aurais pas pu y arriver sans toi.", "El cocinero ya está planeando la próxima comida. No habría podido hacerlo sin ti.", "Il cuoco sta già preparando il prossimo pasto. Non ce l'avrei fatta senza di te.", "Der Koch plant schon die nächste Mahlzeit. Ohne dich hätte ich das nicht geschafft.", "Повар уже планирует следующую трапезу. Без тебя я бы не справился.");
        Add(map, "Uratowałeś zbiory przed przemianą w wyjątkowo smutną zupę. Dziękuję.", "You saved the harvest from becoming a very sad soup. Thank you.", "Tu as sauvé la récolte d'une triste fin en soupe. Merci.", "Has salvado la cosecha de acabar convertida en una sopa muy triste. Gracias.", "Hai salvato il raccolto dal diventare una zuppa davvero triste. Grazie.", "Du hast die Ernte davor bewahrt, zu einer sehr traurigen Suppe zu werden. Danke.", "Ты спас урожай от превращения в очень грустный суп. Спасибо.");
        Add(map, "Jeden z oddziałów Ostrzy Bractwa napada na podróżnych. Znajdź ich i połóż temu kres.", "One of the Brotherhoods blade squads has been harassing travelers. Find them and put an end to it.", "Une escouade de lames de la Confrérie harcèle les voyageurs. Trouve-les et mets-y fin.", "Un escuadrón de Hojas de la Hermandad acosa a los viajeros. Encuéntralos y ponle fin.", "Una squadra di Lame della Fratellanza molesta i viaggiatori. Trovali e fermali.", "Eine Klingeneinheit der Bruderschaft überfällt Reisende. Finde sie und mach dem ein Ende.", "Отряд Клинков Братства преследует путников. Найди их и положи этому конец.");
        Add(map, "Mroczne Bractwo wypuściło ogary na obrzeżach. Zajmij się nimi, zanim znajdą kolejny trop.", "The Dark Brotherhood has set its hounds loose near the outskirts. Deal with them before they find another trail.", "La Confrérie noire a lâché ses chiens près des faubourgs. Occupe-t'en avant qu'ils ne trouvent une autre piste.", "La Hermandad Oscura soltó a sus sabuesos cerca de las afueras. Acaba con ellos antes de que encuentren otro rastro.", "La Fratellanza Oscura ha liberato i suoi segugi vicino alla periferia. Fermali prima che trovino un'altra pista.", "Die Dunkle Bruderschaft hat ihre Jagdhunde am Stadtrand losgelassen. Kümmere dich um sie, bevor sie eine neue Spur finden.", "Тёмное Братство выпустило гончих у окраины. Разберись с ними, пока они не нашли новый след.");
        Add(map, "W lochu zapadła cisza, a to zwykle gorsze niż odgłosy. Zbadaj go i wróć.", "The dungeon has gone quiet, which is usually worse than hearing something. Explore it and return.", "Le donjon est devenu silencieux, ce qui est souvent pire que d'entendre quelque chose. Explore-le et reviens.", "La mazmorra se ha quedado en silencio, y eso suele ser peor que oír algo. Explórala y vuelve.", "Il dungeon è diventato silenzioso, cosa di solito peggiore che sentire qualcosa. Esploralo e torna.", "Im Dungeon ist es still geworden, und das ist meist schlimmer, als etwas zu hören. Erkunde ihn und kehre zurück.", "В подземелье стало тихо, а это обычно хуже любых звуков. Исследуй его и возвращайся.");
        Add(map, "Spod miasta dobiegają dziwne echa. Zbadaj kanały i wróć po oczyszczeniu przejścia.", "Strange sounds echo from below the town. Explore the sewers and return with the route cleared.", "Des sons étranges résonnent sous la ville. Explore les égouts et reviens après avoir dégagé le passage.", "Se oyen sonidos extraños bajo la ciudad. Explora las cloacas y vuelve con la ruta despejada.", "Strani suoni riecheggiano sotto la città. Esplora le fogne e torna dopo aver liberato il percorso.", "Unter der Stadt hallen seltsame Geräusche. Erkunde die Kanalisation und kehre mit freiem Weg zurück.", "Из-под города доносятся странные звуки. Исследуй канализацию и вернись, расчистив путь.");
        Add(map, "Zwiadowca oznaczył bezpieczniejszy szlak przez mroczny las, ale nie wrócił. Podążaj trasą i złóż raport.", "A scout marked a safer trail through the dark forest but never returned. Follow the route and report back.", "Un éclaireur a indiqué un chemin plus sûr dans la forêt sombre, mais il n'est jamais revenu. Suis la route et fais ton rapport.", "Un explorador marcó un camino más seguro por el bosque oscuro, pero nunca regresó. Sigue la ruta e informa al volver.", "Un esploratore ha segnato un sentiero più sicuro nella foresta oscura, ma non è mai tornato. Segui il percorso e riferisci.", "Ein Späher markierte einen sichereren Pfad durch den Dunkelwald, kehrte aber nie zurück. Folge der Route und berichte.", "Разведчик отметил более безопасный путь через тёмный лес, но не вернулся. Пройди по маршруту и доложи.");
        Add(map, "W pobliżu starego szlaku zauważono patrol mrocznych orków. Rozbij go, zanim dotrze do wioski.", "A dark orc patrol has been spotted near the old route. Break it up before they reach the village.", "Une patrouille d'orcs sombres a été repérée près de l'ancienne route. Disperse-la avant qu'elle n'atteigne le village.", "Han visto una patrulla de orcos oscuros cerca de la ruta antigua. Acaba con ella antes de que llegue a la aldea.", "È stata avvistata una pattuglia di orchi oscuri vicino al vecchio sentiero. Fermala prima che raggiunga il villaggio.", "Nahe der alten Route wurde eine Patrouille dunkler Orks gesichtet. Zerschlage sie, bevor sie das Dorf erreicht.", "У старого пути заметили патруль тёмных орков. Разбей его, пока он не добрался до деревни.");
        Add(map, "Podobno w lochach pod miastem zadomowił się kult. Zbadaj tę sprawę.", "Apparently, a cult has taken root in the dungeons beneath the city. Check it out.", "Apparemment, un culte s'est installé dans les donjons sous la ville. Va voir ce qu'il en est.", "Al parecer, un culto se ha establecido en las mazmorras bajo la ciudad. Investiga qué ocurre.", "Pare che un culto si sia insediato nei dungeon sotto la città. Vai a controllare.", "Anscheinend hat sich ein Kult in den Verliesen unter der Stadt eingenistet. Sieh nach, was dort vor sich geht.", "Похоже, в подземельях под городом обосновался культ. Проверь, что там происходит.");
        Add(map, "Dostawca zaginął na leśnym trakcie. Przeszukaj okolicę i wróć cały i zdrowy.", "A supply runner vanished on the forest road. Search the area and make it back in one piece.", "Un ravitailleur a disparu sur la route forestière. Fouille la zone et reviens sain et sauf.", "Un mensajero de suministros desapareció en el camino del bosque. Busca por la zona y vuelve sano y salvo.", "Un corriere di rifornimenti è scomparso sulla strada della foresta. Cerca nella zona e torna tutto intero.", "Ein Versorgungsläufer ist auf dem Waldweg verschwunden. Durchsuche das Gebiet und kehre unversehrt zurück.", "Поставщик пропал на лесной дороге. Осмотри окрестности и вернись целым и невредимым.");
        Add(map, "Zielony szlam rozlewa się po ścieżkach, oblepia buty i wozy, a jedna pechowa koza jest cała umazana. Usuń go.", "Green slime is spreading around the paths and sticking to boots, carts, and one very unlucky goat. Clear it out.", "La gelée verte envahit les chemins et colle aux bottes, aux charrettes et à une chèvre très malchanceuse. Élimine-la.", "El limo verde se extiende por los caminos y se pega a botas, carros y a una cabra con muy mala suerte. Límpialo.", "La melma verde invade i sentieri e si attacca a stivali, carri e a una capra molto sfortunata. Ripulisci la zona.", "Grüner Schleim breitet sich auf den Wegen aus und klebt an Stiefeln, Karren und einer sehr unglücklichen Ziege. Beseitige ihn.", "Зелёная слизь расползается по тропам, липнет к сапогам и телегам, а одна бедная коза уже вся в ней. Убери её.");
        Add(map, "W naszej okolicy pojawiły się hordy zombie. Oczyść trochę ten teren.", "Hordes of zombies have appeared in our area. Clean up the area a bit.", "Des hordes de zombies sont apparues dans les environs. Nettoie un peu la zone.", "Han aparecido hordas de zombis por la zona. Limpia un poco los alrededores.", "Nella zona sono comparse orde di zombie. Fai un po' di pulizia.", "In unserer Gegend sind Horden von Zombies aufgetaucht. Räume hier ein wenig auf.", "В наших краях появились орды зомби. Немного очисти эту местность.");
        Add(map, "Do porządnego wiejskiego posiłku potrzeba ziemniaków. Dokładnie dziesięciu. Kucharz nie przyjmuje negocjacji.", "A proper village meal needs potatoes. Ten of them, to be precise. The cook insists this is not negotiable.", "Un vrai repas de village nécessite des pommes de terre. Dix, pour être précis. Le cuisinier ne veut rien entendre.", "Una buena comida para la aldea necesita patatas. Diez, para ser exactos. El cocinero no acepta negociaciones.", "Per un pasto degno del villaggio servono patate. Dieci, per la precisione. Il cuoco non accetta trattative.", "Für ein ordentliches Dorfessen braucht man Kartoffeln. Genauer gesagt zehn. Der Koch lässt nicht mit sich verhandeln.", "Для хорошей деревенской трапезы нужен картофель. Ровно десять. Повар не принимает возражений.");
        Add(map, "Czerwona maź dostaje się do studni i przykleja do wszystkiego. Usuń dziesięć takich stworów.", "Red slime is getting into the wells and sticking to absolutely everything. Please remove ten of them.", "La gelée rouge s'infiltre dans les puits et colle absolument partout. Élimine-en dix, s'il te plaît.", "El limo rojo se mete en los pozos y se pega absolutamente a todo. Elimina diez, por favor.", "La melma rossa finisce nei pozzi e si attacca proprio a tutto. Eliminane dieci, per favore.", "Roter Schleim gelangt in die Brunnen und klebt wirklich überall. Bitte beseitige zehn davon.", "Красная слизь попадает в колодцы и липнет буквально ко всему. Уничтожь десять таких тварей.");
        Add(map, "Kucharz wioski w jakiś sposób zużył całą rzepę ze spiżarni. Przynieś osiem, zanim o gulaszu zostaną tylko plotki.", "The village cook has somehow used every turnip in the pantry. Bring back 8 before the stew becomes a rumor.", "Le cuisinier du village a réussi à utiliser tous les navets du garde-manger. Ramène-en huit avant que le ragoût ne devienne une légende.", "El cocinero del pueblo ha gastado todos los nabos de la despensa. Trae ocho antes de que el guiso se convierta en un recuerdo.", "Il cuoco del villaggio ha consumato tutte le rape della dispensa. Portane otto prima che dello stufato resti solo una voce.", "Der Dorfkoch hat irgendwie alle Rüben aus der Speisekammer verbraucht. Bring acht, bevor der Eintopf nur noch ein Gerücht ist.", "Деревенский повар умудрился израсходовать всю репу из кладовой. Принеси восемь, пока похлёбка не стала лишь слухом.");

        // Active quest dialogue is also returned by QuestGiver.GetQuestStory while
        // an accepted quest still has objectives left to complete.
        Add(map, "Oddział Ostrzy wciąż zagraża podróżnym. Znajdź ich i przywróć bezpieczeństwo na drodze.", "The Blade squad is still threatening travelers. Please find them and make the road safe again.", "L'escouade des Lames menace toujours les voyageurs. Trouve-les et sécurise à nouveau la route.", "El escuadrón de las Hojas sigue amenazando a los viajeros. Encuéntralo y vuelve a asegurar el camino.", "La squadra delle Lame minaccia ancora i viaggiatori. Trovala e rendi di nuovo sicura la strada.", "Die Klingeneinheit bedroht weiterhin Reisende. Finde sie und sichere die Straße wieder.", "Отряд Клинков всё ещё угрожает путникам. Найди их и обезопась дорогу.");
        Add(map, "Ogary Bractwa wciąż są na obrzeżach. Zajmij się nimi, zanim złapią kolejny trop.", "The Brotherhood hounds are still on the outskirts. Please deal with them before they catch another scent.", "Les chiens de la Confrérie rôdent toujours en périphérie. Occupe-t'en avant qu'ils ne trouvent une autre piste.", "Los sabuesos de la Hermandad siguen en las afueras. Acaba con ellos antes de que encuentren otro rastro.", "I segugi della Fratellanza sono ancora in periferia. Fermali prima che trovino un'altra traccia.", "Die Jagdhunde der Bruderschaft sind noch am Stadtrand. Kümmere dich um sie, bevor sie eine neue Fährte aufnehmen.", "Гончие Братства всё ещё у окраины. Разберись с ними, пока они не взяли новый след.");
        Add(map, "Zwiadowca nie wrócił z mrocznego lasu. Podążaj oznaczonym szlakiem i wróć bezpiecznie.", "The scout never returned from the dark forest. Follow the marked trail and come back safely.", "L'éclaireur n'est jamais revenu de la forêt sombre. Suis le sentier balisé et reviens sain et sauf.", "El explorador nunca regresó del bosque oscuro. Sigue el sendero marcado y vuelve sano y salvo.", "L'esploratore non è mai tornato dalla foresta oscura. Segui il sentiero segnato e torna sano e salvo.", "Der Späher ist nie aus dem Dunkelwald zurückgekehrt. Folge dem markierten Pfad und kehre sicher zurück.", "Разведчик не вернулся из тёмного леса. Иди по отмеченной тропе и возвращайся целым.");
        Add(map, "Patrol mrocznych orków wciąż znajduje się przy starym szlaku. Rozbij go, zanim dotrze do wioski.", "The dark orc patrol is still near the old route. Break it up before it reaches the village.", "La patrouille d'orcs sombres est toujours près de l'ancienne route. Disperse-la avant qu'elle n'atteigne le village.", "La patrulla de orcos oscuros sigue cerca de la ruta antigua. Acaba con ella antes de que llegue a la aldea.", "La pattuglia di orchi oscuri è ancora vicino al vecchio sentiero. Fermala prima che raggiunga il villaggio.", "Die Patrouille dunkler Orks ist noch nahe der alten Route. Zerschlage sie, bevor sie das Dorf erreicht.", "Патруль тёмных орков всё ещё у старого пути. Разбей его, пока он не дошёл до деревни.");
        Add(map, "W lochu zapadła cisza i nie daje mi to spokoju. Zbadaj jego głębiny i wróć z wieściami.", "The dungeon has gone quiet, and that worries me. Explore its depths and return with word of what you find.", "Le donjon est devenu silencieux, et cela m'inquiète. Explore ses profondeurs et reviens nous dire ce que tu as trouvé.", "La mazmorra se ha quedado en silencio y eso me preocupa. Explora sus profundidades y vuelve con noticias.", "Il dungeon è diventato silenzioso e questo mi preoccupa. Esplora le sue profondità e torna a raccontarci cosa trovi.", "Im Dungeon ist es still geworden, und das beunruhigt mich. Erkunde seine Tiefen und berichte, was du findest.", "В подземелье стало тихо, и это тревожит меня. Исследуй его глубины и возвращайся с новостями.");
        Add(map, "Kult może zbierać siły pod miastem. Dokończ wyprawę do lochu i opowiedz, czego się dowiedziałeś.", "The cult may be gathering beneath the city. Finish your dungeon expedition and tell me what you discovered.", "Le culte se rassemble peut-être sous la ville. Termine ton expédition dans le donjon et raconte-moi ce que tu as découvert.", "Puede que el culto se esté reuniendo bajo la ciudad. Completa tu expedición a la mazmorra y cuéntame qué descubriste.", "Il culto potrebbe riunirsi sotto la città. Completa la spedizione nel dungeon e raccontami cosa hai scoperto.", "Der Kult könnte sich unter der Stadt versammeln. Beende deinen Dungeon-Ausflug und erzähle mir, was du herausgefunden hast.", "Возможно, культ собирается под городом. Заверши поход в подземелье и расскажи, что выяснил.");
        Add(map, "Dostawca zaginął na leśnym trakcie. Przeszukaj okolicę i wróć cały i zdrowy.", "A supply runner vanished on the forest road. Search the area and make it back in one piece.", "Un ravitailleur a disparu sur la route forestière. Fouille la zone et reviens sain et sauf.", "Un mensajero de suministros desapareció en el camino del bosque. Busca por la zona y vuelve sano y salvo.", "Un corriere di rifornimenti è scomparso sulla strada della foresta. Cerca nella zona e torna tutto intero.", "Ein Versorgungsläufer ist auf dem Waldweg verschwunden. Durchsuche das Gebiet und kehre unversehrt zurück.", "Поставщик пропал на лесной дороге. Осмотри окрестности и вернись целым и невредимым.");
        Add(map, "Zielony szlam wciąż rozlewa się po ścieżkach. Usuń go, zanim dotrze do wozów i zagrody kóz.", "Green slime is still spreading along the paths. Please clear it before it reaches the carts and the goat pens.", "La gelée verte continue de se répandre sur les chemins. Élimine-la avant qu'elle n'atteigne les charrettes et l'enclos des chèvres.", "El limo verde sigue extendiéndose por los caminos. Límpialo antes de que llegue a los carros y los corrales de las cabras.", "La melma verde continua a diffondersi sui sentieri. Ripuliscili prima che raggiunga i carri e i recinti delle capre.", "Der grüne Schleim breitet sich weiter auf den Wegen aus. Beseitige ihn, bevor er die Karren und Ziegenställe erreicht.", "Зелёная слизь продолжает расползаться по тропам. Убери её, пока она не добралась до телег и козьих загонов.");
        Add(map, "Zombie wciąż zbierają się w pobliżu. Pozbądź się dziesięciu, żeby wioska mogła odetchnąć.", "The zombies are still gathering nearby. Please take care of ten of them so the village can breathe again.", "Les zombies se rassemblent toujours dans les environs. Élimine-en dix pour que le village puisse respirer à nouveau.", "Los zombis siguen reuniéndose cerca. Acaba con diez para que la aldea pueda respirar tranquila.", "Gli zombie continuano a radunarsi qui vicino. Eliminali in dieci, così il villaggio potrà tirare un sospiro di sollievo.", "Die Zombies sammeln sich weiterhin in der Nähe. Erledige zehn von ihnen, damit das Dorf aufatmen kann.", "Зомби всё ещё собираются неподалёку. Убей десять, чтобы деревня могла вздохнуть спокойно.");
        Add(map, "Kucharz potrzebuje dziesięciu ziemniaków na wiejski posiłek. Przynieś je, zanim garnek zostanie zdjęty z ognia.", "The cook needs ten potatoes for the village meal. Please bring them before the pot is taken off the fire.", "Le cuisinier a besoin de dix pommes de terre pour le repas du village. Apporte-les avant que la marmite ne soit retirée du feu.", "El cocinero necesita diez patatas para la comida del pueblo. Tráelas antes de retirar la olla del fuego.", "Il cuoco ha bisogno di dieci patate per il pasto del villaggio. Portale prima che la pentola venga tolta dal fuoco.", "Der Koch braucht zehn Kartoffeln für das Dorfessen. Bring sie, bevor der Topf vom Feuer genommen wird.", "Повару нужно десять картофелин для деревенской трапезы. Принеси их, пока котёл не сняли с огня.");
        Add(map, "Czerwona maź dostaje się do studni. Usuń dziesięć takich stworów, zanim pokryje całe miasteczko.", "Red slime is getting into the wells. Please remove ten of them before the whole town is covered in it.", "La gelée rouge s'infiltre dans les puits. Élimine-en dix avant qu'elle ne recouvre toute la ville.", "El limo rojo está llegando a los pozos. Elimina diez antes de que cubra todo el pueblo.", "La melma rossa sta finendo nei pozzi. Eliminala in dieci prima che ricopra l'intero villaggio.", "Roter Schleim gelangt in die Brunnen. Beseitige zehn davon, bevor er den ganzen Ort bedeckt.", "Красная слизь попадает в колодцы. Уничтожь десять тварей, пока она не покрыла весь город.");
        Add(map, "Te odgłosy wciąż dobiegają spod ziemi. Zbadaj kanały, oczyść przejście i wróć opowiedzieć, co znalazłeś.", "Those sounds are still coming from below. Explore the sewers, clear the route, and come back to tell me what you found.", "Ces bruits viennent toujours des souterrains. Explore les égouts, dégage le passage et reviens me raconter ce que tu as trouvé.", "Esos sonidos siguen viniendo de abajo. Explora las cloacas, despeja la ruta y vuelve para contarme qué encontraste.", "Quei rumori provengono ancora dal sottosuolo. Esplora le fogne, libera il passaggio e torna a raccontarmi cosa hai trovato.", "Die Geräusche kommen noch immer von unten. Erkunde die Kanalisation, räume den Weg frei und berichte mir, was du gefunden hast.", "Эти звуки всё ещё доносятся снизу. Исследуй канализацию, расчисти путь и расскажи, что найдёшь.");
        Add(map, "W spiżarni skończyła się rzepa, a kucharz zaczyna improwizować. Przynieś osiem, zanim gulasz zniknie z menu.", "The pantry is out of turnips, and the cook has started improvising. Please bring back eight before the stew disappears from the menu.", "Il n'y a plus de navets dans le garde-manger et le cuisinier improvise déjà. Ramène-en huit avant que le ragoût disparaisse du menu.", "No quedan nabos en la despensa y el cocinero ya está improvisando. Trae ocho antes de que el guiso desaparezca del menú.", "La dispensa è senza rape e il cuoco ha iniziato a improvvisare. Portane otto prima che lo stufato sparisca dal menu.", "Die Speisekammer ist leer, und der Koch improvisiert bereits. Bring acht Rüben, bevor der Eintopf vom Speiseplan verschwindet.", "Репа в кладовой закончилась, и повар уже импровизирует. Принеси восемь, пока похлёбку не убрали из меню.");
        Add(map, "Ślady wargów wciąż są blisko leśnej ścieżki. Zajmij się pięcioma bestiami, zanim dotrą do drogi.", "The warg tracks are still close to the forest path. Please deal with five of them before they reach the road.", "Les traces des wargs sont toujours proches du sentier forestier. Occupe-toi de cinq d'entre eux avant qu'ils n'atteignent la route.", "Las huellas de los huargos siguen cerca del sendero del bosque. Encárgate de cinco antes de que lleguen al camino.", "Le tracce dei warg sono ancora vicine al sentiero nella foresta. Fermane cinque prima che raggiungano la strada.", "Die Wargspuren sind noch immer nahe am Waldpfad. Kümmere dich um fünf von ihnen, bevor sie die Straße erreichen.", "Следы варгов всё ещё рядом с лесной тропой. Разберись с пятью зверями, пока они не добрались до дороги.");
        Add(map, "Wilki wciąż krążą przy drogach. Zmniejsz ich liczebność, zanim komuś stanie się krzywda.", "The wolves are still prowling near the roads. Please thin their numbers before someone gets hurt.", "Les loups rôdent toujours près des routes. Réduis leur nombre avant que quelqu'un ne soit blessé.", "Los lobos siguen merodeando cerca de los caminos. Reduce su número antes de que alguien salga herido.", "I lupi si aggirano ancora vicino alle strade. Riducine il numero prima che qualcuno si faccia male.", "Die Wölfe streifen noch immer in der Nähe der Straßen umher. Dünne ihre Reihen aus, bevor jemand verletzt wird.", "Волки всё ещё бродят у дорог. Сократи их число, пока никто не пострадал.");
        Add(map, "Przy leśnej ścieżce pojawiły się wielkie ślady. Mieszkańcy spaliby spokojniej, gdyby pięć wargów przestało je zostawiać.", "Large tracks have appeared along the forest path. The villagers would sleep better if five wargs stopped making them.", "De grandes traces sont apparues près du sentier forestier. Les villageois dormiraient mieux si cinq wargs cessaient d'en laisser.", "Han aparecido huellas enormes junto al sendero del bosque. Los aldeanos dormirían mejor si cinco huargos dejaran de hacerlas.", "Sono apparse grandi impronte lungo il sentiero nella foresta. Gli abitanti dormirebbero meglio se cinque warg smettessero di lasciarle.", "Am Waldpfad sind große Spuren aufgetaucht. Die Dorfbewohner könnten besser schlafen, wenn fünf Warge keine mehr hinterließen.", "На лесной тропе появились большие следы. Жители спали бы спокойнее, если бы пять варгов перестали их оставлять.");
        Add(map, "Wilki kręcą się zbyt blisko dróg. Zmniejsz ich liczebność, zanim ktoś stanie się ich obiadem.", "Wolves have been prowling too close to the roads. Thin their numbers before someone becomes dinner.", "Les loups rôdent trop près des routes. Réduis leur nombre avant que quelqu'un ne finisse en repas.", "Los lobos merodean demasiado cerca de los caminos. Reduce su número antes de que alguien acabe siendo su cena.", "I lupi si aggirano troppo vicino alle strade. Riducine il numero prima che qualcuno diventi la loro cena.", "Wölfe streifen zu nah an den Straßen umher. Dünne ihre Reihen aus, bevor jemand als Abendessen endet.", "Волки слишком близко подходят к дорогам. Сократи их число, пока кто-нибудь не стал их ужином.");
    }

    private static Dictionary<string, string[]> BuildTutorialTranslations()
    {
        var result = new Dictionary<string, string[]>(System.StringComparer.Ordinal);
        AddTutorial(result, "start_map_aulla_crystals.title", "Wiedza Aulli", "Aulla's Knowledge", "Le savoir d'Aulla", "El conocimiento de Aulla", "La conoscenza di Aulla", "Aullas Wissen", "Знания Ауллы");

        AddTutorial(result, "tutorial_chapel.title", "Świątynia i Udros", "The Chapel and Udros", "La chapelle et Udros", "La capilla y Udros", "La cappella e Udros", "Die Kapelle und Udros", "Часовня и Удрос");
        AddTutorial(result, "tutorial_chapel.chapel_portal.instructions", "To świątynia. Znajduje się tu portal, który pozwala rozpocząć kolejną noc.", "This is the chapel. The portal here lets you begin the next night.", "Voici la chapelle. Le portail qui s'y trouve te permet de commencer la prochaine nuit.", "Esta es la capilla. El portal que hay aquí te permite comenzar la próxima noche.", "Questa è la cappella. Il portale qui presente ti permette di iniziare la notte successiva.", "Das ist die Kapelle. Über das Portal hier kannst du die nächste Nacht beginnen.", "Это часовня. Через портал здесь можно начать следующую ночь.");
        AddTutorial(result, "tutorial_chapel.chapel_portal.hint", "Skorzystaj z portalu, aby rozpocząć noc.", "Use the portal to begin a night.", "Utilise le portail pour commencer une nuit.", "Usa el portal para comenzar una noche.", "Usa il portale per iniziare una notte.", "Nutze das Portal, um eine Nacht zu beginnen.", "Воспользуйся порталом, чтобы начать ночь.");
        AddTutorial(result, "tutorial_chapel.udros_crystals.instructions", "Kryształy Udrosa zdobywasz za ukończenie nocy oraz wykonywanie zadań dla odnalezionych NPC-ów. W tej misie możesz je złożyć. Aby ukończyć grę, musisz pokonać Czerwony Księżyc w jego domenie. Gdy misa zostanie uzupełniona, pojawi się specjalny portal do domeny Udrosa, gdzie stoczysz z nim walkę.", "Earn Udros Crystals by completing nights and doing quests for NPCs you find. Place them in this bowl. To complete the game, defeat the Red Moon in his domain. Once the bowl is filled, a special portal to Udros's domain will appear, where you can face him.", "Obtiens des cristaux d'Udros en terminant des nuits et en accomplissant les quêtes des PNJ que tu rencontres. Dépose-les dans cette vasque. Pour terminer le jeu, vaincs la Lune rouge dans son domaine. Une fois la vasque remplie, un portail spécial vers le domaine d'Udros apparaîtra pour que tu puisses l'affronter.", "Consigue cristales de Udros al completar noches y hacer misiones para los NPC que encuentres. Deposítalos en esta pila. Para completar el juego, derrota a la Luna Roja en su dominio. Cuando esté llena, aparecerá un portal especial a su dominio para enfrentarte a él.", "Ottieni i cristalli di Udros completando le notti e portando a termine le missioni degli NPC che trovi. Depositali in questa coppa. Per completare il gioco, sconfiggi la Luna Rossa nel suo dominio. Quando sarà piena, apparirà un portale speciale per affrontarlo.", "Du erhältst Udros-Kristalle, indem du Nächte abschließt und Aufgaben für gefundene NPCs erledigst. Lege sie in diese Schale. Um das Spiel abzuschließen, musst du den Roten Mond in seiner Domäne besiegen. Sobald die Schale gefüllt ist, erscheint ein besonderes Portal zu Udros' Domäne, wo du gegen ihn kämpfen kannst.", "Кристаллы Удроса можно получить, завершая ночи и выполняя задания найденных NPC. Положи их в эту чашу. Чтобы пройти игру, победи Красную Луну в его владениях. Когда чаша наполнится, появится особый портал в домен Удроса, где ты сможешь сразиться с ним.");
        AddTutorial(result, "tutorial_chapel.udros_crystals.hint", "Napełnij misę kryształami Udrosa, aby otworzyć portal do jego domeny.", "Fill the bowl with Udros Crystals to open the portal to his domain.", "Remplis la vasque de cristaux d'Udros pour ouvrir le portail vers son domaine.", "Llena la pila con cristales de Udros para abrir el portal a su dominio.", "Riempi la coppa con i cristalli di Udros per aprire il portale verso il suo dominio.", "Fülle die Schale mit Udros-Kristallen, um das Portal in seine Domäne zu öffnen.", "Наполни чашу кристаллами Удроса, чтобы открыть портал в его владения.");
        AddTutorial(result, "tutorial_chapel.prepare_for_udros.instructions", "Udros nie jest łatwym przeciwnikiem. Przygotuj się do walki i dobrze wyposaż bohatera, zanim wejdziesz do jego domeny.", "Udros is a formidable enemy. Prepare carefully and equip your hero well before entering his domain.", "Udros est un adversaire redoutable. Prépare-toi soigneusement et équipe bien ton héros avant d'entrer dans son domaine.", "Udros es un enemigo temible. Prepárate bien y equipa a tu héroe antes de entrar en su dominio.", "Udros è un avversario formidabile. Preparati con cura ed equipaggia bene il tuo eroe prima di entrare nel suo dominio.", "Udros ist ein mächtiger Gegner. Bereite dich gut vor und rüste deinen Helden aus, bevor du seine Domäne betrittst.", "Удрос — грозный противник. Хорошо подготовься и снаряди героя перед входом в его владения.");
        AddTutorial(result, "tutorial_chapel.prepare_for_udros.hint", "Ulepsz ekwipunek i przygotuj zapasy przed walką.", "Upgrade your gear and stock up before the fight.", "Améliore ton équipement et fais des réserves avant le combat.", "Mejora tu equipo y prepara provisiones antes del combate.", "Migliora l'equipaggiamento e fai scorta di provviste prima dello scontro.", "Verbessere deine Ausrüstung und lege Vorräte an, bevor du kämpfst.", "Улучши снаряжение и подготовь запасы перед боем.");

        AddTutorial(result, "tarvern_upgrade_tut.title", "Rozwój wioski", "Village Upgrades", "Améliorer le village", "Mejoras de la aldea", "Miglioramenti del villaggio", "Dorfverbesserungen", "Развитие деревни");
        AddTutorial(result, "tarvern_upgrade_tut.talk_to_manager.instructions", "Znajdujesz się w miejscu, w którym możesz rozwijać swoją osadę. Podejdź do zarządcy, aby rozpocząć rozbudowę.", "You are in the place where you can develop your settlement. Talk to the steward to begin expanding it.", "Tu te trouves dans le lieu où tu peux développer ton village. Parle à l'intendant pour commencer son agrandissement.", "Estás en el lugar donde puedes desarrollar tu asentamiento. Habla con el administrador para empezar a ampliarlo.", "Ti trovi nel luogo in cui puoi sviluppare il tuo insediamento. Parla con il responsabile per iniziare ad ampliarlo.", "Hier kannst du deine Siedlung ausbauen. Sprich mit der Verwaltung, um damit zu beginnen.", "Здесь можно развивать поселение. Поговори с управляющим, чтобы начать его расширение.");
        AddTutorial(result, "tarvern_upgrade_tut.talk_to_manager.hint", "Porozmawiaj z zarządcą.", "Talk to the steward.", "Parle à l'intendant.", "Habla con el administrador.", "Parla con il responsabile.", "Sprich mit der Verwaltung.", "Поговори с управляющим.");
        AddTutorial(result, "tarvern_upgrade_tut.choose_room.instructions", "Wybierz pomieszczenie, które chcesz rozwinąć. Każde z nich oferuje inne możliwości i korzyści dla Twojej osady.", "Choose a room to develop. Each one offers different options and benefits for your settlement.", "Choisis une pièce à développer. Chacune offre des possibilités et des avantages différents pour ton village.", "Elige una sala para desarrollar. Cada una ofrece distintas opciones y ventajas para tu asentamiento.", "Scegli una stanza da sviluppare. Ognuna offre possibilità e vantaggi diversi per il tuo insediamento.", "Wähle einen Raum aus, den du ausbauen möchtest. Jeder bietet deiner Siedlung andere Möglichkeiten und Vorteile.", "Выбери комнату для развития. Каждая открывает разные возможности и преимущества для поселения.");
        AddTutorial(result, "tarvern_upgrade_tut.choose_room.hint", "Wybierz pomieszczenie z listy dostępnych budynków.", "Choose a room from the list of available buildings.", "Choisis une pièce dans la liste des bâtiments disponibles.", "Elige una sala de la lista de edificios disponibles.", "Scegli una stanza dall'elenco degli edifici disponibili.", "Wähle einen Raum aus der Liste verfügbarer Gebäude.", "Выбери комнату из списка доступных зданий.");
        AddTutorial(result, "tarvern_upgrade_tut.create_rooms.instructions", "Po wybraniu pomieszczenia możesz je stworzyć, jeśli masz wymagane Lunary, materiały i pracowników.", "Once you have chosen a room, you can build it if you have the required Lunars, materials, and workers.", "Après avoir choisi une pièce, tu peux la construire si tu as les Lunars, les matériaux et les ouvriers nécessaires.", "Tras elegir una sala, puedes construirla si tienes las lunas, los materiales y los trabajadores necesarios.", "Dopo aver scelto una stanza, puoi costruirla se hai i Lunars, i materiali e i lavoratori richiesti.", "Nachdem du einen Raum ausgewählt hast, kannst du ihn bauen, wenn du genügend Lunars, Materialien und Arbeitskräfte hast.", "Выбрав комнату, ты сможешь построить её, если у тебя достаточно лунаров, материалов и работников.");
        AddTutorial(result, "tarvern_upgrade_tut.create_rooms.hint", "Wybierz pomieszczenie i kliknij przycisk tworzenia.", "Choose a room and select the build button.", "Choisis une pièce et appuie sur le bouton de construction.", "Elige una sala y pulsa el botón de construcción.", "Scegli una stanza e premi il pulsante di costruzione.", "Wähle einen Raum und klicke auf die Schaltfläche zum Bauen.", "Выбери комнату и нажми кнопку строительства.");
        AddTutorial(result, "tarvern_upgrade_tut.upgrade_rooms.instructions", "Ulepszaj kupione pomieszczenia, aby odblokować ich dodatkowe możliwości i zdobyć lepszy ekwipunek.", "Upgrade rooms you have built to unlock more options and better equipment.", "Améliore les pièces construites pour débloquer de nouvelles possibilités et obtenir un meilleur équipement.", "Mejora las salas construidas para desbloquear nuevas opciones y conseguir mejor equipo.", "Potenzia le stanze costruite per sbloccare nuove possibilità e ottenere equipaggiamento migliore.", "Verbessere gebaute Räume, um weitere Möglichkeiten und bessere Ausrüstung freizuschalten.", "Улучшай построенные комнаты, чтобы открыть новые возможности и получить более хорошее снаряжение.");
        AddTutorial(result, "tarvern_upgrade_tut.upgrade_rooms.hint", "Wybierz dostępne ulepszenie w panelu pomieszczenia.", "Choose an available upgrade in the room panel.", "Choisis une amélioration disponible dans le panneau de la pièce.", "Elige una mejora disponible en el panel de la sala.", "Scegli un potenziamento disponibile nel pannello della stanza.", "Wähle im Raumfenster eine verfügbare Verbesserung aus.", "Выбери доступное улучшение в панели комнаты.");
        AddTutorial(result, "tarvern_upgrade_tut.visit_room.instructions", "Po zbudowaniu i ulepszeniu pomieszczenia odwiedź je, aby skorzystać z jego oferty i przygotować ekwipunek na kolejną wyprawę.", "After building and upgrading a room, visit it to use its services and prepare your equipment for the next expedition.", "Après avoir construit et amélioré une pièce, rends-toi sur place pour profiter de ses services et préparer ton équipement pour la prochaine expédition.", "Después de construir y mejorar una sala, visítala para usar sus servicios y preparar tu equipo para la próxima expedición.", "Dopo aver costruito e potenziato una stanza, visitarla ti permetterà di usare i suoi servizi e preparare l'equipaggiamento per la prossima spedizione.", "Besuche den Raum nach dem Bau und Ausbau, um seine Angebote zu nutzen und dich für den nächsten Ausflug auszurüsten.", "Построив и улучшив комнату, зайди в неё, чтобы воспользоваться её услугами и подготовиться к следующему походу.");
        AddTutorial(result, "tarvern_upgrade_tut.visit_room.hint", "Wejdź do pomieszczenia i sprawdź, co możesz zdobyć lub kupić.", "Enter the room and see what you can obtain or buy.", "Entre dans la pièce pour voir ce que tu peux obtenir ou acheter.", "Entra en la sala para ver qué puedes conseguir o comprar.", "Entra nella stanza e scopri cosa puoi ottenere o acquistare.", "Geh in den Raum und sieh nach, was du bekommen oder kaufen kannst.", "Зайди в комнату и посмотри, что можно получить или купить.");

        AddTutorial(result, "start_map_aulla_crystals.find_crystals.instructions", "Znajdź trzy kryształy Aulli.", "Find the three Aulla crystals.", "Trouve les trois cristaux d'Aulla.", "Encuentra los tres cristales de Aulla.", "Trova i tre cristalli di Aulla.", "Finde die drei Aulla-Kristalle.", "Найди три кристалла Ауллы.");
        AddTutorial(result, "start_map_aulla_crystals.find_crystals.hint", "Niebieskie kropki na mapie wskazują kryształy.", "Blue dots on the map mark the crystals.", "Les points bleus sur la carte indiquent les cristaux.", "Los puntos azules del mapa señalan los cristales.", "I punti blu sulla mappa indicano i cristalli.", "Blaue Punkte auf der Karte markieren die Kristalle.", "Синие точки на карте указывают на кристаллы.");

        AddTutorial(result, "start_map_aulla_crystals.enemies_and_portal.instructions", "Po wykonaniu celu mapy lub przetrwaniu wszystkich fal potworów pojawi się portal do miasta.", "Complete the map objective or survive all monster waves to make a portal to town appear.", "Termine l'objectif de la carte ou survis à toutes les vagues de monstres pour faire apparaître un portail vers la ville.", "Completa el objetivo del mapa o sobrevive a todas las oleadas de monstruos para que aparezca un portal a la ciudad.", "Completa l'obiettivo della mappa o sopravvivi a tutte le ondate di mostri per far apparire un portale verso la città.", "Schließe das Karten-Ziel ab oder überlebe alle Monsterwellen, damit ein Portal zur Stadt erscheint.", "Выполни цель карты или переживи все волны монстров, чтобы появился портал в город.");
        AddTutorial(result, "start_map_aulla_crystals.enemies_and_portal.hint", "Czerwone kropki oznaczają przeciwników.", "Red dots mark enemies.", "Les points rouges indiquent les ennemis.", "Los puntos rojos indican enemigos.", "I punti rossi indicano i nemici.", "Rote Punkte markieren Gegner.", "Красные точки обозначают врагов.");

        AddTutorial(result, "start_map_aulla_crystals.stay_or_loot.instructions", "Uważaj: im dłużej zostajesz na mapie, tym więcej potworów przybywa i tym silniejsze się stają.", "Be careful: the longer you stay on the map, the more monsters arrive and the stronger they become.", "Attention : plus tu restes longtemps sur la carte, plus les monstres arrivent nombreux et deviennent puissants.", "Ten cuidado: cuanto más tiempo permanezcas en el mapa, más monstruos llegarán y más fuertes serán.", "Attenzione: più a lungo resti sulla mappa, più mostri arriveranno e diventeranno forti.", "Vorsicht: Je länger du auf der Karte bleibst, desto mehr Monster kommen und desto stärker werden sie.", "Осторожно: чем дольше ты остаёшься на карте, тем больше и сильнее становится монстров.");
        AddTutorial(result, "start_map_aulla_crystals.stay_or_loot.hint", "Zostając dłużej, zdobędziesz więcej łupów na dalszą przygodę.", "Stay longer to gather more loot for your adventure.", "Reste plus longtemps pour récupérer davantage de butin pour la suite de l'aventure.", "Quédate más tiempo para reunir más botín para tu aventura.", "Resta più a lungo per raccogliere più bottino per la tua avventura.", "Bleib länger, um mehr Beute für dein Abenteuer zu sammeln.", "Останься подольше, чтобы собрать больше добычи для приключения.");
        return result;
    }

    private static void AddTutorial(Dictionary<string, string[]> map, string key, params string[] values)
    {
        if (values.Length < 8)
            System.Array.Resize(ref values, 8);
        map[key] = values;
    }

    private static void Add(Dictionary<string, string[]> map, params string[] values)
    {
        if (values.Length < 8)
        {
            var english = values.Length > 1 ? values[1] : values[0];
            var originalLength = values.Length;
            System.Array.Resize(ref values, 8);
            for (var i = originalLength; i < values.Length; i++)
                values[i] = english;
        }

        foreach (var value in values)
        {
            if (!string.IsNullOrEmpty(value))
                map[NormalizeTranslationKey(value)] = values;
        }
    }

    private static string NormalizeTranslationKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Join(" ", value.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));
    }
}
