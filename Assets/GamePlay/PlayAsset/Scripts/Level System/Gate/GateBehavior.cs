using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class GateBehavior : MonoBehaviour
    {
        [SerializeField] MeshRenderer graphicsMeshRenderer;
        public MeshRenderer GraphicsMeshRenderer => graphicsMeshRenderer;

        [SerializeField] Transform arrowTransform;
        public Transform ArrowTransform => arrowTransform;

        private BorderData data;
        public BorderData Data => data;

        private List<GateEffectBehavior> effects;
        public List<GateEffectBehavior> Effects => effects;

        public GateBehavior NextGate { get; private set; }
        public GateBehavior PreviousGate { get; private set; }

        public void Init(BorderData borderData)
        {
            data = borderData;

            graphicsMeshRenderer.material = LevelController.GetBlockColorData(borderData.LevelElementData.BlockColor).Material;

            arrowTransform.localRotation = Quaternion.Euler(0, GateDirection.DIRECTIONS[(int)borderData.GateDirection].ArrowRotation, 0);

            effects = new List<GateEffectBehavior>();
        }

        public void LinkGatesInClockwiseOrder(GateBehavior nextGate, GateBehavior previousGate)
        {
            NextGate = nextGate;
            PreviousGate = previousGate;
        }

        public bool CanGoThroughGate(LevelBlockBehavior levelBlockBehavior)
        {
            if(!effects.IsNullOrEmpty())
            {
                foreach (GateEffectBehavior effect in effects)
                {
                    if (!effect.IsActive) continue;

                    if (!effect.CanGoThroughGate(levelBlockBehavior))
                        return false;
                }
            }

            return data.LevelElementData.BlockColor == levelBlockBehavior.GetActiveBlockColor();
        }

        public void OnBlockEntered(LevelBlockBehavior pickedBlock) 
        {
            if (!effects.IsNullOrEmpty())
            {
                foreach (GateEffectBehavior effect in effects)
                {
                    if (!effect.IsActive) continue;

                    effect.OnBlockEntered(pickedBlock);
                }
            }
        }

        public void OnBlockDestructed(LevelBlockBehavior destructedBlock)
        {
            if (!effects.IsNullOrEmpty())
            {
                foreach (GateEffectBehavior effect in effects)
                {
                    if (!effect.IsActive) continue;

                    effect.OnBlockDestructed(destructedBlock);
                }
            }
        }

        #region Effect
        public bool HasEffect(GateEffectType effectType)
        {
            if (effects.IsNullOrEmpty())
                return false;

            foreach (GateEffectBehavior effect in effects)
            {
                if (!effect.IsActive) continue;

                if (effect.Type == effectType)
                    return true;
            }

            return false;
        }

        public GateEffectBehavior GetEffect(GateEffectType effectType)
        {
            if (effects.IsNullOrEmpty())
                return null;

            foreach (GateEffectBehavior effect in effects)
            {
                if (!effect.IsActive) continue;

                if (effect.Type == effectType)
                    return effect;
            }

            return null;
        }

        public T GetEffect<T>(GateEffectType effectType) where T : GateEffectBehavior
        {
            if (effects.IsNullOrEmpty())
                return null;

            foreach (GateEffectBehavior effect in effects)
            {
                if (!effect.IsActive) continue;

                if (effect.Type == effectType)
                    return effect as T;
            }

            return null;
        }

        public void ApplyEffect(GateEffectBehavior effect)
        {
            int order = effects.Count;

            effects.Add(effect);

            effect.SetOrder(order);
            effect.OnCreated(this);
        }

        public void UnlinkEffect(GateEffectBehavior effect)
        {
            effects.Remove(effect);
        }

        public void DisableEffects()
        {
            if (effects.IsNullOrEmpty())
                return;

            foreach (GateEffectBehavior effect in effects)
            {
                if (effect.IsActive)
                    effect.OnDisabled(this);

                Destroy(effect.gameObject);
            }

            effects.Clear();
        }

        public void OnEffectDisabled(GateEffectBehavior effect)
        {
            effect.OnDisabled(this);
        }

        public bool HasActiveEffect()
        {
            if (effects.IsNullOrEmpty())
                return false;

            foreach (GateEffectBehavior effect in effects)
            {
                if (effect.IsActive)
                    return true;
            }

            return false;
        }
        #endregion
    }
}
