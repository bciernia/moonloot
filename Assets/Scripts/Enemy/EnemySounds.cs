using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{
    private AudioSource _audioSource;
    private EnemyStatistics _enemyStatistics;

    private Coroutine _idleCoroutine;
    
    [SerializeField] private float _idleMinDelay = 2f;
    [SerializeField] private float _idleMaxDelay = 5f;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }
    
    private void OnEnable()
    {
        _idleCoroutine = StartCoroutine(IdleSoundRoutine());
    }

    private void OnDisable()
    {
        if (_idleCoroutine != null)
            StopCoroutine(_idleCoroutine);
    }


    public void Step()
    {
        if (_audioSource == null) return;
        
        var currentFloorClip = SoundManager.Instance.GetCurrentFloorClip(transform.position);
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
        SoundManager.Instance.PlaySound(attackSound, 0.75f);
    }

    public void Hit()
    {
        if (_audioSource == null) return;
        var dmgSound = GetEnemySound(_enemyStatistics.DmgSounds);
        SoundManager.Instance.PlaySound(dmgSound, 0.75f);
    }

    public void Die()
    {
        if (_audioSource == null) return;
        var deathSound = GetEnemySound(_enemyStatistics.DeathSounds);
        SoundManager.Instance.PlaySound(deathSound, 0.75f);
    }

    private AudioClip GetEnemySound(List<AudioClip> enemySounds)
    {
        var rndNumber = RNGManager.Instance.GetRandomNumberFromRange(0, enemySounds.Count);
        return enemySounds[rndNumber];
    }

    private IEnumerator IdleSoundRoutine()
    {
        while (true)
        {
            float delay = UnityEngine.Random.Range(_idleMinDelay, _idleMaxDelay);
            yield return new WaitForSeconds(delay);

            PlayEnemySound();
        }
    }

    private void PlayEnemySound()
    {
        if (_enemyStatistics.IdleSounds == null || _enemyStatistics.IdleSounds.Count == 0)
          return;

        var idleSound = GetEnemySound(_enemyStatistics.IdleSounds);
        var calculatedVolume = SoundManager.Instance.CalculateDistFromPlayerForVolume(transform.position);

        if (idleSound != null)
        {
          SoundManager.Instance.PlaySound(idleSound, calculatedVolume - .2f);
        }
    }
}
