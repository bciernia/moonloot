public interface IBossSkill
{
    float HealthThreshold { get; }
    bool ExecuteOnce { get; }

    bool CanExecute();

    void Execute();
}