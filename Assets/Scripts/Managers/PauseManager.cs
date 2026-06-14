using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    private int _pauseRequests = 0;

    public int pauseRequests => _pauseRequests;
    
    public void RequestPause()
    {
        _pauseRequests++;
        UpdateTimeScale();
    }

    public void ReleasePause()
    {
        _pauseRequests = Mathf.Max(0, _pauseRequests - 1);
        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        Time.timeScale = _pauseRequests > 0 ? 0f : 1f;
    }

    public void ClearAllPauses()
    {
        _pauseRequests = 0;
        UpdateTimeScale();
    }
    
    public bool IsGamePaused => _pauseRequests > 0;
}