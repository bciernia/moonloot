public class DecisionStopRunning : FSMDecision
{
    private EnemyStatistics _enemyStatistics;

    private void Awake()
    {
        _enemyStatistics = GetComponent<EnemyStatistics>();
    }

    public override bool Decide()
    {
        return !_enemyStatistics.ShouldRunAway;
    }
}