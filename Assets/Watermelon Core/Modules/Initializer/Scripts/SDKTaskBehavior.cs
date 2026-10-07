using UnityEngine;

namespace Watermelon
{
    public abstract class SDKTaskBehavior : MonoBehaviour
    {
        public abstract void Init(SDKInitializer initializer);

        public LoadingTask Task { get; protected set; }
    }
}
