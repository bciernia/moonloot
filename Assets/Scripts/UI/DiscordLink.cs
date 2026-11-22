using UnityEngine;

public class DiscordLink : MonoBehaviour
{
    [SerializeField] private string inviteUrl;

    public void OpenDiscord()
    {
        if (!string.IsNullOrWhiteSpace(inviteUrl))
            Application.OpenURL(inviteUrl);
        else
            Debug.LogWarning("Discord invite URL is not assigned.", this);
    }
}
