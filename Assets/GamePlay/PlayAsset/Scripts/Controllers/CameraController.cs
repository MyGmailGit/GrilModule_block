using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance;
        Camera camgers = null;
        Camera orthographicCamera
        {
            get
            {
                if (camgers == null)
                {
                    camgers = GetComponent<Camera>();
                }
                return camgers;
            }
        }
        private float minSize = 3f; // 最小size值

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 根据目标宽度调整正交相机size
        /// </summary>
        /// <param name="targetWidth">目标宽度（世界单位）</param>
        public void SetCameraWidth(float targetWidth)
        {
            if (orthographicCamera == null || !orthographicCamera.orthographic)
                return;

            // 获取当前屏幕宽高比
            float aspectRatio = (float)Screen.width / Screen.height;

            // 计算当前相机可见宽度
            float currentVisibleWidth = orthographicCamera.orthographicSize * 2 * aspectRatio;

            // 如果目标宽度小于等于当前可见宽度，不做改变（保持最小size）
            if (targetWidth <= currentVisibleWidth)
                return;

            // 根据目标宽度计算新的size
            // size = (目标宽度 / 2) / 宽高比
            float newSize = (targetWidth / 2f) / aspectRatio;

            // 应用最小size限制
            newSize = Mathf.Max(newSize, minSize);

            // 设置新的size
            orthographicCamera.orthographicSize = newSize;

            Debug.Log($"相机size已调整：{newSize}，可见宽度：{targetWidth}");
        }

        /// <summary>
        /// 获取当前相机可见宽度
        /// </summary>
        public float GetCurrentVisibleWidth()
        {
            if (orthographicCamera == null)
                return 0f;

            float aspectRatio = (float)Screen.width / Screen.height;
            return orthographicCamera.orthographicSize * 2 * aspectRatio;
        }


        // [SerializeField] Vector3 offset;
        // [SerializeField] float sideOffset = 0.5f;
        // [SerializeField] float yOffset = 12;

        // public void Reposition(Vector3 targetPosition, Vector3 levelSize)
        // {
        //     Camera camera = GetComponent<Camera>();

        //     float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        //     float frustumHeight = 2.0f * distanceToTarget * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        //     float frustumWidth = frustumHeight * camera.aspect;

        //     float levelWidth = levelSize.x + sideOffset;
        //     float levelHeight = levelSize.z + yOffset;

        //     float distanceMultiplier = Mathf.Max(levelWidth / frustumWidth, levelHeight / frustumHeight);

        //     transform.position = targetPosition + (offset * distanceMultiplier);
        // }
    }
}