using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ObjectiveObelisk : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private float activationRange = 4f;
    [SerializeField] private float activationTime = 10f;

    [Header("Visuals")]
    [SerializeField] private Light2D obeliskLight;
    [SerializeField] private ParticleSystem activationParticles;

    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 8f;
    [SerializeField] private float activationFlashIntensity = 18f;
    
    private float _currentTime;

    private bool _activated;
    private bool _isCharging;

    private Transform _player;

    public bool IsActivated => _activated;

    private void Start()
    {
        _player = Player.Instance.transform;

        if (obeliskLight != null)
        {
            obeliskLight.intensity = minIntensity;
        }

        if (activationParticles != null)
        {
            activationParticles.Stop();
        }
    }

    private void Update()
    {
        if (_player == null)
            return;

        var distance = Vector3.Distance(
            transform.position,
            _player.position
        );

        if (distance <= activationRange)
        {
            if (!_isCharging)
            {
                _isCharging = true;
                Debug.Log("Activation started!");
            }

            if (_activated) return;
            
            _currentTime += Time.deltaTime;

            var progress = _currentTime / activationTime;

            var pulse = Mathf.Sin(Time.time * 10f) * 0.6f;
                
            if (obeliskLight != null)
            {
                obeliskLight.intensity =
                    Mathf.Lerp(minIntensity, maxIntensity, progress)
                    + pulse;
            }

            if (_currentTime >= activationTime)
            {
                Activate();
            }
        }
        else
        {
            if (_isCharging)
            {
                Debug.Log("Activation stopped!");
            }

            _isCharging = false;

            if (_activated) return;
            
            _currentTime = 0f;

            if (obeliskLight != null)
            {
                obeliskLight.intensity = minIntensity;
            }
        }
    }

    private void Activate()
    {
        if (_activated)
            return;

        _activated = true;
        _isCharging = false;

        Debug.Log("Obelisk activated!");

        HordeManager.Instance.OnObeliskActivated();

        StartCoroutine(ActivationEffect());
    }

    private IEnumerator ActivationEffect()
    {
        if (obeliskLight != null)
        {
            obeliskLight.intensity = activationFlashIntensity;
        }

        yield return new WaitForSeconds(0.5f);

        if (obeliskLight != null)
        {
            obeliskLight.intensity = maxIntensity * 0.4f;
        }

        if (activationParticles != null)
        {
            activationParticles.Play();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _activated
            ? Color.green
            : Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            activationRange
        );
    }
}