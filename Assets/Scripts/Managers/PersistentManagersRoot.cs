public class PersistentManagersRoot : Singleton<PersistentManagersRoot>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}