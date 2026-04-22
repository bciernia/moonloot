using UnityEngine;

public class RNGManager : Singleton<RNGManager>
{
    private const int MIN_VALUE = 1;
    private const int MAX_VALUE = 100;
    
    private float GetRandomNumber() =>  Random.Range(MIN_VALUE, MAX_VALUE);
    public int GetRandomNumberFromRange(int min = 1, int max = 101) =>  Random.Range(min, max);
    public bool MakeSkillCheck(float skillValue) => GetRandomNumber() <= skillValue;
}