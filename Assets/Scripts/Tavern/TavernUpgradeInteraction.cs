using UnityEngine;

/// <summary>
/// Lets the player open the village upgrade UI directly, without starting a dialogue.
/// </summary>
public class TavernUpgradeInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "Upgrade village";

    private InteractionManager _interactionManager;
    private TavernUpgrade _tavernUpgrade;
    
    private void Awake()
    {
        _interactionManager = FindFirstObjectByType<InteractionManager>();
        _tavernUpgrade = GetComponent<TavernUpgrade>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_interactionManager == null)
            _interactionManager = FindFirstObjectByType<InteractionManager>();

        _interactionManager?.RegisterInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        _interactionManager?.UnregisterInteractable(this);
    }

    public void Interact()
    {
        _tavernUpgrade.OpenTavernUpgrade();
    }

    public string GetInteractionText() => interactionText;
}
