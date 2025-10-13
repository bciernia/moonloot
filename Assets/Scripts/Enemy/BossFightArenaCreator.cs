using System;
using System.Collections.Generic;
using UnityEngine;

public class BossFightArenaCreator : MonoBehaviour
{
    [Header("Arena Settings")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private int arenaWidth = 10;
    [SerializeField] private int arenaHeight = 6;
    [SerializeField] private List<GameObject> enemiesToSpawnInside = new();
    [SerializeField] private int numberOfEnemiesToSpawn = 0;
    [SerializeField] private GameObject destroyAfterArenaFinish;

    private QuestCompletion _questCompletion;

    private void Awake()
    {
        _questCompletion = GetComponent<QuestCompletion>();
    }

    /// <summary>
    /// Tworzy ściany wokół obiektu (pozycja = środek areny).
    /// Uruchamia też monitoring żywotności wrogów (jeśli lista _arenaCreators nie jest pusta).
    ///
    /// Do użycia w dialogach.
    /// </summary>
    public void CreateArena()
    {
        BossFightArena.Instance.CreateArena(
            gameObject,
            wallPrefab,
            arenaWidth,
            arenaHeight,
            transform,
            enemiesToSpawnInside,
            numberOfEnemiesToSpawn,
            _questCompletion,
            destroyAfterArenaFinish
        );
    }
}
