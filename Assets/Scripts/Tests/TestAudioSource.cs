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

}
