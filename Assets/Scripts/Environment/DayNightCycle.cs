using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class DayNightCycle : MonoBehaviour
{
    public Light2D globalLight;

    [Header("Cycle Settings")]
    public float dayDuration = 600f; // 10 minut

    [Header("Time Thresholds (seconds)")]
    public float eveningStart = 480f; // 8 min
    public float nightStart = 570f;   // 9 min 30 sec

    [Header("Colors")]
    public Color dayColor = Color.white;
    public Color eveningColor = new Color(1f, 0.6f, 0.3f);
    public Color nightColor = new Color(0.2f, 0.3f, 0.6f);

    [Header("Local Lights")]
    public float nightLightIntensity = 2f;
    public float dayLightIntensity = 0f;
    
    public Action OnDayStarted;
    public Action OnEveningStarted;
    public Action OnNightStarted;
    public Action HordeAttack;

    private float timer;
    private readonly List<Light2D> lights = new List<Light2D>();

    [SerializeField] private bool isEvening = false;
    [SerializeField] private bool isNight = false;
    [SerializeField] private bool hordeStarted = false;

    private bool hordePending = false;
    
    private void Awake()
    {
        AssignLights();
        ApplyInitialLighting();
    }

    private void Start()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnEnable()
    {
#pragma warning disable UDR0005
        SceneManager.sceneLoaded += OnSceneLoaded;
#pragma warning restore UDR0005
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignLights();
        ApplyInitialLighting();
    }

    private void Update()
    {
        if(!hordePending) UpdateLighting();
    }

    private void UpdateLighting()
    {
        if (globalLight == null) return;

        timer += Time.deltaTime;

        if (timer > dayDuration)
            timer = dayDuration;

        Color targetColor;

        // DZIEŃ
        if (timer < eveningStart)
        {
            float t = timer / eveningStart;
            targetColor = Color.Lerp(dayColor, eveningColor, t);
        }
        // WIECZÓR
        else if (timer < nightStart)
        {
            float t = (timer - eveningStart) / (nightStart - eveningStart);
            targetColor = Color.Lerp(eveningColor, nightColor, t);

            if (!isEvening)
            {
                isEvening = true;
                OnEveningStarted?.Invoke();
            }
        }
        // NOC
        else
        {
            targetColor = nightColor;

            if (!isNight)
            {
                isNight = true;

                OnNightStarted?.Invoke();

                SetLights(nightLightIntensity);
            }
        }

        // HORDA (koniec dnia)
        if (timer >= dayDuration && !hordeStarted && !hordePending)   
        {
            if (DialogueManager.Instance.IsInDialogue())
            {
                hordePending = true;
            }
            else
            {
                StartHorde();
            }
        }

        globalLight.color = targetColor;
    }

    private void StartHorde()
    {
        if (hordeStarted) return;

        hordeStarted = true;
        hordePending = false;
        Debug.Log("HORDE ATTACK STARTED");
        HordeAttack?.Invoke();
    }

    private void ApplyInitialLighting()
    {
        if (globalLight == null) return;

        Color targetColor;

        if (timer < eveningStart)
        {
            float t = timer / eveningStart;
            targetColor = Color.Lerp(dayColor, eveningColor, t);
        }
        else if (timer < nightStart)
        {
            float t = (timer - eveningStart) / (nightStart - eveningStart);
            targetColor = Color.Lerp(eveningColor, nightColor, t);
            isEvening = true;
        }
        else
        {
            targetColor = nightColor;
            isNight = true;
            SetLights(nightLightIntensity);
        }

        globalLight.color = targetColor;
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

    private void SetLights(float intensity)
    {
        foreach (var light in lights)
        {
            if (light != null)
                light.intensity = intensity;
        }
    }

    public void ResetCycle()
    {
        timer = 0f;
        isEvening = false;
        isNight = false;
        hordeStarted = false;

        SetLights(dayLightIntensity);

        Debug.Log("NEW DAY STARTED");
        OnDayStarted?.Invoke();
    }

    public float GetTimeNormalized()
    {
        return timer / dayDuration;
    }

    private void HandleDialogueEnded()
    {
        if (hordePending) StartCoroutine(StartHordeWithDelay());
    }

    private IEnumerator StartHordeWithDelay()
    {
        Debug.Log("Odliczanieee");
        yield return new WaitForSeconds(.5f);
        StartHorde();
    }
}