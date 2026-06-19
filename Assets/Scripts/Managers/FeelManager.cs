using MoreMountains.Feedbacks;

public class FeelManager : Singleton<FeelManager>
{
    private MMF_Player _feelFeedback;
    
    protected override void Awake()
    {
        base.Awake();

        _feelFeedback = GetComponent<MMF_Player>();
    }
    
    public void PlayDamage()
    {
        _feelFeedback?.PlayFeedbacks();
    }
}
