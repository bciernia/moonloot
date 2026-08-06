public interface IChestUnlockCondition
{
    bool CanOpen();

    void Interact();

    float Progress { get; }

    string GetProgressText();
    
    void ShowProgress(bool value);
}