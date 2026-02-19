using System;
using System.Collections.Generic;
using System.Linq;
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
    
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private List<AudioSource> _sfxSources;
    
    private Dictionary<TileBase, TileSoundSO> _dataFromTile;
    
    protected override void Awake()
    {
        base.Awake();
        FindMapForSoundManager();
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
            Debug.Log($"{sound} is missing!");
        }
    }

    public void PlayMusic(string sceneName, float volume = .0f)
    {
        var sceneClip = _sceneMusicList.FirstOrDefault(clip => clip.name == sceneName);

        if (sceneClip != null)
        {
            _musicSource.clip = sceneClip;
            _musicSource.volume = volume;
            _musicSource.loop = true;
            _musicSource.Play();
        }
        else
        {
            Debug.Log($"Music for scene {sceneName} is missing!");
        }
    }

    public void ChangeMusic(SoundType sound, float volume = 1f)
    {
        //TODO zmiana muzyki przy walce 
    }
    
    private AudioClip[] GetClips(SoundType sound) => _soundList[(int)sound].Sounds;

    public void FindMapForSoundManager() => _map = GameObject.FindWithTag("SoundFloor").GetComponent<Tilemap>();
    
    public AudioClip GetCurrentFloorClip(Vector2 worldPosition)
    {
        var gridPosition = _map.WorldToCell(worldPosition);
        var tile = _map.GetTile(gridPosition);

        var index = Random.Range(0, _dataFromTile[tile].Clips.Length);
        var currentFloorClip = _dataFromTile[tile].Clips[index];

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
        foreach (var source in _sfxSources.Where(source => !source.isPlaying))
        {
            source.PlayOneShot(clip, volume);
            return;
        }
    }

    public float CalculateDistFromPlayerForVolume(Vector2 worldPosition)
    {
        var distance = Vector2.Distance(GameObject.FindWithTag("Player").transform.position, worldPosition);
        return Mathf.Clamp01(1 - distance / 15f);
    }
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds => sounds;
    [SerializeField] public SoundType name;
    [SerializeField] private AudioClip[] sounds;
}
