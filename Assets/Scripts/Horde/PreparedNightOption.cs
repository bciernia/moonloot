using System;
using System.Collections.Generic;

[Serializable]
public class PreparedNightOption
{
    public NightLocationSO Location;
    public MoonData Moon;
    public MutationData Mutation;
    public List<NightReward> Rewards = new();
}