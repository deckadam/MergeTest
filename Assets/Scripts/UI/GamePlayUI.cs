using System.Collections.Generic;
using Data;
using DG.Tweening;
using GamePlay;
using Piece;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GamePlayUI : MonoBehaviour
    {
        [SerializeField] private RectTransform[] slots;
        [SerializeField] private PieceRepresentation representationPrefab;
        [SerializeField] private TextMeshProUGUI requirementText;
        [SerializeField] private TextMeshProUGUI levelCounterText;

        [SerializeField] private Image bottomPanelBG;
        [SerializeField] private Image topPanelBG;

        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private AnimationCurve comboTextCurve;

        private PieceType[] _types;
        private GameplayGrid _grid;

        private List<PieceType> _activePieces;
        private LevelData _levelData;

        public void Initialize(LevelData levelData, GameplayGrid grid)
        {
            levelCounterText.text = "LEVEL " + (PlayerPrefs.GetInt("LevelIndex", 0) + 1);
            foreach (var rectTransform in slots)
            {
                for (int i = 0; i < rectTransform.childCount; i++)
                {
                    Destroy(rectTransform.GetChild(0).gameObject);
                }
            }

            _activePieces = new List<PieceType>();
            _levelData = levelData;
            _types = _levelData.AvailablePieces;
            _grid = grid;

            bottomPanelBG.color = _levelData.bottomColor;
            topPanelBG.color = _levelData.topColor;

            LoadDeck();
            UpdateRequirementText(0);
        }

        private void LoadDeck()
        {
            var randomPieces = PickRandomPieces(_types);
            for (var index = 0; index < slots.Length; index++)
            {
                var newPiece = Instantiate(randomPieces[index].PieceRepresentation, slots[index].transform, true);
                newPiece.transform.localPosition = Vector3.zero;
                newPiece.LoadData(randomPieces[index], this,_levelData.edgeColor);
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
            _activePieces.Remove(piece);

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

        public void UpdateRequirementText(int current)
        {
            requirementText.text = current + " / " + _levelData.requiredGemCount;
        }


        public void ShowComboText(int count)
        {
            comboText.transform.DOKill();
            comboText.text = "COMBO X" + count;
            var rot = Random.Range(0f, 1f) > 0.5f ? 8f : -8;
            comboText.transform.localScale = Vector3.zero;
            comboText.transform.localRotation = Quaternion.Euler(0, 0, rot);
            comboText.transform.DOScale(1.2f, 0.3f).SetEase(comboTextCurve);
            comboText.transform.DOScale(0f, 0.15f).SetEase(Ease.OutBack).SetDelay(0.45f);
        }
    }
}