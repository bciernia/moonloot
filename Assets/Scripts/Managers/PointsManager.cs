using TMPro;
using UnityEngine;
using System.Collections;

public class PointsManager : Singleton<PointsManager>, ISaveable
{
    [SerializeField] public TextMeshProUGUI scoreText;

    private int currentScore = 0;
    private Coroutine countingCoroutine;

    public void AddScore(int amount)
    {
        var targetScore = currentScore + amount;

        if (countingCoroutine != null)
            StopCoroutine(countingCoroutine);

        countingCoroutine = StartCoroutine(AnimateScore(targetScore));
    }

    IEnumerator AnimateScore(int target)
    {
        while (currentScore < target)
        {
            currentScore += Mathf.CeilToInt((target - currentScore) * 0.1f);

            scoreText.text = currentScore.ToString();

            yield return null;
        }

        currentScore = target;
        scoreText.text = currentScore.ToString();
    }

    public int GetCurrentScore() => currentScore;

    public void Save()
    {
        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save("points", currentScore, settings);
    }

    public void Load()
    {
        if (ES3.KeyExists("points"))
        {
            currentScore = ES3.Load<int>("points");
            scoreText.text = currentScore.ToString();
        }
    }
}