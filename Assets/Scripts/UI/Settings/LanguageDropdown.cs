using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public sealed class LanguageDropdown : MonoBehaviour
{
    private static bool _initializedAfterLoad;

    [SerializeField] private TMP_Dropdown dropdown;
    private UnityAction<int> _listener;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeSceneDropdowns()
    {
        // Do not depend on LocalizationManager's scene search: these components
        // already exist in MainMenu and Base_Demo, but are serialized disabled.
        var selectors = Resources.FindObjectsOfTypeAll<LanguageDropdown>();
        foreach (var selector in selectors)
        {
            if (selector != null && selector.gameObject.scene.IsValid())
                selector.InitializeFromHierarchy();
        }

        _initializedAfterLoad = true;
    }

    private void Awake()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>() ?? GetComponentInChildren<TMP_Dropdown>(true);
    }

    private void OnEnable()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>() ?? GetComponentInChildren<TMP_Dropdown>(true);
        Setup();
    }

    private void Start()
    {
        // Also retry after all scene objects have completed their Awake calls.
        if (!_initializedAfterLoad)
            InitializeFromHierarchy();
        else
            Setup();
    }

    public void Initialize(TMP_Dropdown target)
    {
        dropdown = target;
        enabled = true;
        Setup();
    }

    public bool InitializeFromHierarchy()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>() ?? GetComponentInChildren<TMP_Dropdown>(true);

        if (dropdown == null)
            return false;

        Initialize(dropdown);
        return true;
    }

    private void Setup()
    {
        if (dropdown == null)
            return;

        if (_listener != null)
            dropdown.onValueChanged.RemoveListener(_listener);

        var options = new List<TMP_Dropdown.OptionData>();
        for (var i = 0; i <= (int)GameLanguage.Russian; i++)
            options.Add(new TMP_Dropdown.OptionData(LocalizationManager.GetLanguageName((GameLanguage)i)));

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        var selectedLanguage = LocalizationManager.Instance != null
            ? (int)LocalizationManager.Instance.CurrentLanguage
            : PlayerPrefs.GetInt("Moonloot.Language", (int)GameLanguage.Polish);
        dropdown.SetValueWithoutNotify(Mathf.Clamp(selectedLanguage, 0, (int)GameLanguage.Russian));
        dropdown.RefreshShownValue();

        _listener = OnLanguageChanged;
        dropdown.onValueChanged.AddListener(_listener);
    }

    private void OnDestroy()
    {
        if (dropdown != null && _listener != null)
            dropdown.onValueChanged.RemoveListener(_listener);
    }

    private static void OnLanguageChanged(int value)
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLanguage((GameLanguage)Mathf.Clamp(value, 0, (int)GameLanguage.Russian));
        else
        {
            PlayerPrefs.SetInt("Moonloot.Language", Mathf.Clamp(value, 0, (int)GameLanguage.Russian));
            PlayerPrefs.Save();
        }
    }
}
