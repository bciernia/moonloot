using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlickerGroup : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float maxIntensityOffset = 0.15f;
    public float flickerSpeed = 5f;

    private Light2D[] lights;
    private float[] baseIntensities;

    void Start()
    {
        lights = GetComponentsInChildren<Light2D>();

        baseIntensities = new float[lights.Length];

        for (var i = 0; i < lights.Length; i++)
        {
            baseIntensities[i] = lights[i].intensity;
        }
    }

    void Update()
    {
        for (var i = 0; i < lights.Length; i++)
        {
            var noise = Mathf.PerlinNoise(i, Time.time * flickerSpeed);

            noise = (noise - 0.5f) * 2f;

            lights[i].intensity =
                baseIntensities[i] + noise * maxIntensityOffset;
        }
    }
}