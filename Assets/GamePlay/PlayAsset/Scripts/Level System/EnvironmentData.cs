using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Environment Data", menuName = "Data/Environment Data")]
    public class EnvironmentData : ScriptableObject
    {
        [SerializeField] GameObject borderPrefab;
        public GameObject BorderPrefab => borderPrefab;

        [SerializeField] GameObject cornerPrefab;
        public GameObject CornerPrefab => cornerPrefab;

        [SerializeField] GameObject gatePrefab;
        public GameObject GatePrefab => gatePrefab;

        [SerializeField] GameObject innerObstaclePrefab;
        public GameObject InnerObstaclePrefab => innerObstaclePrefab;

        [SerializeField] GameObject innerTilePrefab;
        public GameObject InnerTilePrefab => innerTilePrefab;

        [SerializeField] GameObject borderInnerGround;
        public GameObject BorderInnerGround => borderInnerGround;

        [SerializeField] GameObject cornerInnerGroundConvex;
        public GameObject CornerInnerGroundConvex => cornerInnerGroundConvex;

        [SerializeField] GameObject cornerInnerGroundConcave;
        public GameObject CornerInnerGroundConcave => cornerInnerGroundConcave;
    }
}
