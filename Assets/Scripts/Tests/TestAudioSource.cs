using System;
using UnityEngine;

public class TestAudioSource : MonoBehaviour
{
    private AudioSource _audioSource;

    public AudioClip _clip;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            var distance = Vector2.Distance(GameObject.FindWithTag("Player").transform.position, transform.position);
            var volume = Mathf.Clamp01(1 - distance / 15f);
            _audioSource.volume = volume;
            _audioSource.PlayOneShot(_clip, volume);
        }
    }
}
