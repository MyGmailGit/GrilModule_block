using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    [System.Serializable]
    public class LevelElementData
    {
        [SerializeField, Hide] ElementType type; // we hide it in editor to avoid unexpected behaviour
        public ElementType Type => type;

        [SerializeField, Hide] Vector2Int position;// we hide it in editor to avoid unexpected behaviour
        public Vector2Int Position => position;

        [Space]
        [SerializeField] BlockType blockType;
        public BlockType BlockType => blockType;

        [SerializeField] BlockColor blockColor;
        public BlockColor BlockColor => blockColor;

        [Header("Interactable Object")]
        [SerializeField, ShowIf("IsInteractableObject")] InteractableObjectData interactableObjectData;
        public InteractableObjectData InteractableObjectData => interactableObjectData;

        [Header("Special Effects")]
        [SerializeField, ShowIf("IsBlock")] BlockEffectData[] blockEffects;
        public BlockEffectData[] BlockEffects => blockEffects;

        [SerializeField, ShowIf("IsGate")] GateEffectData[] gateEffects;
        public GateEffectData[] GateEffects => gateEffects;

        public void OnValidate()
        {
            blockEffects = blockEffects.OrderBy(x => EditorEffectsHelper.GetSortingOrder(x.Type)).ToArray();
        }

        #region editor stuff

        public bool IsInteractableObject()
        {
            return type == ElementType.InteractableObject;
        }

        public bool IsBlock()
        {
            return type == ElementType.Block;
        }

        public bool IsGate()
        {
            return type == ElementType.Gate;
        }

        public bool IsBlockEffectVisible()
        {
            return (IsBlock() && (blockEffects.Length > 0));
        }

        public bool IsGateEffectVisible()
        {
            return (IsGate() && (gateEffects.Length > 0));
        }

        #endregion
    }
}
