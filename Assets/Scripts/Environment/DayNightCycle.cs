using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class DayNightCycle : MonoBehaviour
{
    public Light2D globalLight;

    [Header("Cycle Settings")]
    public float dayDuration = 60f;

    [Header("Colors")]
    public Color dayColor = Color.white;
    public Color eveningColor = new Color(1f, 0.6f, 0.3f);
    public Color nightColor = new Color(0.2f, 0.3f, 0.6f);

    private float timer;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AssignGlobalLight();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignGlobalLight();
    }

    private void AssignGlobalLight()
    {
        if (globalLight == null)
        {
            foreach (var light in FindObjectsOfType<Light2D>())
            {
                if (light.lightType == Light2D.LightType.Global)
                {
                    globalLight = light;
                    break;
                }
            }

            if (globalLight == null)
                Debug.LogWarning("No Global Light2D found in the scene!");
        }
    }

    private void Update()
    {
        if (globalLight == null) return;

        timer += Time.deltaTime;
        float t = (timer % dayDuration) / dayDuration;

        Color targetColor;

        if (t < 0.4f)
        {
            float lerpT = t / 0.4f;
            targetColor = Color.Lerp(dayColor, eveningColor, lerpT);
        }
        else if (t < 0.6f)
        {
            float lerpT = (t - 0.4f) / 0.2f;
            targetColor = Color.Lerp(eveningColor, nightColor, lerpT);
        }
        else
        {
            float lerpT = (t - 0.6f) / 0.4f;
            targetColor = Color.Lerp(nightColor, dayColor, lerpT);
        }

        globalLight.color = targetColor;
    }
}
