using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay;
using MobileHapticsProFreeEdition;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Piece
{
    public class PieceRepresentation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip placedSound;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform pieceVisual;
        [SerializeField] private Vector2 carryOffset;
        private PieceType _pieceType;
        private CancellationTokenSource _tokenSource;
        private RectTransform _parent;
        private Vector2 _rootSize;
        private GameplayGrid _grid;
        private GamePlayUI _gamePlayUI;

        public async void LoadData(PieceType pieceType, GamePlayUI gamePlayUI, Color color)
        {
            var images = GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                if (image.gameObject == gameObject)
                {
                    continue;
                }

                image.color = color;
            }

            _gamePlayUI = gamePlayUI;
            await UniTask.NextFrame();
            _pieceType = pieceType;
            _parent = transform.parent as RectTransform;
            var rect = transform.root.GetComponent<RectTransform>().rect;
            _rootSize = new Vector2(rect.width, rect.height);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!GameplayLevel.CanTakeInput)
            {
                return;
            }

            AndroidTapticWave.Haptic(HapticModes.Select);
            _tokenSource = new CancellationTokenSource();
            Drag();
            AudioManager.instance.PlayAudio(clickSound);
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
                    if (!TryToPlace())
                    {
                        if (_grid != null)
                        {
                            _grid.DisableHighlight();
                        }
                    }
                    else
                    {
                        AndroidTapticWave.Haptic(HapticModes.Confirm);
                        AudioManager.instance.PlayAudio(placedSound);

                        Destroy(gameObject);

                        await UniTask.NextFrame();
                        _gamePlayUI.OnPiecePicked(_pieceType);
                        _grid.IsBoardLocked();
                        return;
                    }

                    rectTransform.SetParent(_parent);

                    rectTransform.anchorMin = Vector2.one / 2f;
                    rectTransform.anchorMax = Vector2.one / 2f;

                    rectTransform.anchoredPosition = Vector2.zero;
                    pieceVisual.anchoredPosition = Vector2.zero;

                    AndroidTapticWave.Haptic(HapticModes.Alert);


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