using System;
using TMPro;
using UnityEngine;

namespace Horde
{
    public class KillChest : MonoBehaviour, IChestUnlockCondition
    {
        [SerializeField] private int requiredKills = 10;
        
        [SerializeField]
        private Canvas progressCanvas;

        [SerializeField]
        private TextMeshProUGUI progressText;
        
        private int _currentKills;
        private bool _started;

        private void Start()
        {
            if (progressCanvas != null)
                progressCanvas.enabled = false;
        }

        public bool CanOpen()
        {
            return _currentKills >= requiredKills;
        }
        
        public void Interact()
        {
            if (_started)
                return;

            _started = true;

            EnemyEvents.EnemyKilled += OnEnemyKilled;

            if (progressCanvas != null)
                progressCanvas.enabled = true;

            UpdateText();

            Debug.Log("Kill challenge started!");
        }
        
        private void OnDestroy()
        {
            EnemyEvents.EnemyKilled -= OnEnemyKilled;
        }
        
        private void OnEnemyKilled(
            EnemyStatistics enemy)
        {
            AddKill();
        }

        public float Progress =>
            (float)_currentKills / requiredKills;
        
        public string GetProgressText()
        {
            return $"Kills {_currentKills}/{requiredKills}";
        }
        
        public void ShowProgress(bool value)
        {
            if (progressCanvas == null)
                return;

            if (!_started)
            {
                progressCanvas.enabled = false;
                return;
            }

            progressCanvas.enabled = value;
        }
        
        private void UpdateText()
        {
            if (progressText == null)
                return;

            if (_currentKills >= requiredKills)
            {
                progressText.text = "Ready to open";
            }
            else
            {
                progressText.text =
                    $"Kill {_currentKills}/{requiredKills}";
            }
        }
        
        private void AddKill()
        {
            if (!_started)
                return;

            if (_currentKills >= requiredKills)
                return;

            _currentKills++;

            UpdateText();

            if (_currentKills >= requiredKills)
            {
                EnemyEvents.EnemyKilled -= OnEnemyKilled;

                Debug.Log("Kill challenge completed!");
            }
        }
    }
}