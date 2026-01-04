using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    private int pauseRequests = 0;

    public void RequestPause()
    {
        pauseRequests++;
        UpdateTimeScale();
    }

    public void ReleasePause()
    {
        pauseRequests = Mathf.Max(0, pauseRequests - 1);
        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        Time.timeScale = pauseRequests > 0 ? 0f : 1f;
    }
}