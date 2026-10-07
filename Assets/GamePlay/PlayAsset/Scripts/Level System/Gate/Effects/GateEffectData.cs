using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class GateEffectData
    {
#if UNITY_EDITOR
        // Mapping from type to the serialized field names we should draw
        public static readonly Dictionary<GateEffectType, string[]> FIELDS = new Dictionary<GateEffectType, string[]>
        {
            { GateEffectType.Chains,        new[] { "keysAmount" } },
            { GateEffectType.Doors,         new[] { "isOpened" } },
            { GateEffectType.Ice,           new[] { "iceTurnsAmount" } },
        };
#endif

        [SerializeField] GateEffectType type;
        public GateEffectType Type => type;

        [SerializeField] bool isOpened;
        public bool IsOpened => isOpened;

        [SerializeField] int keysAmount;
        public int KeysAmount => keysAmount;

        [SerializeField] int iceTurnsAmount;
        public int IceTurnsAmount => iceTurnsAmount;

        [SerializeField] bool jumpClockwise = true;
        public bool JumpClockwise => jumpClockwise;
    }
}