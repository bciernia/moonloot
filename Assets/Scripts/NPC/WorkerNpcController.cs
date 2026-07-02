using UnityEngine;

public class WorkerNpcController : MonoBehaviour
{
    private VillageNpcRuntime _runtime;

    public void SetRuntime(VillageNpcRuntime runtime)
    {
        _runtime = runtime;
    }

    public VillageNpcRuntime GetRuntime()
    {
        return _runtime;
    }
}