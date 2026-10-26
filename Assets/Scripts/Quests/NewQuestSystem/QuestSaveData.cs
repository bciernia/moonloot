using System;
using System.Collections.Generic;

[Serializable]
public class QuestSaveData
{
    public string questID;
    public List<string> entries;
    public List<int> objectiveProgress;
    public string giverID;
    public string giverName;
    public string giverProfession;
    public string giverPortraitName;
    public bool completed;
}
