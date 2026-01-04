using UnityEngine;

public class PlayerWorldMapVisual : MonoBehaviour
{
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 worldMapScale = Vector3.one * 0.3f;

    private void OnEnable()
    {
#pragma warning disable UDR0005
        GameManager.OnGameModeChanged += OnModeChanged;
#pragma warning restore UDR0005
    }

    private void OnDisable()
    {
        GameManager.OnGameModeChanged -= OnModeChanged;
    }

    private void OnModeChanged(GameMode mode)
    {
        transform.localScale = mode == GameMode.WorldMap
            ? worldMapScale
            : normalScale;
    }
}