using UnityEngine;

[RequireComponent(typeof(EnemyStatistics))]
public class EnemyActivatable : MonoBehaviour
{
    [Header("References to toggle")]
    [SerializeField] private MonoBehaviour[] componentsToEnable; // np. EnemyBrain, AI scripts
    [SerializeField] private Collider2D[] collidersToToggle;
    [SerializeField] private Rigidbody2D[] rigidbodiesToToggle;
    [SerializeField] private Renderer[] renderersToToggle;

    // stan aktywności
    public bool IsActive { get; private set; } = true; // assume active by default

    private void Awake()
    {
        // jeśli nic nie podpięte, próbujemy automatycznie znaleźć typowe komponenty
        if (componentsToEnable == null || componentsToEnable.Length == 0)
        {
            var brain = GetComponent<MonoBehaviour>(); // change if you have specific type
            // leave empty — zachowaj ręczne przypisanie w inspectorze
        }
    }

    private void OnEnable()
    {
        EnemyActivationManager.Instance?.Register(this);
    }

    private void OnDisable()
    {
        EnemyActivationManager.Instance?.Unregister(this);
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;

        for (int i = 0; i < componentsToEnable.Length; i++)
            if (componentsToEnable[i] != null) componentsToEnable[i].enabled = true;

        for (int i = 0; i < collidersToToggle.Length; i++)
            if (collidersToToggle[i] != null) collidersToToggle[i].enabled = true;

        for (int i = 0; i < rigidbodiesToToggle.Length; i++)
            if (rigidbodiesToToggle[i] != null) rigidbodiesToToggle[i].simulated = true;

        for (int i = 0; i < renderersToToggle.Length; i++)
            if (renderersToToggle[i] != null) renderersToToggle[i].enabled = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;

        for (int i = 0; i < componentsToEnable.Length; i++)
            if (componentsToEnable[i] != null) componentsToEnable[i].enabled = false;

        for (int i = 0; i < collidersToToggle.Length; i++)
            if (collidersToToggle[i] != null) collidersToToggle[i].enabled = false;

        for (int i = 0; i < rigidbodiesToToggle.Length; i++)
            if (rigidbodiesToToggle[i] != null) rigidbodiesToToggle[i].simulated = false;

        for (int i = 0; i < renderersToToggle.Length; i++)
            if (renderersToToggle[i] != null) renderersToToggle[i].enabled = false;
    }
}
