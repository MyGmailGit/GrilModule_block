using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class BlockEffectData
    {
#if UNITY_EDITOR
        // Mapping from type to the serialized field names we should draw
        public static readonly Dictionary<BlockEffectType, string[]> FIELDS = new Dictionary<BlockEffectType, string[]>
        {
            { BlockEffectType.FixedDirection, new[] { "horizontalDirection" } },
            { BlockEffectType.Layered,        new[] { "layeredBlockColor" } },
            { BlockEffectType.Bomb,           new[] { "bombDuration" } },
            { BlockEffectType.Ice,            new[] { "iceTurnsAmount" } },
            { BlockEffectType.Chain,          new[] { "keysAmount" } },
            { BlockEffectType.Ropes,          new[] { "ropesColors" } },
            { BlockEffectType.Scissors,       new[] { "scissorsColor" } },
            { BlockEffectType.Combines,       new[] { "combineGroupID" } },
        };
#endif

        [SerializeField] BlockEffectType type;
        public BlockEffectType Type => type;

        public bool horizontalDirection;
        public BlockColor layeredBlockColor;
        public int bombDuration;
        public int iceTurnsAmount;
        public int keysAmount;
        public BlockColor[] ropesColors;
        public BlockColor scissorsColor;
        public int combineGroupID = 0;

        public BlockEffectData(BlockEffectData blockEffectData)
        {
            type = blockEffectData.type;
            horizontalDirection = blockEffectData.horizontalDirection;
            layeredBlockColor = blockEffectData.layeredBlockColor;
            bombDuration = blockEffectData.bombDuration;
            iceTurnsAmount = blockEffectData.iceTurnsAmount;
            keysAmount = blockEffectData.keysAmount;
            ropesColors = blockEffectData.ropesColors;
            scissorsColor = blockEffectData.scissorsColor;
            combineGroupID = blockEffectData.combineGroupID;
        }
    }
}
