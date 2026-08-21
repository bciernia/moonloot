using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrystalBowlInteraction : MonoBehaviour, IInteractable, ISaveable
{
    [Header("Offering")]
    [SerializeField] private InventoryItem _crystalItem;
    [SerializeField] private int _requiredAmount = 100;

    [Header("Portal")]
    [SerializeField] private GameObject _portalPrefab;
    [SerializeField] private Transform _portalSpawnPoint;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _amountText;

    [Header("Effects")]
    [SerializeField] private ParticleSystem _offerParticle;

    private int _offeredAmount;
    private bool _portalUnlocked;

    private GameObject _spawnedPortal;
    private ShakeOnHit _shakeOnHit;
    
    private void Awake()
    {
        if (_amountText != null)
        {
            _amountText.gameObject.SetActive(false);
        }

        _shakeOnHit = GetComponent<ShakeOnHit>();
        
        UpdateUI();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Load();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var interactionManager =
            FindFirstObjectByType<InteractionManager>();

        if (interactionManager != null)
        {
            interactionManager.RegisterInteractable(this);
        }

        ShowAmount();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var interactionManager =
            FindFirstObjectByType<InteractionManager>();

        if (interactionManager != null)
        {
            interactionManager.UnregisterInteractable(this);
        }

        HideAmount();
    }

    public void Interact()
    {
        if (_portalUnlocked)
            return;

        if (_offeredAmount >= _requiredAmount)
            return;

        if (!InventoryController.Instance.RemoveItem(_crystalItem, 1))
        {
            _shakeOnHit.Shake();   
            return;
        }

        _offeredAmount++;

        PlayOfferParticle();

        UpdateUI();

        CheckPortal();

        Save();
    }

    public string GetInteractionText()
    {
        if (_portalUnlocked)
            return "Portal unlocked";

        return $"Offer crystals ({_offeredAmount}/{_requiredAmount})";
    }

    private void ShowAmount()
    {
        if (_amountText == null)
            return;

        _amountText.gameObject.SetActive(true);

        UpdateUI();
    }

    private void HideAmount()
    {
        if (_amountText == null)
            return;

        _amountText.gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        if (_amountText == null)
            return;

        _amountText.text =
            $"{_offeredAmount}/{_requiredAmount}";
    }

    private void PlayOfferParticle()
    {
        if (_offerParticle == null)
            return;

        _offerParticle.gameObject.SetActive(true);

        _offerParticle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        _offerParticle.Play();
    }

    private void CheckPortal()
    {
        if (_portalUnlocked)
            return;

        if (_offeredAmount < _requiredAmount)
            return;

        _portalUnlocked = true;

        SpawnPortal();
    }

    private void SpawnPortal()
    {
        if (_spawnedPortal != null)
            return;

        if (_portalPrefab == null || _portalSpawnPoint == null)
            return;

        _spawnedPortal = Instantiate(
            _portalPrefab,
            _portalSpawnPoint.position,
            _portalSpawnPoint.rotation
        );
    }

    public void Save()
    {
        var settings = SaveLoadManager.Instance.GetSettings();

        ES3.Save(
            "crystalBowlOfferedAmount",
            _offeredAmount,
            settings
        );

        ES3.Save(
            "crystalBowlPortalUnlocked",
            _portalUnlocked,
            settings
        );
    }

    public void Load()
    {
        Debug.Log("LOADED CRYSTALS");

        var settings = SaveLoadManager.Instance.GetSettings();

        if (ES3.KeyExists("crystalBowlOfferedAmount", settings))
        {
            _offeredAmount =
                ES3.Load<int>(
                    "crystalBowlOfferedAmount",
                    settings
                );
        }

        if (ES3.KeyExists("crystalBowlPortalUnlocked", settings))
        {
            _portalUnlocked =
                ES3.Load<bool>(
                    "crystalBowlPortalUnlocked",
                    settings
                );
        }

        Debug.Log(
            $"Crystal Bowl loaded: {_offeredAmount}/{_requiredAmount}, " +
            $"portal: {_portalUnlocked}"
        );

        UpdateUI();

        if (_portalUnlocked)
        {
            SpawnPortal();
        }
    }
}