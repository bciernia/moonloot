using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseFollower : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private UIInventoryItem item;

    private void Awake()
    {
        mainCamera = Camera.main;
        item = GetComponentInChildren<UIInventoryItem>();
    }

    public void SetData(Sprite sprite, int quantity)
    {
        item.SetData(sprite, quantity);
    }

    private void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            Input.mousePosition,
            canvas.worldCamera,
            out var position
        );

        transform.position = canvas.transform.TransformPoint(position);
    }

    public void Toggle(bool val)
    {
        if(val) Update();
        gameObject.SetActive(val);
    }
}
