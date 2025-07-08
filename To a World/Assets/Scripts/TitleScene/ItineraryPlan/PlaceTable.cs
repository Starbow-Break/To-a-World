using System.Collections.Generic;
using UnityEngine;

namespace TitleScene.ItineraryPlan
{
    [CreateAssetMenu(fileName = "PlaceTable", menuName = "ScriptableObjects/PlaceTable")]
    public class PlaceTable : ScriptableObject
    {
        public List<PlaceDesc> places = new List<PlaceDesc>();
    }
}
