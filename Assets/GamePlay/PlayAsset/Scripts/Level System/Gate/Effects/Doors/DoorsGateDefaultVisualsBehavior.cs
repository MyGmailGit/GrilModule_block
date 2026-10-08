using UnityEngine;

using DG.Tweening;

namespace Watermelon
{
    public sealed class DoorsGateDefaultVisualsBehavior : DoorsGateVisualsBehavior
    {
        [SerializeField] Transform leftDoorTransform;
        [SerializeField] Transform rightDoorTransform;

        [Space]
        [SerializeField] float openedSize = 0.1f;

        [Space]
        [SerializeField] Ease openingEasing;
        [SerializeField] float openingDelay = 0.15f;
        [SerializeField] float openingDuration = 0.3f;

        [Space]
        [SerializeField] Ease closingEasing;
        [SerializeField] float closingDelay = 0.15f;
        [SerializeField] float closingDuration = 0.3f;

        private Tween openingTweenCase;
        private Tween closingTweenCase;

        public override void Init(DoorsGateEffectBehavior doorsGateEffect, int doorsSize)
        {
            base.Init(doorsGateEffect, doorsSize);

            int gateRealSize = doorsSize;
            float halfSize = gateRealSize / 2f;

            float gateSize = doorsGateEffect.IsOpened ? openedSize : 1.0f;

            leftDoorTransform.localPosition = new Vector3(0, 0, -halfSize);
            leftDoorTransform.localScale = new Vector3(leftDoorTransform.localScale.x, leftDoorTransform.localScale.y, gateSize);

            rightDoorTransform.localPosition = new Vector3(0, 0, halfSize);
            rightDoorTransform.localScale = new Vector3(rightDoorTransform.localScale.x, rightDoorTransform.localScale.y, gateSize);
        }

        public override void Close()
        {
            openingTweenCase.Kill();
            closingTweenCase.Kill();

            openingTweenCase = leftDoorTransform.DOScaleZ(1.0f, closingDuration).SetDelay(closingDelay).SetEase(closingEasing);
            closingTweenCase = rightDoorTransform.DOScaleZ(1.0f, closingDuration).SetDelay(closingDelay).SetEase(closingEasing);
        }

        public override void Open()
        {
            openingTweenCase.Kill();
            closingTweenCase.Kill();

            openingTweenCase = leftDoorTransform.DOScaleZ(openedSize, openingDuration).SetDelay(openingDelay).SetEase(openingEasing);
            closingTweenCase = rightDoorTransform.DOScaleZ(openedSize, openingDuration).SetDelay(openingDelay).SetEase(openingEasing);
        }

        public override void Unload()
        {
            openingTweenCase.Kill();
            closingTweenCase.Kill();
        }
    }
}