using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public sealed class CombinedEffectBehavior : BlockEffectBehavior
    {
        [SerializeField] GameObject combinedObjectPrefab;

        private List<LevelBlockBehavior> blocksList;
        private List<ConnectedBlocks> connectedBlocks;

        private static readonly Vector2Int[] DIRECTIONS = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };

        public void SpawnVisualObject(List<ConnectedBlocks> connectedBlocks)
        {
            this.connectedBlocks = connectedBlocks;

            foreach (ConnectedBlocks connectedBlock in connectedBlocks)
            {
                Vector3 spawnPosition = (connectedBlock.PositionA + connectedBlock.PositionB) / 2f;
                spawnPosition += new Vector3(0, GameConsts.BLOCK_HEIGHT, 0);

                GameObject visualObject = Instantiate(combinedObjectPrefab, spawnPosition, connectedBlock.Rotation);
                visualObject.transform.SetParent(transform);

                CombinedEffectVisuals combinedEffectVisuals = visualObject.GetComponent<CombinedEffectVisuals>();
                combinedEffectVisuals.Init(connectedBlock);

                connectedBlock.SetVisuals(combinedEffectVisuals);
            }
        }

        public override void OnDisabled(LevelBlockBehavior blockBehavior)
        {
            if (!blocksList.IsNullOrEmpty())
            {
                for (int i = 0; i < blocksList.Count; i++)
                {
                    LevelBlockBehavior block = blocksList[i];
                    if (block.HasActiveEffect())
                    {
                        CombinedEffectBehavior combinedEffect = block.GetEffect<CombinedEffectBehavior>(BlockEffectType.Combines);
                        if (combinedEffect != null)
                        {
                            combinedEffect.OnLinkedBlockDisabled(blockBehavior);
                        }
                    }
                }
            }
        }

        public void OnLinkedBlockDisabled(LevelBlockBehavior levelBlockBehavior)
        {
            int index = blocksList.IndexOf(levelBlockBehavior);
            if (index != -1)
            {
                blocksList.RemoveAt(index);

                for (int i = connectedBlocks.Count - 1; i >= 0; i--)
                {
                    if (connectedBlocks[i].BlockB == levelBlockBehavior)
                    {
                        Destroy(connectedBlocks[i].Visuals.gameObject);

                        connectedBlocks.RemoveAt(i);
                    }
                }

                if (blocksList.Count <= 1 || connectedBlocks.Count == 0)
                {
                    DisableEffect();
                }
            }
        }

        public override void OnBlockCollected()
        {
            DisableEffect();
        }

        public override void OnBlockEnteredGate(LevelBlockBehavior levelBlockBehavior, GateBehavior gateBehavior)
        {

        }

        public override void OnBlockSplit()
        {
            if (!blocksList.IsNullOrEmpty())
            {
                for (int i = 0; i < blocksList.Count; i++)
                {
                    LevelBlockBehavior block = blocksList[i];
                    if (block != null)
                    {
                        CombinedEffectBehavior combinedEffect = block.GetEffect<CombinedEffectBehavior>(BlockEffectType.Combines);
                        if (combinedEffect != null)
                        {
                            combinedEffect.DisableEffect();
                        }
                    }
                }
            }
        }

        public override bool MoveMultiplyObjects()
        {
            return true;
        }

        public override List<LevelBlockBehavior> GetLinkedBlocks()
        {
            return blocksList;
        }

        public static void CombineBlocks(List<LevelBlockBehavior> combinedBlocks)
        {
            if (combinedBlocks == null || combinedBlocks.Count <= 1)
            {
                Debug.LogWarning("Not enough blocks to combine.");

                return;
            }

            // Find all touching pairs for pin visualization
            Dictionary<LevelBlockBehavior, List<ConnectedBlocks>> blockConnections = new Dictionary<LevelBlockBehavior, List<ConnectedBlocks>>();
            for (int i = 0; i < combinedBlocks.Count; i++)
            {
                LevelBlockBehavior blockA = combinedBlocks[i];
                Vector2Int[] cellsA = blockA.GetOccupiedCells();

                blockConnections.Add(blockA, new List<ConnectedBlocks>());

                for (int j = i + 1; j < combinedBlocks.Count; j++)
                {
                    LevelBlockBehavior blockB = combinedBlocks[j];
                    Vector2Int[] cellsB = blockB.GetOccupiedCells();

                    // Check for adjacency between any cell in A and any cell in B
                    foreach (Vector2Int cellA in cellsA)
                    {
                        foreach (Vector2Int dir in DIRECTIONS)
                        {
                            Vector2Int neighborCell = cellA + dir;
                            foreach (Vector2Int cellB in cellsB)
                            {
                                if (neighborCell == cellB)
                                {
                                    blockConnections[blockA].Add(new ConnectedBlocks(blockA, blockB, cellA, cellB));
                                }
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<LevelBlockBehavior, List<ConnectedBlocks>> pair in blockConnections)
            {
                LevelBlockBehavior blockA = pair.Key;
                List<ConnectedBlocks> connectedPairs = pair.Value;

                // Check if the block has a combined effect
                CombinedEffectBehavior effect = blockA.GetEffect<CombinedEffectBehavior>(BlockEffectType.Combines);
                if (effect != null)
                {
                    // Assign the connected pairs to the effect
                    effect.blocksList = combinedBlocks;
                    effect.SpawnVisualObject(connectedPairs);
                }
            }
        }

        public enum Direction { Up, Down, Left, Right }

        public class ConnectedBlocks
        {
            public LevelBlockBehavior BlockA { get; }
            public LevelBlockBehavior BlockB { get; }

            public Vector2Int CellA { get; }
            public Vector2Int CellB { get; }

            public Vector3 PositionA => new Vector3(CellA.x, 0, CellA.y);
            public Vector3 PositionB => new Vector3(CellB.x, 0, CellB.y);

            public Direction Direction { get; }
            public Quaternion Rotation { get; }

            public CombinedEffectVisuals Visuals { get; private set; }

            public ConnectedBlocks(LevelBlockBehavior blockA, LevelBlockBehavior blockB, Vector2Int cellA, Vector2Int cellB)
            {
                BlockA = blockA;
                BlockB = blockB;
                CellA = cellA;
                CellB = cellB;

                // Calculate direction based on the difference between the two cells
                Vector2Int diff = cellB - cellA;
                if (diff == new Vector2Int(0, 1))
                {
                    Direction = CombinedEffectBehavior.Direction.Up;
                    Rotation = Quaternion.Euler(0, 180, 0);
                }
                else if (diff == new Vector2Int(0, -1))
                {
                    Direction = CombinedEffectBehavior.Direction.Down;
                    Rotation = Quaternion.Euler(0, 0, 0);
                }
                else if (diff == new Vector2Int(1, 0))
                {
                    Direction = CombinedEffectBehavior.Direction.Right;
                    Rotation = Quaternion.Euler(0, -90, 0);
                }
                else if (diff == new Vector2Int(-1, 0))
                {
                    Direction = CombinedEffectBehavior.Direction.Left;
                    Rotation = Quaternion.Euler(0, 90, 0);
                }
            }

            public void SetVisuals(CombinedEffectVisuals combinedEffectVisuals)
            {
                Visuals = combinedEffectVisuals;
            }
        }
    }
}
