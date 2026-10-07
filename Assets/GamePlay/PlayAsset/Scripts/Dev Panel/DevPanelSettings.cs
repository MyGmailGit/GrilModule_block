using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Dev Panel Settings", menuName = "Data/Core/Dev Panel Settings")]
    public class DevPanelSettings : ScriptableObject
    {
        [SerializeField] bool isEnabled = true;
        public bool IsEnabled => isEnabled;

        [SerializeField] bool isForceToVideo = true;
        /// <summary>
        /// 视频开关
        /// </summary>
        public bool IsForceToVideo => isForceToVideo;
    }
}
