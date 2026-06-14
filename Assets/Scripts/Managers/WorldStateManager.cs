using System.Collections.Generic;

public class WorldStateManager : Singleton<WorldStateManager>, ISaveable
{
    private HashSet<string> states = new();

    public bool HasState(string state)
    {
        return states.Contains(state);
    }

    public void SetState(string state)
    {
        states.Add(state);
    }

    public void RemoveState(string state)
    {
        states.Remove(state);
    }

    public void Save()
    {
        var settings = SaveLoadManager.Instance.GetSettings();
        
        ES3.Save("world_states", new List<string>(states), settings);
    }

    public void Load()
    {
        if (ES3.KeyExists("world_states"))
        {
            var loaded = ES3.Load<List<string>>("world_states");
            states = new HashSet<string>(loaded);
        }
    }
}