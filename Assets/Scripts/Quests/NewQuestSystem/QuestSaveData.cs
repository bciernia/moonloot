using System;
using System.Collections.Generic;

[Serializable]
public class QuestSaveData
{
    public string questID;
    public List<string> entries;
    public bool completed;
}