using UnityEngine;

public class RNGManager : Singleton<RNGManager>
{
    private const int MIN_VALUE = 1;
    private const int MAX_VALUE = 100;
    
    public float GetRandomNumber() =>  Random.Range(MIN_VALUE, MAX_VALUE);

    public bool MakeSkillCheck(float skillValue) => GetRandomNumber() <= skillValue;
}