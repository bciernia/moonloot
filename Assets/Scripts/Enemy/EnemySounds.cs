using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{
    private AudioSource _audioSource;
    private EnemyStatistics _enemyStatistics;

    private Coroutine _idleCoroutine;
    private bool _idleSoundsEnabled = true;
    
    [SerializeField] private float _idleMinDelay = 2f;
    [SerializeField] private float _idleMaxDelay = 5f;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }
    
    private void OnEnable()
    {
        StartIdleSounds();
    }

    private void OnDisable()
    {
        if (_idleCoroutine != null)
        {
            StopCoroutine(_idleCoroutine);
            _idleCoroutine = null;
        }
    }


    public void Step()
    {
        if (_audioSource == null) return;
        
        var currentFloorClip = SoundManager.Instance.GetCurrentFloorClip(transform.position);

        if (currentFloorClip == null) return;
        
        var calculatedVolume = SoundManager.Instance.CalculateDistFromPlayerForVolume(transform.position);

        if (currentFloorClip != null)
        {
            _audioSource.PlayOneShot(currentFloorClip, calculatedVolume - .2f);
        }
    }

    public void Attack()
    {
        if (_audioSource == null) return;
        var attackSound = GetEnemySound(_enemyStatistics.AttackSounds);
        SoundManager.Instance.PlaySound(attackSound);
    }

    public void Hit()
    {
        if (_audioSource == null) return;
        var attackSound = GetEnemySound(_enemyStatistics.AttackSounds);
        PlayLocalSound(attackSound);
    }

    public void Die()
    {
        if (_audioSource == null) return;
        var deathSound = GetEnemySound(_enemyStatistics.DeathSounds);
        PlayLocalSound(deathSound);
    }

    public void SetIdleSoundsEnabled(bool enabled)
    {
        _idleSoundsEnabled = enabled;

        if (!enabled)
        {
            if (_idleCoroutine != null)
            {
                StopCoroutine(_idleCoroutine);
                _idleCoroutine = null;
            }

            return;
        }

        StartIdleSounds();
    }

    private void StartIdleSounds()
    {
        if (!isActiveAndEnabled || !_idleSoundsEnabled || _idleCoroutine != null)
            return;

        _idleCoroutine = StartCoroutine(IdleSoundRoutine());
    }

    private AudioClip GetEnemySound(List<AudioClip> enemySounds)
    {
        var rndNumber = RNGManager.Instance.GetRandomInt(0, enemySounds.Count);
        return enemySounds[rndNumber];
    }

    private IEnumerator IdleSoundRoutine()
    {
        while (_idleSoundsEnabled)
        {
            var delay = UnityEngine.Random.Range(_idleMinDelay, _idleMaxDelay);
            yield return new WaitForSeconds(delay);

            if (_idleSoundsEnabled)
                PlayEnemySound();
        }

        _idleCoroutine = null;
    }

    private void PlayEnemySound()
    {
        if (_enemyStatistics.IdleSounds == null || _enemyStatistics.IdleSounds.Count == 0)
            return;

        var idleSound = GetEnemySound(_enemyStatistics.IdleSounds);

        PlayLocalSound(idleSound);
    }
    
    private void PlayLocalSound(AudioClip clip)
    {
        if (clip == null)
            return;

        var volume =
            SoundManager.Instance
                .CalculateDistFromPlayerForVolume(
                    transform.position);

        if (volume <= 0.05f)
            return;

        _audioSource.PlayOneShot(
            clip,
            volume);
    }
}
