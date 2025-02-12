using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Piece;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class GameplayGrid : MonoBehaviour
    {
        public static bool CanTakeInput = true;

        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        [SerializeField] private MeshRenderer gridRenderer;
        [SerializeField] private GameObject filledCenterPrefab;
        [SerializeField] private GameObject edgePrefab;
        [SerializeField] private GameObject cornerPrefab;

        [SerializeField] private SerializedDictionary<Vector2Int, PieceType.Block> _blocks;
        private Camera _camera;
        private Vector2Int _levelDataGridSize;
        private Dictionary<Vector2Int, GameObject> _filledCenters;

        private PiecePickerUI _piecePickerUI;

        [Inject]
        private void Inject(PiecePickerUI piecePickerUI)
        {
            _piecePickerUI = piecePickerUI;
        }

        public void Initialize(LevelData levelData)
        {
            CanTakeInput = true;
            _levelDataGridSize = levelData.GridSize;
            _camera = Camera.main;

            _filledCenters = new Dictionary<Vector2Int, GameObject>();

            gridRenderer.transform.localScale = new Vector3(_levelDataGridSize.x, _levelDataGridSize.y, 1);
            gridRenderer.transform.localPosition = new Vector3(_levelDataGridSize.x / 2f, _levelDataGridSize.y / 2f, 0);
            gridRenderer.material.SetTextureScale(BaseMap, new Vector2(_levelDataGridSize.x, _levelDataGridSize.y));

            _blocks = new SerializedDictionary<Vector2Int, PieceType.Block>();

            for (int x = 0; x < _levelDataGridSize.x + 2; x++)
            {
                for (int y = 0; y < _levelDataGridSize.y + 2; y++)
                {
                    var newBlock = new PieceType.Block();
                    _blocks[new Vector2Int(x - 1, y - 1)] = newBlock;

                    if (x == 0)
                    {
                        newBlock.leftOccupied = true;
                        newBlock.upOccupied = true;
                        newBlock.downOccupied = true;
                        newBlock.visible = false;
                    }

                    if (y == 0)
                    {
                        newBlock.leftOccupied = true;
                        newBlock.rightOccupied = true;
                        newBlock.downOccupied = true;
                        newBlock.visible = false;
                    }

                    if (x > _levelDataGridSize.x)
                    {
                        newBlock.rightOccupied = true;
                        newBlock.upOccupied = true;
                        newBlock.downOccupied = true;
                        newBlock.visible = false;
                    }

                    if (y > _levelDataGridSize.y)
                    {
                        newBlock.rightOccupied = true;
                        newBlock.leftOccupied = true;
                        newBlock.upOccupied = true;
                        newBlock.visible = false;
                    }
                }
            }

            for (int x = -1; x <= _levelDataGridSize.x + 1; x++)
            {
                for (int y = -1; y < _levelDataGridSize.y + 1; y++)
                {
                    var horizontalPos = new Vector3(x, y + 0.5f, -0.1f);
                    var horizontalEdge = Instantiate(edgePrefab, horizontalPos, Quaternion.identity, transform);
                    horizontalEdge.SetActive(false);
                    if (_blocks.ContainsKey(new Vector2Int(x, y)))
                    {
                        _blocks[new Vector2Int(x, y)].leftEdge = horizontalEdge;
                    }

                    if (_blocks.ContainsKey(new Vector2Int(x - 1, y)))
                    {
                        _blocks[new Vector2Int(x - 1, y)].rightEdge = horizontalEdge;
                    }
                }
            }

            for (int x = -1; x < _levelDataGridSize.x + 1; x++)
            {
                for (int y = -1; y <= _levelDataGridSize.y + 1; y++)
                {
                    var verticalPos = new Vector3(x + 0.5f, y, -0.15f);
                    var verticalEdge = Instantiate(edgePrefab, verticalPos, Quaternion.Euler(0, 0, 90), transform);
                    verticalEdge.SetActive(false);
                    if (_blocks.ContainsKey(new Vector2Int(x, y - 1)))
                    {
                        _blocks[new Vector2Int(x, y - 1)].upEdge = verticalEdge;
                    }

                    if (_blocks.ContainsKey(new Vector2Int(x, y)))
                    {
                        _blocks[new Vector2Int(x, y)].downEdge = verticalEdge;
                    }
                }
            }

            for (int x = -1; x <= _levelDataGridSize.x + 1; x++)
            {
                for (int y = -1; y < _levelDataGridSize.y + 1; y++)
                {
                    var cornerPosition = new Vector3(x, y, -0.1f);
                    var corner = Instantiate(cornerPrefab, cornerPosition, Quaternion.identity, transform);

                    if (_blocks.ContainsKey(new Vector2Int(x, y - 1)))
                    {
                        _blocks[new Vector2Int(x, y - 1)].upperLeftCorner = corner;
                    }

                    if (_blocks.ContainsKey(new Vector2Int(x, y)))
                    {
                        _blocks[new Vector2Int(x, y)].lowerLeftCorner = corner;
                    }

                    if (_blocks.ContainsKey(new Vector2Int(x - 1, y)))
                    {
                        _blocks[new Vector2Int(x - 1, y)].lowerRightCorner = corner;
                    }

                    if (_blocks.ContainsKey(new Vector2Int(x - 1, y - 1)))
                    {
                        _blocks[new Vector2Int(x - 1, y - 1)].upperRightCorner = corner;
                    }

                    corner.SetActive(false);
                }
            }
        }

        private List<GameObject> _currentlyHighlightedEdges = new();

        public void HighlightGrid(PieceType pieceType)
        {
            var pos = _camera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;

            pos += (Vector3)pieceType.offset;

            var x = Mathf.FloorToInt(pos.x);
            var y = Mathf.FloorToInt(pos.y);

            foreach (var currentlyHighlightedEdge in _currentlyHighlightedEdges)
            {
                currentlyHighlightedEdge.SetActive(false);
            }

            _currentlyHighlightedEdges.Clear();

            foreach (var pieceTypeBlock in pieceType.Blocks)
            {
                var cellPos = pieceTypeBlock.cellPosition + new Vector2Int(x, y);
                if (cellPos.x < 0 || cellPos.y < 0)
                {
                    return;
                }

                if (cellPos.x > _levelDataGridSize.x || cellPos.y > _levelDataGridSize.y)
                {
                    return;
                }

                if (!_blocks.TryGetValue(cellPos, out var block))
                {
                    return;
                }

                if (!block.IsOverlappingEdges(pieceTypeBlock.block))
                {
                    return;
                }
            }

            foreach (var pieceTypeBlock in pieceType.Blocks)
            {
                var cellPos = pieceTypeBlock.cellPosition + new Vector2Int(x, y);

                if (pieceTypeBlock.block.upOccupied)
                {
                    _currentlyHighlightedEdges.AddRange(HighlightEdge(new Vector2Int(cellPos.x, cellPos.y), 0));
                }

                if (pieceTypeBlock.block.downOccupied)
                {
                    _currentlyHighlightedEdges.AddRange(HighlightEdge(new Vector2Int(cellPos.x, cellPos.y), 1));
                }

                if (pieceTypeBlock.block.leftOccupied)
                {
                    _currentlyHighlightedEdges.AddRange(HighlightEdge(new Vector2Int(cellPos.x, cellPos.y), 2));
                }

                if (pieceTypeBlock.block.rightOccupied)
                {
                    _currentlyHighlightedEdges.AddRange(HighlightEdge(new Vector2Int(cellPos.x, cellPos.y), 3));
                }
            }
        }

        public bool TryPlaceToGrid(PieceType pieceType)
        {
            var pos = _camera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            pos += (Vector3)pieceType.offset;

            var x = Mathf.FloorToInt(pos.x);
            var y = Mathf.FloorToInt(pos.y);

            _currentlyHighlightedEdges.Clear();

            foreach (var pieceTypeBlock in pieceType.Blocks)
            {
                var cellPos = pieceTypeBlock.cellPosition + new Vector2Int(x, y);
                if (cellPos.x < 0 || cellPos.y < 0)
                {
                    return false;
                }

                if (cellPos.x > _levelDataGridSize.x || cellPos.y > _levelDataGridSize.y)
                {
                    return false;
                }

                if (!_blocks.TryGetValue(cellPos, out var block))
                {
                    return false;
                }

                if (!block.IsOverlappingEdges(pieceTypeBlock.block))
                {
                    return false;
                }
            }

            foreach (var pieceTypeBlock in pieceType.Blocks)
            {
                var cellPos = pieceTypeBlock.cellPosition + new Vector2Int(x, y);

                if (pieceTypeBlock.block.upOccupied)
                {
                    FillCells(new Vector2Int(cellPos.x, cellPos.y), 0);
                }

                if (pieceTypeBlock.block.downOccupied)
                {
                    FillCells(new Vector2Int(cellPos.x, cellPos.y), 1);
                }

                if (pieceTypeBlock.block.leftOccupied)
                {
                    FillCells(new Vector2Int(cellPos.x, cellPos.y), 2);
                }

                if (pieceTypeBlock.block.rightOccupied)
                {
                    FillCells(new Vector2Int(cellPos.x + 1, cellPos.y), 3);
                }
            }

            CheckForClosedAreas();

            return true;
        }

        private List<GameObject> HighlightEdge(Vector2Int pos, int dir)
        {
            var result = new List<GameObject>();
            if (_blocks.TryGetValue(pos, out var baseCell))
            {
                if (dir == 0)
                {
                    baseCell.upEdge.SetActive(true);
                    result.Add(baseCell.upEdge);
                }

                if (dir == 1)
                {
                    baseCell.downEdge.SetActive(true);
                    result.Add(baseCell.downEdge);
                }

                if (dir == 2)
                {
                    baseCell.leftEdge.SetActive(true);
                    result.Add(baseCell.leftEdge);
                }

                if (dir == 3)
                {
                    baseCell.rightEdge.SetActive(true);
                    result.Add(baseCell.rightEdge);
                }
            }

            return result;
        }

        private void FillCells(Vector2Int pos, int dir)
        {
            if (!_blocks.TryGetValue(pos, out _))
            {
                return;
            }

            if (_blocks.TryGetValue(pos, out var baseCell))
            {
                if (dir == 0)
                {
                    baseCell.upOccupied = true;
                    baseCell.upEdge.SetActive(true);
                }

                if (dir == 1)
                {
                    baseCell.downOccupied = true;
                    baseCell.downEdge.SetActive(true);
                }

                if (dir == 2)
                {
                    baseCell.leftOccupied = true;
                    baseCell.leftEdge.SetActive(true);
                }

                if (dir == 3)
                {
                    baseCell.rightOccupied = true;
                    baseCell.rightEdge.SetActive(true);
                }

                baseCell.AdjustCorners();
            }

            if (dir == 0)
            {
                var temp = new Vector2Int(pos.x, pos.y + 1);
                if (_blocks.TryGetValue(temp, out var upCell))
                {
                    upCell.downOccupied = true;
                }
            }

            if (dir == 1)
            {
                var temp = new Vector2Int(pos.x, pos.y - 1);
                if (_blocks.TryGetValue(temp, out var downCell))
                {
                    downCell.upOccupied = true;
                }
            }

            if (dir == 2)
            {
                var temp = new Vector2Int(pos.x - 1, pos.y);
                if (_blocks.TryGetValue(temp, out var leftCell))
                {
                    leftCell.rightOccupied = true;
                }
            }

            if (dir == 3)
            {
                var temp = new Vector2Int(pos.x + 1, pos.y);
                if (_blocks.TryGetValue(temp, out var rightCell))
                {
                    rightCell.leftOccupied = true;
                }
            }
        }

        private void CheckForClosedAreas()
        {
            var finishedCells = new List<Vector2Int>();
            for (int i = 0; i < _levelDataGridSize.x; i++)
            {
                for (int j = 0; j < _levelDataGridSize.y; j++)
                {
                    var block = _blocks[new Vector2Int(i, j)];

                    if (!block.visible)
                    {
                        continue;
                    }

                    if (block.IsClosedArea)
                    {
                        continue;
                    }

                    if (block.CheckIfClosed())
                    {
                        block.IsClosedArea = true;
                        var filledCenter = Instantiate(filledCenterPrefab, new Vector3(i + 0.5f, j + 0.5f, 0), Quaternion.identity, transform);

                        _filledCenters[new Vector2Int(i, j)] = filledCenter;
                        finishedCells.Add(new Vector2Int(i, j));
                    }
                }
            }

            if (finishedCells.Count == 0) return;

            ProcessFinishedLines(CheckForHorizontalLines(), true);
            ProcessFinishedLines(CheckForVerticalLines(), false);
        }

        private void ProcessFinishedLines(List<List<Vector2Int>> finishedLines, bool horizontal)
        {
            foreach (var finishedLine in finishedLines)
            {
                var isAllFinished = true;
                var foundCells = new HashSet<Vector2Int>();
                foreach (var pos in finishedLine)
                {
                    if (!_filledCenters.TryGetValue(pos, out var center))
                    {
                        isAllFinished = false;
                    }

                    if (!isAllFinished)
                    {
                        break;
                    }

                    foundCells.Add(pos);
                }

                var limit = horizontal ? _levelDataGridSize.x : _levelDataGridSize.y;

                if (foundCells.Count == limit)
                {
                    foreach (var cellPos in foundCells)
                    {
                        Destroy(_filledCenters[cellPos]);
                        ClearReferences(cellPos, foundCells);
                    }
                }
            }

            for (int x = 0; x < _levelDataGridSize.x; x++)
            {
                for (int y = 0; y < _levelDataGridSize.y; y++)
                {
                    AdjustCorners(new Vector2Int(x, y));
                }
            }
        }

        private void AdjustCorners(Vector2Int cellPos)
        {
            _blocks.TryGetValue(cellPos, out var selfBlock);

            var hasUpper = _blocks.TryGetValue(cellPos + Vector2Int.up, out var upperBlock) && upperBlock.visible;
            var hasLower = _blocks.TryGetValue(cellPos + Vector2Int.down, out var lowerBlock) && lowerBlock.visible;
            var hasLeft = _blocks.TryGetValue(cellPos + Vector2Int.left, out var leftBlock) && leftBlock.visible;
            var hasRight = _blocks.TryGetValue(cellPos + Vector2Int.right, out var rightBlock) && rightBlock.visible;

            var hasUpperLeft = _blocks.TryGetValue(cellPos + Vector2Int.left + Vector2Int.up, out var upperLeftBlock) && upperLeftBlock.visible;
            var hasLowerLeft = _blocks.TryGetValue(cellPos + Vector2Int.left + Vector2Int.down, out var lowerLeftBlock) && lowerLeftBlock.visible;
            var hasUpperRight = _blocks.TryGetValue(cellPos + Vector2Int.right + Vector2Int.up, out var upperRightBlock) && upperRightBlock.visible;
            var hasLowerRight = _blocks.TryGetValue(cellPos + Vector2Int.right + Vector2Int.down, out var lowerRightBlock) && lowerRightBlock.visible;

            var shallCloseUpperRight = true;
            var shallCloseLowerRight = true;
            var shallCloseLowerLeft = true;
            var shallCloseUpperLeft = true;

            if (selfBlock.upOccupied || selfBlock.rightOccupied)
            {
                shallCloseUpperRight = false;
            }

            if (hasUpper && (upperBlock.downOccupied || upperBlock.rightOccupied))
            {
                shallCloseUpperRight = false;
            }

            if (hasRight && (rightBlock.leftOccupied || rightBlock.upOccupied))
            {
                shallCloseUpperRight = false;
            }

            if (hasUpperRight && (upperRightBlock.downOccupied || upperRightBlock.leftOccupied))
            {
                shallCloseUpperRight = false;
            }

            if (selfBlock.downOccupied || selfBlock.rightOccupied)
            {
                shallCloseLowerRight = false;
            }

            if (hasRight && (rightBlock.leftOccupied || rightBlock.downOccupied))
            {
                shallCloseLowerRight = false;
            }

            if (hasLower && (lowerBlock.upOccupied || lowerBlock.rightOccupied))
            {
                shallCloseLowerRight = false;
            }

            if (hasLowerRight && (lowerRightBlock.upOccupied || lowerRightBlock.leftOccupied))
            {
                shallCloseLowerRight = false;
            }

            if (selfBlock.upOccupied || selfBlock.leftOccupied)
            {
                shallCloseUpperLeft = false;
            }

            if (hasLeft && (leftBlock.rightOccupied || leftBlock.upOccupied))
            {
                shallCloseUpperLeft = false;
            }

            if (hasUpper && (upperBlock.downOccupied || upperBlock.leftOccupied))
            {
                shallCloseUpperLeft = false;
            }

            if (hasUpperLeft && (upperLeftBlock.downOccupied || upperLeftBlock.rightOccupied))
            {
                shallCloseUpperLeft = false;
            }

            if (selfBlock.downOccupied || selfBlock.leftOccupied)
            {
                shallCloseLowerLeft = false;
            }

            if (hasLeft && (leftBlock.rightOccupied || leftBlock.downOccupied))
            {
                shallCloseLowerLeft = false;
            }

            if (hasLower && (lowerBlock.upOccupied || lowerBlock.leftOccupied))
            {
                shallCloseLowerLeft = false;
            }

            if (hasLowerLeft && (lowerLeftBlock.upOccupied || lowerLeftBlock.rightOccupied))
            {
                shallCloseLowerLeft = false;
            }

            if (shallCloseUpperRight)
            {
                _blocks[cellPos].upperRightCorner.SetActive(false);
            }

            if (shallCloseLowerRight)
            {
                _blocks[cellPos].lowerRightCorner.SetActive(false);
            }

            if (shallCloseUpperLeft)
            {
                _blocks[cellPos].upperLeftCorner.SetActive(false);
            }

            if (shallCloseLowerLeft)
            {
                _blocks[cellPos].lowerLeftCorner.SetActive(false);
            }
        }

        private void ClearReferences(Vector2Int cellPos, HashSet<Vector2Int> cells)
        {
            _blocks[cellPos].IsClosedArea = false;
            if (cells.Contains(cellPos + Vector2Int.up) || !_blocks[cellPos + Vector2Int.up].IsClosedArea)
            {
                _blocks[cellPos].upEdge.SetActive(false);
                _blocks[cellPos].upOccupied = false;
                _blocks[cellPos + Vector2Int.up].downOccupied = false;
            }

            if (cells.Contains(cellPos + Vector2Int.down) || !_blocks[cellPos + Vector2Int.down].IsClosedArea)
            {
                _blocks[cellPos].downEdge.SetActive(false);
                _blocks[cellPos].downOccupied = false;
                _blocks[cellPos + Vector2Int.down].upOccupied = false;
            }

            if (cells.Contains(cellPos + Vector2Int.left) || !_blocks[cellPos + Vector2Int.left].IsClosedArea)
            {
                _blocks[cellPos].leftEdge.SetActive(false);
                _blocks[cellPos].leftOccupied = false;
                _blocks[cellPos + Vector2Int.left].rightOccupied = false;
            }

            if (cells.Contains(cellPos + Vector2Int.right) || !_blocks[cellPos + Vector2Int.right].IsClosedArea)
            {
                _blocks[cellPos].rightEdge.SetActive(false);
                _blocks[cellPos].rightOccupied = false;
                _blocks[cellPos + Vector2Int.right].leftOccupied = false;
            }
        }

        private List<List<Vector2Int>> CheckForHorizontalLines()
        {
            var result = new List<List<Vector2Int>>();
            for (int y = 0; y < _levelDataGridSize.y; y++)
            {
                var line = new List<Vector2Int>();
                var filled = true;
                for (int x = 0; x < _levelDataGridSize.x; x++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!_blocks[pos].IsClosedArea)
                    {
                        filled = false;
                        break;
                    }

                    line.Add(pos);
                }

                if (filled)
                {
                    result.Add(line);
                }
            }

            return result;
        }

        public PieceType GetFirstPlaceablePiece(PieceType[] pieces)
        {
            var leftPieces = new List<PieceType>(pieces);
            while (leftPieces.Count > 0)
            {
                var currentPiece = leftPieces[0];
                leftPieces.RemoveAt(0);

                for (int x = 0; x < _levelDataGridSize.x; x++)
                {
                    for (int y = 0; y < _levelDataGridSize.y; y++)
                    {
                        var isPlaceable = true;
                        foreach (var pieceTypeBlock in currentPiece.Blocks)
                        {
                            var cellPos = pieceTypeBlock.cellPosition + new Vector2Int(x, y);
                            if (cellPos.x < 0 || cellPos.y < 0)
                            {
                                isPlaceable = false;
                                break;
                            }

                            if (cellPos.x > _levelDataGridSize.x || cellPos.y > _levelDataGridSize.y)
                            {
                                isPlaceable = false;
                                break;
                            }

                            if (!_blocks.TryGetValue(cellPos, out var block))
                            {
                                isPlaceable = false;
                                break;
                            }

                            if (!block.IsOverlappingEdges(pieceTypeBlock.block))
                            {
                                isPlaceable = false;
                                break;
                            }
                        }

                        if (isPlaceable)
                        {
                            return currentPiece;
                        }
                    }
                }
            }

            IsBoardLocked();
            return null;
        }

        [Button]
        public bool IsBoardLocked()
        {
            Debug.LogError("Is board locked");
            var pieces = _piecePickerUI.GetActivePieces();

            foreach (var pieceType in pieces)
            {
                for (int x = 0; x < _levelDataGridSize.x; x++)
                {
                    for (int y = 0; y < _levelDataGridSize.y; y++)
                    {
                        var isPlaceable = true;
                        foreach (var pieceTypeBlock in pieceType.Blocks)
                        {
                            var cellPos = pieceTypeBlock.cellPosition + new Vector2Int(x, y);
                            if (cellPos.x < 0 || cellPos.y < 0)
                            {
                                isPlaceable = false;
                                break;
                            }

                            if (cellPos.x > _levelDataGridSize.x || cellPos.y > _levelDataGridSize.y)
                            {
                                isPlaceable = false;
                                break;
                            }

                            if (!_blocks.TryGetValue(cellPos, out var block))
                            {
                                isPlaceable = false;
                                break;
                            }

                            if (!block.IsOverlappingEdges(pieceTypeBlock.block))
                            {
                                isPlaceable = false;
                                break;
                            }
                        }

                        if (isPlaceable)
                        {
                            Debug.LogError(x + "  " + y);
                            return true;
                        }
                    }
                }
            }

            Debug.LogError("Board locked");

            return false;
        }

        private List<List<Vector2Int>> CheckForVerticalLines()
        {
            var result = new List<List<Vector2Int>>();
            for (int x = 0; x < _levelDataGridSize.x; x++)
            {
                var line = new List<Vector2Int>();
                var filled = true;
                for (int y = 0; y < _levelDataGridSize.y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!_blocks[pos].IsClosedArea)
                    {
                        filled = false;
                        break;
                    }

                    line.Add(pos);
                }

                if (filled)
                {
                    result.Add(line);
                }
            }

            return result;
        }

        private void OnDrawGizmos()
        {
            return;
            for (int x = 0; x < _levelDataGridSize.x; x++)
            {
                for (int y = 0; y < _levelDataGridSize.y; y++)
                {
                    var cellPos = new Vector2Int(x, y);

                    _blocks.TryGetValue(cellPos, out var selfBlock);

                    var hasUpper = _blocks.TryGetValue(cellPos + Vector2Int.up, out var upperBlock) && upperBlock.visible;
                    var hasLower = _blocks.TryGetValue(cellPos + Vector2Int.down, out var lowerBlock) && lowerBlock.visible;
                    var hasLeft = _blocks.TryGetValue(cellPos + Vector2Int.left, out var leftBlock) && leftBlock.visible;
                    var hasRight = _blocks.TryGetValue(cellPos + Vector2Int.right, out var rightBlock) && rightBlock.visible;

                    var hasUpperLeft = _blocks.TryGetValue(cellPos + Vector2Int.left + Vector2Int.up, out var upperLeftBlock) && upperLeftBlock.visible;
                    var hasLowerLeft = _blocks.TryGetValue(cellPos + Vector2Int.left + Vector2Int.down, out var lowerLeftBlock) && lowerLeftBlock.visible;
                    var hasUpperRight = _blocks.TryGetValue(cellPos + Vector2Int.right + Vector2Int.up, out var upperRightBlock) && upperRightBlock.visible;
                    var hasLowerRight = _blocks.TryGetValue(cellPos + Vector2Int.right + Vector2Int.down, out var lowerRightBlock) && lowerRightBlock.visible;

                    var shallCloseUpperRight = true;
                    var shallCloseLowerRight = true;
                    var shallCloseLowerLeft = true;
                    var shallCloseUpperLeft = true;

                    if (selfBlock.upOccupied || selfBlock.rightOccupied)
                    {
                        shallCloseUpperRight = false;
                    }

                    if (hasUpper && (upperBlock.downOccupied || upperBlock.rightOccupied))
                    {
                        shallCloseUpperRight = false;
                    }

                    if (hasRight && (rightBlock.leftOccupied || rightBlock.upOccupied))
                    {
                        shallCloseUpperRight = false;
                    }

                    if (hasUpperRight && (upperRightBlock.downOccupied || upperRightBlock.leftOccupied))
                    {
                        shallCloseUpperRight = false;
                    }

                    if (selfBlock.downOccupied || selfBlock.rightOccupied)
                    {
                        shallCloseLowerRight = false;
                    }

                    if (hasRight && (rightBlock.leftOccupied || rightBlock.downOccupied))
                    {
                        shallCloseLowerRight = false;
                    }

                    if (hasLower && (lowerBlock.upOccupied || lowerBlock.rightOccupied))
                    {
                        shallCloseLowerRight = false;
                    }

                    if (hasLowerRight && (lowerRightBlock.upOccupied || lowerRightBlock.leftOccupied))
                    {
                        shallCloseLowerRight = false;
                    }

                    if (selfBlock.upOccupied || selfBlock.leftOccupied)
                    {
                        shallCloseUpperLeft = false;
                    }

                    if (hasLeft && (leftBlock.rightOccupied || leftBlock.upOccupied))
                    {
                        shallCloseUpperLeft = false;
                    }

                    if (hasUpper && (upperBlock.downOccupied || upperBlock.leftOccupied))
                    {
                        shallCloseUpperLeft = false;
                    }

                    if (hasUpperLeft && (upperLeftBlock.downOccupied || upperLeftBlock.rightOccupied))
                    {
                        shallCloseUpperLeft = false;
                    }

                    if (selfBlock.downOccupied || selfBlock.leftOccupied)
                    {
                        shallCloseLowerLeft = false;
                    }

                    if (hasLeft && (leftBlock.rightOccupied || leftBlock.downOccupied))
                    {
                        shallCloseLowerLeft = false;
                    }

                    if (hasLower && (lowerBlock.upOccupied || lowerBlock.leftOccupied))
                    {
                        shallCloseLowerLeft = false;
                    }

                    if (hasLowerLeft && (lowerLeftBlock.upOccupied || lowerLeftBlock.rightOccupied))
                    {
                        shallCloseLowerLeft = false;
                    }

                    if (shallCloseUpperRight)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }

                    // Gizmos.DrawCube(new Vector3(cellPos.x + 1f, cellPos.y + 1f, 0), Vector3.one * 0.2f);

                    if (shallCloseLowerRight)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }

                    // Gizmos.DrawCube(new Vector3(cellPos.x + 1, cellPos.y, 0), Vector3.one * 0.2f);


                    if (shallCloseUpperLeft)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }

                    // Gizmos.DrawCube(new Vector3(cellPos.x, cellPos.y + 1, 0), Vector3.one * 0.2f);

                    if (shallCloseLowerLeft)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }

                    // Gizmos.DrawCube(new Vector3(cellPos.x, cellPos.y, 0), Vector3.one * 0.2f);
                }
            }
        }

        public void DisableHighlight()
        {
            foreach (var currentlyHighlightedEdge in _currentlyHighlightedEdges)
            {
                currentlyHighlightedEdge.SetActive(false);
            }

            _currentlyHighlightedEdges.Clear();
        }
    }
}