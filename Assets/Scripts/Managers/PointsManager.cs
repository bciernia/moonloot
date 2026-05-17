using TMPro;
using UnityEngine;
using System.Collections;

public class PointsManager : Singleton<PointsManager>
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
}