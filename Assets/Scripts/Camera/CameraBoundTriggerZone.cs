using Unity.Cinemachine;
using UnityEngine;

public class CameraBoundTriggerZone : MonoBehaviour
{
    [SerializeField] private Transform targetFocusPoint;
    public float moveSpeed = 3f;
    private Transform playerTransform;
    private Transform currentTarget;

    private CinemachineCamera virtualCamera;
    private Transform cameraFollowTransform;
    private Transform cameraTargetTransform;
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        currentTarget = playerTransform;

        virtualCamera = FindAnyObjectByType<CinemachineCamera>();

        var camTargetObj = new GameObject("CameraTarget");
        cameraTargetTransform = camTargetObj.transform;
        cameraTargetTransform.position = playerTransform.position;

        virtualCamera.Follow = cameraTargetTransform;
    }

    private void Update()
    {
        if (cameraTargetTransform == null || currentTarget == null) return;

        cameraTargetTransform.position = Vector3.Lerp(
            cameraTargetTransform.position,
            currentTarget.position,
            Time.deltaTime * moveSpeed
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        currentTarget = targetFocusPoint;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        currentTarget = playerTransform;
    }
}