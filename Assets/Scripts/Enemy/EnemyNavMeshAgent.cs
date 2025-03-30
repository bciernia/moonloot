using UnityEngine.AI;

public static class EnemyNavMeshAgent
{
    public static void EnableNavMeshAgent(NavMeshAgent navMeshAgent)
    {
        if (!navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }
    }

    public static void DisableNavMeshAgent(NavMeshAgent navMeshAgent)
    {
        if (navMeshAgent.enabled)
        {
            navMeshAgent.enabled = false;
        }
    }        
}
