using System;
using UnityEngine;

namespace Watermelon
{
    public class GateDirection
    {
        public static readonly GateDirection[] DIRECTIONS = new GateDirection[]
        {
            // Left
            new GateDirection()
            {
                PositionOffset = new Vector2Int(-1, 0),

                CalculateAlignedPosition = (basePos, fig, i) => new Vector2Int(basePos.x - 1, basePos.y + i),

                GetAlignedSize = (fig) => fig.Size.y,
                GetNonAlignedSize = (fig) => fig.Size.x,

                GetMoveOffset = (fig) => new Vector3(-fig.Size.x, 0, 0),
                DirectionNormal = new Vector3(1, 0, 0),
                ClipOffset = new Vector3(0.2f, 0, 0),

                ArrowRotation = -90,
                ParticleRotation = Quaternion.Euler(0, -90, 0),
            },

            // Right
            new GateDirection()
            {
                PositionOffset = new Vector2Int(1, 0),

                CalculateAlignedPosition = (basePos, fig, i) => new Vector2Int(basePos.x + fig.Size.x, basePos.y + i),

                GetAlignedSize = (fig) => fig.Size.y,
                GetNonAlignedSize = (fig) => fig.Size.x,

                GetMoveOffset = (fig) => new Vector3(fig.Size.x, 0, 0),
                DirectionNormal = new Vector3(-1, 0, 0),
                ClipOffset = new Vector3(-0.2f, 0, 0),

                ArrowRotation = 90,
                ParticleRotation = Quaternion.Euler(0, 90, 0),
            },

            // Top
            new GateDirection()
            {
                PositionOffset = new Vector2Int(0, 1),

                CalculateAlignedPosition = (basePos, fig, i) => new Vector2Int(basePos.x + i, basePos.y + fig.Size.y),

                GetAlignedSize = (fig) => fig.Size.x,
                GetNonAlignedSize = (fig) => fig.Size.y,

                GetMoveOffset = (fig) => new Vector3(0, 0, fig.Size.y),
                DirectionNormal = new Vector3(0, 0, -1),
                ClipOffset = new Vector3(0, 0, -0.2f),

                ArrowRotation = -90,
                ParticleRotation = Quaternion.Euler(0, 0, 0),
            },

            // Bottom
            new GateDirection()
            {
                PositionOffset = new Vector2Int(0, -1),

                CalculateAlignedPosition = (basePos, fig, i) => new Vector2Int(basePos.x + i, basePos.y - 1),

                GetAlignedSize = (fig) => fig.Size.x,
                GetNonAlignedSize = (fig) => fig.Size.y,

                GetMoveOffset = (fig) => new Vector3(0, 0, -fig.Size.y),
                DirectionNormal = new Vector3(0, 0, 1),
                ClipOffset = new Vector3(0, 0, 0.2f),

                ArrowRotation = 90,
                ParticleRotation = Quaternion.Euler(0, 180, 0),
            }
        };

        public Vector2Int PositionOffset { get; private set; }

        public Func<Vector2Int, LevelFigure, int, Vector2Int> CalculateAlignedPosition { get; private set; }

        public Func<LevelFigure, int> GetAlignedSize { get; private set; }
        public Func<LevelFigure, int> GetNonAlignedSize { get; private set; }
        public Func<LevelFigure, Vector3> GetMoveOffset { get; private set; }

        public Vector3 DirectionNormal { get; private set; }
        public Vector3 ClipOffset { get; private set; }

        public int ArrowRotation { get; private set; }
        public Quaternion ParticleRotation { get; private set; }

        public Vector3 Position => new Vector3(PositionOffset.x, 0, PositionOffset.y);

        public GateDirection()
        {

        }

        public enum Type
        {
            None = -1,
            Left = 0,
            Right = 1,
            Top = 2,
            Bottom = 3
        }
    }
}
