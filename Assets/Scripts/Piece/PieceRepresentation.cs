using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Piece
{
    public class PieceRepresentation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform pieceVisual;
        [SerializeField] private Vector2 carryOffset;
        private PieceType _pieceType;
        private CancellationTokenSource _tokenSource;
        private RectTransform _parent;
        private Vector2 _rootSize;
        private GameplayGrid _grid;
        private PiecePickerUI _piecePickerUI;

        public async void LoadData(PieceType pieceType, PiecePickerUI piecePickerUI)
        {
            _piecePickerUI = piecePickerUI;
            await UniTask.NextFrame();
            _pieceType = pieceType;
            _parent = transform.parent as RectTransform;
            var rect = transform.root.GetComponent<RectTransform>().rect;
            _rootSize = new Vector2(rect.width, rect.height);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _tokenSource = new CancellationTokenSource();
            Drag();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _tokenSource?.Cancel();
        }

        private async void Drag()
        {
            rectTransform.SetParent(transform.root as RectTransform, true);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;

            pieceVisual.anchoredPosition = carryOffset;

            while (!_tokenSource.IsCancellationRequested)
            {
                var result = await UniTask.NextFrame(_tokenSource.Token).SuppressCancellationThrow();
                if (result)
                {
                    rectTransform.SetParent(_parent);

                    rectTransform.anchorMin = Vector2.one / 2f;
                    rectTransform.anchorMax = Vector2.one / 2f;

                    rectTransform.anchoredPosition = Vector2.zero;
                    pieceVisual.anchoredPosition = Vector2.zero;

                    if (!TryToPlace())
                    {
                        if (_grid != null)
                        {
                            _grid.DisableHighlight();
                        }
                    }
                    else
                    {
                        Destroy(gameObject);

                        await UniTask.NextFrame();
                        _piecePickerUI.OnPiecePicked(_pieceType);
                        _grid.IsBoardLocked();
                    }


                    _tokenSource?.Dispose();
                    _tokenSource = null;
                    return;
                }

                rectTransform.anchoredPosition = GetMappedPosition();
                TryHighlight();
            }
        }

        private Vector2 GetMappedPosition()
        {
            var inputPos = Input.mousePosition;
            var x = inputPos.x / Screen.width * _rootSize.x;
            var y = inputPos.y / Screen.height * _rootSize.y;
            return new Vector2(x, y);
        }

        private void TryHighlight()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                return;
            }

            var grid = hit.transform.GetComponentInParent<GameplayGrid>();

            if (grid == null)
            {
                return;
            }

            _grid = grid;

            _grid.HighlightGrid(_pieceType);
        }

        private bool TryToPlace()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                return false;
            }

            var grid = hit.transform.GetComponentInParent<GameplayGrid>();

            if (grid == null)
            {
                return false;
            }

            return grid.TryPlaceToGrid(_pieceType);
        }

        public PieceType GetPieceType()
        {
            return _pieceType;
        }
    }
}