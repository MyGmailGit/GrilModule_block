using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class BorderData
    {
        public ElementType Type { get; private set; }
        public LevelElementData LevelElementData { get; private set; }
        public Vector2Int Position { get; private set; }
        public bool IsCorner { get; private set; }
        public bool IsConvex { get; private set; }
        public bool IsHorizonal { get; private set; }
        public bool IsUnified { get; private set; }
        public bool ShoulBeSpawned { get; private set; }
        public List<BorderData> UnifiedElements { get; private set; }
        public Vector3 SpawnPosition { get; private set; }
        public float SpawnSize { get; private set; }
        public Vector3 Offset { get; private set; }
        public Vector3 Rotation { get; private set; }

        public GateDirection.Type GateDirection { get; private set; }
        public GateBehavior GateBehavior { get; private set; }

        public BorderData(LevelElementData levelElementData)
        {
            LevelElementData = levelElementData;

            Type = levelElementData.Type;
            Position = levelElementData.Position;
        }

        public void SetCornerState(bool state)
        {
            IsCorner = state;
        }

        public void SetUnifiedElements(List<BorderData> unifiedElements)
        {
            UnifiedElements = unifiedElements;
        }

        public void SetGateBehavior(GateBehavior gateBehavior)
        {
            GateBehavior = gateBehavior;
        }

        public void SetGateDirection(GateDirection.Type gateDirection)
        {
            GateDirection = gateDirection;
        }

        public void InitOffset(Vector3 offset)
        {
            Offset = offset;
        }

        public void InitRotation(Vector3 rotation)
        {
            Rotation = rotation;
        }

        public void InitConvexState(bool state)
        {
            IsConvex = state;
        }

        public void InitHorizonalState(bool state)
        {
            IsHorizonal = state;
        }

        public void InitUnifiedState(bool state)
        {
            IsUnified = state;
        }

        public void InitShouldBeSpawned(bool state)
        {
            ShoulBeSpawned = state;
        }

        public void InitSpawnPosition(Vector3 position)
        {
            SpawnPosition = position;
        }

        public void InitSpawnSize(float size)
        {
            SpawnSize = size;
        }
    }
}
