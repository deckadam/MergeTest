using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "LevelList", menuName = "Level/LevelList", order = 1)]
    public class LevelList : ScriptableObject
    {
        [SerializeField] private LevelData[] levels;

        public LevelData[] Levels => levels;
    }
}