using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Piece
{
    [CreateAssetMenu(fileName = "Piece Type", menuName = "Piece/Piece Type", order = 0)]
    public class PieceType : ScriptableObject
    {
        [SerializeField] private BlockData[] blocks;
        [SerializeField] private PieceRepresentation pieceRepresentation;

        public BlockData[] Blocks => blocks;
        public PieceRepresentation PieceRepresentation => pieceRepresentation;
        public Vector2 offset;

        [Serializable]
        public class BlockData
        {
            public Vector2Int cellPosition;
            public Block block;
        }

        [Serializable]
        public class Block
        {
            private bool _isClosedArea;
            public bool IsClosedArea => _isClosedArea;

            public GameObject gemObject;
            public bool hasGemInIt;

            public bool upOccupied;
            public bool downOccupied;
            public bool leftOccupied;
            public bool rightOccupied;

            public SpriteRenderer leftEdge;
            public SpriteRenderer rightEdge;
            public SpriteRenderer upEdge;
            public SpriteRenderer downEdge;

            public GameObject upperLeftCorner;
            public GameObject upperRightCorner;
            public GameObject lowerLeftCorner;
            public GameObject lowerRightCorner;

            public bool visible = true;

            public void SetClosedArea(bool isClosedArea, Color edgeColor, Color centerColor)
            {
                _isClosedArea = isClosedArea;

                if (isClosedArea)
                {
                    upEdge.color = centerColor;
                    downEdge.color = centerColor;
                    leftEdge.color = centerColor;
                    rightEdge.color = centerColor;
                }
                else
                {
                    upEdge.color = edgeColor;
                    downEdge.color = edgeColor;
                    leftEdge.color = edgeColor;
                    rightEdge.color = edgeColor;
                }
            }

            private Vector3 _originalUpEdgePosition;
            private Vector3 _originalDownEdgePosition;
            private Vector3 _originalLeftEdgePosition;
            private Vector3 _originalRightEdgePosition;

            private Quaternion _upEdgeOriginalRotation;
            private Quaternion _downEdgeOriginalRotation;
            private Quaternion _leftEdgeOriginalRotation;
            private Quaternion _rightEdgeOriginalRotation;

            public void Initialize()
            {
                _originalUpEdgePosition = upEdge.transform.position;
                _originalDownEdgePosition = downEdge.transform.position;
                _originalLeftEdgePosition = leftEdge.transform.position;
                _originalRightEdgePosition = rightEdge.transform.position;

                _upEdgeOriginalRotation = upEdge.transform.rotation;
                _downEdgeOriginalRotation = downEdge.transform.rotation;
                _leftEdgeOriginalRotation = leftEdge.transform.rotation;
                _rightEdgeOriginalRotation = rightEdge.transform.rotation;
            }

            private static Vector3 _originalScale = new(0.25f, 0.25f, 0.25f);

            public void ResetEdges()
            {
                upEdge.transform.SetPositionAndRotation(_originalUpEdgePosition, _upEdgeOriginalRotation);
                downEdge.transform.SetPositionAndRotation(_originalDownEdgePosition, _downEdgeOriginalRotation);
                leftEdge.transform.SetPositionAndRotation(_originalLeftEdgePosition, _leftEdgeOriginalRotation);
                rightEdge.transform.SetPositionAndRotation(_originalRightEdgePosition, _rightEdgeOriginalRotation);

                upEdge.transform.localScale = _originalScale;
                downEdge.transform.localScale = _originalScale;
                leftEdge.transform.localScale = _originalScale;
                rightEdge.transform.localScale = _originalScale;
            }

            public bool IsOverlappingEdges(Block block)
            {
                if (upOccupied && block.upOccupied)
                {
                    return false;
                }

                if (downOccupied && block.downOccupied)
                {
                    return false;
                }

                if (leftOccupied && block.leftOccupied)
                {
                    return false;
                }

                if (rightOccupied && block.rightOccupied)
                {
                    return false;
                }

                return true;
            }

            public bool CheckIfClosed()
            {
                return upOccupied && downOccupied && leftOccupied && rightOccupied;
            }

            public void AdjustCorners()
            {
                if (upOccupied)
                {
                    if (upperLeftCorner != null)
                    {
                        upperLeftCorner.SetActive(true);
                    }

                    if (upperRightCorner != null)
                    {
                        upperRightCorner.SetActive(true);
                    }
                }

                if (downOccupied)
                {
                    if (lowerLeftCorner != null)
                    {
                        lowerLeftCorner.SetActive(true);
                    }

                    if (lowerRightCorner != null)
                    {
                        lowerRightCorner.SetActive(true);
                    }
                }

                if (leftOccupied)
                {
                    if (lowerLeftCorner != null)
                    {
                        lowerLeftCorner.SetActive(true);
                    }

                    if (upperLeftCorner != null)
                    {
                        upperLeftCorner.SetActive(true);
                    }
                }

                if (rightOccupied)
                {
                    if (lowerRightCorner != null)
                    {
                        lowerRightCorner.SetActive(true);
                    }

                    if (upperRightCorner != null)
                    {
                        upperRightCorner.SetActive(true);
                    }
                }
            }
        }
    }
}