using UnityEngine;

public class MainMenuSoundManager : Singleton<MainMenuSoundManager>
{
    [SerializeField] private AudioClip _mainMenuMusic;
    
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayMainMenuMusic()
    {
        _audioSource.clip = _mainMenuMusic;
        _audioSource.volume = 1f;
        _audioSource.loop = true;
        _audioSource.Play();
    }
}
