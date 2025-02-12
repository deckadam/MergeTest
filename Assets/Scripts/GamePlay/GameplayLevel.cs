using Data;
using UI;
using UnityEngine;
using Zenject;

namespace GamePlay
{
    public class GameplayLevel : MonoBehaviour
    {
        [SerializeField] private GameplayGrid grid;
        [SerializeField] private GameplayCamera gCamera;

        private PiecePickerUI _piecePickerUI;
        private LevelData _levelData;

        [Inject]
        private void Inject(PiecePickerUI piecePickerUI)
        {
            _piecePickerUI = piecePickerUI;
        }

        public void Initialize(LevelData levelData)
        {
            _levelData = levelData;
            grid.Initialize(levelData);
            gCamera.Initialize(levelData.GridSize);

            LoadDeck();
        }

        private void LoadDeck()
        {
            _piecePickerUI.Initialize(_levelData.AvailablePieces,grid);
        }
    }
}