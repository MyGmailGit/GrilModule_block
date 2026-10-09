using System.Collections.Generic;
using DG.Tweening;

namespace Watermelon
{
    public class TweenCaseCollection
    {
        private List<Tween> tweenCases = new List<Tween>();
        public List<Tween> TweenCases => tweenCases;

        private SimpleCallback tweensCompleted;

        public void AddTween(Tween tweenCase)
        {
            TweenCallback onComplete = tweenCase.onComplete;
            tweenCase.OnComplete(() =>
            {
                onComplete?.Invoke();
                OnTweenCaseComplete();
            });

            tweenCases.Add(tweenCase);
        }

        public bool IsComplete()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                if (!tweenCases[i].IsComplete())
                    return false;
            }

            return true;
        }

        public void Complete()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                tweenCases[i].Complete();
            }
        }

        public void Kill()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                tweenCases[i].Kill();
            }
        }

        public void OnComplete(SimpleCallback callback)
        {
            tweensCompleted += callback;
        }

        private void OnTweenCaseComplete()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                if (!tweenCases[i].IsComplete())
                    return;
            }

            if (tweensCompleted != null)
                tweensCompleted.Invoke();
        }

        public static TweenCaseCollection operator +(TweenCaseCollection caseCollection, Tween tweenCase)
        {
            if (caseCollection == null)
                caseCollection = new TweenCaseCollection();

            caseCollection.AddTween(tweenCase);

            return caseCollection;
        }
    }
}
