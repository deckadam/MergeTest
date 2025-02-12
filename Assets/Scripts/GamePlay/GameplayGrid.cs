using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using DG.Tweening;
using MobileHapticsProFreeEdition;
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
        private static readonly int Tiling = Shader.PropertyToID("_Tiling");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private AudioClip cellCreatedSound;
        [SerializeField] private MeshRenderer gridRenderer;
        [SerializeField] private GameObject filledCenterPrefab;
        [SerializeField] private SpriteRenderer edgePrefab;
        [SerializeField] private GameObject cornerPrefab;
        [SerializeField] private GameObject gemPrefab;
        [SerializeField] private SerializedDictionary<Vector2Int, PieceType.Block> _blocks;
        [SerializeField] private GameObject blastParticle;

        private Camera _camera;
        private LevelData _levelData;
        private Vector2Int _levelDataGridSize;
        private Dictionary<Vector2Int, GameObject> _filledCenters;

        private GamePlayUI _gamePlayUI;
        private List<SpriteRenderer> _currentlyHighlightedEdges = new();
        private GameplayLevel _level;

        private int _comboCount;

        [Inject]
        private void Inject(GamePlayUI gamePlayUI)
        {
            _gamePlayUI = gamePlayUI;
        }

        public void Initialize(LevelData levelData, GameplayLevel gameplayLevel)
        {
            _level = gameplayLevel;
            _levelData = levelData;
            _levelDataGridSize = levelData.GridSize;
            _camera = Camera.main;

            _filledCenters = new Dictionary<Vector2Int, GameObject>();

            gridRenderer.transform.localScale = new Vector3(_levelDataGridSize.x, _levelDataGridSize.y, 1);
            gridRenderer.transform.localPosition = new Vector3(_levelDataGridSize.x / 2f, _levelDataGridSize.y / 2f, 0);
            gridRenderer.material.SetVector(Tiling, new Vector4(_levelDataGridSize.x, _levelDataGridSize.y, 0, 0));
            gridRenderer.material.SetColor(BaseColor, levelData.levelColor);

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
                    horizontalEdge.GetComponent<SpriteRenderer>().color = levelData.edgeColor;
                    horizontalEdge.gameObject.SetActive(false);
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
                    verticalEdge.GetComponent<SpriteRenderer>().color = levelData.edgeColor;
                    verticalEdge.gameObject.SetActive(false);
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
                    var cornerPosition = new Vector3(x, y, -0.2f);
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

            for (int x = 0; x <= _levelDataGridSize.x; x++)
            {
                for (int y = 0; y < _levelDataGridSize.y; y++)
                {
                    var gem = Instantiate(gemPrefab, new Vector3(x + 0.5f, y + 0.5f, -0.25f), Quaternion.identity, transform);
                    gem.SetActive(false);
                    _blocks[new Vector2Int(x, y)].gemObject = gem;
                }
            }

            for (int x = 0; x <= _levelDataGridSize.x; x++)
            {
                for (int y = 0; y < _levelDataGridSize.y; y++)
                {
                    _blocks[new Vector2Int(x, y)].Initialize();
                }
            }


            CreateGems(new HashSet<Vector2Int>());
        }

        private void CreateGems(HashSet<Vector2Int> cellsToAvoid)
        {
            var currentGemCount = 0;
            for (int x = 0; x < _levelDataGridSize.x; x++)
            {
                for (int y = 0; y < _levelDataGridSize.y; y++)
                {
                    if (_blocks[new Vector2Int(x, y)].hasGemInIt)
                    {
                        currentGemCount++;
                    }
                }
            }

            var allowedGemCount = Random.Range(_levelData.GemCountRange.x, _levelData.GemCountRange.y);

            var gemCountToSpawn = allowedGemCount - currentGemCount;

            while (gemCountToSpawn > 0)
            {
                var randomCell = new Vector2Int(Random.Range(0, _levelData.GemCountRange.x - 1), Random.Range(0, _levelData.GemCountRange.y - 1));

                if (cellsToAvoid.Contains(randomCell))
                {
                    continue;
                }

                if (_blocks[randomCell].hasGemInIt)
                {
                    continue;
                }

                _blocks[randomCell].gemObject.SetActive(true);
                _blocks[randomCell].hasGemInIt = true;

                gemCountToSpawn--;
            }
        }

        public void HighlightGrid(PieceType pieceType)
        {
            var pos = _camera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;

            pos += (Vector3)pieceType.offset;

            var x = Mathf.FloorToInt(pos.x);
            var y = Mathf.FloorToInt(pos.y);

            foreach (var currentlyHighlightedEdge in _currentlyHighlightedEdges)
            {
                currentlyHighlightedEdge.gameObject.SetActive(false);
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
                    FillCells(cellPos, 0);
                }

                if (pieceTypeBlock.block.downOccupied)
                {
                    FillCells(cellPos, 1);
                }

                if (pieceTypeBlock.block.leftOccupied)
                {
                    FillCells(cellPos, 2);
                }

                if (pieceTypeBlock.block.rightOccupied)
                {
                    FillCells(cellPos, 3);
                }
            }

            var finishedCellCount = CheckForClosedAreas();

            if (finishedCellCount == 0)
            {
                _comboCount = 0;
            }
            else
            {
                _comboCount += finishedCellCount;
                _gamePlayUI.ShowComboText(_comboCount);
            }

            return true;
        }

        private List<SpriteRenderer> HighlightEdge(Vector2Int pos, int dir)
        {
            var result = new List<SpriteRenderer>();
            if (_blocks.TryGetValue(pos, out var baseCell))
            {
                if (dir == 0)
                {
                    baseCell.upEdge.gameObject.SetActive(true);
                    result.Add(baseCell.upEdge);
                }

                if (dir == 1)
                {
                    baseCell.downEdge.gameObject.SetActive(true);
                    result.Add(baseCell.downEdge);
                }

                if (dir == 2)
                {
                    baseCell.leftEdge.gameObject.SetActive(true);
                    result.Add(baseCell.leftEdge);
                }

                if (dir == 3)
                {
                    baseCell.rightEdge.gameObject.SetActive(true);
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
                    baseCell.upEdge.gameObject.SetActive(true);
                }

                if (dir == 1)
                {
                    baseCell.downOccupied = true;
                    baseCell.downEdge.gameObject.SetActive(true);
                }

                if (dir == 2)
                {
                    baseCell.leftOccupied = true;
                    baseCell.leftEdge.gameObject.SetActive(true);
                }

                if (dir == 3)
                {
                    baseCell.rightOccupied = true;
                    baseCell.rightEdge.gameObject.SetActive(true);
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

        private int CheckForClosedAreas()
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
                        block.SetClosedArea(true, _levelData.edgeColor, _levelData.centerColor);

                        var filledCenter = Instantiate(filledCenterPrefab, new Vector3(i + 0.5f, j + 0.5f, 0), Quaternion.identity, transform);

                        filledCenter.GetComponent<SpriteRenderer>().color = _levelData.centerColor;
                        filledCenter.transform.localScale = Vector3.zero;
                        filledCenter.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBounce);
                        _filledCenters[new Vector2Int(i, j)] = filledCenter;
                        AudioManager.instance.PlayAudio(cellCreatedSound, 0.2f);
                        finishedCells.Add(new Vector2Int(i, j));
                    }
                }
            }

            if (finishedCells.Count == 0) return 0;
            var cells = new HashSet<Vector2Int>();
            ProcessFinishedLines(cells, CheckForHorizontalLines(), true);
            ProcessFinishedLines(cells, CheckForVerticalLines(), false);

            PlayBlastParticles(cells);

            return finishedCells.Count;
        }

        private void PlayBlastParticles(HashSet<Vector2Int> finishedCells)
        {
            if (finishedCells.Count == 0)
            {
                return;
            }

            foreach (var finishedCell in finishedCells)
            {
                AndroidTapticWave.Haptic(HapticModes.MediumTap);
                var particle = Instantiate(blastParticle, new Vector3(finishedCell.x + 0.5f, finishedCell.y + 0.5f, -8f), Quaternion.identity);
                Destroy(particle, 1f);
                _filledCenters[finishedCell].transform.DOScale(0f, 0.2f).SetEase(Ease.OutBounce).OnComplete(() => { Destroy(_filledCenters[finishedCell]); });
            }
        }

        private void ProcessFinishedLines(HashSet<Vector2Int> cells, List<List<Vector2Int>> finishedLines, bool horizontal)
        {
            var foundGems = new HashSet<Vector2Int>();
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
                        if (_blocks[cellPos].hasGemInIt)
                        {
                            foundGems.Add(cellPos);
                        }

                        cells.Add(cellPos);
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

            if (foundGems.Count != 0)
            {
                CollectGems(foundGems);
            }
        }

        private int _collectedGemCount = 0;

        private void CollectGems(HashSet<Vector2Int> foundGems)
        {
            foreach (var cellPos in foundGems)
            {
                AndroidTapticWave.Haptic(HapticModes.Confirm);
                _collectedGemCount++;
                _blocks[cellPos].gemObject.SetActive(false);
                _blocks[cellPos].hasGemInIt = false;
            }

            _gamePlayUI.UpdateRequirementText(_collectedGemCount);

            Debug.LogError(_levelData.requiredGemCount + "  " + _collectedGemCount);

            if (_collectedGemCount >= _levelData.requiredGemCount)
            {
                _level.OnLevelFinished();
                return;
            }

            CreateGems(foundGems);
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

        private async void ClearReferences(Vector2Int cellPos, HashSet<Vector2Int> cells)
        {
            var block = _blocks[cellPos];
            block.SetClosedArea(false, _levelData.edgeColor, _levelData.centerColor);
            if (cells.Contains(cellPos + Vector2Int.up) || !_blocks[cellPos + Vector2Int.up].IsClosedArea)
            {
                var tr = block.upEdge.transform;
                tr.DOJump(new Vector3(Random.Range(-2, 2), -3), 2.5f, 1, 1f).SetRelative(true).OnComplete(() => tr.gameObject.SetActive(false));
                tr.DOScale(0f, 0.5f).SetDelay(0.2f);
                block.upOccupied = false;
                _blocks[cellPos + Vector2Int.up].downOccupied = false;
            }

            if (cells.Contains(cellPos + Vector2Int.down) || !_blocks[cellPos + Vector2Int.down].IsClosedArea)
            {
                var tr = block.downEdge.transform;
                tr.DOJump(new Vector3(Random.Range(-2, 2), -3), 2.5f, 1, 1f).SetRelative(true).OnComplete(() => tr.gameObject.SetActive(false));
                tr.DOScale(0f, 0.5f).SetDelay(0.2f);
                block.downOccupied = false;
                _blocks[cellPos + Vector2Int.down].upOccupied = false;
            }

            if (cells.Contains(cellPos + Vector2Int.left) || !_blocks[cellPos + Vector2Int.left].IsClosedArea)
            {
                var tr = block.leftEdge.transform;
                tr.DOJump(new Vector3(Random.Range(-2, 2), -3), 2.5f, 1, 1f).SetRelative(true).OnComplete(() => tr.gameObject.SetActive(false));
                tr.DOScale(0f, 0.5f).SetDelay(0.2f);
                block.leftOccupied = false;
                _blocks[cellPos + Vector2Int.left].rightOccupied = false;
            }

            if (cells.Contains(cellPos + Vector2Int.right) || !_blocks[cellPos + Vector2Int.right].IsClosedArea)
            {
                var tr = block.rightEdge.transform;
                tr.DOJump(new Vector3(Random.Range(-2, 2), -3), 2.5f, 1, 1f).SetRelative(true).OnComplete(() => tr.gameObject.SetActive(false));
                tr.DOScale(0f, 0.5f).SetDelay(0.2f);
                block.rightOccupied = false;
                _blocks[cellPos + Vector2Int.right].leftOccupied = false;
            }

            await UniTask.Delay(1100);

            block.ResetEdges();
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
                var currentPiece = leftPieces[Random.Range(0, leftPieces.Count)];
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
            var pieces = _gamePlayUI.GetActivePieces();

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
                            return true;
                        }
                    }
                }
            }


            _level.OnLevelFailed();
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

        public void DisableHighlight()
        {
            foreach (var currentlyHighlightedEdge in _currentlyHighlightedEdges)
            {
                currentlyHighlightedEdge.gameObject.SetActive(false);
            }

            _currentlyHighlightedEdges.Clear();
        }
    }
}