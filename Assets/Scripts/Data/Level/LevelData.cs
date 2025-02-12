using Piece;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Level Data", menuName = "Level/LevelData", order = 0)]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private Vector2Int gridSize;
        [SerializeField] private PieceType[] availablePieces;
        
        public Vector2Int GridSize => gridSize;
        public PieceType[] AvailablePieces => availablePieces;
    }
}