using UnityEngine;

public class WorkerNpcController : MonoBehaviour
{
    private VillageNpcRuntime _runtime;

    public void SetRuntime(VillageNpcRuntime runtime)
    {
        _runtime = runtime;
    }

    public VillageNpcRuntime GetRuntime() => _runtime;

    public void GetJob(int jobIndex)
    {
        if (_runtime == null)
        {
            Debug.LogWarning("NPC runtime is null!");
            return;
        }

        if (!_runtime.IsWorker)
        {
            Debug.Log("This NPC is not a worker");
            return;
        }

        if (!System.Enum.IsDefined(typeof(WorkerJob), jobIndex))
        {
            Debug.LogWarning($"Invalid job index: {jobIndex}");
            return;
        }

        var job = (WorkerJob)jobIndex;

        if (_runtime.CurrentJob == job)
            return;

        var success = WorkManager.Instance.TryAssignWorker(_runtime, job);

        if (!success)
        {
            Debug.Log($"Cannot assign {job} (no space)");
        }
    }

    public void FireWorker()
    {
        if (_runtime == null)
            return;

        if (!_runtime.IsWorker)
            return;

        if (_runtime.CurrentJob == WorkerJob.None)
            return;

        WorkManager.Instance.TryAssignWorker(_runtime, WorkerJob.None);

        Debug.Log($"{_runtime.Name} is now unemployed");
    }
}