using System;

public class QuestInfo
{
    public string ID { get; private set; }
    
    public string Name { get; private set; }
    public string Description { get; private set; }
        
    public string[] QuestPrerequisiteIDs { get; private set; }
    
    public QuestInfo(
        string id,
        string name,
        string description,
        QuestData[] questPrerequisites)
    {
        ID = id;
        Name = name;
        Description = description;
        
        var questPrerequisiteIDs = Array.ConvertAll(questPrerequisites, questData => questData.ID);
        QuestPrerequisiteIDs = questPrerequisiteIDs;
    }
}
