using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace Watermelon
{
    public class PUTimer
    {
        public float State => delayTweenCase.ElapsedPercentage();
        public string Seconds => (duration - (duration * delayTweenCase.ElapsedPercentage())).ToString("F0");

        private float duration;
        private float startTime;

        private Tween delayTweenCase;

        private bool isActive;
        public bool IsActive => isActive;

        public PUTimer(float duration, SimpleCallback onCompleted)
        {
            this.duration = duration;

            startTime = Time.time;

            isActive = true;
            delayTweenCase = DOVirtual.DelayedCall(duration, () =>
            {
                isActive = false;
                onCompleted.Invoke();
            });
        }

        public void Pause()
        {
            delayTweenCase?.Pause();
        }

        public void Resume()
        {
            delayTweenCase?.Play();
        }

        public void OnCompleted(SimpleCallback onCompleted)
        {
            if (delayTweenCase.IsActive() && !delayTweenCase.IsComplete())
            {
                delayTweenCase.OnComplete(() => { onCompleted?.Invoke(); });
            }
            else
            {
                onCompleted?.Invoke();
            }
        }

        public void Disable()
        {
            isActive = false;

            delayTweenCase.Kill();
        }
    }
}
