using System.Collections;
using UnityEngine;

namespace Watermelon
{
    public class SDKInitializer : MonoBehaviour
    {
        private SDKBehavior[] behaviors;
        private SDKTaskBehavior[] tasksBehaviors;

        private bool isCompleted;

        public void Init()
        {
            isCompleted = false;

            behaviors = GetComponents<SDKBehavior>();
            foreach (SDKBehavior behavior in behaviors)
            {
                behavior?.Init();
            }

            tasksBehaviors = GetComponents<SDKTaskBehavior>();
            foreach (SDKTaskBehavior taskBehavior in tasksBehaviors)
            {
                taskBehavior?.Init(this);

                GameLoading.AddTask(taskBehavior.Task);
            }

#if UNITY_EDITOR
            OnTasksCompleted();
#else
            StartCoroutine(TasksCheck());
#endif
        }

        private IEnumerator TasksCheck()
        {
            while (!isCompleted)
            {
                yield return null;

                bool completed = true;
                foreach (SDKTaskBehavior taskBehavior in tasksBehaviors)
                {
                    if (taskBehavior == null) continue;

                    if (!taskBehavior.Task.IsFinished)
                        completed = false;
                }

                if (completed)
                    OnTasksCompleted();
            }
        }

        private void OnTasksCompleted()
        {
            if (isCompleted) return;

            isCompleted = true;

            foreach (SDKBehavior behavior in behaviors)
            {
                behavior?.OnUserConsentReceived();
            }
        }
    }
}
