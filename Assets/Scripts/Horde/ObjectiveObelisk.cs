using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ObjectiveObelisk : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private float activationRange = 4f;
    [SerializeField] private float activationTime = 10f;

    [Header("Visuals")]
    [SerializeField] private Light2D obeliskLight;
    [SerializeField] private ParticleSystem activationParticles;

    [SerializeField] private SpriteRenderer greyStatue;
    [SerializeField] private Transform statueTransform;

    [SerializeField] private Image loaderBar;
    [SerializeField] private Canvas loaderCanvas;

    [SerializeField] private RangeIndicator rangeIndicator;

    [SerializeField] private Color rangeMinColor =
        new(1f, 0.8f, 0.2f, 0.25f);

    [SerializeField] private Color rangeMaxColor =
        new(1f, 0.95f, 0.4f, 0.7f);

    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 8f;
    [SerializeField] private float activationFlashIntensity = 18f;

    [SerializeField] private float minOuterRadius = 2f;
    [SerializeField] private float maxOuterRadius = 6f;

    [SerializeField] private float minInnerRadius = 0.2f;
    [SerializeField] private float maxInnerRadius = 2f;

    [Header("Colors")]
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor =
        new(1f, 0.85f, 0.2f);

    private float _currentTime;

    private bool _activated;
    private bool _isCharging;

    private Transform _player;
    private Vector3 _loaderDefaultScale;
    public bool IsActivated => _activated;

    private void Start()
    {
        _player = Player.Instance?.transform;

        if (activationParticles != null)
            activationParticles.Stop();

        if (rangeIndicator != null)
        {
            rangeIndicator.SetRangeIndicatorRadius(activationRange);

            rangeIndicator.SetRangeIndicatorColor(
                rangeMinColor,
                rangeMaxColor);
        }
        
        if (loaderCanvas != null)
            _loaderDefaultScale = loaderCanvas.transform.localScale;
        
        ResetVisuals();
    }

    private void Update()
    {
        if (_player == null || _activated)
            return;

        var sqrDistance =
            (transform.position - _player.position).sqrMagnitude;

        var inRange =
            sqrDistance <= activationRange * activationRange;

        if (!inRange)
        {
            if (!_isCharging)
                return;

            _isCharging = false;
            _currentTime = 0f;

            ResetVisuals();

            return;
        }

        if (!_isCharging)
        {
            _isCharging = true;

            if (loaderCanvas != null)
                loaderCanvas.enabled = true;

            if (rangeIndicator != null)
            {
                rangeIndicator.SetRangeIndicatorRadius(activationRange);
                rangeIndicator.gameObject.SetActive(true);
            }
        }

        _currentTime += Time.deltaTime;

        var progress =
            Mathf.Clamp01(_currentTime / activationTime);

        UpdateVisuals(progress);

        if (_currentTime >= activationTime)
        {
            Activate();
        }
    }

    private void Activate()
    {
        if (_activated)
            return;

        _activated = true;
        _isCharging = false;
        _currentTime = activationTime;

        if (loaderCanvas != null)
        {
            loaderCanvas.enabled = false;
            loaderCanvas.transform.localScale = _loaderDefaultScale;
        }

        if (rangeIndicator != null)
            rangeIndicator.gameObject.SetActive(false);

        if (greyStatue != null)
            greyStatue.color = activeColor;

        HordeManager.Instance.OnObeliskActivated();

        StartCoroutine(ActivationEffect());
    }
    
        private IEnumerator ActivationEffect()
    {
        if (obeliskLight != null)
        {
            obeliskLight.intensity = activationFlashIntensity;
            obeliskLight.pointLightOuterRadius = maxOuterRadius * 1.5f;
        }

        if (activationParticles != null)
            activationParticles.Play();

        yield return StartCoroutine(ScaleAndShake());

        if (obeliskLight != null)
        {
            obeliskLight.intensity = maxIntensity;
            obeliskLight.pointLightOuterRadius = maxOuterRadius;
            obeliskLight.pointLightInnerRadius = maxInnerRadius;
        }
    }

    private IEnumerator ScaleAndShake()
    {
        if (statueTransform == null)
            yield break;

        var originalScale = statueTransform.localScale;
        var originalPosition = statueTransform.localPosition;

        var targetScale = originalScale * 1.35f;

        float t = 0f;

        while (t < 0.2f)
        {
            t += Time.deltaTime;

            statueTransform.localScale =
                Vector3.Lerp(
                    originalScale,
                    targetScale,
                    t / 0.2f);

            statueTransform.localPosition =
                originalPosition +
                new Vector3(
                    Mathf.Sin(Time.time * 80f) * 0.05f,
                    Mathf.Cos(Time.time * 65f) * 0.03f,
                    0f);

            yield return null;
        }

        t = 0f;

        while (t < 0.15f)
        {
            t += Time.deltaTime;

            statueTransform.localScale =
                Vector3.Lerp(
                    targetScale,
                    originalScale,
                    t / 0.15f);

            statueTransform.localPosition =
                originalPosition +
                new Vector3(
                    Mathf.Sin(Time.time * 90f) * 0.02f,
                    Mathf.Cos(Time.time * 75f) * 0.015f,
                    0f);

            yield return null;
        }

        statueTransform.localScale = originalScale;
        statueTransform.localPosition = originalPosition;
    }

    private void UpdateVisuals(float progress)
    {
        if (loaderBar != null)
            loaderBar.fillAmount = progress;

        if (loaderCanvas != null)
        {
            loaderCanvas.transform.localScale =
                _loaderDefaultScale *
                (1f + Mathf.Sin(Time.time * 5f) * 0.03f);
        }

        if (greyStatue != null)
        {
            greyStatue.color =
                Color.Lerp(
                    inactiveColor,
                    activeColor,
                    progress);
        }

        if (obeliskLight != null)
        {
            var pulseSpeed =
                Mathf.Lerp(4f, 15f, progress);

            var pulseAmount =
                Mathf.Lerp(0.1f, 0.5f, progress);

            var pulse =
                Mathf.Sin(Time.time * pulseSpeed) *
                pulseAmount;

            obeliskLight.intensity =
                Mathf.Lerp(
                    minIntensity,
                    maxIntensity,
                    progress) + pulse;

            obeliskLight.pointLightOuterRadius =
                Mathf.Lerp(
                    minOuterRadius,
                    maxOuterRadius,
                    progress);

            obeliskLight.pointLightInnerRadius =
                Mathf.Lerp(
                    minInnerRadius,
                    maxInnerRadius,
                    progress);
        }
    }

    private void ResetVisuals()
    {
        if (loaderCanvas != null)
        {
            loaderCanvas.enabled = false;
            loaderCanvas.transform.localScale = _loaderDefaultScale;
        }

        if (loaderBar != null)
            loaderBar.fillAmount = 0f;

        if (rangeIndicator != null)
            rangeIndicator.gameObject.SetActive(false);

        if (greyStatue != null)
            greyStatue.color = inactiveColor;

        if (obeliskLight != null)
        {
            obeliskLight.intensity = minIntensity;
            obeliskLight.pointLightOuterRadius = minOuterRadius;
            obeliskLight.pointLightInnerRadius = minInnerRadius;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            _activated
                ? Color.green
                : Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            activationRange);
    }
}