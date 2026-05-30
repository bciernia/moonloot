using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private Tilemap _map;
    [SerializeField] private List<TileSoundSO> _tileSounds;
    [SerializeField] private SoundList[] _soundList;

    [SerializeField] private List<AudioClip> _sceneMusicList;
    [SerializeField] private List<AudioClip> _combatMusicList;
    
    [SerializeField] private List<AudioClip> _pickUpSfx;

    [SerializeField] private Transform _sfxSourcesParent;
    private List<AudioSource> _sfxSources;
    
    [SerializeField] private AudioSource _explorationSource;
    [SerializeField] private AudioSource _combatSource;
    [SerializeField] private float _musicFadeDuration = 2f;
    
    [Header("Death music")] [SerializeField]
    private AudioClip _deathMusic;

    [Header("Win music")] [SerializeField]
    private AudioClip _winMusic;
    
    private Coroutine _fadeRoutine;
    private bool _isInCombat;
    
    private Dictionary<TileBase, TileSoundSO> _dataFromTile;
    private Dictionary<AudioClip, float> _lastPlayedTimes = new();
    
    protected override void Awake()
    {
        base.Awake();

        _sfxSources = _sfxSourcesParent
            .GetComponentsInChildren<AudioSource>(true)
            .ToList();

        _dataFromTile = new Dictionary<TileBase, TileSoundSO>();

        foreach (var tileData in _tileSounds)
        {
            foreach (var tile in tileData.Tiles)
            {
                _dataFromTile.Add(tile, tileData);
            }
        }

        PlayMusic(SceneManager.GetActiveScene().name);
    }

    private void Start()
    {
        FindMapForSoundManager();
    }
    
    public void PlaySound(SoundType sound, float volume = 1f)
    {
        var clips = GetSoundsByType(sound);
        var randomClip = clips[Random.Range(0, clips.Length)];

        if (randomClip != null)
        {
            PlaySFX(randomClip, volume);
        }
        else
        {
            Debug.Log($"{sound} is missing!");
        }
    }

    public void PlaySound(AudioClip sound, float volume = 1f)
    {
        if (sound != null)
        {
            PlaySFX(sound, volume);
        }
        else
        {
            Debug.Log("Sound is missing!");
        }
    }

    public void PlayMusic(string sceneName, float volume = 1f)
    {
        var sceneClip = _sceneMusicList.FirstOrDefault(clip => clip.name == sceneName);
        
        if (sceneClip != null)
        {
            ConfigureAndPlayMusic(sceneClip, volume);
        }
        else
        {
            ConfigureAndPlayMusic(_sceneMusicList[0], volume);
            Debug.Log($"Music for scene {sceneName} is missing!");
        }
    }

    private void ConfigureAndPlayMusic(AudioClip clip, float volume = 1f)
    {
        if (_explorationSource == null)
            return;

        _explorationSource.enabled = true;
        _explorationSource.gameObject.SetActive(true);

        _explorationSource.clip = clip;
        _explorationSource.volume = volume;
        _explorationSource.loop = true;
        _explorationSource.Play();
    }

    public void PlayCombatMusic(float volume = 1f)
    {
        if (_combatMusicList.Count == 0)
        {
            Debug.Log("There is no combat music");
            return;
        }

        if (_combatSource == null)
            return;

        _combatSource.enabled = true;
        _combatSource.gameObject.SetActive(true);

        var randomClip =
            _combatMusicList[Random.Range(0, _combatMusicList.Count)];

        _combatSource.clip = randomClip;
        _combatSource.volume = volume;
        _combatSource.loop = true;
        _combatSource.Play();

        StartMusicFade(true);
    }
    
    // public void PlayCombatMusic()
    // {
    //     if (_isInCombat) return;
    //
    //     _isInCombat = true;
    //     StartMusicFade(true);
    // }
    
    public void StopCombatMusic()
    {
        if (!_isInCombat) return;

        _isInCombat = false;
        StartMusicFade(false);
    } 
    
    private void StartMusicFade(bool toCombat)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeMusic(toCombat));
    }
    
    private IEnumerator FadeMusic(bool toCombat)
    {
        var time = 0f;

        var startExploration = _explorationSource.volume;
        var startCombat = _combatSource.volume;

        var targetExploration = toCombat ? 0f : 1f;
        var targetCombat = toCombat ? 1f : 0f;

        while (time < _musicFadeDuration)
        {
            time += Time.deltaTime;
            var t = time / _musicFadeDuration;

            _explorationSource.volume =
                Mathf.Lerp(startExploration, targetExploration, t);

            _combatSource.volume =
                Mathf.Lerp(startCombat, targetCombat, t);

            yield return null;
        }

        _explorationSource.volume = targetExploration;
        _combatSource.volume = targetCombat;
    }

    public void ChangeMusic(SoundType sound, float volume = 1f)
    {
        //TODO zmiana muzyki przy walce 
    }
    
    private AudioClip[] GetClips(SoundType sound) => _soundList[(int)sound].Sounds;

    public void FindMapForSoundManager()
    {
        var soundFloor = GameObject.FindWithTag("SoundFloor");
        if (soundFloor != null) _map = soundFloor.GetComponent<Tilemap>();
    }

    [CanBeNull]
    public AudioClip GetCurrentFloorClip(Vector2 worldPosition)
    {
        if (_map == null) return null;
        var gridPosition = _map.WorldToCell(worldPosition);
        
        var tile = _map.GetTile(gridPosition);
        var index = 0;
        if (_dataFromTile[tile]?.Clips != null)
        {
            index = Random.Range(0, _dataFromTile[tile].Clips.Length);
        }
        
        var currentFloorClip = _dataFromTile[tile]?.Clips[index];

        return currentFloorClip;
    }

    private AudioClip[] GetSoundsByType(SoundType type)
    {
        for (var i = 0; i < _soundList.Length; i++)
        {
            if (_soundList[i].name == type)
            {
                return _soundList[i].Sounds;
            }
        }

        Debug.LogWarning($"Brak dźwięków dla typu: {type}");
        return _soundList[0].Sounds;
    }

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (_lastPlayedTimes.TryGetValue(clip, out var lastTime))
        {
            if (Time.time - lastTime < 0.05f)
                return;
        }

        _lastPlayedTimes[clip] = Time.time;

        foreach (var source in _sfxSources.Where(source => !source.isPlaying))
        {
            source.PlayOneShot(clip, volume);
            return;
        }

        var fallback = _sfxSources[0];

        fallback.Stop();
        fallback.PlayOneShot(clip, volume);
    }

    public float CalculateDistFromPlayerForVolume(Vector2 worldPosition)
    {
        var distance = Vector2.Distance(GameObject.FindWithTag("Player").transform.position, worldPosition);
        return Mathf.Clamp01(1 - distance / 15f);
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindMapForSoundManager();
        PlayMusic(scene.name);
    }
    
    public void PlayDeathMusic(float volume = 1f)
    {
        StopAllCoroutines();

        _isInCombat = false;

        _explorationSource.Stop();
        _combatSource.Stop();

        foreach (var source in _sfxSources)
        {
            source.Stop();
        }

        _explorationSource.clip = _deathMusic;
        _explorationSource.volume = volume;
        _explorationSource.loop = false;
        _explorationSource.Play();
    }

    public void PlayWinMusic(float volume = 1f)
    {
        StopAllCoroutines();

        _isInCombat = false;

        _explorationSource.Stop();
        _combatSource.Stop();

        foreach (var source in _sfxSources)
        {
            source.Stop();
        }

        _explorationSource.clip = _winMusic;
        _explorationSource.volume = volume;
        _explorationSource.loop = false;
        _explorationSource.Play();
    }

    #region SFX

    public void PlayPickUpSFX()
    {
        if (_pickUpSfx.Count == 0)
        {
            Debug.Log("There is no pickup sfx");
            return;
        }

        var randomClip =
            _pickUpSfx[Random.Range(0, _pickUpSfx.Count)];

        PlaySFX(randomClip, 1f);
    }
    
    #endregion
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds => sounds; 
    [SerializeField] public SoundType name;
    [SerializeField] private AudioClip[] sounds;
}
