using System.Collections.Generic;

public class CombatManager : Singleton<CombatManager>
{
    private readonly HashSet<EnemyBrain> _engagedEnemies = new HashSet<EnemyBrain>();

    public bool IsPlayerInCombat => _engagedEnemies.Count > 0;

    public void RegisterEnemy(EnemyBrain enemy)
    {
        if (enemy == null) return;

        var wasEmpty = _engagedEnemies.Count == 0;

        _engagedEnemies.Add(enemy);

        if (wasEmpty && _engagedEnemies.Count == 1)
        {
            SetCombatGameState(true);
        }
    }

    public void UnregisterEnemy(EnemyBrain enemy)
    {
        if (enemy == null) return;

        _engagedEnemies.Remove(enemy);

        if (_engagedEnemies.Count == 0)
        {
            SetCombatGameState(false);
        }
    }

    public void ClearCombat()
    {
        _engagedEnemies.Clear();
        SetCombatGameState(false);
    }

    private void SetCombatGameState(bool isInCombat)
    {
        if (isInCombat)
        {
            SoundManager.Instance.PlayCombatMusic();
        }
        else
        {
            SoundManager.Instance.StopCombatMusic();
        }

        //  tutaj w przyszłości:
        // - blokada save
        // - zmiana UI
        // - zmiana światła
        // - heartbeat
    }
}