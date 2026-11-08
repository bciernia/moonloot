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
            (int)GameLanguage.Russian);
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
        Add(result, "Max HP", "Maks. zdrowie", "PV max", "Salud máx.", "PS max", "Max. LP", "Макс. здоровье");
        Add(result, "Max Mana", "Maks. mana", "Mana max", "Maná máx.", "Mana max", "Max. Mana", "Макс. мана");
        Add(result, "Attack Speed", "Szybkość ataku", "Vitesse d'attaque", "Velocidad de ataque", "Velocità d'attacco", "Angriffsgeschwindigkeit", "Скорость атаки");
        Add(result, "Damage Reduction", "Redukcja obrażeń", "Réduction des dégâts", "Reducción de daño", "Riduzione dei danni", "Schadensreduktion", "Снижение урона");
        Add(result, "Kup", "Buy", "Acheter", "Comprar", "Compra", "Kaufen", "Купить");
        Add(result, "Sprzedaj", "Sell", "Vendre", "Vender", "Vendi", "Verkaufen", "Продать");
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
        AddQuestTranslations(result);
        return result;
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
        map[key] = values;
    }

    private static void Add(Dictionary<string, string[]> map, params string[] values)
    {
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
