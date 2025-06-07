using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraBoundTriggerZone : MonoBehaviour
{
    [SerializeField] private Transform targetFocusPoint;
    [SerializeField] private GameObject upColliderGameObject;
    [SerializeField] private GameObject downColliderGameObject;
    [SerializeField] private GameObject leftColliderGameObject;
    [SerializeField] private GameObject rightColliderGameObject;
    
    private CinemachineCamera vcam;

    private void Start()
    {
        vcam = FindObjectOfType<CinemachineCamera>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            UnblockExitFromArea();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        MoveCameraToPoint();
        BlockExitFromArea();
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        MoveCameraToPlayer();
    }

    private void MoveCameraToPoint()
    {
        vcam.Target.TrackingTarget = targetFocusPoint;
    }

    private void MoveCameraToPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        vcam.Target.TrackingTarget = player.transform;
    }

    private void BlockExitFromArea()
    {
        upColliderGameObject.SetActive(true);
        downColliderGameObject.SetActive(true);
        leftColliderGameObject.SetActive(true);
        rightColliderGameObject.SetActive(true);
    }

    private void UnblockExitFromArea()
    {
        upColliderGameObject.SetActive(false);
        downColliderGameObject.SetActive(false);
        leftColliderGameObject.SetActive(false);
        rightColliderGameObject.SetActive(false);
    }
}
