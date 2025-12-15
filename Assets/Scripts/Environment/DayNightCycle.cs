using System.Collections;
using System.Collections.Generic;
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

    [Header("Local Lights")]
    public float nightLightIntensity = 2f;
    public float dayLightIntensity = 0f;
    
    [Header("Performance")]
    public float updateInterval = 0.1f;
    
    private float timer;
    private readonly List<Light2D> lights = new List<Light2D>();
    
    [SerializeField] private bool isNight = false;

    private void OnEnable()
    {
#pragma warning disable UDR0005
        SceneManager.sceneLoaded += OnSceneLoaded;
#pragma warning restore UDR0005
        AssignLights();
        InitializeLightsState();
        StartCoroutine(CycleRoutine());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignLights();
    }

    private void AssignLights()
    {
        if (globalLight == null)
        {
            foreach (var l in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
            {
                if (l.lightType == Light2D.LightType.Global)
                {
                    globalLight = l;
                    break;
                }
            }

            if (globalLight == null)
                Debug.LogWarning("No Global Light2D found in the scene!");
        }
        
        lights.Clear();
        foreach (var obj in GameObject.FindGameObjectsWithTag("Light"))
        {
            var sceneLight = obj.GetComponent<Light2D>();
            if (sceneLight != null && sceneLight.lightType != Light2D.LightType.Global)
                lights.Add(sceneLight);
        }
    }
    
    private IEnumerator CycleRoutine()
    {
        while (true)
        {
            UpdateLighting();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdateLighting()
    {
        if (globalLight == null) return;

        timer += Time.deltaTime;
        var t = (timer % dayDuration) / dayDuration;

        Color targetColor;

        if (t < 0.4f)
        {
            var lerpT = t / 0.4f;
            targetColor = Color.Lerp(dayColor, eveningColor, lerpT);
        }
        else if (t < 0.6f)
        {
            var lerpT = (t - 0.4f) / 0.2f;
            targetColor = Color.Lerp(eveningColor, nightColor, lerpT);
        }
        else
        {
            var lerpT = (t - 0.6f) / 0.4f;
            targetColor = Color.Lerp(nightColor, dayColor, lerpT);
        }

        globalLight.color = targetColor;

        var nowNight = (t >= 0.55f && t < 0.9f);

        if (nowNight && !isNight)
        {
            SetLights(nightLightIntensity);
            isNight = true;
        }
        else if (!nowNight && isNight)
        {
            SetLights(dayLightIntensity);
            isNight = false;
        }
    }
    
    private void InitializeLightsState()
    {
        if (globalLight == null) return;

        var t = (timer % dayDuration) / dayDuration;
        var nowNight = (t >= 0.55f && t < 0.9f);

        if (nowNight)
        {
            SetLights(nightLightIntensity);
            isNight = true;
        }
        else
        {
            SetLights(dayLightIntensity);
            isNight = false;
        }
    }

    private void SetLights(float intensity)
    {
        foreach (var light in lights)
        {
            if (light != null)
                light.intensity = intensity;
        }
    }
}
