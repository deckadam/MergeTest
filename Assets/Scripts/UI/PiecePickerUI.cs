using System.Collections.Generic;
using GamePlay;
using Piece;
using UnityEngine;

namespace UI
{
    public class PiecePickerUI : MonoBehaviour
    {
        [SerializeField] private RectTransform[] slots;
        [SerializeField] private PieceRepresentation representationPrefab;

        private PieceType[] _types;
        private GameplayGrid _grid;

        private List<PieceType> _activePieces;

        public void Initialize(PieceType[] pieceTypes, GameplayGrid grid)
        {
            _activePieces = new List<PieceType>();
            _types = pieceTypes;
            _grid = grid;
            LoadDeck();
        }

        private void LoadDeck()
        {
            var randomPieces = PickRandomPieces(_types);
            for (var index = 0; index < slots.Length; index++)
            {
                var newPiece = Instantiate(randomPieces[index].PieceRepresentation, slots[index].transform, true);
                newPiece.transform.localPosition = Vector3.zero;
                newPiece.LoadData(randomPieces[index], this);
                _activePieces.Add(randomPieces[index]);
            }
        }

        private PieceType[] PickRandomPieces(PieceType[] pieceTypes)
        {
            var result = new PieceType[slots.Length];

            var placeablePiece = _grid.GetFirstPlaceablePiece(_types);
            result[0] = placeablePiece;
            for (int i = 1; i < 3; i++)
            {
                result[i] = pieceTypes[Random.Range(0, pieceTypes.Length)];
            }

            return result;
        }

        public void OnPiecePicked(PieceType piece)
        {
            var pieceCount = _activePieces.Count;
            _activePieces.Remove(piece);
            var endPieceCount = _activePieces.Count;

            Debug.LogError(pieceCount + "  " + endPieceCount);

            var filledCount = 0;
            for (var i = 0; i < slots.Length; i++)
            {
                var childCount = slots[i].childCount;
                if (childCount != 0)
                {
                    filledCount++;
                }
            }

            if (filledCount != 0)
            {
                return;
            }

            LoadDeck();
        }

        public List<PieceType> GetActivePieces()
        {
            return _activePieces;
        }
    }
}