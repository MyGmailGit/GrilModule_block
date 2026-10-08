using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Watermelon
{
    public sealed class RopeEffectBehavior : BlockEffectBehavior
    {
        [SerializeField] Transform labelTransform;
        [SerializeField] TextMeshProUGUI amountText;
        [SerializeField] GameObject ropePrefab;

        [Space]
        [SerializeField] RopePositionData[] ropePositionDatas;

        private List<RopeBehavior> ropes = new List<RopeBehavior>();
        public List<RopeBehavior> Ropes => ropes;

        private Tween delayTweenCase;

        public override void OnCreated(LevelBlockBehavior blockBehavior)
        {
            BlockColor[] ropesColors = effectData.ropesColors;

            RopePositionData positionData = GetPositionData(linkedBlock.BlockData.Type);
            if (ropesColors.Length > positionData.TransformDatas.Length)
            {
                Debug.LogError($"Ropes amount is more than available positions for element type: {linkedBlock.BlockData.Type} in {gameObject.name}.", gameObject);

                DisableEffect();

                return;
            }

            Bounds bounds = blockBehavior.Figure.GetHorizontalCenterBounds();
            transform.position = blockBehavior.transform.position + bounds.center;

            for (int i = 0; i < ropesColors.Length; i++)
            {
                GameObject ropeObject = Instantiate(ropePrefab, transform);

                RopeBehavior ropeBehavior = ropeObject.GetComponent<RopeBehavior>();
                ropeBehavior.Init(this, positionData.TransformDatas[i], ropesColors[i]);

                ropes.Add(ropeBehavior);
            }

            amountText.text = ropes.Count.ToString();

            RopesManager.RegisterElement(this);
        }

        public override void OnDisabled(LevelBlockBehavior blockBehavior)
        {
            RopesManager.UnregisterElement(this);
        }

        public void OnRopeCut(RopeBehavior ropeBehavior)
        {
            ropes.Remove(ropeBehavior);

            amountText.text = ropes.Count.ToString();

            if (ropes.Count == 0)
            {
                labelTransform.gameObject.SetActive(false);

                delayTweenCase = transform.DOScaleY(0, 0.6f).OnComplete(() =>
                {
                    DisableEffect();
                });
            }

            AudioController.PlaySound(AudioController.AudioClips.actionDone);
        }

        public override bool IsClickable()
        {
            return ropes.Count == 0;
        }

        private void OnDestroy()
        {
            delayTweenCase.Kill();
        }

        public RopePositionData GetPositionData(BlockType blockType)
        {
            for (int i = 0; i < ropePositionDatas.Length; i++)
            {
                if (ropePositionDatas[i].BlockType == blockType)
                {
                    return ropePositionDatas[i];
                }
            }

            Debug.LogError($"Rope visuals data not found for element type: {blockType} in {gameObject.name}.", gameObject);

            return null;
        }

        public override bool CanBeReapplied()
        {
            return false;
        }

        public override BlockEffectData GetCurrentEffectData()
        {
            BlockColor[] currentRopesColors = effectData.ropesColors;
            if (currentRopesColors.Length != ropes.Count)
                currentRopesColors = ropes.Select(x => x.RopeColor).ToArray();

            return new BlockEffectData(effectData)
            {
                ropesColors = currentRopesColors,
            };
        }

        [System.Serializable]
        public class RopePositionData
        {
            [SerializeField] BlockType blockType;
            public BlockType BlockType => blockType;

            [SerializeField] RopeTransform[] transformDatas;
            public RopeTransform[] TransformDatas => transformDatas;
        }

        [System.Serializable]
        public class RopeTransform
        {
            [SerializeField] Vector3 position;
            public Vector3 Position => position;

            [SerializeField] Vector3 rotation;
            public Vector3 Rotation => rotation;

            [SerializeField] Vector3 scale = new Vector3(1, 1, 1);
            public Vector3 Scale => scale;

            [Space]
            [SerializeField] RopeType ropeType;
            public RopeType RopeType => ropeType;
        }
    }
}
