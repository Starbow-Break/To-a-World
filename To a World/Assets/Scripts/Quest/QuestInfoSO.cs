using UnityEngine;

[CreateAssetMenu(fileName = "QuestInfoSO", menuName = "ScriptableObjects/QuestInfoSO", order = 1)]
public class QuestInfoSO : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }

    [Header("General")] 
    public string DisplayName;

    [Header("Requirements")] 
    public QuestInfoSO[] questPrerequisites;

    [Header("Steps")]
    public GameObject[] questStepPrefabs;
    
    private void OnValidate()
    {
        #if UNITY_EDITOR
        ID = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}
