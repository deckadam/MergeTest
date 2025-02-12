using Piece;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Level Data", menuName = "Level/LevelData", order = 0)]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private Vector2Int gridSize;
        [SerializeField] private PieceType[] availablePieces;
        [SerializeField] private Vector2Int gemCountRange;

        public Color bgColor;
        public Color levelColor;
        public Color edgeColor;
        public Color centerColor;
        public Color bottomColor;
        public Color topColor;
        
        public int requiredGemCount;

        public Vector2Int GridSize => gridSize;
        public PieceType[] AvailablePieces => availablePieces;
        public Vector2Int GemCountRange => gemCountRange;
    }
}