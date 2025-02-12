using System;
using Sirenix.OdinInspector;
using UnityEngine;

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
            [HideInInspector] public bool IsClosedArea;

            public bool upOccupied;
            public bool downOccupied;
            public bool leftOccupied;
            public bool rightOccupied;

            public GameObject leftEdge;
            public GameObject rightEdge;
            public GameObject upEdge;
            public GameObject downEdge;

            public GameObject upperLeftCorner;
            public GameObject upperRightCorner;
            public GameObject lowerLeftCorner;
            public GameObject lowerRightCorner;

            public bool visible = true;

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

            [Button]
            public void Test()
            {
                if (leftEdge != null)
                {
                    leftEdge.SetActive(!leftEdge.activeInHierarchy);
                }

                if (downEdge != null)
                {
                    downEdge.SetActive(!downEdge.activeInHierarchy);
                }

                if (rightEdge != null)
                {
                    rightEdge.SetActive(!rightEdge.activeInHierarchy);
                }

                if (upEdge != null)
                {
                    upEdge.SetActive(!upEdge.activeInHierarchy);
                }
            }

            [Button]
            public void Test2()
            {
                if (upperLeftCorner != null)
                {
                    upperLeftCorner.SetActive(!upperLeftCorner.activeInHierarchy);
                }

                if (lowerLeftCorner != null)
                {
                    lowerLeftCorner.SetActive(!lowerLeftCorner.activeInHierarchy);
                }

                if (upperRightCorner != null)
                {
                    upperRightCorner.SetActive(!upperRightCorner.activeInHierarchy);
                }

                if (lowerRightCorner != null)
                {
                    lowerRightCorner.SetActive(!lowerRightCorner.activeInHierarchy);
                }
            }
        }
    }
}