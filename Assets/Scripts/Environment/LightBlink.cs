using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightBlink : MonoBehaviour
{
    public Light2D light2D;
    public float baseIntensity = 1f;
    public float innerRadiusIntensity = 2f;
    public float flickerAmount = 0.3f;
    public float flickerSpeed = 5f;

    private float randomSeed;

    void Start()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        randomSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        var noise = Mathf.PerlinNoise(randomSeed, Time.time * flickerSpeed);
        light2D.intensity = baseIntensity + (noise - 0.5f) * flickerAmount * 2f;
        light2D.pointLightInnerRadius = innerRadiusIntensity + (noise - 0.5f) * flickerAmount * 2f; 
    }
}
