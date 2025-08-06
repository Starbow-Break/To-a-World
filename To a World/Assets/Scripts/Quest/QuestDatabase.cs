using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Scriptable Objects/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    [field: SerializeField] public List<QuestData> QuestDatas { get; private set; }
}
