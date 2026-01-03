using UnityEngine;

public class NPCTeleport : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameObject teleportTarget; // Miejsce, gdzie NPC ma się pojawić
    [SerializeField] private float invisibleDuration = 3f; // Ile sekund musi być niewidoczny
    [SerializeField] private Camera playerCamera; // Kamera gracza (jeśli nie przypiszesz, weźmie main camera)

    private Renderer npcRenderer;
    private float invisibleTimer = 0f;

    private void Awake()
    {
        npcRenderer = GetComponentInChildren<Renderer>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (IsVisibleByCamera())
        {
            invisibleTimer = 0f;
        }
        else
        {
            invisibleTimer += Time.deltaTime;
            if (invisibleTimer >= invisibleDuration)
            {
                TeleportToTarget();
                invisibleTimer = 0f;
            }
        }
    }

    private bool IsVisibleByCamera()
    {
        if (npcRenderer == null || playerCamera == null)
            return false;

        var planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        return GeometryUtility.TestPlanesAABB(planes, npcRenderer.bounds);
    }

    private void TeleportToTarget()
    {
        if (teleportTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = teleportTarget.transform.position;
        enabled = false;
    }
}